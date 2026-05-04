namespace PoliceWebServer.Models;

public sealed record PoliceLocationRequest(
    double Latitude,
    double Longitude,
    string? ShiftId,
    string? Status);

public sealed record PoliceLocationResponse(
    string Username,
    string DisplayName,
    string Role,
    double Latitude,
    double Longitude,
    string District,
    string? ShiftId,
    string Status,
    DateTimeOffset UpdatedAt);