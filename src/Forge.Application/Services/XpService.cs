using Forge.Application.DTOs.XP;
using Forge.Application.Interfaces;
using Forge.Application.Mappings;
using Forge.Domain.Entities;
using Forge.Domain.Enums;

namespace Forge.Application.Services;

public class XpService(
    IXpRepository xpRepository,
    IUserProfileRepository userProfileRepository,
    ILevelProgressionService levelProgressionService) : IXpService
{
    private const int BaseWorkoutCompletedXp = 50;
    private const int XpPerWorkoutExercise = 10;

    public async Task<XpSummaryResponse?> GetSummaryByUserAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        if (userProfileId == Guid.Empty)
        {
            return null;
        }

        var userProfile = await userProfileRepository.GetByIdAsync(userProfileId, cancellationToken);
        if (userProfile is null)
        {
            return null;
        }

        var transactions = await GetTransactionsByUserAsync(userProfileId, cancellationToken);

        var progression = await levelProgressionService.GetProgressionAsync(userProfile.TotalXp, cancellationToken);

        return new XpSummaryResponse(
            userProfile.Id,
            userProfile.TotalXp,
            progression.NumericLevel,
            progression.XpToNextLevel,
            transactions,
            progression.CurrentLevel?.Name,
            progression.CurrentLevel?.Description,
            progression.CurrentLevel?.BadgeImageUrl,
            progression.CurrentLevel?.GuardianImageUrl,
            progression.CurrentLevel?.RarityName,
            progression.NextLevel?.Name,
            progression.ProgressPercentage,
            progression.IsMaximumLevel);
    }

    public async Task<IReadOnlyCollection<XpTransactionResponse>> GetTransactionsByUserAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        if (userProfileId == Guid.Empty)
        {
            return Array.Empty<XpTransactionResponse>();
        }

        var transactions = await xpRepository.GetByUserProfileAsync(userProfileId, cancellationToken);

        return transactions
            .Select(xpTransaction => xpTransaction.ToResponse())
            .ToArray();
    }

    public async Task<XpTransactionResponse?> AwardWorkoutCompletedAsync(
        Workout workout,
        CancellationToken cancellationToken = default)
    {
        if (await xpRepository.ExistsAsync(
                workout.UserProfileId,
                XpSource.WorkoutCompleted,
                workout.Id,
                cancellationToken))
        {
            return null;
        }

        var exerciseCount = workout.WorkoutExercises.Count;
        var amount = BaseWorkoutCompletedXp + (exerciseCount * XpPerWorkoutExercise);

        return await AddXpAsync(
            workout.UserProfileId,
            amount,
            XpSource.WorkoutCompleted,
            $"Treino concluído: {workout.Name}",
            workout.Id,
            cancellationToken);
    }

    public async Task<XpTransactionResponse?> AwardAchievementUnlockedAsync(
        Guid userProfileId,
        Achievement achievement,
        CancellationToken cancellationToken = default)
    {
        if (achievement.XpReward <= 0)
        {
            return null;
        }

        if (await xpRepository.ExistsAsync(
                userProfileId,
                XpSource.AchievementUnlocked,
                achievement.Id,
                cancellationToken))
        {
            return null;
        }

        return await AddXpAsync(
            userProfileId,
            achievement.XpReward,
            XpSource.AchievementUnlocked,
            $"Conquista desbloqueada: {achievement.Name}",
            achievement.Id,
            cancellationToken);
    }

    private async Task<XpTransactionResponse?> AddXpAsync(
        Guid userProfileId,
        int amount,
        XpSource source,
        string description,
        Guid? referenceId,
        CancellationToken cancellationToken)
    {
        if (userProfileId == Guid.Empty || amount <= 0)
        {
            return null;
        }

        var userProfile = await userProfileRepository.GetByIdAsync(userProfileId, cancellationToken);
        if (userProfile is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var xpTransaction = new XpTransaction
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfileId,
            Amount = amount,
            Source = source,
            Description = description,
            ReferenceId = referenceId,
            CreatedAt = now
        };

        userProfile.TotalXp += amount;
        var progression = await levelProgressionService.GetProgressionAsync(userProfile.TotalXp, cancellationToken);
        userProfile.Level = progression.NumericLevel;
        userProfile.UpdatedAt = now;

        await xpRepository.AddAsync(xpTransaction, cancellationToken);
        await userProfileRepository.UpdateAsync(userProfile, cancellationToken);

        return xpTransaction.ToResponse();
    }
}
