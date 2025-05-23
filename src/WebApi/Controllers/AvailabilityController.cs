using EnvironmentsService.Src.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentsService.src.WebApi.Controllers;

[ApiController]
[Route("environments/{envId}/non-available-slots")]
public class AvailabilityController(IAvailabilityService service) : ControllerBase
{
    private readonly IAvailabilityService _service = service;

    [HttpGet]
    [HttpGet("unavailable")]
    public async Task<IActionResult> GetUnavailableSlots(Guid envId, [FromQuery] long start, [FromQuery] long end)
    {
        var result = await _service.GetUnavailableTimeSlotsAsync(envId, start, end);
        return Ok(result);
    }
}
