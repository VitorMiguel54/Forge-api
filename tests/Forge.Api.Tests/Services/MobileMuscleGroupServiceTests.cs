using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Application.Services;
using Forge.Domain.Entities;
using Xunit;

namespace Forge.Api.Tests.Services;

public class MobileMuscleGroupServiceTests
{
    [Fact]
    public async Task GetAsync_ReturnsOnlyActiveMuscleGroups()
    {
        var activeGroupId = Guid.NewGuid();
        var inactiveGroupId = Guid.NewGuid();
        var service = new MobileMuscleGroupService(new FakeMuscleGroupRepository(
            CreateMuscleGroup(activeGroupId, "chest", "Peito", isActive: true),
            CreateMuscleGroup(inactiveGroupId, "archived", "Arquivado", isActive: false)));

        var response = await service.GetAsync();

        var muscleGroup = Assert.Single(response);
        Assert.Equal(activeGroupId, muscleGroup.Id);
        Assert.Equal("chest", muscleGroup.Name);
        Assert.Equal("Peito", muscleGroup.DisplayName);
    }

    private static MuscleGroup CreateMuscleGroup(Guid id, string name, string displayName, bool isActive)
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

    private sealed class FakeMuscleGroupRepository(params MuscleGroup[] muscleGroups) : IMuscleGroupRepository
    {
        public Task<IReadOnlyCollection<MuscleGroup>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<MuscleGroup>>(
                muscleGroups.Where(muscleGroup => muscleGroup.IsActive).ToArray());
        }

        public Task<IReadOnlyCollection<BackofficeMuscleGroupData>> GetBackofficeAsync(
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<BackofficeMuscleGroupData?> GetBackofficeByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<MuscleGroup?> GetByIdIncludingInactiveAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> NameExistsAsync(
            string name,
            Guid? ignoredId = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasExercisesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task AddAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
