using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PoliceWebServer.Data;
using PoliceWebServer.Hubs;
using PoliceWebServer.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
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
        case "sqlserver":
            if (string.IsNullOrWhiteSpace(sqlServerConnection))
            {
                throw new InvalidOperationException("ConnectionStrings:SqlServer chua duoc cau hinh.");
            }

            options.UseSqlServer(sqlServerConnection);
            break;

        case "postgres":
            if (string.IsNullOrWhiteSpace(postgresConnection))
            {
                throw new InvalidOperationException("ConnectionStrings:Postgres chua duoc cau hinh.");
            }

            options.UseNpgsql(postgresConnection);
            break;

        default:
            throw new InvalidOperationException("DatabaseProvider phai la 'sqlserver' hoac 'postgres'.");
    }
});

var app = builder.Build();

var websiteRoot = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, ".."));
var indexFile = Path.Combine(websiteRoot, "index.html");

if (!File.Exists(indexFile))
{
    throw new FileNotFoundException("Khong tim thay index.html cho website.", indexFile);
}

await EnsureDatabaseReadyAsync(app.Services);

var websiteFiles = new PhysicalFileProvider(websiteRoot);

app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = websiteFiles
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = websiteFiles
});
app.UseCors();

app.MapGet("/api/health", (IConfiguration configuration) => Results.Ok(new
{
    status = "ok",
    websiteRoot,
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
});

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
});

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
});

app.MapHub<IncidentHub>("/hubs/incidents");

app.MapFallback(async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(indexFile);
});

app.Run();

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
        return "postgres";
    }

    if (!string.IsNullOrWhiteSpace(ResolveConnectionString(configuration, "SqlServer")))
    {
        return "sqlserver";
    }

    return "postgres";
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

internal sealed record CreateIncidentRequest(string Title, string Location, string? Detail, string? Level);
internal sealed record UpdateIncidentStatusRequest(string Status);

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
