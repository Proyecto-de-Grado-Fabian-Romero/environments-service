using EnvironmentsService.Src.Application.DTOs.Create;
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
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
