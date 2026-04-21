namespace PoliceWebServer.Models;

public sealed class IncidentRecord
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Category { get; set; } = "Chua xac dinh";
    public string Level { get; set; } = "high";
    public int UrgencyScore { get; set; }
    public string ClassificationReason { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string District { get; set; } = string.Empty;
    public string TimeLabel { get; set; } = string.Empty;
    public string Status { get; set; } = "Moi tiep nhan";
    public string Source { get; set; } = "user";
    public string ReporterName { get; set; } = string.Empty;
    public string LastUpdatedBy { get; set; } = string.Empty;
    public string InternalNote { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
