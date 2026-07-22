using Forge.Application.DTOs.Backoffice.Achievements;
using Forge.Application.DTOs.Backoffice;
using Forge.Application.Interfaces;
using Forge.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/backoffice/achievements")]
public class BackofficeAchievementsController(IBackofficeAchievementService achievementService) : ControllerBase
{
    private const string GetBackofficeAchievementByIdRouteName = "GetBackofficeAchievementById";
    private const int MaxImageUploadRequestBytes = 11 * 1024 * 1024;

    [HttpGet]
    public async Task<ActionResult<BackofficeAchievementListResponse>> GetAsync(
        [FromQuery] string? search,
        [FromQuery] AchievementCategory? category,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isSecret,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDirection,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var achievements = await achievementService.GetAsync(
            search,
            category,
            isActive,
            isSecret,
            sortBy,
            sortDirection,
            page <= 0 ? 1 : page,
            pageSize <= 0 ? 20 : pageSize,
            cancellationToken);

        return Ok(achievements);
    }

    [HttpGet("{id:guid}", Name = GetBackofficeAchievementByIdRouteName)]
    public async Task<ActionResult<BackofficeAchievementResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var achievement = await achievementService.GetByIdAsync(id, cancellationToken);
        if (achievement is null)
        {
            return NotFound();
        }

        return Ok(achievement);
    }

    [HttpPost]
    public async Task<ActionResult<BackofficeAchievementResponse>> CreateAsync(
        CreateBackofficeAchievementRequest request,
        CancellationToken cancellationToken)
    {
        var achievement = await achievementService.CreateAsync(request, cancellationToken);

        return CreatedAtRoute(
            GetBackofficeAchievementByIdRouteName,
            new { id = achievement.Id },
            achievement);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BackofficeAchievementResponse>> UpdateAsync(
        Guid id,
        UpdateBackofficeAchievementRequest request,
        CancellationToken cancellationToken)
    {
        var achievement = await achievementService.UpdateAsync(id, request, cancellationToken);
        if (achievement is null)
        {
            return NotFound();
        }

        return Ok(achievement);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<BackofficeAchievementResponse>> UpdateStatusAsync(
        Guid id,
        UpdateBackofficeAchievementStatusRequest request,
        CancellationToken cancellationToken)
    {
        var achievement = await achievementService.UpdateStatusAsync(id, request, cancellationToken);
        if (achievement is null)
        {
            return NotFound();
        }

        return Ok(achievement);
    }

    [HttpPost("{id:guid}/badge-image")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxImageUploadRequestBytes)]
    public async Task<ActionResult<AdminImageUploadResponse>> UploadBadgeImageAsync(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest();
        }

        await using var stream = file.OpenReadStream();
        var result = await achievementService.UploadBadgeImageAsync(
            id,
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}/badge-image")]
    public async Task<IActionResult> DeleteBadgeImageAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await achievementService.DeleteBadgeImageAsync(id, cancellationToken);
        if (deleted is null)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await achievementService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
