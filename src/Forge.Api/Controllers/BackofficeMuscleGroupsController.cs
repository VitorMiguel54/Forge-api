using Forge.Application.DTOs.Backoffice.MuscleGroups;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/backoffice/muscle-groups")]
public class BackofficeMuscleGroupsController(IBackofficeMuscleGroupService muscleGroupService) : ControllerBase
{
    private const string GetBackofficeMuscleGroupByIdRouteName = "GetBackofficeMuscleGroupById";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<BackofficeMuscleGroupResponse>>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var muscleGroups = await muscleGroupService.GetAllAsync(cancellationToken);

        return Ok(muscleGroups);
    }

    [HttpGet("{id:guid}", Name = GetBackofficeMuscleGroupByIdRouteName)]
    public async Task<ActionResult<BackofficeMuscleGroupResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var muscleGroup = await muscleGroupService.GetByIdAsync(id, cancellationToken);
        if (muscleGroup is null)
        {
            return NotFound();
        }

        return Ok(muscleGroup);
    }

    [HttpPost]
    public async Task<ActionResult<BackofficeMuscleGroupResponse>> CreateAsync(
        CreateBackofficeMuscleGroupRequest request,
        CancellationToken cancellationToken)
    {
        var muscleGroup = await muscleGroupService.CreateAsync(request, cancellationToken);

        return CreatedAtRoute(
            GetBackofficeMuscleGroupByIdRouteName,
            new { id = muscleGroup.Id },
            muscleGroup);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BackofficeMuscleGroupResponse>> UpdateAsync(
        Guid id,
        UpdateBackofficeMuscleGroupRequest request,
        CancellationToken cancellationToken)
    {
        var muscleGroup = await muscleGroupService.UpdateAsync(id, request, cancellationToken);
        if (muscleGroup is null)
        {
            return NotFound();
        }

        return Ok(muscleGroup);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<BackofficeMuscleGroupResponse>> UpdateStatusAsync(
        Guid id,
        UpdateBackofficeMuscleGroupStatusRequest request,
        CancellationToken cancellationToken)
    {
        var muscleGroup = await muscleGroupService.UpdateStatusAsync(id, request, cancellationToken);
        if (muscleGroup is null)
        {
            return NotFound();
        }

        return Ok(muscleGroup);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await muscleGroupService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
