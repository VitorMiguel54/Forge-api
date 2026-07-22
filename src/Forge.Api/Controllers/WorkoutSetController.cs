using Forge.Application.DTOs.Workout;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api")]
public class WorkoutSetController(IWorkoutSetService workoutSetService) : ControllerBase
{
    private const string GetWorkoutSetsByWorkoutExerciseRouteName = "GetWorkoutSetsByWorkoutExercise";

    [HttpPost("workout-exercises/{workoutExerciseId:guid}/sets")]
    public async Task<ActionResult<WorkoutSetResponse>> CreateAsync(
        Guid workoutExerciseId,
        CreateWorkoutSetRequest request,
        CancellationToken cancellationToken)
    {
        var workoutSet = await workoutSetService.CreateAsync(
            workoutExerciseId,
            request,
            cancellationToken);

        return CreatedAtRoute(
            GetWorkoutSetsByWorkoutExerciseRouteName,
            new { workoutExerciseId },
            workoutSet);
    }

    [HttpGet("workout-exercises/{workoutExerciseId:guid}/sets", Name = GetWorkoutSetsByWorkoutExerciseRouteName)]
    public async Task<ActionResult<IReadOnlyCollection<WorkoutSetResponse>>> GetByWorkoutExerciseAsync(
        Guid workoutExerciseId,
        CancellationToken cancellationToken)
    {
        var workoutSets = await workoutSetService.GetByWorkoutExerciseAsync(
            workoutExerciseId,
            cancellationToken);

        if (workoutSets is null)
        {
            return NotFound();
        }

        return Ok(workoutSets);
    }

    [HttpPut("workout-sets/{id:guid}")]
    public async Task<ActionResult<WorkoutSetResponse>> UpdateAsync(
        Guid id,
        UpdateWorkoutSetRequest request,
        CancellationToken cancellationToken)
    {
        var workoutSet = await workoutSetService.UpdateAsync(id, request, cancellationToken);
        if (workoutSet is null)
        {
            return NotFound();
        }

        return Ok(workoutSet);
    }

    [HttpDelete("workout-sets/{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await workoutSetService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
