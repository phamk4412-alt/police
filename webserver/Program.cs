using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PoliceWebServer.Data;
using PoliceWebServer.Hubs;
using PoliceWebServer.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "PoliceSmartHub.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
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
    options.AddPolicy(Policies.CanUpdateIncidents, policy => policy.RequireRole(AppRoles.Admin, AppRoles.Police, AppRoles.Support));
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

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/login", async (LoginRequest request, HttpContext context) =>
{
    if (!TryGetDemoUser(request.Username, request.Password, out var user))
    {
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

    return Results.Ok(new AuthenticatedUserResponse(
        user.Username,
        user.DisplayName,
        user.Role,
        GetLandingPathForRole(user.Role)));
});

app.MapPost("/api/auth/logout", async (HttpContext context) =>
{
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

app.MapGet("/api/incidents", async (IncidentDbContext dbContext) =>
{
    var incidents = await dbContext.Incidents
        .OrderByDescending(item => item.CreatedAt)
        .Select(item => item.ToDto())
        .ToListAsync();

    return Results.Ok(incidents);
}).RequireAuthorization(Policies.CanViewIncidents);

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

    var now = DateTimeOffset.UtcNow;
    var incident = new IncidentRecord
    {
        Id = Guid.NewGuid(),
        Title = request.Title.Trim(),
        Detail = string.IsNullOrWhiteSpace(request.Detail) ? "Nguoi dung vua gui bao cao moi." : request.Detail.Trim(),
        Level = NormalizeLevel(request.Level),
        Latitude = latitude,
        Longitude = longitude,
        TimeLabel = DateTimeOffset.Now.ToString("HH:mm"),
        Status = "Moi tiep nhan",
        Source = "user",
        CreatedAt = now,
        UpdatedAt = now
    };

    dbContext.Incidents.Add(incident);
    await dbContext.SaveChangesAsync();

    var payload = incident.ToDto();
    await hubContext.Clients.All.SendAsync("IncidentCreated", payload);

    return Results.Ok(new
    {
        message = "Da gui bao cao thanh cong.",
        incident = payload
    });
}).RequireAuthorization(Policies.CanSubmitIncident);

app.MapPatch("/api/incidents/{id:guid}/status", async (
    Guid id,
    HttpContext context,
    IncidentDbContext dbContext,
    IHubContext<IncidentHub> hubContext) =>
{
    var request = await context.Request.ReadFromJsonAsync<UpdateIncidentStatusRequest>();
    if (request is null || string.IsNullOrWhiteSpace(request.Status))
    {
        return Results.BadRequest(new { message = "Can co trang thai moi." });
    }

    var incident = await dbContext.Incidents.FirstOrDefaultAsync(item => item.Id == id);
    if (incident is null)
    {
        return Results.NotFound(new { message = "Khong tim thay vu viec." });
    }

    incident.Status = NormalizeStatus(request.Status);
    incident.UpdatedAt = DateTimeOffset.UtcNow;

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

app.MapHub<IncidentHub>("/hubs/incidents").RequireAuthorization(Policies.CanViewIncidents);

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

static string NormalizeStatus(string status)
{
    return status.Trim().ToLowerInvariant() switch
    {
        "moi tiep nhan" => "Moi tiep nhan",
        "da tiep nhan" => "Da tiep nhan",
        "da xu ly" => "Da xu ly",
        _ => status.Trim()
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

internal sealed record CreateIncidentRequest(string Title, string Location, string? Detail, string? Level);
internal sealed record UpdateIncidentStatusRequest(string Status);
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
    string Level,
    double Latitude,
    double Longitude,
    string TimeLabel,
    string Status,
    string Source,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

internal static class IncidentMappingExtensions
{
    public static IncidentResponse ToDto(this IncidentRecord incident) => new(
        incident.Id,
        incident.Title,
        incident.Detail,
        incident.Level,
        incident.Latitude,
        incident.Longitude,
        incident.TimeLabel,
        incident.Status,
        incident.Source,
        incident.CreatedAt,
        incident.UpdatedAt);
}

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
    public const string CanUpdateIncidents = nameof(CanUpdateIncidents);
}

internal static class DatabaseProviders
{
    public const string InMemory = "inmemory";
    public const string SqlServer = "sqlserver";
    public const string Postgres = "postgres";
}
