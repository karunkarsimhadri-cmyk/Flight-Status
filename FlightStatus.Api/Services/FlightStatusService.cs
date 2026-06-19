using FlightStatus.Api.Models;
using FlightStatus.Api.Providers;
using Status = FlightStatus.Api.Models.FlightStatus;

namespace FlightStatus.Api.Services;

public class FlightStatusService
{
    private readonly IEnumerable<IFlightStatusProvider> _providers;

    public FlightStatusService(IEnumerable<IFlightStatusProvider> providers)
        => _providers = providers;

    public async Task<FlightStatusResult> GetFlightStatusAsync(string flightNumber, DateOnly date)
    {
        var tasks = _providers.Select(p => p.GetStatusAsync(flightNumber, date));
        var responses = await Task.WhenAll(tasks);
        var valid = responses.Where(r => r is not null).ToList();

        if (valid.Count == 0)
        {
            return new FlightStatusResult(
                FlightNumber: flightNumber,
                Date: date.ToString("yyyy-MM-dd"),
                Status: Status.Unknown,
                ScheduledDeparture: null,
                ActualDeparture: null,
                ScheduledArrival: null,
                ActualArrival: null,
                Terminal: null,
                Gate: null,
                DelayReason: null,
                Message: "No data available from any provider.",
                LastUpdatedUtc: DateTime.UtcNow,
                Source: "None"
            );
        }

        var best = valid.OrderByDescending(r => r!.LastUpdatedUtc).First()!;
        var status = StatusNormaliser.Normalise(best);

        return new FlightStatusResult(
            FlightNumber: best.FlightNumber,
            Date: best.Date.ToString("yyyy-MM-dd"),
            Status: status,
            ScheduledDeparture: best.ScheduledDeparture,
            ActualDeparture: best.ActualDeparture,
            ScheduledArrival: best.ScheduledArrival,
            ActualArrival: best.ActualArrival,
            Terminal: best.Terminal,
            Gate: best.Gate,
            DelayReason: best.DelayReason,
            Message: null,
            LastUpdatedUtc: best.LastUpdatedUtc,
            Source: best.ProviderName
        );
    }
}
