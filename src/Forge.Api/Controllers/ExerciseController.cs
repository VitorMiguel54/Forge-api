using Forge.Application.DTOs.Exercise;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/exercises")]
public class ExerciseController(IExerciseService exerciseService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ExerciseResponse>> CreateAsync(
        CreateExerciseRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var exercise = await exerciseService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetByIdAsync), new { id = exercise.Id }, exercise);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ExerciseResponse>>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var exercises = await exerciseService.GetAllAsync(cancellationToken);

        return Ok(exercises);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExerciseResponse>> GetByIdAsync(
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

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExerciseResponse>> UpdateAsync(
        Guid id,
        UpdateExerciseRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var exercise = await exerciseService.UpdateAsync(id, request, cancellationToken);
            if (exercise is null)
            {
                return NotFound();
            }

            return Ok(exercise);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await exerciseService.DeleteAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { error = exception.Message });
        }
    }
}
