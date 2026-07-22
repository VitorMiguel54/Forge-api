using Forge.Application.DTOs.Backoffice.Levels;
using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Application.Services;
using Forge.Application.Validators.Levels;
using Forge.Domain.Constants;
using Forge.Domain.Entities;
using Xunit;

namespace Forge.Api.Tests.Services;

public class BackofficeLevelDefinitionServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesValidLevelDefinition()
    {
        var repository = new FakeLevelDefinitionRepository();
        var service = CreateService(repository);

        var response = await service.CreateAsync(new CreateBackofficeLevelDefinitionRequest("Ascendente", "Nivel extra", 70000, 6, repository.DefaultRarityId));

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("Ascendente", response.Name);
        Assert.Equal(70000, response.MinimumXp);
        Assert.Equal(repository.DefaultRarityId, response.RarityId);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenNameAlreadyExists()
    {
        var repository = new FakeLevelDefinitionRepository(CreateLevel(Guid.NewGuid(), "Forjado", 6000, 2));
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new CreateBackofficeLevelDefinitionRequest("forjado", "Outro", 70000, 6, repository.DefaultRarityId)));

        Assert.Equal("Level name already exists.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenDisplayOrderAlreadyExists()
    {
        var repository = new FakeLevelDefinitionRepository(CreateLevel(Guid.NewGuid(), "Forjado", 6000, 2));
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new CreateBackofficeLevelDefinitionRequest("Outro", "Outro", 70000, 2, repository.DefaultRarityId)));

        Assert.Equal("Level display order already exists.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenMinimumXpAlreadyExists()
    {
        var repository = new FakeLevelDefinitionRepository(CreateLevel(Guid.NewGuid(), "Forjado", 6000, 2));
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new CreateBackofficeLevelDefinitionRequest("Outro", "Outro", 6000, 6, repository.DefaultRarityId)));

        Assert.Equal("Level minimum XP already exists.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenRarityDoesNotExist()
    {
        var service = CreateService(new FakeLevelDefinitionRepository());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new CreateBackofficeLevelDefinitionRequest("Outro", "Outro", 70000, 6, Guid.NewGuid())));

        Assert.Equal("Level rarity does not exist.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingLevelDefinition()
    {
        var id = Guid.NewGuid();
        var repository = new FakeLevelDefinitionRepository(CreateLevel(id, "Forjado", 6000, 2));
        var service = CreateService(repository);

        var response = await service.UpdateAsync(id, new UpdateBackofficeLevelDefinitionRequest("Forjado II", "Atualizado", 6500, 6, repository.DefaultRarityId));

        Assert.NotNull(response);
        Assert.Equal("Forjado II", response.Name);
        Assert.Equal(6500, response.MinimumXp);
        Assert.Equal(6, response.DisplayOrder);
    }

    [Fact]
    public async Task UpdateStatusAsync_DeactivatesLevelDefinition()
    {
        var id = Guid.NewGuid();
        var repository = new FakeLevelDefinitionRepository(CreateLevel(id, "Forjado", 6000, 2));
        var service = CreateService(repository);

        var response = await service.UpdateStatusAsync(id, new UpdateBackofficeLevelDefinitionStatusRequest(false));

        Assert.NotNull(response);
        Assert.False(response.IsActive);
    }

    [Fact]
    public async Task UpdateStatusAsync_BlocksDeactivationOfInitialLevel()
    {
        var id = Guid.NewGuid();
        var service = CreateService(new FakeLevelDefinitionRepository(CreateLevel(id, "Esquecido", 0, 1)));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateStatusAsync(id, new UpdateBackofficeLevelDefinitionStatusRequest(false)));

        Assert.Equal("The initial level cannot be deactivated.", exception.Message);
    }

    [Fact]
    public async Task UploadGuardianImageAsync_UpdatesGuardianImageUrl()
    {
        var id = Guid.NewGuid();
        var repository = new FakeLevelDefinitionRepository(CreateLevel(id, "Forjado", 6000, 2));
        var storage = new FakeAdminImageStorage();
        var service = CreateService(repository, storage);

        await using var stream = CreatePngStream();
        var response = await service.UploadGuardianImageAsync(id, stream, "guardian.png", "image/png", stream.Length);

        Assert.NotNull(response);
        Assert.Equal(nameof(LevelDefinition.GuardianImageUrl), response.FieldName);
        Assert.Equal("/uploads/backoffice/levels/" + id + "/guardian/guardian.png", response.Url);
        Assert.Equal(response.Url, repository.Levels.Single().GuardianImageUrl);
        Assert.Equal("levels/" + id + "/guardian/guardian.png", repository.Levels.Single().GuardianImageStorageKey);
    }

    [Fact]
    public async Task DeleteGuardianImageAsync_ClearsGuardianImageAndDeletesFile()
    {
        var id = Guid.NewGuid();
        var level = CreateLevel(id, "Forjado", 6000, 2);
        level.GuardianImageUrl = "/uploads/backoffice/old.png";
        level.GuardianImageStorageKey = "levels/old.png";
        var storage = new FakeAdminImageStorage();
        var service = CreateService(new FakeLevelDefinitionRepository(level), storage);

        var deleted = await service.DeleteGuardianImageAsync(id);

        Assert.True(deleted);
        Assert.Null(level.GuardianImageUrl);
        Assert.Null(level.GuardianImageStorageKey);
        Assert.Contains("levels/old.png", storage.DeletedKeys);
    }

    [Fact]
    public async Task DeleteAsync_BlocksOfficialLevel()
    {
        var id = OfficialLevelDefinitionIds.Forgotten;
        var service = CreateService(new FakeLevelDefinitionRepository(CreateLevel(id, "Esquecido", 0, 1)));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(id));

        Assert.Equal("Official levels cannot be deleted. Deactivate them instead.", exception.Message);
    }

    private static BackofficeLevelDefinitionService CreateService(FakeLevelDefinitionRepository repository, FakeAdminImageStorage? storage = null)
    {
        return new BackofficeLevelDefinitionService(repository, storage ?? new FakeAdminImageStorage(), new CreateBackofficeLevelDefinitionRequestValidator(), new UpdateBackofficeLevelDefinitionRequestValidator());
    }

    private static MemoryStream CreatePngStream()
    {
        return new MemoryStream([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00]);
    }

    private static LevelDefinition CreateLevel(Guid id, string name, int minimumXp, int displayOrder)
    {
        var now = DateTime.UtcNow;
        return new LevelDefinition { Id = id, Name = name, Description = "Descricao", MinimumXp = minimumXp, DisplayOrder = displayOrder, RarityId = FakeLevelDefinitionRepository.StaticDefaultRarityId, IsActive = true, CreatedAt = now, UpdatedAt = now };
    }

    private sealed class FakeLevelDefinitionRepository(params LevelDefinition[] levels) : ILevelDefinitionRepository
    {
        public static readonly Guid StaticDefaultRarityId = Guid.Parse("33333333-3333-4333-8333-333333333333");
        public Guid DefaultRarityId => StaticDefaultRarityId;
        public List<LevelDefinition> Levels { get; } = levels.ToList();

        public Task<IReadOnlyCollection<LevelDefinitionData>> GetActiveAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<LevelDefinitionData>>(Levels.Where(level => level.IsActive).Select(ToLevelData).ToArray());
        public Task<LevelDefinitionData?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Levels.Where(level => level.Id == id && level.IsActive).Select(ToLevelData).FirstOrDefault());
        public Task<BackofficeLevelDefinitionListData> GetBackofficeAsync(BackofficeLevelDefinitionListQuery query, CancellationToken cancellationToken = default) => Task.FromResult(new BackofficeLevelDefinitionListData(Levels.Select(ToBackofficeData).ToArray(), Levels.Count));
        public Task<BackofficeLevelDefinitionData?> GetBackofficeByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Levels.Where(level => level.Id == id).Select(ToBackofficeData).FirstOrDefault());
        public Task<LevelDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Levels.FirstOrDefault(level => level.Id == id));
        public Task<bool> ExistsActiveInitialLevelAsync(Guid? ignoredId = null, CancellationToken cancellationToken = default) => Task.FromResult(Levels.Any(level => level.IsActive && level.MinimumXp == 0 && (ignoredId == null || level.Id != ignoredId)));
        public Task<bool> RarityExistsAsync(Guid rarityId, CancellationToken cancellationToken = default) => Task.FromResult(rarityId == DefaultRarityId);
        public Task<bool> NameExistsAsync(string name, Guid? ignoredId = null, CancellationToken cancellationToken = default) => Task.FromResult(Levels.Any(level => string.Equals(level.Name, name, StringComparison.OrdinalIgnoreCase) && (ignoredId == null || level.Id != ignoredId)));
        public Task<bool> DisplayOrderExistsAsync(int displayOrder, Guid? ignoredId = null, CancellationToken cancellationToken = default) => Task.FromResult(Levels.Any(level => level.DisplayOrder == displayOrder && (ignoredId == null || level.Id != ignoredId)));
        public Task<bool> MinimumXpExistsAsync(int minimumXp, Guid? ignoredId = null, CancellationToken cancellationToken = default) => Task.FromResult(Levels.Any(level => level.MinimumXp == minimumXp && (ignoredId == null || level.Id != ignoredId)));
        public Task AddAsync(LevelDefinition levelDefinition, CancellationToken cancellationToken = default) { Levels.Add(levelDefinition); return Task.CompletedTask; }
        public Task UpdateAsync(LevelDefinition levelDefinition, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(LevelDefinition levelDefinition, CancellationToken cancellationToken = default) { Levels.Remove(levelDefinition); return Task.CompletedTask; }

        private static LevelDefinitionData ToLevelData(LevelDefinition level) => new(level.Id, level.Name, level.Description, level.MinimumXp, level.DisplayOrder, level.RarityId, "Comum", "#00FF00", null, level.BadgeImageUrl, level.GuardianImageUrl, level.IsActive, level.CreatedAt, level.UpdatedAt);
        private static BackofficeLevelDefinitionData ToBackofficeData(LevelDefinition level) => new(level.Id, level.Name, level.Description, level.MinimumXp, level.DisplayOrder, level.RarityId, "Comum", "#00FF00", null, level.BadgeImageUrl, level.GuardianImageUrl, level.IsActive, 0, level.CreatedAt, level.UpdatedAt);
    }

    private sealed class FakeAdminImageStorage : IAdminImageStorage
    {
        public List<string> DeletedKeys { get; } = [];

        public Task<StoredFileResult> UploadAsync(
            Stream stream,
            string fileName,
            string contentType,
            string folder,
            CancellationToken cancellationToken = default)
        {
            var storageKey = folder + "/" + fileName;
            return Task.FromResult(new StoredFileResult(
                storageKey,
                "/uploads/backoffice/" + storageKey,
                fileName,
                contentType,
                stream.Length));
        }

        public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            DeletedKeys.Add(storageKey);
            return Task.CompletedTask;
        }
    }
}
