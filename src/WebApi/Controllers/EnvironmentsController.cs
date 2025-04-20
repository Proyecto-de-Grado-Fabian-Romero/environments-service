using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentsService.Src.WebApi.Controllers;

[ApiController]
[Route("api/environments")]
public class EnvironmentsController(IEnvironmentService service) : ControllerBase
{
    private readonly IEnvironmentService _service = service;

    [HttpPost("available")]
    public async Task<IActionResult> GetAvailableEnvironments(
    [FromBody] GetAvailableEnvironmentsRequest request,
    [FromQuery] int page = 1,
    [FromQuery] int limit = 10)
    {
        var result = await _service.GetAvailableEnvironmentsAsync(request, page, limit);
        return Ok(result);
    }

    [HttpGet("single")]
    public async Task<IActionResult> GetAvailableEnvironments([FromQuery] Guid publicId)
    {
        var result = await _service.GetSingleEnvironmentAsync(publicId);
        return Ok(result);
    }
}
