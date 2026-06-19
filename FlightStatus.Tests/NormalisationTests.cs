using FlightStatus.Api.Models;
using FlightStatus.Api.Services;

namespace FlightStatus.Tests;

public class NormalisationTests
{
    private static ProviderResponse Make(string rawStatus,
        DateTime? schedDep = null, DateTime? actualDep = null,
        DateTime? schedArr = null, DateTime? actualArr = null)
        => new("FL001", DateOnly.FromDateTime(DateTime.Today), rawStatus,
            schedDep, actualDep, schedArr, actualArr,
            null, null, null, DateTime.UtcNow, "Test");

    [Theory]
    [InlineData("CANCELLED",  Api.Models.FlightStatus.Cancelled)]
    [InlineData("CANCELED",   Api.Models.FlightStatus.Cancelled)]
    [InlineData("DIVERTED",   Api.Models.FlightStatus.Diverted)]
    [InlineData("REROUTED",   Api.Models.FlightStatus.Diverted)]
    [InlineData("DELAYED",    Api.Models.FlightStatus.Delayed)]
    [InlineData("LATE",       Api.Models.FlightStatus.Delayed)]
    [InlineData("ON_TIME",    Api.Models.FlightStatus.OnTime)]
    [InlineData("on-time",    Api.Models.FlightStatus.OnTime)]
    [InlineData("SCHEDULED",  Api.Models.FlightStatus.OnTime)]
    [InlineData("GIBBERISH",  Api.Models.FlightStatus.Unknown)]
    public void Normalise_RawStatusString_MapsToExpected(string raw, Api.Models.FlightStatus expected)
        => Assert.Equal(expected, StatusNormaliser.Normalise(Make(raw)));

    [Fact]
    public void Normalise_DepWithin15Min_ReturnsOnTime()
    {
        var sched = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        var actual = sched.AddMinutes(14);
        Assert.Equal(Api.Models.FlightStatus.OnTime,
            StatusNormaliser.Normalise(Make("SCHEDULED", schedDep: sched, actualDep: actual)));
    }

    [Fact]
    public void Normalise_DepExactly15Min_ReturnsOnTime()
    {
        var sched = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        Assert.Equal(Api.Models.FlightStatus.OnTime,
            StatusNormaliser.Normalise(Make("SCHEDULED",
                schedDep: sched, actualDep: sched.AddMinutes(15))));
    }

    [Fact]
    public void Normalise_DepBeyond15Min_ReturnsDelayed()
    {
        var sched = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        Assert.Equal(Api.Models.FlightStatus.Delayed,
            StatusNormaliser.Normalise(Make("SCHEDULED",
                schedDep: sched, actualDep: sched.AddMinutes(16))));
    }

    [Fact]
    public void Normalise_ArrBeyond15Min_ReturnsDelayed_EvenIfDepOnTime()
    {
        var depSched  = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        var arrSched  = new DateTime(2024, 6, 1, 11, 0, 0, DateTimeKind.Utc);
        Assert.Equal(Api.Models.FlightStatus.Delayed,
            StatusNormaliser.Normalise(Make("SCHEDULED",
                schedDep: depSched, actualDep: depSched.AddMinutes(5),   // dep on time
                schedArr: arrSched, actualArr: arrSched.AddMinutes(20)))); // arr late
    }

    [Fact]
    public void Normalise_CancelledOverridesActualTimes()
    {
        // Even if actual times look fine, CANCELLED means Cancelled
        var sched = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        Assert.Equal(Api.Models.FlightStatus.Cancelled,
            StatusNormaliser.Normalise(Make("CANCELLED",
                schedDep: sched, actualDep: sched.AddMinutes(2))));
    }
}
