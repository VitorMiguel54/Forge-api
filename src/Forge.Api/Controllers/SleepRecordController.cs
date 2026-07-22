using Forge.Application.DTOs.Sleep;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api")]
public class SleepRecordController(ISleepService sleepService) : ControllerBase
{
    private const string GetLatestSleepRecordByUserProfileRouteName = "GetLatestSleepRecordByUserProfile";

    [HttpPost("user-profiles/{userProfileId:guid}/sleep-records")]
    public async Task<ActionResult<SleepRecordResponse>> CreateAsync(
        Guid userProfileId,
        CreateSleepRecordRequest request,
        CancellationToken cancellationToken)
    {
        var sleepRecord = await sleepService.CreateAsync(
            userProfileId,
            request,
            cancellationToken);

        return CreatedAtRoute(
            GetLatestSleepRecordByUserProfileRouteName,
            new { userProfileId },
            sleepRecord);
    }

    [HttpGet("user-profiles/{userProfileId:guid}/sleep-records")]
    public async Task<ActionResult<IReadOnlyCollection<SleepRecordResponse>>> GetByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken)
    {
        var sleepRecords = await sleepService.GetByUserProfileAsync(userProfileId, cancellationToken);
        if (sleepRecords is null)
        {
            return NotFound();
        }

        return Ok(sleepRecords);
    }

    [HttpGet(
        "user-profiles/{userProfileId:guid}/sleep-records/latest",
        Name = GetLatestSleepRecordByUserProfileRouteName)]
    public async Task<ActionResult<SleepRecordResponse>> GetLatestByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken)
    {
        var sleepRecord = await sleepService.GetLatestByUserProfileAsync(userProfileId, cancellationToken);
        if (sleepRecord is null)
        {
            return NotFound();
        }

        return Ok(sleepRecord);
    }

    [HttpDelete("sleep-records/{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await sleepService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
