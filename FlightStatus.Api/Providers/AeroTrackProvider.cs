using FlightStatus.Api.Models;

namespace FlightStatus.Api.Providers;

// Stub: full detail — status, scheduled & actual times, terminal, gate, delay reason
public class AeroTrackProvider : IFlightStatusProvider
{
    public string Name => "AeroTrack";

    private static readonly Dictionary<string, Func<DateOnly, ProviderResponse>> Scenarios = new()
    {
        // AA100: ON_TIME, updated 11:00 UTC — later than QuickFlight → AeroTrack wins → OnTime
        ["AA100"] = date => new ProviderResponse(
            "AA100", date, "ON_TIME",
            date.ToDateTime(new TimeOnly(8, 0), DateTimeKind.Utc),
            date.ToDateTime(new TimeOnly(7, 58), DateTimeKind.Utc),   // 2 min early
            date.ToDateTime(new TimeOnly(11, 0), DateTimeKind.Utc),
            date.ToDateTime(new TimeOnly(11, 4), DateTimeKind.Utc),
            "B", "B14", null,
            date.ToDateTime(new TimeOnly(11, 0), DateTimeKind.Utc), "AeroTrack"),

        // AA200: DELAYED 50 min, updated 09:00 UTC — earlier than QuickFlight → QuickFlight wins
        ["AA200"] = date => new ProviderResponse(
            "AA200", date, "DELAYED",
            date.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc),
            date.ToDateTime(new TimeOnly(9, 50), DateTimeKind.Utc),   // 50 min late
            date.ToDateTime(new TimeOnly(12, 30), DateTimeKind.Utc),
            date.ToDateTime(new TimeOnly(13, 20), DateTimeKind.Utc),
            "A", "A7", "Air traffic control hold",
            date.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc), "AeroTrack"),

        // AA300: CANCELLED — only AeroTrack has data
        ["AA300"] = date => new ProviderResponse(
            "AA300", date, "CANCELLED",
            date.ToDateTime(new TimeOnly(14, 0), DateTimeKind.Utc),
            null,
            date.ToDateTime(new TimeOnly(17, 30), DateTimeKind.Utc),
            null,
            "C", null, "Mechanical issue",
            date.ToDateTime(new TimeOnly(10, 0), DateTimeKind.Utc), "AeroTrack"),

        // AA400: no data — only QuickFlight has data for this flight
        // AA500: no data — neither provider has data

        // AA600: ON_TIME, updated 10:00 UTC — earlier than QuickFlight → QuickFlight wins → Delayed
        ["AA600"] = date => new ProviderResponse(
            "AA600", date, "ON_TIME",
            date.ToDateTime(new TimeOnly(16, 0), DateTimeKind.Utc),
            date.ToDateTime(new TimeOnly(16, 5), DateTimeKind.Utc),
            date.ToDateTime(new TimeOnly(19, 0), DateTimeKind.Utc),
            date.ToDateTime(new TimeOnly(19, 8), DateTimeKind.Utc),
            "D", "D22", null,
            date.ToDateTime(new TimeOnly(10, 0), DateTimeKind.Utc), "AeroTrack"),

        // BA100: DIVERTED, updated 10:00 UTC — later than QuickFlight → AeroTrack wins → Diverted
        ["BA100"] = date => new ProviderResponse(
            "BA100", date, "DIVERTED",
            date.ToDateTime(new TimeOnly(6, 0), DateTimeKind.Utc),
            date.ToDateTime(new TimeOnly(6, 5), DateTimeKind.Utc),
            date.ToDateTime(new TimeOnly(9, 30), DateTimeKind.Utc),
            null,
            "A", "A1", "Weather diversion",
            date.ToDateTime(new TimeOnly(10, 0), DateTimeKind.Utc), "AeroTrack"),
    };

    public Task<ProviderResponse?> GetStatusAsync(string flightNumber, DateOnly date)
    {
        var result = Scenarios.TryGetValue(flightNumber.ToUpperInvariant(), out var factory)
            ? factory(date)
            : null;
        return Task.FromResult<ProviderResponse?>(result);
    }
}
