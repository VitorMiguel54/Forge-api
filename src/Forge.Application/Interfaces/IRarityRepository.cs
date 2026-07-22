using Forge.Application.Models;
using Forge.Domain.Entities;

namespace Forge.Application.Interfaces;

public interface IRarityRepository
{
    Task<BackofficeRarityListData> GetBackofficeAsync(BackofficeRarityListQuery query, CancellationToken cancellationToken = default);
    Task<BackofficeRarityData?> GetBackofficeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Rarity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? ignoredId = null, CancellationToken cancellationToken = default);
    Task<bool> HasAchievementsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasLevelsAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Rarity rarity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Rarity rarity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Rarity rarity, CancellationToken cancellationToken = default);
}
