using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.src.Application.DTOs.Patch;
using EnvironmentsService.Src.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentsService.Src.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController(IReservationService service) : ControllerBase
{
    private readonly IReservationService _service = service;

    [HttpPost]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequest request)
    {
        var publicIdClaim = Request.Cookies["publicId"];

        if (publicIdClaim == null || !Guid.TryParse(publicIdClaim, out var userPublicId))
        {
            return Unauthorized("Invalid or missing user public_id: " + publicIdClaim);
        }

        try
        {
            var response = await _service.CreateAsync(request, userPublicId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetUserReservations(
    [FromQuery] string? status,
    [FromQuery] int page = 1,
    [FromQuery] int limit = 10)
    {
        var publicIdClaim = Request.Cookies["publicId"];

        if (publicIdClaim == null || !Guid.TryParse(publicIdClaim, out var userPublicId))
        {
            return Unauthorized("Invalid or missing user public_id");
        }

        var result = await _service.GetByUserAsync(userPublicId, status, page, limit);
        return Ok(result);
    }

    [HttpGet("{publicId:guid}")]
    public async Task<IActionResult> GetReservationByPublicId(Guid publicId)
    {
        try
        {
            var reservation = await _service.GetByPublicIdAsync(publicId);
            if (reservation == null)
            {
                return NotFound("Reserva no encontrada");
            }

            return Ok(reservation);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPatch("{publicId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid publicId, [FromBody] UpdateReservationStatusDto request)
    {
        var publicIdClaim = Request.Cookies["publicId"];

        if (publicIdClaim == null || !Guid.TryParse(publicIdClaim, out var userPublicId))
        {
            return Unauthorized("Invalid or missing user public_id");
        }

        try
        {
            var updated = await _service.UpdateStatusAsync(publicId, userPublicId, request.Status);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("conflicts")]
    public async Task<IActionResult> GetConflictingReservations(
    [FromQuery] Guid environmentId,
    [FromQuery] long start,
    [FromQuery] long end)
    {
        var conflicts = await _service.GetConflictingReservationsAsync(environmentId, start, end);
        return Ok(conflicts);
    }

    [HttpGet("day")]
    public async Task<IActionResult> GetReservationsByDay([FromQuery] long timestamp)
    {
        var publicIdClaim = Request.Cookies["publicId"];
        if (publicIdClaim == null || !Guid.TryParse(publicIdClaim, out var userPublicId))
        {
            return Unauthorized("Invalid or missing user public_id");
        }

        try
        {
            var reservations = await _service.GetByOwnerAndDayAsync(userPublicId, timestamp);
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
