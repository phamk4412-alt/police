using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

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
    "/api/health"
}));

app.MapFallback(async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(indexFile);
});

app.Run();
