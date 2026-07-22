using Forge.Application.DTOs.Weight;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api")]
public class WeightRecordController(IWeightService weightService) : ControllerBase
{
    private const string GetLatestWeightRecordByUserProfileRouteName = "GetLatestWeightRecordByUserProfile";

    [HttpPost("user-profiles/{userProfileId:guid}/weight-records")]
    public async Task<ActionResult<WeightRecordResponse>> CreateAsync(
        Guid userProfileId,
        CreateWeightRecordRequest request,
        CancellationToken cancellationToken)
    {
        var weightRecord = await weightService.CreateAsync(
            userProfileId,
            request,
            cancellationToken);

        return CreatedAtRoute(
            GetLatestWeightRecordByUserProfileRouteName,
            new { userProfileId },
            weightRecord);
    }

    [HttpGet("user-profiles/{userProfileId:guid}/weight-records")]
    public async Task<ActionResult<IReadOnlyCollection<WeightRecordResponse>>> GetByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken)
    {
        var weightRecords = await weightService.GetByUserProfileAsync(userProfileId, cancellationToken);
        if (weightRecords is null)
        {
            return NotFound();
        }

        return Ok(weightRecords);
    }

    [HttpGet(
        "user-profiles/{userProfileId:guid}/weight-records/latest",
        Name = GetLatestWeightRecordByUserProfileRouteName)]
    public async Task<ActionResult<WeightRecordResponse>> GetLatestByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken)
    {
        var weightRecord = await weightService.GetLatestByUserProfileAsync(userProfileId, cancellationToken);
        if (weightRecord is null)
        {
            return NotFound();
        }

        return Ok(weightRecord);
    }

    [HttpDelete("weight-records/{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await weightService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
