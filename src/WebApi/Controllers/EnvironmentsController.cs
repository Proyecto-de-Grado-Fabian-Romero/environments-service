using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.DTOs.Patch;
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
        [FromQuery] int limit = 16)
    {
        var result = await _service.GetAvailableEnvironmentsAsync(request, page, limit);
        return Ok(result);
    }

    [HttpGet("owner")]
    public async Task<IActionResult> GetOwnerEnvironments(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 16)
    {
        var publicIdClaim = Request.Cookies["publicId"];
        if (publicIdClaim == null || !Guid.TryParse(publicIdClaim, out var userPublicId))
        {
            return Unauthorized("Invalid or missing user public_id" + publicIdClaim);
        }

        var result = await _service.GetOwnerEnvironmentsAsync(userPublicId, page, limit);
        return Ok(result);
    }

    [HttpGet("single")]
    public async Task<IActionResult> GetSingleEnvironment([FromQuery] Guid publicId)
    {
        var result = await _service.GetSingleEnvironmentAsync(publicId);
        return Ok(result);
    }

    [HttpPost]
    [RequestSizeLimit(30 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 30 * 1024 * 1024)]
    public async Task<IActionResult> CreateEnvironment([FromForm] CreateEnvironmentDto dto)
    {
        var publicIdClaim = Request.Cookies["publicId"];
        if (publicIdClaim == null || !Guid.TryParse(publicIdClaim, out var userPublicId))
        {
            return Unauthorized("Invalid or missing user public_id" + publicIdClaim);
        }

        var result = await _service.CreateAsync(dto, userPublicId);
        return CreatedAtAction(nameof(GetSingleEnvironment), new { publicId = result!.PublicId }, result);
    }

    [HttpPatch("{publicId:guid}/detected-objects")]
    public async Task<IActionResult> UpdateDetectedObjects(Guid publicId, [FromBody] UpdateDetectedObjectsDto dto)
    {
        try
        {
            await _service.UpdateDetectedObjectsAsync(publicId, dto.DetectedObjects);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
