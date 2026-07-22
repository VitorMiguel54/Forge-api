using Forge.Application.DTOs.Mobile.History;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/mobile/users/{userProfileId:guid}/history")]
public class MobileHistoryController(IMobileHistoryService mobileHistoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MobileHistoryResponse>> GetAsync(
        Guid userProfileId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var history = await mobileHistoryService.GetAsync(
            userProfileId,
            page ?? 1,
            pageSize ?? 20,
            cancellationToken);

        if (history is null)
        {
            return NotFound();
        }

        return Ok(history);
    }
}
