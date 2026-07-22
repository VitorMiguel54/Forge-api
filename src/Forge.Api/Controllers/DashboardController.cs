using Forge.Application.DTOs.Dashboard;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("{userProfileId:guid}")]
    public async Task<ActionResult<DashboardResponse>> GetAsync(
        Guid userProfileId,
        CancellationToken cancellationToken)
    {
        var dashboard = await dashboardService.GetAsync(userProfileId, cancellationToken);
        if (dashboard is null)
        {
            return NotFound();
        }

        return Ok(dashboard);
    }
}
