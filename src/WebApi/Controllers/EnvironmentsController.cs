using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.src.Application.DTOs.Patch;
using EnvironmentsService.Src.Application.DTOs.Patch;
using EnvironmentsService.Src.Application.DTOs.Update;
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
        [FromQuery] int limit = 16
    )
    {
        var publicIdClaim = Request.Cookies["publicId"];
        Guid? userPublicId = null;

        if (Guid.TryParse(publicIdClaim, out var parsedId))
        {
            userPublicId = parsedId;
        }

        var result = await _service.GetAvailableEnvironmentsAsync(
            request,
            page,
            limit,
            userPublicId
        );
        return Ok(result);
    }

    [HttpGet("owner")]
    public async Task<IActionResult> GetOwnerEnvironments(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 16
    )
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
        return CreatedAtAction(
            nameof(GetSingleEnvironment),
            new { publicId = result!.PublicId },
            result
        );
    }

    [HttpPatch("{publicId:guid}/detected-objects")]
    public async Task<IActionResult> UpdateDetectedObjects(
        Guid publicId,
        [FromBody] UpdateDetectedObjectsDto dto
    )
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

    [HttpPost("available-equipment")]
    public async Task<IActionResult> GetAvailableEquipment(
        [FromBody] GetAvailableEnvironmentsRequest request
    )
    {
        var result = await _service.GetAvailableEquipmentAsync(request);
        return Ok(result);
    }

    [HttpPut("{publicId:guid}")]
    [RequestSizeLimit(30 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 30 * 1024 * 1024)]
    public async Task<IActionResult> UpdateEnvironment(
        Guid publicId,
        [FromForm] UpdateEnvironmentDto dto
    )
    {
        var publicIdClaim = Request.Cookies["publicId"];
        if (publicIdClaim == null || !Guid.TryParse(publicIdClaim, out var userPublicId))
        {
            return Unauthorized("Invalid or missing user public_id" + publicIdClaim);
        }

        var result = await _service.UpdateAsync(publicId, dto, userPublicId);
        return Ok(result);
    }

    // PATCH api/environments/{publicId}/hide
    [HttpPatch("{publicId}/hide")]
    public async Task<IActionResult> PatchHideEnvironment(
        Guid publicId,
        [FromBody] PatchHideDto body
    )
    {
        var publicIdClaim = Request.Cookies["publicId"];
        if (!Guid.TryParse(publicIdClaim, out var userPublicId))
        {
            return Forbid();
        }

        var success = await _service.PatchHideEnvironmentAsync(publicId, body.Hide, userPublicId);
        if (!success)
        {
            return Forbid(); // or NotFound if you prefer
        }

        return NoContent();
    }

    // PATCH api/environments/{publicId}/delete
    [HttpPatch("{publicId}/delete")]
    public async Task<IActionResult> PatchDeleteEnvironment(Guid publicId)
    {
        var publicIdClaim = Request.Cookies["publicId"];
        if (!Guid.TryParse(publicIdClaim, out var userPublicId))
        {
            return Forbid();
        }

        var success = await _service.PatchDeleteEnvironmentAsync(publicId, userPublicId);
        if (!success)
        {
            return Forbid();
        }

        return NoContent();
    }
}
