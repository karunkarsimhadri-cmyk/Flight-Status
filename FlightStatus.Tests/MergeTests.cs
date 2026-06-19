using FlightStatus.Api.Models;
using FlightStatus.Api.Providers;
using FlightStatus.Api.Services;
using Moq;

namespace FlightStatus.Tests;

public class MergeTests
{
    private static readonly DateOnly Date = new(2024, 6, 1);

    private static ProviderResponse Resp(string flight, string raw, DateTime updated, string provider = "Test")
        => new(flight, Date, raw, null, null, null, null, null, null, null, updated, provider);

    private static FlightStatusService Service(ProviderResponse? a, ProviderResponse? b)
    {
        var mockA = new Mock<IFlightStatusProvider>();
        mockA.Setup(p => p.GetStatusAsync(It.IsAny<string>(), It.IsAny<DateOnly>())).ReturnsAsync(a);

        var mockB = new Mock<IFlightStatusProvider>();
        mockB.Setup(p => p.GetStatusAsync(It.IsAny<string>(), It.IsAny<DateOnly>())).ReturnsAsync(b);

        return new FlightStatusService(new[] { mockA.Object, mockB.Object });
    }

    [Fact]
    public async Task BothProviders_UsesLaterLastUpdatedUtc()
    {
        var earlier = Resp("FL1", "CANCELLED", DateTime.UtcNow.AddHours(-2), "AeroTrack");
        var later   = Resp("FL1", "ON_TIME",   DateTime.UtcNow.AddHours(-1), "QuickFlight");

        var result = await Service(earlier, later).GetFlightStatusAsync("FL1", Date);

        Assert.Equal(Api.Models.FlightStatus.OnTime, result.Status);
        Assert.Equal("QuickFlight", result.Source);
    }

    [Fact]
    public async Task OnlyFirstProvider_UsesFirstProvider()
    {
        var resp = Resp("FL2", "CANCELLED", DateTime.UtcNow, "AeroTrack");

        var result = await Service(resp, null).GetFlightStatusAsync("FL2", Date);

        Assert.Equal(Api.Models.FlightStatus.Cancelled, result.Status);
        Assert.Equal("AeroTrack", result.Source);
    }

    [Fact]
    public async Task OnlySecondProvider_UsesSecondProvider()
    {
        var resp = Resp("FL3", "diverted", DateTime.UtcNow, "QuickFlight");

        var result = await Service(null, resp).GetFlightStatusAsync("FL3", Date);

        Assert.Equal(Api.Models.FlightStatus.Diverted, result.Status);
        Assert.Equal("QuickFlight", result.Source);
    }

    [Fact]
    public async Task NeitherProvider_ReturnsUnknownWithMessage()
    {
        var result = await Service(null, null).GetFlightStatusAsync("FL999", Date);

        Assert.Equal(Api.Models.FlightStatus.Unknown, result.Status);
        Assert.Equal("None", result.Source);
        Assert.NotEmpty(result.Message!);
    }

    [Fact]
    public async Task ConflictingStatuses_WinnerIsLaterUpdated()
    {
        // AeroTrack says on-time (updated first), QuickFlight says late (updated later) → Delayed
        var aero  = Resp("FL4", "ON_TIME", new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc), "AeroTrack");
        var quick = Resp("FL4", "late",    new DateTime(2024, 6, 1, 11, 0, 0, DateTimeKind.Utc), "QuickFlight");

        var result = await Service(aero, quick).GetFlightStatusAsync("FL4", Date);

        Assert.Equal(Api.Models.FlightStatus.Delayed, result.Status);
        Assert.Equal("QuickFlight", result.Source);
    }
}
