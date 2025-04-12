using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentsService.Src.WebApi.Controllers;

[ApiController]
[Route("api/environments")]
public class EnvironmentsController : ControllerBase
{
    private readonly IEnvironmentService _service;

    public EnvironmentsController(IEnvironmentService service)
    {
        _service = service;
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableEnvironments([FromBody] GetAvailableEnvironmentsRequest request)
    {
        var result = await _service.GetAvailableEnvironmentsAsync(request);
        return Ok(result);
    }
}
