using System.Collections.Concurrent;
using System.Security.Claims;
using PoliceWebServer.Models;

namespace PoliceWebServer.Services;

public sealed class PolicePresenceService
{
    private readonly ConcurrentDictionary<string, PoliceLocationResponse> _activeLocations =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<PoliceLocationResponse> GetActiveLocations()
    {
        return _activeLocations.Values
            .OrderBy(item => item.DisplayName)
            .ToArray();
    }

    public (PoliceLocationResponse? Location, string? Error) UpdateLocation(
        ClaimsPrincipal? user,
        PoliceLocationRequest request)
    {
        var actor = GetActor(user);
        if (!string.Equals(actor.Role, "Police", StringComparison.OrdinalIgnoreCase))
        {
            return (null, "Chi tai khoan canh sat moi duoc chia se vi tri.");
        }

        if (request.Latitude is < -90 or > 90 || request.Longitude is < -180 or > 180)
        {
            return (null, "Toa do khong hop le.");
        }

        var status = string.IsNullOrWhiteSpace(request.Status)
            ? "Dang trong ca"
            : TrimTo(request.Status, 80);
        var shiftId = string.IsNullOrWhiteSpace(request.ShiftId)
            ? null
            : TrimTo(request.ShiftId, 120);

        var location = new PoliceLocationResponse(
            actor.Username,
            actor.DisplayName,
            actor.Role,
            request.Latitude,
            request.Longitude,
            ResolveDistrict(request.Latitude, request.Longitude),
            shiftId,
            status,
            DateTimeOffset.UtcNow);

        _activeLocations.AddOrUpdate(actor.Username, location, (_, _) => location);
        return (location, null);
    }

    public PoliceLocationResponse? RemoveLocation(ClaimsPrincipal? user)
    {
        var actor = GetActor(user);
        return _activeLocations.TryRemove(actor.Username, out var removed)
            ? removed
            : null;
    }

    private static (string Username, string DisplayName, string Role) GetActor(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return ("anonymous", "Canh sat", "Anonymous");
        }

        return (
            user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown",
            user.Identity?.Name ?? "Canh sat",
            user.FindFirstValue(ClaimTypes.Role) ?? "Unknown");
    }

    private static string TrimTo(string value, int maxLength)
    {
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static string ResolveDistrict(double latitude, double longitude)
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
}