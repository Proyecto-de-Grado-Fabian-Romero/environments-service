using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentsService.Src.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ToursController(ITourService service) : ControllerBase
{
    private readonly ITourService _service = service;

    [HttpPost]
    public async Task<IActionResult> UploadTour(
        [FromQuery] Guid environmentPublicId,
        [FromBody] TourUploadDto dto
    )
    {
        var tour = await _service.CreateTourAsync(environmentPublicId, dto.Scenes);
        return Ok(tour);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTour(string id)
    {
        var tour = await _service.GetTourByIdAsync(id);
        return tour is null ? NotFound() : Ok(tour);
    }
}
