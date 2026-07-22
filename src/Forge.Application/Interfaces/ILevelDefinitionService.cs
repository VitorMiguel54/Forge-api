using Forge.Application.DTOs.Levels;

namespace Forge.Application.Interfaces;

public interface ILevelDefinitionService
{
    Task<IReadOnlyCollection<LevelDefinitionResponse>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<LevelDefinitionResponse?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
