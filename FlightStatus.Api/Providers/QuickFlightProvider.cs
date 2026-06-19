using FlightStatus.Api.Models;

namespace FlightStatus.Api.Providers;

// Stub: minimal detail — status and scheduled times only
public class QuickFlightProvider : IFlightStatusProvider
{
    public string Name => "QuickFlight";

    private static readonly Dictionary<string, Func<DateOnly, ProviderResponse>> Scenarios = new()
    {
        // AA100: on-time, updated 10:00 UTC — earlier than AeroTrack → AeroTrack wins
        ["AA100"] = date => new ProviderResponse(
            "AA100", date, "on-time",
            date.ToDateTime(new TimeOnly(8, 0), DateTimeKind.Utc),
            null,
            date.ToDateTime(new TimeOnly(11, 0), DateTimeKind.Utc),
            null,
            null, null, null,
            date.ToDateTime(new TimeOnly(10, 0), DateTimeKind.Utc), "QuickFlight"),

        // AA200: late, updated 11:00 UTC — later than AeroTrack → QuickFlight wins → Delayed
        ["AA200"] = date => new ProviderResponse(
            "AA200", date, "late",
            date.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc),
            null,
            date.ToDateTime(new TimeOnly(12, 30), DateTimeKind.Utc),
            null,
            null, null, null,
            date.ToDateTime(new TimeOnly(11, 0), DateTimeKind.Utc), "QuickFlight"),

        // AA300: no data — only AeroTrack has this flight

        // AA400: diverted — only QuickFlight has data for this flight
        ["AA400"] = date => new ProviderResponse(
            "AA400", date, "diverted",
            date.ToDateTime(new TimeOnly(10, 0), DateTimeKind.Utc),
            null,
            date.ToDateTime(new TimeOnly(13, 0), DateTimeKind.Utc),
            null,
            null, null, null,
            date.ToDateTime(new TimeOnly(10, 0), DateTimeKind.Utc), "QuickFlight"),

        // AA500: no data — neither provider knows this flight

        // AA600: late, updated 11:00 UTC — later than AeroTrack → QuickFlight wins → Delayed
        ["AA600"] = date => new ProviderResponse(
            "AA600", date, "late",
            date.ToDateTime(new TimeOnly(16, 0), DateTimeKind.Utc),
            null,
            date.ToDateTime(new TimeOnly(19, 0), DateTimeKind.Utc),
            null,
            null, null, null,
            date.ToDateTime(new TimeOnly(11, 0), DateTimeKind.Utc), "QuickFlight"),

        // BA100: on-time, updated 09:00 UTC — earlier than AeroTrack → AeroTrack wins → Diverted
        ["BA100"] = date => new ProviderResponse(
            "BA100", date, "on-time",
            date.ToDateTime(new TimeOnly(6, 0), DateTimeKind.Utc),
            null,
            date.ToDateTime(new TimeOnly(9, 30), DateTimeKind.Utc),
            null,
            null, null, null,
            date.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc), "QuickFlight"),
    };

    public Task<ProviderResponse?> GetStatusAsync(string flightNumber, DateOnly date)
    {
        var result = Scenarios.TryGetValue(flightNumber.ToUpperInvariant(), out var factory)
            ? factory(date)
            : null;
        return Task.FromResult<ProviderResponse?>(result);
    }
}
