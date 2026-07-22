using Forge.Application.DTOs.Exercise;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/mobile/exercises")]
public class MobileExercisesController(IExerciseService exerciseService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ExerciseResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ExerciseResponse>>> GetAll(
        [FromQuery] Guid? muscleGroupId,
        CancellationToken cancellationToken)
    {
        var exercises = await exerciseService.GetAllAsync(cancellationToken);

        if (muscleGroupId is not null && muscleGroupId != Guid.Empty)
        {
            exercises = exercises
                .Where(exercise => exercise.MuscleGroupId == muscleGroupId)
                .ToArray();
        }

        return Ok(exercises);
    }
}
