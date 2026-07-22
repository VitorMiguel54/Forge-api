using Forge.Application.Models;
using Forge.Domain.Entities;

namespace Forge.Application.Interfaces;

public interface IAchievementRepository
{
    Task<IReadOnlyCollection<Achievement>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Achievement>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Achievement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BackofficeAchievementListData> GetBackofficeAsync(BackofficeAchievementListQuery query, CancellationToken cancellationToken = default);
    Task<BackofficeAchievementData?> GetBackofficeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? ignoredId = null, CancellationToken cancellationToken = default);
    Task<bool> HasUserAchievementsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasXpTransactionsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserAchievement>> GetUnlockedByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default);

    Task<bool> IsUnlockedAsync(
        Guid userProfileId,
        Guid achievementId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Achievement achievement, CancellationToken cancellationToken = default);
    Task UpdateAsync(Achievement achievement, CancellationToken cancellationToken = default);
    Task DeleteAsync(Achievement achievement, CancellationToken cancellationToken = default);
    Task AddUnlockedAsync(UserAchievement userAchievement, CancellationToken cancellationToken = default);
}