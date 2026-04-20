using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
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
var dataDirectory = Path.Combine(app.Environment.ContentRootPath, "data");
var dataFile = Path.Combine(dataDirectory, "incidents.json");
var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true
};

if (!File.Exists(indexFile))
{
    throw new FileNotFoundException("Khong tim thay index.html cho website.", indexFile);
}

Directory.CreateDirectory(dataDirectory);

var websiteFiles = new PhysicalFileProvider(websiteRoot);
var incidents = new ConcurrentDictionary<Guid, LiveIncident>();
var fileLock = new SemaphoreSlim(1, 1);

foreach (var incident in await LoadIncidentsAsync(dataFile))
{
    incidents[incident.Id] = incident;
}

app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = websiteFiles
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = websiteFiles
});

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    websiteRoot,
    dataFile,
    timestamp = DateTimeOffset.UtcNow
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
            item.CreatedAt,
            item.UpdatedAt
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

    var now = DateTimeOffset.UtcNow;
    var incident = new LiveIncident(
        Guid.NewGuid(),
        request.Title.Trim(),
        string.IsNullOrWhiteSpace(request.Detail) ? "Nguoi dung vua gui bao cao moi." : request.Detail.Trim(),
        NormalizeLevel(request.Level),
        latitude,
        longitude,
        DateTimeOffset.Now.ToString("HH:mm"),
        "Moi tiep nhan",
        "user",
        now,
        now);

    incidents[incident.Id] = incident;
    await SaveIncidentsAsync();

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

    var updated = current with
    {
        Status = NormalizeStatus(request.Status),
        UpdatedAt = DateTimeOffset.UtcNow
    };

    incidents[id] = updated;
    await SaveIncidentsAsync();

    return Results.Ok(new
    {
        message = "Da cap nhat trang thai.",
        updated.Status
    });
});

app.MapFallback(async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(indexFile);
});

app.Run();

async Task SaveIncidentsAsync()
{
    await fileLock.WaitAsync();
    try
    {
        var snapshot = incidents.Values
            .OrderByDescending(item => item.CreatedAt)
            .ToArray();

        await using var stream = File.Create(dataFile);
        await JsonSerializer.SerializeAsync(stream, snapshot, jsonOptions);
    }
    finally
    {
        fileLock.Release();
    }
}

static async Task<IReadOnlyList<LiveIncident>> LoadIncidentsAsync(string path)
{
    if (!File.Exists(path))
    {
        return [];
    }

    await using var stream = File.OpenRead(path);
    var items = await JsonSerializer.DeserializeAsync<List<LiveIncident>>(stream);
    return items ?? [];
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
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
