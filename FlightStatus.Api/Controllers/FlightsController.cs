using FlightStatus.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlightStatus.Api.Controllers;

[ApiController]
[Route("flights")]
public class FlightsController : ControllerBase
{
    private readonly FlightStatusService _service;

    public FlightsController(FlightStatusService service)
    {
        _service = service;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(
        [FromQuery] string? flightNumber,
        [FromQuery] string? date)
    {
        if (string.IsNullOrWhiteSpace(flightNumber) || string.IsNullOrWhiteSpace(date))
            return BadRequest(new { error = "flightNumber and date are required." });

        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", null,
                System.Globalization.DateTimeStyles.None, out var parsedDate))
            return BadRequest(new { error = "date must be in yyyy-MM-dd format." });

        var result = await _service.GetFlightStatusAsync(flightNumber.ToUpperInvariant(), parsedDate);
        return Ok(result);
    }
}
