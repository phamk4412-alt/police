using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();

var websiteRoot = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, ".."));
var indexFile = Path.Combine(websiteRoot, "index.html");

if (!File.Exists(indexFile))
{
    throw new FileNotFoundException("Khong tim thay index.html cho website.", indexFile);
}

var websiteFiles = new PhysicalFileProvider(websiteRoot);

app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = websiteFiles
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = websiteFiles
});

var incidents = new ConcurrentDictionary<Guid, LiveIncident>();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    websiteRoot,
    timestamp = DateTimeOffset.UtcNow
}));

app.MapGet("/api/routes", () => Results.Ok(new[]
{
    "/",
    "/index.html",
    "/admin/admin.html",
    "/user/user.html",
    "/police/police.html",
    "/support/support.html",
    "/hcm-boundary.geojson",
    "/api/health",
    "/api/incidents"
}));

app.MapGet("/api/incidents", () =>
{
    var payload = incidents.Values
        .OrderByDescending(item => item.CreatedAt)
        .Select(item => new
        {
            item.Id,
            item.Title,
            item.Detail,
            item.Level,
            item.Latitude,
            item.Longitude,
            item.TimeLabel,
            item.Status,
            item.Source,
            CreatedAt = item.CreatedAt
        });

    return Results.Ok(payload);
});

app.MapPost("/api/incidents", async (HttpContext context) =>
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

    var incident = new LiveIncident(
        Guid.NewGuid(),
        request.Title.Trim(),
        string.IsNullOrWhiteSpace(request.Detail) ? "Nguoi dung vua gui bao cao moi." : request.Detail.Trim(),
        NormalizeLevel(request.Level),
        latitude,
        longitude,
        DateTimeOffset.Now.ToString("HH:mm"),
        "Mới tiếp nhận",
        "user",
        DateTimeOffset.UtcNow);

    incidents[incident.Id] = incident;

    return Results.Ok(new
    {
        message = "Da gui bao cao thanh cong.",
        incident.Id
    });
});

app.MapPatch("/api/incidents/{id:guid}/status", async (Guid id, HttpContext context) =>
{
    var request = await context.Request.ReadFromJsonAsync<UpdateIncidentStatusRequest>();
    if (request is null || string.IsNullOrWhiteSpace(request.Status))
    {
        return Results.BadRequest(new { message = "Can co trang thai moi." });
    }

    if (!incidents.TryGetValue(id, out var current))
    {
        return Results.NotFound(new { message = "Khong tim thay vu viec." });
    }

    incidents[id] = current with { Status = request.Status.Trim() };
    return Results.Ok(new { message = "Da cap nhat trang thai." });
});

app.MapFallback(async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(indexFile);
});

app.Run();

static bool TryParseLocation(string raw, out double latitude, out double longitude)
{
    latitude = 0;
    longitude = 0;

    var parts = raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length != 2)
    {
        return false;
    }

    if (!double.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out latitude) ||
        !double.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out longitude))
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

internal sealed record CreateIncidentRequest(string Title, string Location, string? Detail, string? Level);

internal sealed record UpdateIncidentStatusRequest(string Status);

internal sealed record LiveIncident(
    Guid Id,
    string Title,
    string Detail,
    string Level,
    double Latitude,
    double Longitude,
    string TimeLabel,
    string Status,
    string Source,
    DateTimeOffset CreatedAt);
