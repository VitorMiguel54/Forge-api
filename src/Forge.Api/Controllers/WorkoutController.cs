using Forge.Application.DTOs.Workout;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/workouts")]
public class WorkoutController(IWorkoutService workoutService) : ControllerBase
{
    private const string GetWorkoutByIdRouteName = "GetWorkoutById";

    [HttpPost]
    public async Task<ActionResult<WorkoutResponse>> CreateAsync(
        CreateWorkoutRequest request,
        CancellationToken cancellationToken)
    {
        var workout = await workoutService.CreateAsync(request, cancellationToken);

        return CreatedAtRoute(GetWorkoutByIdRouteName, new { id = workout.Id }, workout);
    }

    [HttpPost("with-exercises")]
    public async Task<ActionResult<WorkoutResponse>> CreateWithExercisesAsync(
        CreateWorkoutPlanRequest request,
        CancellationToken cancellationToken)
    {
        var workout = await workoutService.CreatePlanAsync(request, cancellationToken);

        return CreatedAtRoute(GetWorkoutByIdRouteName, new { id = workout.Id }, workout);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<WorkoutResponse>>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var workouts = await workoutService.GetAllAsync(cancellationToken);

        return Ok(workouts);
    }

    [HttpGet("{id:guid}", Name = GetWorkoutByIdRouteName)]
    public async Task<ActionResult<WorkoutResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var workout = await workoutService.GetByIdAsync(id, cancellationToken);
        if (workout is null)
        {
            return NotFound();
        }

        return Ok(workout);
    }

    [HttpGet("{id:guid}/analysis")]
    public async Task<ActionResult<WorkoutAnalysisResponse>> GetAnalysisByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var workout = await workoutService.GetAnalysisByIdAsync(id, cancellationToken);
        if (workout is null)
        {
            return NotFound();
        }

        return Ok(workout);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WorkoutResponse>> UpdateAsync(
        Guid id,
        UpdateWorkoutRequest request,
        CancellationToken cancellationToken)
    {
        var workout = await workoutService.UpdateAsync(id, request, cancellationToken);
        if (workout is null)
        {
            return NotFound();
        }

        return Ok(workout);
    }

    [HttpPost("{id:guid}/finish")]
    public async Task<ActionResult<WorkoutResponse>> FinishAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var workout = await workoutService.FinishAsync(id, cancellationToken);
        if (workout is null)
        {
            return NotFound();
        }

        return Ok(workout);
    }

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<WorkoutResponse>> StartAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var workout = await workoutService.StartAsync(id, cancellationToken);
        if (workout is null)
        {
            return NotFound();
        }

        return Ok(workout);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<WorkoutResponse>> CancelAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var workout = await workoutService.CancelAsync(id, cancellationToken);
        if (workout is null)
        {
            return NotFound();
        }

        return Ok(workout);
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> ReorderAsync(
        ReorderWorkoutsRequest request,
        CancellationToken cancellationToken)
    {
        await workoutService.ReorderAsync(request, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await workoutService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
