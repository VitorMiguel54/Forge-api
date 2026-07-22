using Forge.Application.DTOs.Workout;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/workouts/{workoutId:guid}/exercises")]
public class WorkoutExerciseController(IWorkoutExerciseService workoutExerciseService) : ControllerBase
{
    private const string GetWorkoutExercisesByWorkoutRouteName = "GetWorkoutExercisesByWorkout";

    [HttpPost]
    public async Task<ActionResult<WorkoutExerciseResponse>> CreateAsync(
        Guid workoutId,
        CreateWorkoutExerciseRequest request,
        CancellationToken cancellationToken)
    {
        var workoutExercise = await workoutExerciseService.CreateAsync(
            workoutId,
            request,
            cancellationToken);

        return CreatedAtRoute(GetWorkoutExercisesByWorkoutRouteName, new { workoutId }, workoutExercise);
    }

    [HttpGet(Name = GetWorkoutExercisesByWorkoutRouteName)]
    public async Task<ActionResult<IReadOnlyCollection<WorkoutExerciseResponse>>> GetByWorkoutAsync(
        Guid workoutId,
        CancellationToken cancellationToken)
    {
        var workoutExercises = await workoutExerciseService.GetByWorkoutAsync(workoutId, cancellationToken);
        if (workoutExercises is null)
        {
            return NotFound();
        }

        return Ok(workoutExercises);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        Guid workoutId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await workoutExerciseService.DeleteAsync(workoutId, id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
