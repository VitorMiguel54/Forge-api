using Forge.Application.DTOs.Achievement;
using Forge.Application.Interfaces;

namespace Forge.Application.Services;

public class AchievementService : IAchievementService
{
    public Task<IReadOnlyCollection<AchievementResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<UserAchievementResponse>> GetUnlockedByUserAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
