using Forge.Application.DTOs.Backoffice.Rarities;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/backoffice/rarities")]
public class BackofficeRaritiesController(IBackofficeRarityService rarityService) : ControllerBase
{
    private const string GetBackofficeRarityByIdRouteName = "GetBackofficeRarityById";

    [HttpGet]
    public async Task<ActionResult<BackofficeRarityListResponse>> GetAsync(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDirection,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var rarities = await rarityService.GetAsync(
            search,
            isActive,
            sortBy,
            sortDirection,
            page <= 0 ? 1 : page,
            pageSize <= 0 ? 20 : pageSize,
            cancellationToken);

        return Ok(rarities);
    }

    [HttpGet("{id:guid}", Name = GetBackofficeRarityByIdRouteName)]
    public async Task<ActionResult<BackofficeRarityResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var rarity = await rarityService.GetByIdAsync(id, cancellationToken);
        if (rarity is null)
        {
            return NotFound();
        }

        return Ok(rarity);
    }

    [HttpPost]
    public async Task<ActionResult<BackofficeRarityResponse>> CreateAsync(
        CreateBackofficeRarityRequest request,
        CancellationToken cancellationToken)
    {
        var rarity = await rarityService.CreateAsync(request, cancellationToken);

        return CreatedAtRoute(
            GetBackofficeRarityByIdRouteName,
            new { id = rarity.Id },
            rarity);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BackofficeRarityResponse>> UpdateAsync(
        Guid id,
        UpdateBackofficeRarityRequest request,
        CancellationToken cancellationToken)
    {
        var rarity = await rarityService.UpdateAsync(id, request, cancellationToken);
        if (rarity is null)
        {
            return NotFound();
        }

        return Ok(rarity);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<BackofficeRarityResponse>> UpdateStatusAsync(
        Guid id,
        UpdateBackofficeRarityStatusRequest request,
        CancellationToken cancellationToken)
    {
        var rarity = await rarityService.UpdateStatusAsync(id, request, cancellationToken);
        if (rarity is null)
        {
            return NotFound();
        }

        return Ok(rarity);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await rarityService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
