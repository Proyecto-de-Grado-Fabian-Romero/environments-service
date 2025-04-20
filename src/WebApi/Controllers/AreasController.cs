namespace EnvironmentsService.src.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using EnvironmentsService.src.Application.Interfaces;
using EnvironmentsService.Src.Application.DTOs.Get;

[ApiController]
[Route("api/[controller]")]
public class AreasController : ControllerBase
{
    private readonly IAreaService _areaService;

    public AreasController(IAreaService areaService)
    {
        _areaService = areaService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AreaDto>>> GetAllAreas()
    {
        var areas = await _areaService.GetAllAsync();
        return Ok(areas);
    }
}
