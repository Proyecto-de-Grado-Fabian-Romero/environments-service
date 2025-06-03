using EnvironmentsService.src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentsService.src.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvailabilityController(IAvailabilityService service) : ControllerBase
{
    private readonly IAvailabilityService _service = service;

    [HttpGet("unavailable")]
    public async Task<IActionResult> GetUnavailableSlots(Guid envId, [FromQuery] long start, [FromQuery] long end)
    {
        var result = await _service.GetUnavailableTimeSlotsAsync(envId, start, end);
        return Ok(result);
    }

    [HttpPost("block")]
    public async Task<IActionResult> BlockDate([FromBody] BlockAvailabilityRequest request)
    {
        var publicIdClaim = Request.Cookies["publicId"];

        if (publicIdClaim == null || !Guid.TryParse(publicIdClaim, out var ownerId))
        {
            return Unauthorized("Invalid or missing user public_id");
        }

        try
        {
            await _service.BlockDateAsync(request, ownerId);
            return Ok(new { message = "Fecha bloqueada exitosamente" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
