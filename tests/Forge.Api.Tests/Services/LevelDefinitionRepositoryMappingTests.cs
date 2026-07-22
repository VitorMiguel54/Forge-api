using Forge.Domain.Entities;
using Forge.Infrastructure.Repositories;
using Xunit;

namespace Forge.Api.Tests.Services;

public class LevelDefinitionRepositoryMappingTests
{
    [Fact]
    public void ToBackofficeData_ReturnsRarityData_WhenRarityIsLoaded()
    {
        var rarity = CreateRarity();
        var level = CreateLevel(rarity);

        var data = LevelDefinitionRepository.ToBackofficeData(level, currentUserCount: 7);

        Assert.Equal(rarity.Id, data.RarityId);
        Assert.Equal("Comum", data.RarityName);
        Assert.Equal("#22C55E", data.RarityPrimaryColor);
        Assert.Equal(7, data.CurrentUserCount);
    }

    [Fact]
    public void ToLevelDefinitionData_ReturnsRarityData_WhenRarityIsLoaded()
    {
        var rarity = CreateRarity();
        var level = CreateLevel(rarity);

        var data = LevelDefinitionRepository.ToLevelDefinitionData(level);

        Assert.Equal(rarity.Id, data.RarityId);
        Assert.Equal("Comum", data.RarityName);
        Assert.Equal("#22C55E", data.RarityPrimaryColor);
    }

    [Fact]
    public void ToBackofficeData_ThrowsClearError_WhenRarityWasNotLoaded()
    {
        var level = CreateLevelWithoutRarity();

        var exception = Assert.Throws<InvalidOperationException>(() => LevelDefinitionRepository.ToBackofficeData(level, currentUserCount: 0));

        Assert.Contains("references rarity", exception.Message);
        Assert.Contains("was not loaded or does not exist", exception.Message);
    }

    private static Rarity CreateRarity()
    {
        var now = DateTime.UtcNow;
        return new Rarity
        {
            Id = Guid.NewGuid(),
            Name = "Comum",
            PrimaryColor = "#22C55E",
            DisplayOrder = 2,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static LevelDefinition CreateLevel(Rarity rarity)
    {
        var now = DateTime.UtcNow;
        return new LevelDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Forjado",
            Description = "Nivel carregado com raridade.",
            MinimumXp = 6000,
            DisplayOrder = 2,
            RarityId = rarity.Id,
            Rarity = rarity,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static LevelDefinition CreateLevelWithoutRarity()
    {
        var now = DateTime.UtcNow;
        return new LevelDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Forjado",
            Description = "Nivel sem raridade carregada.",
            MinimumXp = 6000,
            DisplayOrder = 2,
            RarityId = Guid.NewGuid(),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
