using Forge.Application.DTOs.Levels;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/levels")]
public class LevelsController(ILevelDefinitionService levelDefinitionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<LevelDefinitionResponse>>> GetAsync(CancellationToken cancellationToken)
    {
        return Ok(await levelDefinitionService.GetActiveAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LevelDefinitionResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var level = await levelDefinitionService.GetActiveByIdAsync(id, cancellationToken);
        if (level is null) return NotFound();
        return Ok(level);
    }
}
