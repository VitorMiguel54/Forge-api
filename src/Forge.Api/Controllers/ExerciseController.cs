using Forge.Application.DTOs.Exercise;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/exercises")]
public class ExerciseController(IExerciseService exerciseService) : ControllerBase
{
    private const string GetExerciseByIdRouteName = "GetExerciseById";

    [HttpPost]
    public async Task<ActionResult<ExerciseResponse>> CreateAsync(
        CreateExerciseRequest request,
        CancellationToken cancellationToken)
    {
        var exercise = await exerciseService.CreateAsync(request, cancellationToken);

        return CreatedAtRoute(GetExerciseByIdRouteName, new { id = exercise.Id }, exercise);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ExerciseResponse>>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var exercises = await exerciseService.GetAllAsync(cancellationToken);

        return Ok(exercises);
    }

    [HttpGet("{id:guid}", Name = GetExerciseByIdRouteName)]
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
        var exercise = await exerciseService.UpdateAsync(id, request, cancellationToken);
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
}
