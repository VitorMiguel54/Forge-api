using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/user-profiles/{userProfileId:guid}/xp")]
public class XpController(IXpService xpService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetByUserAsync(
        Guid userProfileId,
        CancellationToken cancellationToken)
    {
        var summary = await xpService.GetSummaryByUserAsync(userProfileId, cancellationToken);
        if (summary is null)
        {
            return NotFound();
        }

        return Ok(summary);
    }
}
