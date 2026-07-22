using Forge.Application.DTOs.Backoffice.Levels;
using Forge.Application.DTOs.Backoffice;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/backoffice/levels")]
public class BackofficeLevelsController(IBackofficeLevelDefinitionService levelDefinitionService) : ControllerBase
{
    private const string GetBackofficeLevelByIdRouteName = "GetBackofficeLevelById";
    private const int MaxImageUploadRequestBytes = 11 * 1024 * 1024;

    [HttpGet]
    public async Task<ActionResult<BackofficeLevelDefinitionListResponse>> GetAsync(
        [FromQuery] string? search,
        [FromQuery] Guid? rarityId,
        [FromQuery] bool? isActive,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDirection,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var levels = await levelDefinitionService.GetAsync(search, rarityId, isActive, sortBy, sortDirection, page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, cancellationToken);
        return Ok(levels);
    }

    [HttpGet("{id:guid}", Name = GetBackofficeLevelByIdRouteName)]
    public async Task<ActionResult<BackofficeLevelDefinitionResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var level = await levelDefinitionService.GetByIdAsync(id, cancellationToken);
        if (level is null) return NotFound();
        return Ok(level);
    }

    [HttpPost]
    public async Task<ActionResult<BackofficeLevelDefinitionResponse>> CreateAsync(CreateBackofficeLevelDefinitionRequest request, CancellationToken cancellationToken)
    {
        var level = await levelDefinitionService.CreateAsync(request, cancellationToken);
        return CreatedAtRoute(GetBackofficeLevelByIdRouteName, new { id = level.Id }, level);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BackofficeLevelDefinitionResponse>> UpdateAsync(Guid id, UpdateBackofficeLevelDefinitionRequest request, CancellationToken cancellationToken)
    {
        var level = await levelDefinitionService.UpdateAsync(id, request, cancellationToken);
        if (level is null) return NotFound();
        return Ok(level);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<BackofficeLevelDefinitionResponse>> UpdateStatusAsync(Guid id, UpdateBackofficeLevelDefinitionStatusRequest request, CancellationToken cancellationToken)
    {
        var level = await levelDefinitionService.UpdateStatusAsync(id, request, cancellationToken);
        if (level is null) return NotFound();
        return Ok(level);
    }

    [HttpPost("{id:guid}/guardian-image")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxImageUploadRequestBytes)]
    public async Task<ActionResult<AdminImageUploadResponse>> UploadGuardianImageAsync(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest();
        }

        await using var stream = file.OpenReadStream();
        var result = await levelDefinitionService.UploadGuardianImageAsync(
            id,
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            cancellationToken);

        if (result is null) return NotFound();

        return Ok(result);
    }

    [HttpDelete("{id:guid}/guardian-image")]
    public async Task<IActionResult> DeleteGuardianImageAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await levelDefinitionService.DeleteGuardianImageAsync(id, cancellationToken);
        if (deleted is null) return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await levelDefinitionService.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
