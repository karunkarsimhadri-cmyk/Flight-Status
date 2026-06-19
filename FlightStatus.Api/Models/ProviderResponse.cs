namespace FlightStatus.Api.Models;

public record ProviderResponse(
    string FlightNumber,
    DateOnly Date,
    string RawStatus,
    DateTime? ScheduledDeparture,
    DateTime? ActualDeparture,
    DateTime? ScheduledArrival,
    DateTime? ActualArrival,
    string? Terminal,
    string? Gate,
    string? DelayReason,
    DateTime LastUpdatedUtc,
    string ProviderName
);
