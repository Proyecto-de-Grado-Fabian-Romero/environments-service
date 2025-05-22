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
        try
        {
            var response = await _service.CreateAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
