using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentsService.Src.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TourController : ControllerBase
{
    private readonly ITourService _service;

    public TourController(ITourService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> UploadTour([FromBody] TourUploadDto dto)
    {
        var tour = await _service.CreateTourAsync(dto.Scenes);
        return Ok(tour);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTour(string id)
    {
        var tour = await _service.GetTourByIdAsync(id);
        return tour is null ? NotFound() : Ok(tour);
    }
}
