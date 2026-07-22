using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Application.Services;
using Forge.Application.Validators.Exercise;
using Forge.Domain.Entities;
using MuscleGroupEnum = Forge.Domain.Enums.MuscleGroup;
using Xunit;

namespace Forge.Api.Tests.Services;

public class ExerciseServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOnlyActiveExercisesOrderedForCatalog()
    {
        var firstGroup = new MuscleGroup
        {
            Id = Guid.NewGuid(),
            Name = "First",
            DisplayName = "Primeiro",
            DisplayOrder = 1
        };
        var secondGroup = new MuscleGroup
        {
            Id = Guid.NewGuid(),
            Name = "Second",
            DisplayName = "Segundo",
            DisplayOrder = 2
        };
        var repository = new FakeExerciseRepository(
            CreateExercise("Remada", secondGroup, isActive: true, displayOrder: 1),
            CreateExercise("Inativo", firstGroup, isActive: false, displayOrder: 1),
            CreateExercise("Supino B", firstGroup, isActive: true, displayOrder: 2),
            CreateExercise("Supino A", firstGroup, isActive: true, displayOrder: 1));
        var service = new ExerciseService(
            repository,
            new FakeUserProfileRepository(),
            new FakeMuscleGroupRepository(firstGroup.Id, secondGroup.Id),
            new CreateExerciseRequestValidator(),
            new UpdateExerciseRequestValidator());

        var exercises = await service.GetAllAsync();

        Assert.Equal(["Supino A", "Supino B", "Remada"], exercises.Select(exercise => exercise.Name).ToArray());
    }

    private static Exercise CreateExercise(string name, MuscleGroup muscleGroup, bool isActive, int displayOrder)
    {
        return new Exercise
        {
            Id = Guid.NewGuid(),
            Name = name,
            MuscleGroup = MuscleGroupEnum.Chest,
            MuscleGroupId = muscleGroup.Id,
            MuscleGroupEntity = muscleGroup,
            IsCustom = false,
            IsActive = isActive,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private sealed class FakeExerciseRepository(params Exercise[] exercises) : IExerciseRepository
    {
        public Task<IReadOnlyCollection<Exercise>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Exercise>>(exercises);
        }

        public Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<BackofficeExerciseListData> GetBackofficeAsync(
            BackofficeExerciseListQuery query,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<BackofficeExerciseData?> GetBackofficeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> NameExistsAsync(string name, Guid? ignoredId = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsInUseAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task AddAsync(Exercise exercise, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Exercise exercise, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Exercise exercise, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class FakeUserProfileRepository : IUserProfileRepository
    {
        public Task<IReadOnlyCollection<UserProfile>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> EmailExistsAsync(string email, Guid? ignoredUserProfileId = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> HasCustomExercisesAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(UserProfile userProfile, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(UserProfile userProfile, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeMuscleGroupRepository(params Guid[] ids) : IMuscleGroupRepository
    {
        public Task<IReadOnlyCollection<MuscleGroup>> GetActiveAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<BackofficeMuscleGroupData>> GetBackofficeAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<BackofficeMuscleGroupData?> GetBackofficeByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<MuscleGroup?> GetByIdIncludingInactiveAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(ids.Contains(id));
        public Task<bool> NameExistsAsync(string name, Guid? ignoredId = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> HasExercisesAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task AddAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
