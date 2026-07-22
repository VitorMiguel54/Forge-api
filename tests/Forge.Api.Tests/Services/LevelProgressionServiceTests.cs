using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Application.Services;
using Forge.Domain.Constants;
using Forge.Domain.Entities;
using Forge.Infrastructure.Seeding;
using Xunit;

namespace Forge.Api.Tests.Services;

public class LevelProgressionServiceTests
{
    [Fact]
    public void LevelDefinitionCatalog_HasFiveOfficialLevels_WithStableProgression()
    {
        var levels = LevelDefinitionCatalog.All.OrderBy(level => level.DisplayOrder).ToArray();

        Assert.Equal(5, levels.Length);
        Assert.Equal(["Esquecido", "Forjado", "Guarda", "Sentinela", "Guardião da Forja"], levels.Select(level => level.Name).ToArray());
        Assert.Equal([0, 6000, 16000, 30000, 50000], levels.Select(level => level.MinimumXp).ToArray());
        Assert.Equal([OfficialLevelDefinitionIds.Forgotten, OfficialLevelDefinitionIds.Forged, OfficialLevelDefinitionIds.Guard, OfficialLevelDefinitionIds.Sentinel, OfficialLevelDefinitionIds.ForgeGuardian], levels.Select(level => level.Id).ToArray());
        Assert.Equal(["Incomum", "Comum", "Raro", "Épico", "Lendário"], levels.Select(level => level.RarityName).ToArray());
    }

    [Theory]
    [InlineData(0, 1, "Esquecido", "Forjado", 6000, 0, false)]
    [InlineData(5999, 1, "Esquecido", "Forjado", 1, 99.98, false)]
    [InlineData(6000, 2, "Forjado", "Guarda", 10000, 0, false)]
    [InlineData(15999, 2, "Forjado", "Guarda", 1, 99.99, false)]
    [InlineData(16000, 3, "Guarda", "Sentinela", 14000, 0, false)]
    [InlineData(29999, 3, "Guarda", "Sentinela", 1, 99.99, false)]
    [InlineData(30000, 4, "Sentinela", "Guardião da Forja", 20000, 0, false)]
    [InlineData(49999, 4, "Sentinela", "Guardião da Forja", 1, 100, false)]
    [InlineData(50000, 5, "Guardião da Forja", null, 0, 100, true)]
    [InlineData(75000, 5, "Guardião da Forja", null, 0, 100, true)]
    public async Task GetProgressionAsync_ReturnsExpectedLevelBands(int totalXp, int expectedLevel, string expectedCurrent, string? expectedNext, int expectedXpToNext, double expectedProgress, bool expectedMax)
    {
        var service = new LevelProgressionService(new FakeLevelDefinitionRepository(CreateOfficialLevelData()));

        var progression = await service.GetProgressionAsync(totalXp);

        Assert.Equal(expectedLevel, progression.NumericLevel);
        Assert.Equal(expectedCurrent, progression.CurrentLevel?.Name);
        Assert.Equal(expectedNext, progression.NextLevel?.Name);
        Assert.Equal(expectedXpToNext, progression.XpToNextLevel);
        Assert.Equal((decimal)expectedProgress, progression.ProgressPercentage);
        Assert.Equal(expectedMax, progression.IsMaximumLevel);
    }

    [Fact]
    public async Task GetProgressionAsync_UsesFallback_WhenCatalogIsEmpty()
    {
        var service = new LevelProgressionService(new FakeLevelDefinitionRepository([]));

        var progression = await service.GetProgressionAsync(1000);

        Assert.Equal(3, progression.NumericLevel);
        Assert.Equal(500, progression.XpToNextLevel);
        Assert.Null(progression.CurrentLevel);
    }

    private static IReadOnlyCollection<LevelDefinitionData> CreateOfficialLevelData()
    {
        return LevelDefinitionCatalog.All
            .Select(level => new LevelDefinitionData(
                level.Id,
                level.Name,
                level.Description,
                level.MinimumXp,
                level.DisplayOrder,
                Guid.NewGuid(),
                level.RarityName,
                "#FFFFFF",
                null,
                null,
                null,
                true,
                DateTime.UtcNow,
                DateTime.UtcNow))
            .ToArray();
    }

    private sealed class FakeLevelDefinitionRepository(IReadOnlyCollection<LevelDefinitionData> levels) : ILevelDefinitionRepository
    {
        public Task<IReadOnlyCollection<LevelDefinitionData>> GetActiveAsync(CancellationToken cancellationToken = default) => Task.FromResult(levels);
        public Task<LevelDefinitionData?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(levels.FirstOrDefault(level => level.Id == id));
        public Task<BackofficeLevelDefinitionListData> GetBackofficeAsync(BackofficeLevelDefinitionListQuery query, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<BackofficeLevelDefinitionData?> GetBackofficeByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<LevelDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsActiveInitialLevelAsync(Guid? ignoredId = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> RarityExistsAsync(Guid rarityId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> NameExistsAsync(string name, Guid? ignoredId = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> DisplayOrderExistsAsync(int displayOrder, Guid? ignoredId = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> MinimumXpExistsAsync(int minimumXp, Guid? ignoredId = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task AddAsync(LevelDefinition levelDefinition, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(LevelDefinition levelDefinition, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(LevelDefinition levelDefinition, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
