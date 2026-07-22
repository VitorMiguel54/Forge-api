using Forge.Application.DTOs.Backoffice.MuscleGroups;
using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Application.Services;
using Forge.Application.Validators.MuscleGroups;
using Forge.Domain.Entities;
using Xunit;

namespace Forge.Api.Tests.Services;

public class BackofficeMuscleGroupServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsAdministrativeListWithExerciseCount()
    {
        var groupId = Guid.NewGuid();
        var service = CreateService(new FakeMuscleGroupRepository(
            new Dictionary<Guid, int> { [groupId] = 3 },
            CreateMuscleGroup(groupId, "chest", "Peito")));

        var response = await service.GetAllAsync();

        var muscleGroup = Assert.Single(response);
        Assert.Equal(groupId, muscleGroup.Id);
        Assert.Equal("chest", muscleGroup.Name);
        Assert.Equal("Peito", muscleGroup.DisplayName);
        Assert.True(muscleGroup.IsActive);
        Assert.Equal(3, muscleGroup.ExerciseCount);
    }

    [Fact]
    public async Task CreateAsync_CreatesActiveMuscleGroupWithNormalizedName()
    {
        var repository = new FakeMuscleGroupRepository();
        var service = CreateService(repository);

        var response = await service.CreateAsync(new CreateBackofficeMuscleGroupRequest(
            "  UpperBody  ",
            "Superiores",
            " dumbbell ",
            10));

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("upperbody", response.Name);
        Assert.Equal("Superiores", response.DisplayName);
        Assert.Equal("dumbbell", response.Icon);
        Assert.True(response.IsActive);
        Assert.Equal(0, response.ExerciseCount);
        Assert.Contains(repository.MuscleGroups, muscleGroup => muscleGroup.Id == response.Id);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenNameAlreadyExists()
    {
        var service = CreateService(new FakeMuscleGroupRepository(
            CreateMuscleGroup(Guid.NewGuid(), "chest", "Peito")));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateBackofficeMuscleGroupRequest(
                " Chest ",
                "Peito",
                null,
                10)));

        Assert.Equal("Muscle group name already exists.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingMuscleGroup()
    {
        var groupId = Guid.NewGuid();
        var service = CreateService(new FakeMuscleGroupRepository(
            CreateMuscleGroup(groupId, "chest", "Peito")));

        var response = await service.UpdateAsync(
            groupId,
            new UpdateBackofficeMuscleGroupRequest("Push", "Empurrar", "shield", 20));

        Assert.NotNull(response);
        Assert.Equal(groupId, response.Id);
        Assert.Equal("push", response.Name);
        Assert.Equal("Empurrar", response.DisplayName);
        Assert.Equal("shield", response.Icon);
        Assert.Equal(20, response.DisplayOrder);
    }

    [Fact]
    public async Task UpdateStatusAsync_DeactivatesExistingMuscleGroup()
    {
        var groupId = Guid.NewGuid();
        var service = CreateService(new FakeMuscleGroupRepository(
            CreateMuscleGroup(groupId, "chest", "Peito")));

        var response = await service.UpdateStatusAsync(
            groupId,
            new UpdateBackofficeMuscleGroupStatusRequest(IsActive: false));

        Assert.NotNull(response);
        Assert.False(response.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsConflict_WhenMuscleGroupHasExercises()
    {
        var groupId = Guid.NewGuid();
        var service = CreateService(new FakeMuscleGroupRepository(
            new Dictionary<Guid, int> { [groupId] = 1 },
            CreateMuscleGroup(groupId, "custom", "Custom")));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeleteAsync(groupId));

        Assert.Equal("Muscle group cannot be deleted because it is linked to exercises.", exception.Message);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenMuscleGroupDoesNotExist()
    {
        var service = CreateService(new FakeMuscleGroupRepository());

        var response = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(response);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsConflict_WhenMuscleGroupIsOfficial()
    {
        var officialGroup = MuscleGroupCatalog.CreateEntities(DateTime.UtcNow).First();
        var service = CreateService(new FakeMuscleGroupRepository(officialGroup));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeleteAsync(officialGroup.Id));

        Assert.Equal("Official muscle groups cannot be deleted; deactivate them instead.", exception.Message);
    }

    private static BackofficeMuscleGroupService CreateService(FakeMuscleGroupRepository repository)
    {
        return new BackofficeMuscleGroupService(
            repository,
            new CreateBackofficeMuscleGroupRequestValidator(),
            new UpdateBackofficeMuscleGroupRequestValidator());
    }

    private static MuscleGroup CreateMuscleGroup(Guid id, string name, string displayName, bool isActive = true)
    {
        return new MuscleGroup
        {
            Id = id,
            Name = name,
            DisplayName = displayName,
            Icon = "dumbbell",
            DisplayOrder = 10,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private sealed class FakeMuscleGroupRepository : IMuscleGroupRepository
    {
        private readonly Dictionary<Guid, int> exerciseCounts;

        public FakeMuscleGroupRepository(params MuscleGroup[] muscleGroups)
            : this(new Dictionary<Guid, int>(), muscleGroups)
        {
        }

        public FakeMuscleGroupRepository(
            Dictionary<Guid, int> exerciseCounts,
            params MuscleGroup[] muscleGroups)
        {
            this.exerciseCounts = exerciseCounts;
            MuscleGroups = muscleGroups.ToList();
        }

        public List<MuscleGroup> MuscleGroups { get; }

        public Task<IReadOnlyCollection<MuscleGroup>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<MuscleGroup>>(
                MuscleGroups.Where(muscleGroup => muscleGroup.IsActive).ToArray());
        }

        public Task<IReadOnlyCollection<BackofficeMuscleGroupData>> GetBackofficeAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<BackofficeMuscleGroupData>>(
                MuscleGroups
                    .OrderBy(muscleGroup => muscleGroup.DisplayOrder)
                    .ThenBy(muscleGroup => muscleGroup.DisplayName)
                    .Select(ToBackofficeData)
                    .ToArray());
        }

        public Task<BackofficeMuscleGroupData?> GetBackofficeByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(MuscleGroups
                .Where(muscleGroup => muscleGroup.Id == id)
                .Select(ToBackofficeData)
                .FirstOrDefault());
        }

        public Task<MuscleGroup?> GetByIdIncludingInactiveAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(MuscleGroups.FirstOrDefault(muscleGroup => muscleGroup.Id == id));
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(MuscleGroups.Any(muscleGroup => muscleGroup.Id == id && muscleGroup.IsActive));
        }

        public Task<bool> NameExistsAsync(
            string name,
            Guid? ignoredId = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(MuscleGroups.Any(muscleGroup =>
                string.Equals(muscleGroup.Name, name, StringComparison.OrdinalIgnoreCase)
                && (ignoredId == null || muscleGroup.Id != ignoredId)));
        }

        public Task<bool> HasExercisesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(exerciseCounts.GetValueOrDefault(id) > 0);
        }

        public Task AddAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default)
        {
            MuscleGroups.Add(muscleGroup);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default)
        {
            MuscleGroups.Remove(muscleGroup);
            return Task.CompletedTask;
        }

        private BackofficeMuscleGroupData ToBackofficeData(MuscleGroup muscleGroup)
        {
            return new BackofficeMuscleGroupData(
                muscleGroup.Id,
                muscleGroup.Name,
                muscleGroup.DisplayName,
                muscleGroup.Icon,
                muscleGroup.DisplayOrder,
                muscleGroup.IsActive,
                exerciseCounts.GetValueOrDefault(muscleGroup.Id));
        }
    }
}
