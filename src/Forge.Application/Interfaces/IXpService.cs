using Forge.Application.DTOs.XP;
using Forge.Domain.Entities;

namespace Forge.Application.Interfaces;

public interface IXpService
{
    Task<XpSummaryResponse?> GetSummaryByUserAsync(Guid userProfileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<XpTransactionResponse>> GetTransactionsByUserAsync(Guid userProfileId, CancellationToken cancellationToken = default);
    Task<XpTransactionResponse?> AwardWorkoutCompletedAsync(Workout workout, CancellationToken cancellationToken = default);
    Task<XpTransactionResponse?> AwardAchievementUnlockedAsync(
        Guid userProfileId,
        Achievement achievement,
        CancellationToken cancellationToken = default);
}
