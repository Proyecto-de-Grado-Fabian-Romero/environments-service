namespace EnvironmentsService.src.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Application.DTOs.Get;

[ApiController]
[Route("api/[controller]")]
public class ServicesController(IServiceService serviceService) : ControllerBase
{
    private readonly IServiceService _serviceService = serviceService;

    [HttpGet]
    public async Task<ActionResult<List<AreaDto>>> GetAllAreas()
    {
        var services = await _serviceService.GetAllAsync();
        return Ok(services);
    }
}
