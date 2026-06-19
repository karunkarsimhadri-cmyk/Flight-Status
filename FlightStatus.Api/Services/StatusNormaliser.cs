using FlightStatus.Api.Models;
using Status = FlightStatus.Api.Models.FlightStatus;

namespace FlightStatus.Api.Services;

public static class StatusNormaliser
{
    private static readonly TimeSpan Threshold = TimeSpan.FromMinutes(15);

    public static Status Normalise(ProviderResponse response)
    {
        var key = response.RawStatus
            .Trim()
            .ToUpperInvariant()
            .Replace("-", "")
            .Replace("_", "");

        return key switch
        {
            "CANCELLED" or "CANCELED"   => Status.Cancelled,
            "DIVERTED"  or "REROUTED"   => Status.Diverted,
            "DELAYED"   or "LATE"       => Status.Delayed,
            "ONTIME"    or "SCHEDULED"  => DeriveFromTimes(response) ?? Status.OnTime,
            _                           => DeriveFromTimes(response) ?? Status.Unknown,
        };
    }

    // When actual times are present, the 15-minute rule overrides the raw status string.
    private static Status? DeriveFromTimes(ProviderResponse r)
    {
        var maxDelay = MaxDelay(r);
        if (maxDelay is null) return null;
        return maxDelay.Value <= Threshold ? Status.OnTime : Status.Delayed;
    }

    private static TimeSpan? MaxDelay(ProviderResponse r)
    {
        TimeSpan? dep = (r.ActualDeparture.HasValue && r.ScheduledDeparture.HasValue)
            ? (r.ActualDeparture.Value - r.ScheduledDeparture.Value).Duration()
            : null;

        TimeSpan? arr = (r.ActualArrival.HasValue && r.ScheduledArrival.HasValue)
            ? (r.ActualArrival.Value - r.ScheduledArrival.Value).Duration()
            : null;

        if (dep is null && arr is null) return null;
        if (dep is null) return arr;
        if (arr is null) return dep;
        return dep > arr ? dep : arr;
    }
}
