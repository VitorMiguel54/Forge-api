using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Domain.Entities;
using Forge.Domain.Enums;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Repositories;

public class AchievementRepository(ForgeDbContext dbContext) : IAchievementRepository
{
    public async Task<IReadOnlyCollection<Achievement>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Achievements
            .AsNoTracking()
            .OrderBy(achievement => achievement.Category)
            .ThenBy(achievement => achievement.RequiredValue)
            .ThenBy(achievement => achievement.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Achievement>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Achievements
            .AsNoTracking()
            .Where(achievement => achievement.IsActive)
            .OrderBy(achievement => achievement.Category)
            .ThenBy(achievement => achievement.RequiredValue)
            .ThenBy(achievement => achievement.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Achievement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Achievements
            .FirstOrDefaultAsync(achievement => achievement.Id == id, cancellationToken);
    }

    public async Task<BackofficeAchievementListData> GetBackofficeAsync(
        BackofficeAchievementListQuery query,
        CancellationToken cancellationToken = default)
    {
        var achievements = dbContext.Achievements
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            achievements = achievements.Where(achievement => achievement.Name.Contains(search));
        }

        if (query.Category is not null)
        {
            achievements = achievements.Where(achievement => achievement.Category == query.Category);
        }

        if (query.IsActive is not null)
        {
            achievements = achievements.Where(achievement => achievement.IsActive == query.IsActive);
        }

        if (query.IsSecret is not null)
        {
            achievements = achievements.Where(achievement => achievement.IsSecret == query.IsSecret);
        }

        achievements = ApplyOrdering(achievements, query.SortBy, query.SortDirection);

        var totalItems = await achievements.CountAsync(cancellationToken);
        var skip = (query.Page - 1) * query.PageSize;
        var items = await achievements
            .Skip(skip)
            .Take(query.PageSize)
            .Select(achievement => new BackofficeAchievementData(
                achievement.Id,
                achievement.Name,
                achievement.Description,
                achievement.Category,
                achievement.RequiredValue,
                achievement.XpReward,
                achievement.IsSecret,
                achievement.IsActive,
                achievement.BadgeImageUrl,
                achievement.UserAchievements.Count,
                achievement.CreatedAt,
                achievement.UpdatedAt))
            .ToArrayAsync(cancellationToken);

        return new BackofficeAchievementListData(items, totalItems);
    }

    public async Task<BackofficeAchievementData?> GetBackofficeByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Achievements
            .AsNoTracking()
            .Where(achievement => achievement.Id == id)
            .Select(achievement => new BackofficeAchievementData(
                achievement.Id,
                achievement.Name,
                achievement.Description,
                achievement.Category,
                achievement.RequiredValue,
                achievement.XpReward,
                achievement.IsSecret,
                achievement.IsActive,
                achievement.BadgeImageUrl,
                achievement.UserAchievements.Count,
                achievement.CreatedAt,
                achievement.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(
        string name,
        Guid? ignoredId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.ToLower();

        return await dbContext.Achievements
            .AsNoTracking()
            .AnyAsync(
                achievement =>
                    achievement.Name.ToLower() == normalizedName
                    && (ignoredId == null || achievement.Id != ignoredId),
                cancellationToken);
    }

    public async Task<bool> HasUserAchievementsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserAchievements
            .AnyAsync(userAchievement => userAchievement.AchievementId == id, cancellationToken);
    }

    public async Task<bool> HasXpTransactionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.XpTransactions
            .AnyAsync(
                xpTransaction =>
                    xpTransaction.Source == XpSource.AchievementUnlocked
                    && xpTransaction.ReferenceId == id,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserAchievement>> GetUnlockedByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.UserAchievements
            .AsNoTracking()
            .Include(userAchievement => userAchievement.Achievement)
            .Where(userAchievement => userAchievement.UserProfileId == userProfileId)
            .OrderByDescending(userAchievement => userAchievement.UnlockedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<bool> IsUnlockedAsync(
        Guid userProfileId,
        Guid achievementId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.UserAchievements
            .AnyAsync(
                userAchievement =>
                    userAchievement.UserProfileId == userProfileId
                    && userAchievement.AchievementId == achievementId,
                cancellationToken);
    }

    public async Task AddAsync(Achievement achievement, CancellationToken cancellationToken = default)
    {
        await dbContext.Achievements.AddAsync(achievement, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Achievement achievement, CancellationToken cancellationToken = default)
    {
        dbContext.Achievements.Update(achievement);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Achievement achievement, CancellationToken cancellationToken = default)
    {
        dbContext.Achievements.Remove(achievement);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddUnlockedAsync(UserAchievement userAchievement, CancellationToken cancellationToken = default)
    {
        await dbContext.UserAchievements.AddAsync(userAchievement, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Achievement> ApplyOrdering(
        IQueryable<Achievement> achievements,
        string? sortBy,
        string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();

        return normalizedSortBy switch
        {
            "category" => descending
                ? achievements.OrderByDescending(achievement => achievement.Category).ThenBy(achievement => achievement.Name)
                : achievements.OrderBy(achievement => achievement.Category).ThenBy(achievement => achievement.Name),
            "requiredvalue" => descending
                ? achievements.OrderByDescending(achievement => achievement.RequiredValue).ThenBy(achievement => achievement.Name)
                : achievements.OrderBy(achievement => achievement.RequiredValue).ThenBy(achievement => achievement.Name),
            "xpreward" => descending
                ? achievements.OrderByDescending(achievement => achievement.XpReward).ThenBy(achievement => achievement.Name)
                : achievements.OrderBy(achievement => achievement.XpReward).ThenBy(achievement => achievement.Name),
            "status" => descending
                ? achievements.OrderByDescending(achievement => achievement.IsActive).ThenBy(achievement => achievement.Name)
                : achievements.OrderBy(achievement => achievement.IsActive).ThenBy(achievement => achievement.Name),
            "secret" => descending
                ? achievements.OrderByDescending(achievement => achievement.IsSecret).ThenBy(achievement => achievement.Name)
                : achievements.OrderBy(achievement => achievement.IsSecret).ThenBy(achievement => achievement.Name),
            "unlockedcount" => descending
                ? achievements.OrderByDescending(achievement => achievement.UserAchievements.Count).ThenBy(achievement => achievement.Name)
                : achievements.OrderBy(achievement => achievement.UserAchievements.Count).ThenBy(achievement => achievement.Name),
            "createdat" => descending
                ? achievements.OrderByDescending(achievement => achievement.CreatedAt).ThenBy(achievement => achievement.Name)
                : achievements.OrderBy(achievement => achievement.CreatedAt).ThenBy(achievement => achievement.Name),
            _ => descending
                ? achievements.OrderByDescending(achievement => achievement.Name)
                : achievements.OrderBy(achievement => achievement.Name)
        };
    }
}
