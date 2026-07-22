using Forge.Domain.Entities;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Seeding;

public class RaritySeeder(ForgeDbContext dbContext)
{
    public async Task<int> SeedAsync(CancellationToken cancellationToken = default)
    {
        var existingRarities = await dbContext.Rarities
            .Select(rarity => new ExistingRarity(rarity.Id, rarity.Name))
            .ToArrayAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var missingRarities = CreateMissingOfficialRarities(existingRarities, now).ToArray();

        if (missingRarities.Length == 0)
        {
            return 0;
        }

        await dbContext.Rarities.AddRangeAsync(missingRarities, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return missingRarities.Length;
    }

    public static IReadOnlyCollection<Rarity> CreateMissingOfficialRarities(
        IEnumerable<ExistingRarity> existingRarities,
        DateTime utcNow)
    {
        var existing = existingRarities.ToArray();
        var existingIds = existing.Select(rarity => rarity.Id).ToHashSet();
        var existingNames = existing.Select(rarity => rarity.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return RarityCatalog.All
            .Where(rarity =>
                !existingIds.Contains(rarity.Id)
                && !existingNames.Contains(rarity.Name))
            .Select(rarity => ToEntity(rarity, utcNow))
            .ToArray();
    }

    private static Rarity ToEntity(RaritySeedDefinition rarity, DateTime utcNow)
    {
        return new Rarity
        {
            Id = rarity.Id,
            Name = rarity.Name,
            PrimaryColor = rarity.PrimaryColor,
            SecondaryColor = rarity.SecondaryColor,
            DisplayOrder = rarity.DisplayOrder,
            IsActive = true,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }
}

public sealed record ExistingRarity(Guid Id, string Name);
