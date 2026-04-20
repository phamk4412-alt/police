namespace PoliceWebServer.Models;

public sealed class IncidentRecord
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Level { get; set; } = "high";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string TimeLabel { get; set; } = string.Empty;
    public string Status { get; set; } = "Moi tiep nhan";
    public string Source { get; set; } = "user";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
