using Forge.Application.DTOs.Backoffice.Exercises;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/backoffice/exercises")]
public class BackofficeExercisesController(IBackofficeExerciseService exerciseService) : ControllerBase
{
    private const string GetBackofficeExerciseByIdRouteName = "GetBackofficeExerciseById";
    private const int MaxMediaUploadRequestBytes = 51 * 1024 * 1024;

    [HttpGet]
    public async Task<ActionResult<BackofficeExerciseListResponse>> GetAsync(
        [FromQuery] string? search,
        [FromQuery] Guid? muscleGroupId,
        [FromQuery] bool? isActive,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDirection,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var exercises = await exerciseService.GetAsync(
            search,
            muscleGroupId,
            isActive,
            sortBy,
            sortDirection,
            page <= 0 ? 1 : page,
            pageSize <= 0 ? 20 : pageSize,
            cancellationToken);

        return Ok(exercises);
    }

    [HttpGet("{id:guid}", Name = GetBackofficeExerciseByIdRouteName)]
    public async Task<ActionResult<BackofficeExerciseResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var exercise = await exerciseService.GetByIdAsync(id, cancellationToken);
        if (exercise is null)
        {
            return NotFound();
        }

        return Ok(exercise);
    }

    [HttpPost]
    public async Task<ActionResult<BackofficeExerciseResponse>> CreateAsync(
        CreateBackofficeExerciseRequest request,
        CancellationToken cancellationToken)
    {
        var exercise = await exerciseService.CreateAsync(request, cancellationToken);

        return CreatedAtRoute(
            GetBackofficeExerciseByIdRouteName,
            new { id = exercise.Id },
            exercise);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BackofficeExerciseResponse>> UpdateAsync(
        Guid id,
        UpdateBackofficeExerciseRequest request,
        CancellationToken cancellationToken)
    {
        var exercise = await exerciseService.UpdateAsync(id, request, cancellationToken);
        if (exercise is null)
        {
            return NotFound();
        }

        return Ok(exercise);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<BackofficeExerciseResponse>> UpdateStatusAsync(
        Guid id,
        UpdateBackofficeExerciseStatusRequest request,
        CancellationToken cancellationToken)
    {
        var exercise = await exerciseService.UpdateStatusAsync(id, request, cancellationToken);
        if (exercise is null)
        {
            return NotFound();
        }

        return Ok(exercise);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await exerciseService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/media/{mediaType}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxMediaUploadRequestBytes)]
    public async Task<ActionResult<BackofficeExerciseMediaUploadResponse>> UploadMediaAsync(
        Guid id,
        string mediaType,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest();
        }

        await using var stream = file.OpenReadStream();
        var result = await exerciseService.UploadMediaAsync(
            id,
            mediaType,
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

    [HttpDelete("{id:guid}/media/{mediaType}")]
    public async Task<IActionResult> DeleteMediaAsync(
        Guid id,
        string mediaType,
        CancellationToken cancellationToken)
    {
        var deleted = await exerciseService.DeleteMediaAsync(id, mediaType, cancellationToken);
        if (deleted is null)
        {
            return NotFound();
        }

        return NoContent();
    }
}
