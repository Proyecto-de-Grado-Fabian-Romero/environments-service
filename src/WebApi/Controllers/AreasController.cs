namespace EnvironmentsService.src.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Application.DTOs.Get;

[ApiController]
[Route("api/[controller]")]
public class AreasController(IAreaService areaService) : ControllerBase
{
    private readonly IAreaService _areaService = areaService;

    [HttpGet]
    public async Task<ActionResult<List<AreaDto>>> GetAllAreas()
    {
        var areas = await _areaService.GetAllAsync();
        return Ok(areas);
    }
}
