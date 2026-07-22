using Forge.Application.Models;
using Forge.Domain.Entities;

namespace Forge.Application.Interfaces;

public interface ILevelDefinitionRepository
{
    Task<IReadOnlyCollection<LevelDefinitionData>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<LevelDefinitionData?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BackofficeLevelDefinitionListData> GetBackofficeAsync(BackofficeLevelDefinitionListQuery query, CancellationToken cancellationToken = default);
    Task<BackofficeLevelDefinitionData?> GetBackofficeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LevelDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveInitialLevelAsync(Guid? ignoredId = null, CancellationToken cancellationToken = default);
    Task<bool> RarityExistsAsync(Guid rarityId, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? ignoredId = null, CancellationToken cancellationToken = default);
    Task<bool> DisplayOrderExistsAsync(int displayOrder, Guid? ignoredId = null, CancellationToken cancellationToken = default);
    Task<bool> MinimumXpExistsAsync(int minimumXp, Guid? ignoredId = null, CancellationToken cancellationToken = default);
    Task AddAsync(LevelDefinition levelDefinition, CancellationToken cancellationToken = default);
    Task UpdateAsync(LevelDefinition levelDefinition, CancellationToken cancellationToken = default);
    Task DeleteAsync(LevelDefinition levelDefinition, CancellationToken cancellationToken = default);
}
