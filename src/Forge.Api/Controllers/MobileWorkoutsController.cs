using Forge.Application.DTOs.Mobile.Workouts;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/mobile/users/{userProfileId:guid}/workouts")]
public class MobileWorkoutsController(IMobileWorkoutService mobileWorkoutService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MobileWorkoutsResponse>> GetAsync(
        Guid userProfileId,
        CancellationToken cancellationToken)
    {
        var workouts = await mobileWorkoutService.GetAsync(userProfileId, cancellationToken);
        if (workouts is null)
        {
            return NotFound();
        }

        return Ok(workouts);
    }
}
