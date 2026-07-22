using Forge.Application.DTOs.Mobile.Home;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/mobile/users/{userProfileId:guid}/home")]
public class MobileHomeController(IMobileHomeService mobileHomeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MobileHomeResponse>> GetAsync(
        Guid userProfileId,
        CancellationToken cancellationToken)
    {
        var home = await mobileHomeService.GetAsync(userProfileId, cancellationToken);
        if (home is null)
        {
            return NotFound();
        }

        return Ok(home);
    }
}
