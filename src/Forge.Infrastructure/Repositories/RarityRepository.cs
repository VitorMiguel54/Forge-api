using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Domain.Entities;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Repositories;

public class RarityRepository(ForgeDbContext dbContext) : IRarityRepository
{
    public async Task<BackofficeRarityListData> GetBackofficeAsync(
        BackofficeRarityListQuery query,
        CancellationToken cancellationToken = default)
    {
        var rarities = dbContext.Rarities
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            rarities = rarities.Where(rarity => rarity.Name.Contains(search));
        }

        if (query.IsActive is not null)
        {
            rarities = rarities.Where(rarity => rarity.IsActive == query.IsActive);
        }

        rarities = ApplyOrdering(rarities, query.SortBy, query.SortDirection);

        var totalItems = await rarities.CountAsync(cancellationToken);
        var skip = (query.Page - 1) * query.PageSize;
        var items = await rarities
            .Skip(skip)
            .Take(query.PageSize)
            .Select(rarity => new BackofficeRarityData(
                rarity.Id,
                rarity.Name,
                rarity.PrimaryColor,
                rarity.SecondaryColor,
                rarity.DisplayOrder,
                rarity.IsActive,
                0,
                rarity.LevelDefinitions.Count,
                rarity.CreatedAt,
                rarity.UpdatedAt))
            .ToArrayAsync(cancellationToken);

        return new BackofficeRarityListData(items, totalItems);
    }

    public async Task<BackofficeRarityData?> GetBackofficeByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Rarities
            .AsNoTracking()
            .Where(rarity => rarity.Id == id)
            .Select(rarity => new BackofficeRarityData(
                rarity.Id,
                rarity.Name,
                rarity.PrimaryColor,
                rarity.SecondaryColor,
                rarity.DisplayOrder,
                rarity.IsActive,
                0,
                rarity.LevelDefinitions.Count,
                rarity.CreatedAt,
                rarity.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Rarity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Rarities
            .FirstOrDefaultAsync(rarity => rarity.Id == id, cancellationToken);
    }

    public async Task<bool> NameExistsAsync(
        string name,
        Guid? ignoredId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.ToLower();

        return await dbContext.Rarities
            .AsNoTracking()
            .AnyAsync(
                rarity =>
                    rarity.Name.ToLower() == normalizedName
                    && (ignoredId == null || rarity.Id != ignoredId),
                cancellationToken);
    }

    public Task<bool> HasAchievementsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public async Task<bool> HasLevelsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.LevelDefinitions.AnyAsync(level => level.RarityId == id, cancellationToken);
    }

    public async Task AddAsync(Rarity rarity, CancellationToken cancellationToken = default)
    {
        await dbContext.Rarities.AddAsync(rarity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Rarity rarity, CancellationToken cancellationToken = default)
    {
        dbContext.Rarities.Update(rarity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Rarity rarity, CancellationToken cancellationToken = default)
    {
        dbContext.Rarities.Remove(rarity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Rarity> ApplyOrdering(
        IQueryable<Rarity> rarities,
        string? sortBy,
        string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();

        return normalizedSortBy switch
        {
            "displayorder" or "order" => descending
                ? rarities.OrderByDescending(rarity => rarity.DisplayOrder).ThenBy(rarity => rarity.Name)
                : rarities.OrderBy(rarity => rarity.DisplayOrder).ThenBy(rarity => rarity.Name),
            "status" => descending
                ? rarities.OrderByDescending(rarity => rarity.IsActive).ThenBy(rarity => rarity.DisplayOrder).ThenBy(rarity => rarity.Name)
                : rarities.OrderBy(rarity => rarity.IsActive).ThenBy(rarity => rarity.DisplayOrder).ThenBy(rarity => rarity.Name),
            "achievementcount" or "levelcount" => descending
                ? rarities.OrderByDescending(rarity => rarity.Name)
                : rarities.OrderBy(rarity => rarity.Name),
            "createdat" => descending
                ? rarities.OrderByDescending(rarity => rarity.CreatedAt).ThenBy(rarity => rarity.Name)
                : rarities.OrderBy(rarity => rarity.CreatedAt).ThenBy(rarity => rarity.Name),
            _ => descending
                ? rarities.OrderByDescending(rarity => rarity.Name)
                : rarities.OrderBy(rarity => rarity.Name)
        };
    }
}
