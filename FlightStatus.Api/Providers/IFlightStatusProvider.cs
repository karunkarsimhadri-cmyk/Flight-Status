using FlightStatus.Api.Models;

namespace FlightStatus.Api.Providers;

public interface IFlightStatusProvider
{
    string Name { get; }
    Task<ProviderResponse?> GetStatusAsync(string flightNumber, DateOnly date);
}
