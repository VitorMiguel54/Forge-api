using Forge.Application.DTOs.Backoffice.Rarities;
using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Application.Services;
using Forge.Application.Validators.Rarities;
using Forge.Domain.Entities;
using Xunit;

namespace Forge.Api.Tests.Services;

public class BackofficeRarityServiceTests
{
    [Fact]
    public async Task GetAsync_ReturnsPagedAdministrativeList_WithFilters()
    {
        var repository = new FakeRarityRepository(
            CreateRarity(Guid.NewGuid(), "Comum", "#9CA3AF", displayOrder: 1, isActive: true),
            CreateRarity(Guid.NewGuid(), "Rara", "#3B82F6", displayOrder: 2, isActive: true),
            CreateRarity(Guid.NewGuid(), "Rara inativa", "#1D4ED8", displayOrder: 3, isActive: false));
        var service = CreateService(repository);

        var response = await service.GetAsync(
            search: "rara",
            isActive: true,
            sortBy: "displayOrder",
            sortDirection: "desc",
            page: 1,
            pageSize: 1);

        Assert.Equal(1, response.TotalItems);
        Assert.Equal(1, response.TotalPages);
        var rarity = Assert.Single(response.Items);
        Assert.Equal("Rara", rarity.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDetail_WhenRarityExists()
    {
        var id = Guid.NewGuid();
        var repository = new FakeRarityRepository(CreateRarity(id, "Comum", "#9CA3AF"));
        var service = CreateService(repository);

        var response = await service.GetByIdAsync(id);

        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal(0, response.AchievementCount);
        Assert.Equal(0, response.LevelCount);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenRarityDoesNotExist()
    {
        var service = CreateService(new FakeRarityRepository());

        var response = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(response);
    }

    [Fact]
    public async Task CreateAsync_CreatesValidRarity()
    {
        var repository = new FakeRarityRepository();
        var service = CreateService(repository);

        var response = await service.CreateAsync(new CreateBackofficeRarityRequest(
            " ?pica ",
            " #7c3aed ",
            " #a855f7 ",
            DisplayOrder: 3,
            IsActive: true));

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("?pica", response.Name);
        Assert.Equal("#7C3AED", response.PrimaryColor);
        Assert.Equal("#A855F7", response.SecondaryColor);
        Assert.Equal(3, response.DisplayOrder);
        Assert.True(response.IsActive);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenNameAlreadyExists()
    {
        var repository = new FakeRarityRepository(CreateRarity(Guid.NewGuid(), "Rara", "#3B82F6"));
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateBackofficeRarityRequest("rara", "#2563EB", null, 2)));

        Assert.Equal("Rarity name already exists.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsValidationError_WhenDisplayOrderIsInvalid()
    {
        var service = CreateService(new FakeRarityRepository());

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(new CreateBackofficeRarityRequest("Rara", "#2563EB", null, 0)));

        Assert.Contains("Rarity display order must be greater than zero.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsValidationError_WhenColorIsInvalid()
    {
        var service = CreateService(new FakeRarityRepository());

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(new CreateBackofficeRarityRequest("Rara", "blue", null, 1)));

        Assert.Contains("Rarity primary color must be a valid hexadecimal color.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingRarity()
    {
        var id = Guid.NewGuid();
        var service = CreateService(new FakeRarityRepository(CreateRarity(id, "Rara", "#3B82F6")));

        var response = await service.UpdateAsync(
            id,
            new UpdateBackofficeRarityRequest("Rara atualizada", "#2563EB", null, 4));

        Assert.NotNull(response);
        Assert.Equal("Rara atualizada", response.Name);
        Assert.Equal("#2563EB", response.PrimaryColor);
        Assert.Null(response.SecondaryColor);
        Assert.Equal(4, response.DisplayOrder);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsConflict_WhenNameBelongsToAnotherRarity()
    {
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var service = CreateService(new FakeRarityRepository(
            CreateRarity(firstId, "Comum", "#9CA3AF"),
            CreateRarity(secondId, "Rara", "#3B82F6")));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateAsync(secondId, new UpdateBackofficeRarityRequest("Comum", "#2563EB", null, 2)));

        Assert.Equal("Rarity name already exists.", exception.Message);
    }

    [Fact]
    public async Task UpdateStatusAsync_DeactivatesRarity()
    {
        var id = Guid.NewGuid();
        var service = CreateService(new FakeRarityRepository(CreateRarity(id, "Rara", "#3B82F6")));

        var response = await service.UpdateStatusAsync(id, new UpdateBackofficeRarityStatusRequest(IsActive: false));

        Assert.NotNull(response);
        Assert.False(response.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_RemovesRarity_WhenItHasNoLinks()
    {
        var id = Guid.NewGuid();
        var repository = new FakeRarityRepository(CreateRarity(id, "Rara", "#3B82F6"));
        var service = CreateService(repository);

        var deleted = await service.DeleteAsync(id);

        Assert.True(deleted);
        Assert.Empty(repository.Rarities);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsConflict_WhenRarityHasAchievements()
    {
        var id = Guid.NewGuid();
        var repository = new FakeRarityRepository(CreateRarity(id, "Rara", "#3B82F6"))
        {
            RarityIdsWithAchievements = { id }
        };
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(id));

        Assert.Equal("Rarity cannot be deleted because it is used by achievements. Deactivate it instead.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsConflict_WhenRarityHasLevels()
    {
        var id = Guid.NewGuid();
        var repository = new FakeRarityRepository(CreateRarity(id, "Rara", "#3B82F6"))
        {
            RarityIdsWithLevels = { id }
        };
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(id));

        Assert.Equal("Rarity cannot be deleted because it is used by levels. Deactivate it instead.", exception.Message);
    }

    private static BackofficeRarityService CreateService(FakeRarityRepository repository)
    {
        return new BackofficeRarityService(
            repository,
            new CreateBackofficeRarityRequestValidator(),
            new UpdateBackofficeRarityRequestValidator());
    }

    private static Rarity CreateRarity(
        Guid id,
        string name,
        string primaryColor,
        string? secondaryColor = null,
        int displayOrder = 1,
        bool isActive = true)
    {
        var now = DateTime.UtcNow;
        return new Rarity
        {
            Id = id,
            Name = name,
            PrimaryColor = primaryColor,
            SecondaryColor = secondaryColor,
            DisplayOrder = displayOrder,
            IsActive = isActive,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private sealed class FakeRarityRepository(params Rarity[] rarities) : IRarityRepository
    {
        public List<Rarity> Rarities { get; } = rarities.ToList();
        public HashSet<Guid> RarityIdsWithAchievements { get; } = [];
        public HashSet<Guid> RarityIdsWithLevels { get; } = [];

        public Task<BackofficeRarityListData> GetBackofficeAsync(
            BackofficeRarityListQuery query,
            CancellationToken cancellationToken = default)
        {
            var rarities = Rarities.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                rarities = rarities.Where(rarity => rarity.Name.Contains(query.Search.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (query.IsActive is not null)
            {
                rarities = rarities.Where(rarity => rarity.IsActive == query.IsActive);
            }

            var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            rarities = query.SortBy?.Trim().ToLowerInvariant() switch
            {
                "displayorder" or "order" => descending
                    ? rarities.OrderByDescending(rarity => rarity.DisplayOrder).ThenBy(rarity => rarity.Name)
                    : rarities.OrderBy(rarity => rarity.DisplayOrder).ThenBy(rarity => rarity.Name),
                _ => descending
                    ? rarities.OrderByDescending(rarity => rarity.Name)
                    : rarities.OrderBy(rarity => rarity.Name)
            };

            var filtered = rarities.ToArray();
            var pageItems = filtered
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(ToData)
                .ToArray();

            return Task.FromResult(new BackofficeRarityListData(pageItems, filtered.Length));
        }

        public Task<BackofficeRarityData?> GetBackofficeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Rarities.Where(rarity => rarity.Id == id).Select(ToData).FirstOrDefault());
        }

        public Task<Rarity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Rarities.FirstOrDefault(rarity => rarity.Id == id));
        }

        public Task<bool> NameExistsAsync(string name, Guid? ignoredId = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Rarities.Any(rarity =>
                string.Equals(rarity.Name, name, StringComparison.OrdinalIgnoreCase)
                && (ignoredId == null || rarity.Id != ignoredId)));
        }

        public Task<bool> HasAchievementsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RarityIdsWithAchievements.Contains(id));
        }

        public Task<bool> HasLevelsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RarityIdsWithLevels.Contains(id));
        }

        public Task AddAsync(Rarity rarity, CancellationToken cancellationToken = default)
        {
            Rarities.Add(rarity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Rarity rarity, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Rarity rarity, CancellationToken cancellationToken = default)
        {
            Rarities.Remove(rarity);
            return Task.CompletedTask;
        }

        private static BackofficeRarityData ToData(Rarity rarity)
        {
            return new BackofficeRarityData(
                rarity.Id,
                rarity.Name,
                rarity.PrimaryColor,
                rarity.SecondaryColor,
                rarity.DisplayOrder,
                rarity.IsActive,
                0,
                0,
                rarity.CreatedAt,
                rarity.UpdatedAt);
        }
    }
}
