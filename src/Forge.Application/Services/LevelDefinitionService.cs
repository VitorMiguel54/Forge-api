using Forge.Application.DTOs.Levels;
using Forge.Application.Interfaces;

namespace Forge.Application.Services;

public class LevelDefinitionService(ILevelDefinitionRepository levelDefinitionRepository) : ILevelDefinitionService
{
    public async Task<IReadOnlyCollection<LevelDefinitionResponse>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var levels = (await levelDefinitionRepository.GetActiveAsync(cancellationToken))
            .OrderBy(level => level.DisplayOrder)
            .ToArray();

        return levels
            .Select((level, index) => LevelResponseFactory.ToPublicResponse(level, index == levels.Length - 1))
            .ToArray();
    }

    public async Task<LevelDefinitionResponse?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) return null;

        var levels = (await levelDefinitionRepository.GetActiveAsync(cancellationToken))
            .OrderBy(level => level.DisplayOrder)
            .ToArray();
        var level = levels.FirstOrDefault(item => item.Id == id);
        if (level is null) return null;

        return LevelResponseFactory.ToPublicResponse(level, Array.IndexOf(levels, level) == levels.Length - 1);
    }
}
