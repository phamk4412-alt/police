using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using PoliceWebServer.Models;
using PoliceWebServer.Services;

namespace PoliceWebServer.Hubs;

public sealed class IncidentHub(PolicePresenceService policePresenceService) : Hub
{
    public IReadOnlyCollection<PoliceLocationResponse> GetPoliceLocations()
    {
        return policePresenceService.GetActiveLocations();
    }

    public async Task UpdatePoliceLocation(PoliceLocationRequest request)
    {
        var (location, error) = policePresenceService.UpdateLocation(Context.User ?? new ClaimsPrincipal(), request);
        if (location is null)
        {
            throw new HubException(error ?? "Khong the cap nhat vi tri.");
        }

        await Clients.All.SendAsync("PoliceLocationUpdated", location);
        await Clients.All.SendAsync("PoliceLocationsSnapshot", policePresenceService.GetActiveLocations());
    }

    public async Task EndPoliceShift()
    {
        var removed = policePresenceService.RemoveLocation(Context.User ?? new ClaimsPrincipal());
        if (removed is not null)
        {
            await Clients.All.SendAsync("PoliceLocationRemoved", removed);
            await Clients.All.SendAsync("PoliceLocationsSnapshot", policePresenceService.GetActiveLocations());
        }
    }
}