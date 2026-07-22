using Forge.Domain.Entities;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Seeding;

public class LevelDefinitionSeeder(ForgeDbContext dbContext)
{
    public async Task<int> SeedAsync(CancellationToken cancellationToken = default)
    {
        var existingLevels = await dbContext.LevelDefinitions.AsNoTracking().Select(level => new { level.Id, level.Name }).ToArrayAsync(cancellationToken);
        var existingIds = existingLevels.Select(level => level.Id).ToHashSet();
        var existingNames = existingLevels.Select(level => level.Name.ToLower()).ToHashSet();
        var rarities = await dbContext.Rarities.AsNoTracking().ToArrayAsync(cancellationToken);
        var rarityByName = rarities.ToDictionary(rarity => rarity.Name.ToLower(), rarity => rarity.Id);
        var now = DateTime.UtcNow;
        var missingLevels = new List<LevelDefinition>();

        foreach (var definition in LevelDefinitionCatalog.All)
        {
            if (existingIds.Contains(definition.Id) || existingNames.Contains(definition.Name.ToLower())) continue;
            if (!rarityByName.TryGetValue(definition.RarityName.ToLower(), out var rarityId))
            {
                throw new InvalidOperationException($"Official level '{definition.Name}' requires rarity '{definition.RarityName}', but it was not found. Create the official rarity before seeding levels.");
            }
            missingLevels.Add(new LevelDefinition
            {
                Id = definition.Id,
                Name = definition.Name,
                Description = definition.Description,
                MinimumXp = definition.MinimumXp,
                DisplayOrder = definition.DisplayOrder,
                RarityId = rarityId,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (missingLevels.Count == 0) return 0;
        await dbContext.LevelDefinitions.AddRangeAsync(missingLevels, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return missingLevels.Count;
    }
}
