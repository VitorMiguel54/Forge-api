using Forge.Domain.Constants;
using Forge.Infrastructure.Seeding;
using Xunit;

namespace Forge.Api.Tests.Services;

public class RarityCatalogTests
{
    [Fact]
    public void RarityCatalog_HasOfficialRarities_WithStableIds()
    {
        var rarities = RarityCatalog.All.OrderBy(rarity => rarity.DisplayOrder).ToArray();

        Assert.Equal(5, rarities.Length);
        Assert.Equal(["Incomum", "Comum", "Raro", "Épico", "Lendário"], rarities.Select(rarity => rarity.Name).ToArray());
        Assert.Equal([OfficialRarityIds.Uncommon, OfficialRarityIds.Common, OfficialRarityIds.Rare, OfficialRarityIds.Epic, OfficialRarityIds.Legendary], rarities.Select(rarity => rarity.Id).ToArray());
        Assert.Equal([1, 2, 3, 4, 5], rarities.Select(rarity => rarity.DisplayOrder).ToArray());
        Assert.Equal(5, OfficialRarityIds.All.Count);
    }

    [Fact]
    public void RaritySeeder_CreateMissingOfficialRarities_IsIdempotent()
    {
        var existing = RarityCatalog.All.Select(rarity => new ExistingRarity(rarity.Id, rarity.Name)).ToArray();

        var missing = RaritySeeder.CreateMissingOfficialRarities(existing, DateTime.UtcNow);

        Assert.Empty(missing);
    }

    [Fact]
    public void RaritySeeder_CreateMissingOfficialRarities_DoesNotDuplicateExistingAdministrativeName()
    {
        var existing = new[]
        {
            new ExistingRarity(Guid.NewGuid(), "Incomum"),
            new ExistingRarity(Guid.NewGuid(), "Comum")
        };

        var missing = RaritySeeder.CreateMissingOfficialRarities(existing, DateTime.UtcNow);

        Assert.DoesNotContain(missing, rarity => rarity.Name is "Incomum" or "Comum");
        Assert.Equal(3, missing.Count);
    }

    [Fact]
    public void LevelDefinitionCatalog_UsesOnlyOfficialRarityNames()
    {
        var officialRarityNames = RarityCatalog.All.Select(rarity => rarity.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.All(LevelDefinitionCatalog.All, level => Assert.Contains(level.RarityName, officialRarityNames));
    }
}
