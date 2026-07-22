using Forge.Application.DTOs.Achievement;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
public class AchievementController(IAchievementService achievementService) : ControllerBase
{
    [HttpGet("api/achievements")]
    public async Task<ActionResult<IReadOnlyCollection<AchievementResponse>>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var achievements = await achievementService.GetAllAsync(cancellationToken);

        return Ok(achievements);
    }

    [HttpGet("api/user-profiles/{userProfileId:guid}/achievements")]
    public async Task<ActionResult<IReadOnlyCollection<UserAchievementResponse>>> GetUnlockedByUserAsync(
        Guid userProfileId,
        CancellationToken cancellationToken)
    {
        var achievements = await achievementService.GetUnlockedByUserAsync(userProfileId, cancellationToken);

        return Ok(achievements);
    }
}
