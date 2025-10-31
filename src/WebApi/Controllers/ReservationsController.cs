using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.src.Application.DTOs.Patch;
using EnvironmentsService.Src.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentsService.Src.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController(IReservationService service, IPaymentService paymentService)
    : ControllerBase
{
    private readonly IReservationService _service = service;
    private readonly IPaymentService _paymentService = paymentService;

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
        [FromQuery] string? type,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10
    )
    {
        var publicIdClaim = Request.Cookies["publicId"];

        if (publicIdClaim == null || !Guid.TryParse(publicIdClaim, out var userPublicId))
        {
            return Unauthorized("Invalid or missing user public_id");
        }

        var result = await _service.GetByUserAsync(userPublicId, status, type, page, limit);
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
    public async Task<IActionResult> UpdateStatus(
        Guid publicId,
        [FromBody] UpdateReservationStatusDto request
    )
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
            Console.WriteLine(ex);
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("conflicts")]
    public async Task<IActionResult> GetConflictingReservations(
        [FromQuery] Guid environmentId,
        [FromQuery] long start,
        [FromQuery] long end
    )
    {
        var conflicts = await _service.GetConflictingReservationsAsync(environmentId, start, end);
        return Ok(conflicts);
    }

    [HttpGet("mine/by-day")]
    public async Task<IActionResult> GetUserReservationsByDay(
        [FromQuery] long scheduledDayTimestamp,
        [FromQuery] string? status,
        [FromQuery] string? type,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10
    )
    {
        var publicIdClaim = Request.Cookies["publicId"];

        if (publicIdClaim == null || !Guid.TryParse(publicIdClaim, out var userPublicId))
        {
            return Unauthorized("Invalid or missing user public_id");
        }

        var result = await _service.GetByUserAndDayAsync(
            userPublicId,
            scheduledDayTimestamp,
            status,
            type,
            page,
            limit
        );
        return Ok(result);
    }

    [HttpPost("pay")]
    public async Task<IActionResult> IniciarPago(
        [FromBody] CreatePaymentWithClientDto dto,
        [FromQuery] string fechaVencimiento
    )
    {
        var url = await _paymentService.CreatePayment(dto, fechaVencimiento);
        return Ok(url);
    }

    [HttpGet("payments/status/{reservationId}")]
    public async Task<IActionResult> CheckStatus(Guid reservationId)
    {
        var result = await _paymentService.CheckAndUpdatePaymentAsync(reservationId);
        return Ok(result);
    }
}
