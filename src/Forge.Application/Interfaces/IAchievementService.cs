using Forge.Application.DTOs.Achievement;

namespace Forge.Application.Interfaces;

public interface IAchievementService
{
    Task<IReadOnlyCollection<AchievementResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserAchievementResponse>> GetUnlockedByUserAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
