using Forge.Application.DTOs.Water;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api")]
public class WaterIntakeController(IWaterService waterService) : ControllerBase
{
    private const string GetTodayWaterIntakeByUserProfileRouteName = "GetTodayWaterIntakeByUserProfile";

    [HttpPost("user-profiles/{userProfileId:guid}/water-intakes")]
    public async Task<ActionResult<WaterIntakeResponse>> CreateAsync(
        Guid userProfileId,
        CreateWaterIntakeRequest request,
        CancellationToken cancellationToken)
    {
        var waterIntake = await waterService.CreateAsync(
            userProfileId,
            request,
            cancellationToken);

        return CreatedAtRoute(
            GetTodayWaterIntakeByUserProfileRouteName,
            new { userProfileId },
            waterIntake);
    }

    [HttpGet("user-profiles/{userProfileId:guid}/water-intakes")]
    public async Task<ActionResult<IReadOnlyCollection<WaterIntakeResponse>>> GetByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken)
    {
        var waterIntakes = await waterService.GetByUserProfileAsync(userProfileId, cancellationToken);
        if (waterIntakes is null)
        {
            return NotFound();
        }

        return Ok(waterIntakes);
    }

    [HttpGet(
        "user-profiles/{userProfileId:guid}/water-intakes/today",
        Name = GetTodayWaterIntakeByUserProfileRouteName)]
    public async Task<ActionResult<WaterIntakeResponse>> GetTodayByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken)
    {
        var waterIntake = await waterService.GetTodayByUserProfileAsync(userProfileId, cancellationToken);
        if (waterIntake is null)
        {
            return NotFound();
        }

        return Ok(waterIntake);
    }

    [HttpDelete("water-intakes/{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await waterService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
