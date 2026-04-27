using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PoliceWebServer.Data;
using PoliceWebServer.Hubs;
using PoliceWebServer.Models;

var builder = WebApplication.CreateBuilder(args);
var useCrossSiteCookies = builder.Configuration.GetValue(
    "POLICE_CROSS_SITE_COOKIES",
    !builder.Environment.IsDevelopment());

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicies.OpenRealtime, policy =>
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "PoliceSmartHub.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = useCrossSiteCookies ? SameSiteMode.None : SameSiteMode.Lax;
        options.Cookie.SecurePolicy = useCrossSiteCookies
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;
        options.LoginPath = "/";
        options.AccessDeniedPath = "/";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context => HandleAuthRedirectAsync(context, StatusCodes.Status401Unauthorized),
            OnRedirectToAccessDenied = context => HandleAuthRedirectAsync(context, StatusCodes.Status403Forbidden)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(AppRoles.Admin));
    options.AddPolicy(Policies.UserOnly, policy => policy.RequireRole(AppRoles.User));
    options.AddPolicy(Policies.PoliceOnly, policy => policy.RequireRole(AppRoles.Police));
    options.AddPolicy(Policies.SupportOnly, policy => policy.RequireRole(AppRoles.Support));
    options.AddPolicy(Policies.CanSubmitIncident, policy => policy.RequireRole(AppRoles.User));
    options.AddPolicy(Policies.CanViewIncidents, policy => policy.RequireRole(AppRoles.Admin, AppRoles.Police, AppRoles.Support));
    options.AddPolicy(Policies.CanTrackIncident, policy => policy.RequireRole(AppRoles.Admin, AppRoles.Police, AppRoles.Support, AppRoles.User));
    options.AddPolicy(Policies.CanUpdateIncidents, policy => policy.RequireRole(AppRoles.Admin, AppRoles.Police));
    options.AddPolicy(Policies.CanAuditAndExport, policy => policy.RequireRole(AppRoles.Admin));
});

builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddDbContext<IncidentDbContext>(options =>
{
    var provider = ResolveDatabaseProvider(builder.Configuration);
    var sqlServerConnection = ResolveConnectionString(builder.Configuration, "SqlServer");
    var postgresConnection = ResolveConnectionString(builder.Configuration, "Postgres");

    switch (provider)
    {
        case DatabaseProviders.SqlServer:
            if (string.IsNullOrWhiteSpace(sqlServerConnection))
            {
                throw new InvalidOperationException("ConnectionStrings:SqlServer chua duoc cau hinh.");
            }

            options.UseSqlServer(sqlServerConnection);
            break;

        case DatabaseProviders.Postgres:
            if (string.IsNullOrWhiteSpace(postgresConnection))
            {
                throw new InvalidOperationException("ConnectionStrings:Postgres chua duoc cau hinh.");
            }

            options.UseNpgsql(postgresConnection);
            break;

        case DatabaseProviders.InMemory:
            options.UseInMemoryDatabase("PoliceSmartHub");
            break;

        default:
            throw new InvalidOperationException("DatabaseProvider phai la 'inmemory', 'sqlserver' hoac 'postgres'.");
    }
});

var app = builder.Build();
var demoOpenAccess = builder.Configuration.GetValue("DemoOpenAccess", true);

var repoRoot = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, ".."));
var staticAssets = new StaticAssetPaths(
    IndexFile: Path.Combine(repoRoot, "index.html"),
    AdminFile: Path.Combine(repoRoot, "admin", "admin.html"),
    UserFile: Path.Combine(repoRoot, "user", "user.html"),
    PoliceFile: Path.Combine(repoRoot, "police", "police.html"),
    SupportFile: Path.Combine(repoRoot, "support", "support.html"),
    BoundaryFile: Path.Combine(repoRoot, "hcm-boundary.geojson"));

EnsureStaticAssetsExist(staticAssets);
await EnsureDatabaseReadyAsync(app.Services);

app.UseCors(CorsPolicies.OpenRealtime);
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/login", async (
    LoginRequest request,
    HttpContext context,
    IncidentDbContext dbContext) =>
{
    if (!TryGetDemoUser(request.Username, request.Password, out var user))
    {
        await WriteAuditLogAsync(
            dbContext,
            context,
            action: AuditActions.LoginFailed,
            entityType: AuditEntities.Auth,
            entityId: request.Username?.Trim() ?? "unknown",
            summary: "Dang nhap that bai.",
            detail: $"Tai khoan {request.Username?.Trim() ?? "trong"} dang nhap that bai.",
            actorUsername: request.Username?.Trim() ?? "anonymous",
            actorDisplayName: "Dang nhap that bai",
            actorRole: "Unknown");

        return Results.Unauthorized();
    }

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Username),
        new Claim(ClaimTypes.Name, user.DisplayName),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    await WriteAuditLogAsync(
        dbContext,
        context,
        action: AuditActions.LoginSuccess,
        entityType: AuditEntities.Auth,
        entityId: user.Username,
        summary: "Dang nhap thanh cong.",
        detail: $"{user.DisplayName} dang nhap vao he thong voi vai tro {user.Role}.",
        actorUsername: user.Username,
        actorDisplayName: user.DisplayName,
        actorRole: user.Role);

    return Results.Ok(new AuthenticatedUserResponse(
        user.Username,
        user.DisplayName,
        user.Role,
        GetLandingPathForRole(user.Role)));
});

app.MapPost("/api/auth/logout", async (
    HttpContext context,
    IncidentDbContext dbContext) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var actor = GetActorSnapshot(context.User);
        await WriteAuditLogAsync(
            dbContext,
            context,
            action: AuditActions.Logout,
            entityType: AuditEntities.Auth,
            entityId: actor.Username,
            summary: "Dang xuat.",
            detail: $"{actor.DisplayName} dang xuat khoi he thong.",
            actorUsername: actor.Username,
            actorDisplayName: actor.DisplayName,
            actorRole: actor.Role);
    }

    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok(new { message = "Da dang xuat." });
});

app.MapGet("/api/auth/me", (ClaimsPrincipal user) =>
{
    if (user.Identity?.IsAuthenticated != true)
    {
        return Results.Unauthorized();
    }

    var role = user.FindFirstValue(ClaimTypes.Role);
    if (string.IsNullOrWhiteSpace(role))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new AuthenticatedUserResponse(
        user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
        user.Identity?.Name ?? string.Empty,
        role,
        GetLandingPathForRole(role)));
});

app.MapGet("/api/health", (IConfiguration configuration) => Results.Ok(new
{
    status = "ok",
    databaseProvider = ResolveDatabaseProvider(configuration),
    signalRHub = "/hubs/incidents",
    timestamp = DateTimeOffset.UtcNow
}));

app.MapPost("/api/incidents/analyze", async (
    AnalyzeIncidentRequest request,
    HttpContext context,
    IncidentDbContext dbContext) =>
{
    var assessment = AnalyzeIncident(request.Title, request.Detail, request.Level);

    await WriteAuditLogAsync(
        dbContext,
        context,
        action: AuditActions.AnalyzeIncident,
        entityType: AuditEntities.Incident,
        entityId: "preview",
        summary: "Phan tich muc do khan cap.",
        detail: $"He thong phan tich yeu cau preview va danh gia {assessment.Category} - {assessment.Level}.",
        actorUsername: GetActorSnapshot(context.User).Username,
        actorDisplayName: GetActorSnapshot(context.User).DisplayName,
        actorRole: GetActorSnapshot(context.User).Role);

    return Results.Ok(assessment.ToResponse());
}).ApplyIncidentPolicy(demoOpenAccess, Policies.CanSubmitIncident);

app.MapGet("/api/incidents", async (
    IncidentDbContext dbContext,
    string? search,
    string? status,
    string? level,
    string? source,
    string? district,
    DateTimeOffset? from,
    DateTimeOffset? to,
    string? sort) =>
{
    var query = ApplyIncidentFilters(
        dbContext.Incidents.AsNoTracking(),
        search,
        status,
        level,
        source,
        district,
        from,
        to);

    query = ApplyIncidentSort(query, sort);

    var incidents = await query
        .Select(item => item.ToDto())
        .ToListAsync();

    return Results.Ok(incidents);
}).ApplyIncidentPolicy(demoOpenAccess, Policies.CanViewIncidents);

app.MapGet("/api/incidents/{id:guid}", async (
    Guid id,
    IncidentDbContext dbContext) =>
{
    var incident = await dbContext.Incidents.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
    return incident is null
        ? Results.NotFound(new { message = "Khong tim thay vu viec." })
        : Results.Ok(incident.ToDto());
}).RequireAuthorization(Policies.CanTrackIncident);

app.MapGet("/api/incidents/export", async (
    HttpContext context,
    IncidentDbContext dbContext,
    string? search,
    string? status,
    string? level,
    string? source,
    string? district,
    DateTimeOffset? from,
    DateTimeOffset? to,
    string? sort) =>
{
    var query = ApplyIncidentFilters(
        dbContext.Incidents.AsNoTracking(),
        search,
        status,
        level,
        source,
        district,
        from,
        to);

    var incidents = await ApplyIncidentSort(query, sort)
        .Select(item => item.ToDto())
        .ToListAsync();

    var csv = BuildIncidentCsv(incidents);
    var actor = GetActorSnapshot(context.User);

    await WriteAuditLogAsync(
        dbContext,
        context,
        action: AuditActions.ExportIncidents,
        entityType: AuditEntities.Incident,
        entityId: $"count:{incidents.Count}",
        summary: "Xuat bao cao vu viec.",
        detail: $"{actor.DisplayName} xuat {incidents.Count} dong du lieu bao cao.",
        actorUsername: actor.Username,
        actorDisplayName: actor.DisplayName,
        actorRole: actor.Role);

    return Results.File(
        Encoding.UTF8.GetBytes(csv),
        "text/csv; charset=utf-8",
        $"incident-export-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.csv");
}).RequireAuthorization(Policies.CanAuditAndExport);

app.MapGet("/api/audit-logs", async (
    IncidentDbContext dbContext,
    string? action,
    string? actorRole,
    string? entityType,
    int? limit) =>
{
    var query = dbContext.AuditLogs.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(action))
    {
        var normalizedAction = action.Trim().ToLowerInvariant();
        query = query.Where(item => item.Action.ToLower() == normalizedAction);
    }

    if (!string.IsNullOrWhiteSpace(actorRole))
    {
        var normalizedActorRole = actorRole.Trim().ToLowerInvariant();
        query = query.Where(item => item.ActorRole.ToLower() == normalizedActorRole);
    }

    if (!string.IsNullOrWhiteSpace(entityType))
    {
        var normalizedEntityType = entityType.Trim().ToLowerInvariant();
        query = query.Where(item => item.EntityType.ToLower() == normalizedEntityType);
    }

    var take = Math.Clamp(limit ?? 50, 1, 200);

    var logs = await query
        .OrderByDescending(item => item.CreatedAt)
        .Take(take)
        .Select(item => item.ToDto())
        .ToListAsync();

    return Results.Ok(logs);
}).RequireAuthorization(Policies.CanAuditAndExport);

app.MapPost("/api/incidents", async (
    HttpContext context,
    IncidentDbContext dbContext,
    IHubContext<IncidentHub> hubContext) =>
{
    var request = await context.Request.ReadFromJsonAsync<CreateIncidentRequest>();
    if (request is null)
    {
        return Results.BadRequest(new { message = "Khong doc duoc du lieu gui len." });
    }

    if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Location))
    {
        return Results.BadRequest(new { message = "Can co loai vu viec va toa do." });
    }

    if (!TryParseLocation(request.Location, out var latitude, out var longitude))
    {
        return Results.BadRequest(new { message = "Toa do khong hop le. Dung dinh dang '10.7769, 106.7009'." });
    }

    var assessment = AnalyzeIncident(request.Title, request.Detail, request.Level);
    var actor = GetActorSnapshot(context.User);
    var now = DateTimeOffset.UtcNow;

    var incident = new IncidentRecord
    {
        Id = Guid.NewGuid(),
        Title = request.Title.Trim(),
        Detail = string.IsNullOrWhiteSpace(request.Detail) ? "Nguoi dung vua gui bao cao moi." : request.Detail.Trim(),
        Category = assessment.Category,
        Level = assessment.Level,
        UrgencyScore = assessment.UrgencyScore,
        ClassificationReason = assessment.Reason,
        Latitude = latitude,
        Longitude = longitude,
        District = ResolveDistrict(latitude, longitude),
        TimeLabel = DateTimeOffset.Now.ToString("HH:mm"),
        Status = assessment.UrgencyScore >= 85 ? IncidentStatuses.DangXacMinh : IncidentStatuses.MoiTiepNhan,
        Source = "user",
        ReporterName = actor.DisplayName,
        LastUpdatedBy = actor.DisplayName,
        InternalNote = assessment.Recommendation,
        CreatedAt = now,
        UpdatedAt = now
    };

    dbContext.Incidents.Add(incident);
    await WriteAuditLogAsync(
        dbContext,
        context,
        action: AuditActions.CreateIncident,
        entityType: AuditEntities.Incident,
        entityId: incident.Id.ToString(),
        summary: "Tao bao cao moi.",
        detail: $"{actor.DisplayName} tao bao cao {incident.Title} voi muc {incident.Level} ({incident.Category}).",
        actorUsername: actor.Username,
        actorDisplayName: actor.DisplayName,
        actorRole: actor.Role,
        saveChanges: false);

    await dbContext.SaveChangesAsync();

    var payload = incident.ToDto();
    await hubContext.Clients.All.SendAsync("IncidentCreated", payload);

    return Results.Ok(new
    {
        message = assessment.ShouldCallEmergency
            ? "Da gui bao cao thanh cong. He thong danh gia day la tinh huong khan cap cao."
            : "Da gui bao cao thanh cong.",
        analysis = assessment.ToResponse(),
        incident = payload
    });
}).ApplyIncidentPolicy(demoOpenAccess, Policies.CanSubmitIncident);

app.MapPatch("/api/incidents/{id:guid}/status", async (
    Guid id,
    UpdateIncidentStatusRequest request,
    HttpContext context,
    IncidentDbContext dbContext,
    IHubContext<IncidentHub> hubContext) =>
{
    var actor = GetActorSnapshot(context.User);
    var normalizedStatus = NormalizeStatus(request.Status);

    if (!CanUpdateIncidentStatus(actor.Role, normalizedStatus))
    {
        await WriteAuditLogAsync(
            dbContext,
            context,
            action: AuditActions.UpdateIncidentDenied,
            entityType: AuditEntities.Incident,
            entityId: id.ToString(),
            summary: "Bi tu choi cap nhat trang thai.",
            detail: $"{actor.DisplayName} khong du quyen cap nhat trang thai sang {normalizedStatus}.",
            actorUsername: actor.Username,
            actorDisplayName: actor.DisplayName,
            actorRole: actor.Role);

        return Results.Json(
            new { message = "Vai tro hien tai khong du quyen cap nhat trang thai nay." },
            statusCode: StatusCodes.Status403Forbidden);
    }

    var incident = await dbContext.Incidents.FirstOrDefaultAsync(item => item.Id == id);
    if (incident is null)
    {
        return Results.NotFound(new { message = "Khong tim thay vu viec." });
    }

    incident.Status = normalizedStatus;
    incident.LastUpdatedBy = actor.DisplayName;
    incident.UpdatedAt = DateTimeOffset.UtcNow;

    if (!string.IsNullOrWhiteSpace(request.InternalNote))
    {
        incident.InternalNote = request.InternalNote.Trim();
    }

    await WriteAuditLogAsync(
        dbContext,
        context,
        action: AuditActions.UpdateIncidentStatus,
        entityType: AuditEntities.Incident,
        entityId: incident.Id.ToString(),
        summary: "Cap nhat trang thai vu viec.",
        detail: $"{actor.DisplayName} cap nhat vu viec {incident.Title} sang {incident.Status}.",
        actorUsername: actor.Username,
        actorDisplayName: actor.DisplayName,
        actorRole: actor.Role,
        saveChanges: false);

    await dbContext.SaveChangesAsync();

    var payload = incident.ToDto();
    await hubContext.Clients.All.SendAsync("IncidentUpdated", payload);

    return Results.Ok(new
    {
        message = "Da cap nhat trang thai.",
        incident = payload
    });
}).RequireAuthorization(Policies.CanUpdateIncidents);

MapPage(app, "/", staticAssets.IndexFile);
MapPage(app, "/index.html", staticAssets.IndexFile);
MapProtectedPage(app, "/admin", staticAssets.AdminFile, Policies.AdminOnly);
MapProtectedPage(app, "/admin/admin.html", staticAssets.AdminFile, Policies.AdminOnly);
MapProtectedPage(app, "/user", staticAssets.UserFile, Policies.UserOnly);
MapProtectedPage(app, "/user/user.html", staticAssets.UserFile, Policies.UserOnly);
MapProtectedPage(app, "/police", staticAssets.PoliceFile, Policies.PoliceOnly);
MapProtectedPage(app, "/police/police.html", staticAssets.PoliceFile, Policies.PoliceOnly);
MapProtectedPage(app, "/support", staticAssets.SupportFile, Policies.SupportOnly);
MapProtectedPage(app, "/support/support.html", staticAssets.SupportFile, Policies.SupportOnly);

app.MapGet("/hcm-boundary.geojson", () => Results.File(staticAssets.BoundaryFile, "application/geo+json"));
app.MapGet("/user/data/hcm-boundary.geojson", () => Results.File(staticAssets.BoundaryFile, "application/geo+json"))
    .RequireAuthorization(Policies.UserOnly);

app.MapHub<IncidentHub>("/hubs/incidents").RequireAuthorization(Policies.CanTrackIncident);

app.MapFallback(async context =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        context.Response.Redirect(GetLandingPathForRole(context.User.FindFirstValue(ClaimTypes.Role)));
        return;
    }

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(staticAssets.IndexFile);
});

app.Run();

static void MapPage(WebApplication app, string route, string filePath)
{
    app.MapGet(route, async context =>
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            context.Response.Redirect(GetLandingPathForRole(context.User.FindFirstValue(ClaimTypes.Role)));
            return;
        }

        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync(filePath);
    });
}

static void MapProtectedPage(WebApplication app, string route, string filePath, string policy)
{
    app.MapGet(route, (HttpContext context) =>
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        return context.Response.SendFileAsync(filePath);
    }).RequireAuthorization(policy);
}

static Task HandleAuthRedirectAsync(RedirectContext<CookieAuthenticationOptions> context, int statusCode)
{
    if (IsApiOrHubRequest(context.Request))
    {
        context.Response.StatusCode = statusCode;
        return Task.CompletedTask;
    }

    context.Response.Redirect("/");
    return Task.CompletedTask;
}

static bool IsApiOrHubRequest(HttpRequest request)
{
    return request.Path.StartsWithSegments("/api") || request.Path.StartsWithSegments("/hubs");
}

static void EnsureStaticAssetsExist(StaticAssetPaths staticAssets)
{
    var requiredFiles = new[]
    {
        staticAssets.IndexFile,
        staticAssets.AdminFile,
        staticAssets.UserFile,
        staticAssets.PoliceFile,
        staticAssets.SupportFile,
        staticAssets.BoundaryFile
    };

    foreach (var file in requiredFiles)
    {
        if (!File.Exists(file))
        {
            throw new FileNotFoundException("Khong tim thay file web can thiet.", file);
        }
    }
}

static async Task EnsureDatabaseReadyAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IncidentDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

static IQueryable<IncidentRecord> ApplyIncidentFilters(
    IQueryable<IncidentRecord> query,
    string? search,
    string? status,
    string? level,
    string? source,
    string? district,
    DateTimeOffset? from,
    DateTimeOffset? to)
{
    if (!string.IsNullOrWhiteSpace(search))
    {
        var normalizedSearch = search.Trim().ToLowerInvariant();
        query = query.Where(item =>
            item.Title.ToLower().Contains(normalizedSearch) ||
            item.Detail.ToLower().Contains(normalizedSearch) ||
            item.Category.ToLower().Contains(normalizedSearch) ||
            item.District.ToLower().Contains(normalizedSearch));
    }

    if (!string.IsNullOrWhiteSpace(status))
    {
        var normalizedStatus = NormalizeStatus(status);
        query = query.Where(item => item.Status == normalizedStatus);
    }

    if (!string.IsNullOrWhiteSpace(level))
    {
        var normalizedLevel = NormalizeLevel(level);
        query = query.Where(item => item.Level == normalizedLevel);
    }

    if (!string.IsNullOrWhiteSpace(source))
    {
        var normalizedSource = source.Trim().ToLowerInvariant();
        query = query.Where(item => item.Source.ToLower() == normalizedSource);
    }

    if (!string.IsNullOrWhiteSpace(district))
    {
        var normalizedDistrict = district.Trim().ToLowerInvariant();
        query = query.Where(item => item.District.ToLower().Contains(normalizedDistrict));
    }

    if (from.HasValue)
    {
        query = query.Where(item => item.CreatedAt >= from.Value);
    }

    if (to.HasValue)
    {
        query = query.Where(item => item.CreatedAt <= to.Value);
    }

    return query;
}

static IQueryable<IncidentRecord> ApplyIncidentSort(IQueryable<IncidentRecord> query, string? sort)
{
    return sort?.Trim().ToLowerInvariant() switch
    {
        "created_asc" => query.OrderBy(item => item.CreatedAt),
        "updated_desc" => query.OrderByDescending(item => item.UpdatedAt),
        "updated_asc" => query.OrderBy(item => item.UpdatedAt),
        "urgency_desc" => query.OrderByDescending(item => item.UrgencyScore).ThenByDescending(item => item.CreatedAt),
        "urgency_asc" => query.OrderBy(item => item.UrgencyScore).ThenByDescending(item => item.CreatedAt),
        _ => query.OrderByDescending(item => item.CreatedAt)
    };
}

static string ResolveDatabaseProvider(IConfiguration configuration)
{
    var configured = configuration["POLICE_DATABASE_PROVIDER"]
        ?? configuration["DatabaseProvider"];

    configured = configured?.Trim().ToLowerInvariant();
    if (!string.IsNullOrWhiteSpace(configured))
    {
        return configured;
    }

    if (!string.IsNullOrWhiteSpace(ResolveConnectionString(configuration, "Postgres")))
    {
        return DatabaseProviders.Postgres;
    }

    if (!string.IsNullOrWhiteSpace(ResolveConnectionString(configuration, "SqlServer")))
    {
        return DatabaseProviders.SqlServer;
    }

    return DatabaseProviders.InMemory;
}

static string? ResolveConnectionString(IConfiguration configuration, string name)
{
    var envValue = configuration[$"POLICE_{name.ToUpperInvariant()}_CONNECTION"];
    if (!string.IsNullOrWhiteSpace(envValue))
    {
        return envValue.Trim();
    }

    return configuration.GetConnectionString(name);
}

static bool TryParseLocation(string raw, out double latitude, out double longitude)
{
    latitude = 0;
    longitude = 0;

    var parts = raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length != 2)
    {
        return false;
    }

    if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out latitude) ||
        !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out longitude))
    {
        return false;
    }

    return latitude is >= 10.3 and <= 11.1 && longitude is >= 106.4 and <= 107.1;
}

static string NormalizeLevel(string? level)
{
    return level?.Trim().ToLowerInvariant() switch
    {
        "high" => "high",
        "medium" => "medium",
        "low" => "low",
        "khancap" => "high",
        "cao" => "high",
        "trungbinh" => "medium",
        "thap" => "low",
        _ => "high"
    };
}

static string NormalizeStatus(string? status)
{
    return status?.Trim().ToLowerInvariant() switch
    {
        "moi tiep nhan" => IncidentStatuses.MoiTiepNhan,
        "da tiep nhan" => IncidentStatuses.DaTiepNhan,
        "dang xac minh" => IncidentStatuses.DangXacMinh,
        "da dieu phoi" => IncidentStatuses.DaDieuPhoi,
        "da xu ly" => IncidentStatuses.DaXuLy,
        _ => string.IsNullOrWhiteSpace(status) ? IncidentStatuses.MoiTiepNhan : status.Trim()
    };
}

static bool TryGetDemoUser(string? username, string? password, out DemoUser user)
{
    user = default;
    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
    {
        return false;
    }

    var normalizedUsername = username.Trim().ToLowerInvariant();
    if (!DemoUsers.All.TryGetValue(normalizedUsername, out var candidate) ||
        !string.Equals(candidate.Password, password, StringComparison.Ordinal))
    {
        return false;
    }

    user = candidate;
    return true;
}

static string GetLandingPathForRole(string? role)
{
    return role switch
    {
        AppRoles.Admin => "/admin/admin.html",
        AppRoles.User => "/user/user.html",
        AppRoles.Police => "/police/police.html",
        AppRoles.Support => "/support/support.html",
        _ => "/"
    };
}

static bool CanUpdateIncidentStatus(string role, string status)
{
    if (string.Equals(role, AppRoles.Admin, StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    if (string.Equals(role, AppRoles.Police, StringComparison.OrdinalIgnoreCase))
    {
        return status is IncidentStatuses.DaTiepNhan or IncidentStatuses.DangXacMinh or IncidentStatuses.DaDieuPhoi or IncidentStatuses.DaXuLy;
    }

    return false;
}

static ActorSnapshot GetActorSnapshot(ClaimsPrincipal user)
{
    if (user.Identity?.IsAuthenticated != true)
    {
        return new ActorSnapshot("demo-user", "Nguoi dung demo", "Anonymous");
    }

    return new ActorSnapshot(
        user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown",
        user.Identity?.Name ?? "Unknown user",
        user.FindFirstValue(ClaimTypes.Role) ?? "Unknown");
}

static async Task WriteAuditLogAsync(
    IncidentDbContext dbContext,
    HttpContext context,
    string action,
    string entityType,
    string entityId,
    string summary,
    string detail,
    string? actorUsername = null,
    string? actorDisplayName = null,
    string? actorRole = null,
    bool saveChanges = true)
{
    var fallbackActor = GetActorSnapshot(context.User);

    dbContext.AuditLogs.Add(new AuditLogRecord
    {
        Id = Guid.NewGuid(),
        Action = action,
        EntityType = entityType,
        EntityId = entityId,
        ActorUsername = actorUsername ?? fallbackActor.Username,
        ActorDisplayName = actorDisplayName ?? fallbackActor.DisplayName,
        ActorRole = actorRole ?? fallbackActor.Role,
        Summary = summary,
        Detail = detail,
        IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
        CreatedAt = DateTimeOffset.UtcNow
    });

    if (saveChanges)
    {
        await dbContext.SaveChangesAsync();
    }
}

static IncidentAssessment AnalyzeIncident(string? title, string? detail, string? requestedLevel)
{
    var combined = $"{title} {detail}".Trim();
    var normalized = RemoveDiacritics(combined).ToLowerInvariant();

    var profiles = new[]
    {
        new IncidentProfile("Bao luc / vu khi", "Nguy co bao luc", 96, "giet nguoi", "sat hai", "dam chet", "thu tieu", "co vu khi", "dao", "sung", "hanh hung", "bi tan cong", "chem", "cuop"),
        new IncidentProfile("Tai nan / cap cuu", "Su co hien truong", 88, "tai nan", "va cham", "chay", "no", "bi thuong", "mau nhieu", "ngat"),
        new IncidentProfile("Mat cap tai san", "Mat cap tai san", 70, "mat cap", "trom", "giat", "xe may bi lay", "dot nhap"),
        new IncidentProfile("Lua dao", "Nghi ngo lua dao", 64, "lua dao", "otp", "gia mao", "chuyen khoan", "tai khoan ngan hang"),
        new IncidentProfile("Gay roi cong cong", "Mat trat tu cong cong", 52, "gay roi", "danh nhau", "tap trung dong nguoi", "on ao", "tu tap"),
        new IncidentProfile("Nghi van can xac minh", "Tinh huong can xac minh", 40, "dang nghi", "kha nghi", "la mat", "theo doi")
    };

    var bestProfile = profiles
        .Select(profile => new
        {
            Profile = profile,
            Matches = profile.Keywords.Where(normalized.Contains).ToArray()
        })
        .OrderByDescending(item => item.Matches.Length)
        .ThenByDescending(item => item.Profile.BaseScore)
        .FirstOrDefault(item => item.Matches.Length > 0);

    var score = bestProfile?.Profile.BaseScore ?? 38;
    var reasons = new List<string>();

    if (bestProfile is not null)
    {
        reasons.Add($"phat hien tu khoa: {string.Join(", ", bestProfile.Matches.Take(3))}");
    }
    else
    {
        reasons.Add("mo ta chua co tu khoa ro rang, can xac minh them");
    }

    var urgencyBoosters = new Dictionary<string, int>
    {
        ["ngay bay gio"] = 8,
        ["dang"] = 6,
        ["vua xay ra"] = 8,
        ["tre em"] = 10,
        ["nguoi gia"] = 10,
        ["co nguoi bi thuong"] = 14,
        ["bat tinh"] = 16,
        ["chay lon"] = 16,
        ["de doa"] = 10
    };

    foreach (var booster in urgencyBoosters)
    {
        if (normalized.Contains(booster.Key))
        {
            score += booster.Value;
            reasons.Add($"co dau hieu tang muc khan: {booster.Key}");
        }
    }

    var requestedNormalized = NormalizeLevel(requestedLevel);
    score = Math.Max(score, requestedNormalized switch
    {
        "high" => 82,
        "medium" => 58,
        _ => 35
    });

    score = Math.Clamp(score, 15, 99);

    var level = score >= 85 ? "high" : score >= 55 ? "medium" : "low";
    var shouldCallEmergency = score >= 88;
    var category = bestProfile?.Profile.Category ?? "Tinh huong can xac minh";

    var recommendation = shouldCallEmergency
        ? "Uu tien ket noi 113 ngay, dong thoi bo sung vi tri va dau hieu nhan dang."
        : level == "medium"
            ? "Can xac minh them thong tin va theo doi phan hoi tu trung tam."
            : "Luu vao hang doi, uu tien bo sung chi tiet de phan loai chinh xac hon.";

    return new IncidentAssessment(
        Category: category,
        Level: level,
        UrgencyScore: score,
        Reason: string.Join("; ", reasons),
        ShouldCallEmergency: shouldCallEmergency,
        Recommendation: recommendation);
}

static string ResolveDistrict(double latitude, double longitude)
{
    if (latitude >= 10.76 && latitude <= 10.79 && longitude >= 106.69 && longitude <= 106.71)
    {
        return "Quan 1";
    }

    if (latitude >= 10.77 && longitude >= 106.72)
    {
        return "Thu Duc";
    }

    if (latitude >= 10.79 && longitude <= 106.69)
    {
        return "Binh Thanh";
    }

    if (latitude < 10.76 && longitude <= 106.69)
    {
        return "Quan 3";
    }

    if (latitude < 10.74)
    {
        return "Quan 7";
    }

    return "TP.HCM";
}

static string RemoveDiacritics(string input)
{
    if (string.IsNullOrWhiteSpace(input))
    {
        return string.Empty;
    }

    var normalized = input.Normalize(NormalizationForm.FormD);
    var builder = new StringBuilder(normalized.Length);

    foreach (var character in normalized)
    {
        var category = CharUnicodeInfo.GetUnicodeCategory(character);
        if (category != UnicodeCategory.NonSpacingMark)
        {
            builder.Append(character);
        }
    }

    return builder
        .ToString()
        .Normalize(NormalizationForm.FormC)
        .Replace('đ', 'd')
        .Replace('Đ', 'D');
}

static string BuildIncidentCsv(IReadOnlyCollection<IncidentResponse> incidents)
{
    var builder = new StringBuilder();
    builder.AppendLine("Id,Title,Category,Level,UrgencyScore,District,Status,Source,ReporterName,LastUpdatedBy,CreatedAt,UpdatedAt");

    foreach (var incident in incidents)
    {
        builder.AppendLine(string.Join(",",
            CsvEscape(incident.Id.ToString()),
            CsvEscape(incident.Title),
            CsvEscape(incident.Category),
            CsvEscape(incident.Level),
            incident.UrgencyScore.ToString(CultureInfo.InvariantCulture),
            CsvEscape(incident.District),
            CsvEscape(incident.Status),
            CsvEscape(incident.Source),
            CsvEscape(incident.ReporterName),
            CsvEscape(incident.LastUpdatedBy),
            CsvEscape(incident.CreatedAt.ToString("O")),
            CsvEscape(incident.UpdatedAt.ToString("O"))));
    }

    return builder.ToString();
}

static string CsvEscape(string? value)
{
    var safe = value ?? string.Empty;
    return $"\"{safe.Replace("\"", "\"\"")}\"";
}

internal sealed record CreateIncidentRequest(string Title, string Location, string? Detail, string? Level);
internal sealed record UpdateIncidentStatusRequest(string Status, string? InternalNote);
internal sealed record AnalyzeIncidentRequest(string? Title, string? Detail, string? Level);
internal sealed record LoginRequest(string Username, string Password);

internal sealed record AuthenticatedUserResponse(
    string Username,
    string DisplayName,
    string Role,
    string RedirectPath);

internal sealed record IncidentResponse(
    Guid Id,
    string Title,
    string Detail,
    string Category,
    string Level,
    int UrgencyScore,
    string ClassificationReason,
    double Latitude,
    double Longitude,
    string District,
    string TimeLabel,
    string Status,
    string Source,
    string ReporterName,
    string LastUpdatedBy,
    string InternalNote,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

internal sealed record IncidentAnalysisResponse(
    string Category,
    string Level,
    int UrgencyScore,
    string Reason,
    bool ShouldCallEmergency,
    string Recommendation);

internal sealed record AuditLogResponse(
    Guid Id,
    string Action,
    string EntityType,
    string EntityId,
    string ActorUsername,
    string ActorDisplayName,
    string ActorRole,
    string Summary,
    string Detail,
    string IpAddress,
    DateTimeOffset CreatedAt);

internal sealed record IncidentAssessment(
    string Category,
    string Level,
    int UrgencyScore,
    string Reason,
    bool ShouldCallEmergency,
    string Recommendation);

internal sealed record IncidentProfile(
    string Label,
    string Category,
    int BaseScore,
    params string[] Keywords);

internal readonly record struct StaticAssetPaths(
    string IndexFile,
    string AdminFile,
    string UserFile,
    string PoliceFile,
    string SupportFile,
    string BoundaryFile);

internal readonly record struct DemoUser(
    string Username,
    string Password,
    string DisplayName,
    string Role);

internal readonly record struct ActorSnapshot(
    string Username,
    string DisplayName,
    string Role);

internal static class IncidentMappingExtensions
{
    public static IncidentResponse ToDto(this IncidentRecord incident) => new(
        incident.Id,
        incident.Title,
        incident.Detail,
        incident.Category,
        incident.Level,
        incident.UrgencyScore,
        incident.ClassificationReason,
        incident.Latitude,
        incident.Longitude,
        incident.District,
        incident.TimeLabel,
        incident.Status,
        incident.Source,
        incident.ReporterName,
        incident.LastUpdatedBy,
        incident.InternalNote,
        incident.CreatedAt,
        incident.UpdatedAt);

    public static IncidentAnalysisResponse ToResponse(this IncidentAssessment assessment) => new(
        assessment.Category,
        assessment.Level,
        assessment.UrgencyScore,
        assessment.Reason,
        assessment.ShouldCallEmergency,
        assessment.Recommendation);

    public static AuditLogResponse ToDto(this AuditLogRecord auditLog) => new(
        auditLog.Id,
        auditLog.Action,
        auditLog.EntityType,
        auditLog.EntityId,
        auditLog.ActorUsername,
        auditLog.ActorDisplayName,
        auditLog.ActorRole,
        auditLog.Summary,
        auditLog.Detail,
        auditLog.IpAddress,
        auditLog.CreatedAt);
}

internal static class EndpointConventionBuilderExtensions
{
    public static TBuilder ApplyIncidentPolicy<TBuilder>(this TBuilder builder, bool demoOpenAccess, string policy)
        where TBuilder : IEndpointConventionBuilder
    {
        if (!demoOpenAccess)
        {
            builder.RequireAuthorization(policy);
        }

        return builder;
    }
}

internal static class DemoUsers
{
    public static readonly IReadOnlyDictionary<string, DemoUser> All = new Dictionary<string, DemoUser>(StringComparer.OrdinalIgnoreCase)
    {
        ["admin"] = new("admin", "admin123", "Quan tri vien", AppRoles.Admin),
        ["user"] = new("user", "user123", "Nguoi dung", AppRoles.User),
        ["police"] = new("police", "police123", "Canh sat", AppRoles.Police),
        ["support"] = new("support", "support123", "Nhan vien ho tro", AppRoles.Support)
    };
}

internal static class AppRoles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Police = "Police";
    public const string Support = "Support";
}

internal static class Policies
{
    public const string AdminOnly = nameof(AdminOnly);
    public const string UserOnly = nameof(UserOnly);
    public const string PoliceOnly = nameof(PoliceOnly);
    public const string SupportOnly = nameof(SupportOnly);
    public const string CanSubmitIncident = nameof(CanSubmitIncident);
    public const string CanViewIncidents = nameof(CanViewIncidents);
    public const string CanTrackIncident = nameof(CanTrackIncident);
    public const string CanUpdateIncidents = nameof(CanUpdateIncidents);
    public const string CanAuditAndExport = nameof(CanAuditAndExport);
}

internal static class DatabaseProviders
{
    public const string InMemory = "inmemory";
    public const string SqlServer = "sqlserver";
    public const string Postgres = "postgres";
}

internal static class CorsPolicies
{
    public const string OpenRealtime = nameof(OpenRealtime);
}

internal static class IncidentStatuses
{
    public const string MoiTiepNhan = "Moi tiep nhan";
    public const string DaTiepNhan = "Da tiep nhan";
    public const string DangXacMinh = "Dang xac minh";
    public const string DaDieuPhoi = "Da dieu phoi";
    public const string DaXuLy = "Da xu ly";
}

internal static class AuditActions
{
    public const string LoginSuccess = "auth.login.success";
    public const string LoginFailed = "auth.login.failed";
    public const string Logout = "auth.logout";
    public const string AnalyzeIncident = "incident.analyze";
    public const string CreateIncident = "incident.create";
    public const string UpdateIncidentStatus = "incident.status.update";
    public const string UpdateIncidentDenied = "incident.status.denied";
    public const string ExportIncidents = "incident.export";
}

internal static class AuditEntities
{
    public const string Auth = "auth";
    public const string Incident = "incident";
}
