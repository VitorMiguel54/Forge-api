using Forge.Application.DTOs.Mobile.MuscleGroups;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/mobile/muscle-groups")]
public class MobileMuscleGroupsController(IMobileMuscleGroupService mobileMuscleGroupService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<MobileMuscleGroupResponse>>> GetAsync(
        CancellationToken cancellationToken)
    {
        var muscleGroups = await mobileMuscleGroupService.GetAsync(cancellationToken);

        return Ok(muscleGroups);
    }
}
