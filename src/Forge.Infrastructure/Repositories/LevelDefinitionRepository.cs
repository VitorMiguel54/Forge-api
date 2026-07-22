using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Domain.Entities;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Repositories;

public class LevelDefinitionRepository(ForgeDbContext dbContext) : ILevelDefinitionRepository
{
    public async Task<IReadOnlyCollection<LevelDefinitionData>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var levels = await dbContext.LevelDefinitions.AsNoTracking()
            .Include(level => level.Rarity)
            .Where(level => level.IsActive)
            .OrderBy(level => level.MinimumXp)
            .ThenBy(level => level.DisplayOrder)
            .ToArrayAsync(cancellationToken);

        return levels.Select(ToLevelDefinitionData).ToArray();
    }

    public async Task<LevelDefinitionData?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var level = await dbContext.LevelDefinitions.AsNoTracking()
            .Include(level => level.Rarity)
            .Where(level => level.Id == id && level.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        return level is null ? null : ToLevelDefinitionData(level);
    }

    public async Task<BackofficeLevelDefinitionListData> GetBackofficeAsync(BackofficeLevelDefinitionListQuery query, CancellationToken cancellationToken = default)
    {
        var levels = dbContext.LevelDefinitions.AsNoTracking()
            .Include(level => level.Rarity)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            levels = levels.Where(level => level.Name.Contains(search));
        }

        if (query.RarityId is not null) levels = levels.Where(level => level.RarityId == query.RarityId);
        if (query.IsActive is not null) levels = levels.Where(level => level.IsActive == query.IsActive);

        levels = ApplyOrdering(levels, query.SortBy, query.SortDirection);

        var totalItems = await levels.CountAsync(cancellationToken);
        var activeLevels = await GetActiveLevelInfoAsync(cancellationToken);
        var userTotalXps = await dbContext.UserProfiles.AsNoTracking()
            .Select(user => user.TotalXp)
            .ToArrayAsync(cancellationToken);

        var pageLevels = await levels
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToArrayAsync(cancellationToken);

        var items = pageLevels
            .Select(level => ToBackofficeData(level, CountUsersForLevel(level.Id, activeLevels, userTotalXps)))
            .ToArray();

        return new BackofficeLevelDefinitionListData(items, totalItems);
    }

    public async Task<BackofficeLevelDefinitionData?> GetBackofficeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var activeLevels = await GetActiveLevelInfoAsync(cancellationToken);
        var userTotalXps = await dbContext.UserProfiles.AsNoTracking()
            .Select(user => user.TotalXp)
            .ToArrayAsync(cancellationToken);

        var level = await dbContext.LevelDefinitions.AsNoTracking()
            .Include(item => item.Rarity)
            .Where(item => item.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return level is null ? null : ToBackofficeData(level, CountUsersForLevel(level.Id, activeLevels, userTotalXps));
    }

    public async Task<LevelDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.LevelDefinitions.FirstOrDefaultAsync(level => level.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsActiveInitialLevelAsync(Guid? ignoredId = null, CancellationToken cancellationToken = default)
    {
        return await dbContext.LevelDefinitions.AsNoTracking().AnyAsync(level => level.IsActive && level.MinimumXp == 0 && (ignoredId == null || level.Id != ignoredId), cancellationToken);
    }

    public async Task<bool> RarityExistsAsync(Guid rarityId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Rarities.AsNoTracking().AnyAsync(rarity => rarity.Id == rarityId, cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? ignoredId = null, CancellationToken cancellationToken = default)
    {
        var normalized = name.ToLower();
        return await dbContext.LevelDefinitions.AsNoTracking().AnyAsync(level => level.Name.ToLower() == normalized && (ignoredId == null || level.Id != ignoredId), cancellationToken);
    }

    public async Task<bool> DisplayOrderExistsAsync(int displayOrder, Guid? ignoredId = null, CancellationToken cancellationToken = default)
    {
        return await dbContext.LevelDefinitions.AsNoTracking().AnyAsync(level => level.DisplayOrder == displayOrder && (ignoredId == null || level.Id != ignoredId), cancellationToken);
    }

    public async Task<bool> MinimumXpExistsAsync(int minimumXp, Guid? ignoredId = null, CancellationToken cancellationToken = default)
    {
        return await dbContext.LevelDefinitions.AsNoTracking().AnyAsync(level => level.MinimumXp == minimumXp && (ignoredId == null || level.Id != ignoredId), cancellationToken);
    }

    public async Task AddAsync(LevelDefinition levelDefinition, CancellationToken cancellationToken = default)
    {
        await dbContext.LevelDefinitions.AddAsync(levelDefinition, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(LevelDefinition levelDefinition, CancellationToken cancellationToken = default)
    {
        dbContext.LevelDefinitions.Update(levelDefinition);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(LevelDefinition levelDefinition, CancellationToken cancellationToken = default)
    {
        dbContext.LevelDefinitions.Remove(levelDefinition);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    internal static LevelDefinitionData ToLevelDefinitionData(LevelDefinition level)
    {
        var rarity = GetRequiredRarity(level);
        return new LevelDefinitionData(level.Id, level.Name, level.Description, level.MinimumXp, level.DisplayOrder, level.RarityId, rarity.Name, rarity.PrimaryColor, rarity.SecondaryColor, level.BadgeImageUrl, level.GuardianImageUrl, level.IsActive, level.CreatedAt, level.UpdatedAt);
    }

    internal static BackofficeLevelDefinitionData ToBackofficeData(LevelDefinition level, int currentUserCount)
    {
        var rarity = GetRequiredRarity(level);
        return new BackofficeLevelDefinitionData(level.Id, level.Name, level.Description, level.MinimumXp, level.DisplayOrder, level.RarityId, rarity.Name, rarity.PrimaryColor, rarity.SecondaryColor, level.BadgeImageUrl, level.GuardianImageUrl, level.IsActive, currentUserCount, level.CreatedAt, level.UpdatedAt);
    }

    private static Rarity GetRequiredRarity(LevelDefinition level)
    {
        return level.Rarity ?? throw new InvalidOperationException($"Level definition '{level.Id}' references rarity '{level.RarityId}', but the rarity was not loaded or does not exist.");
    }

    private async Task<IReadOnlyList<ActiveLevelInfo>> GetActiveLevelInfoAsync(CancellationToken cancellationToken)
    {
        return await dbContext.LevelDefinitions.AsNoTracking()
            .Where(level => level.IsActive)
            .OrderBy(level => level.MinimumXp)
            .Select(level => new ActiveLevelInfo(level.Id, level.MinimumXp))
            .ToArrayAsync(cancellationToken);
    }

    private static int CountUsersForLevel(Guid id, IReadOnlyList<ActiveLevelInfo> activeLevels, IReadOnlyCollection<int> userTotalXps)
    {
        var index = -1;
        for (var i = 0; i < activeLevels.Count; i++) if (activeLevels[i].Id == id) index = i;
        if (index < 0) return 0;
        var minXp = activeLevels[index].MinimumXp;
        if (index == activeLevels.Count - 1) return userTotalXps.Count(totalXp => totalXp >= minXp);
        var nextXp = activeLevels[index + 1].MinimumXp;
        return userTotalXps.Count(totalXp => totalXp >= minXp && totalXp < nextXp);
    }

    private sealed record ActiveLevelInfo(Guid Id, int MinimumXp);

    private static IQueryable<LevelDefinition> ApplyOrdering(IQueryable<LevelDefinition> levels, string? sortBy, string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();
        return normalizedSortBy switch
        {
            "name" => descending ? levels.OrderByDescending(level => level.Name) : levels.OrderBy(level => level.Name),
            "rarity" or "rarityname" => descending ? levels.OrderByDescending(level => level.Rarity.Name).ThenBy(level => level.DisplayOrder) : levels.OrderBy(level => level.Rarity.Name).ThenBy(level => level.DisplayOrder),
            "minimumxp" => descending ? levels.OrderByDescending(level => level.MinimumXp).ThenBy(level => level.DisplayOrder) : levels.OrderBy(level => level.MinimumXp).ThenBy(level => level.DisplayOrder),
            "status" => descending ? levels.OrderByDescending(level => level.IsActive).ThenBy(level => level.DisplayOrder) : levels.OrderBy(level => level.IsActive).ThenBy(level => level.DisplayOrder),
            _ => descending ? levels.OrderByDescending(level => level.DisplayOrder).ThenBy(level => level.MinimumXp) : levels.OrderBy(level => level.DisplayOrder).ThenBy(level => level.MinimumXp)
        };
    }
}
