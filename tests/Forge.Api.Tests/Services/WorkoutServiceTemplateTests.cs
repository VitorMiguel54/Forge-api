using Forge.Application.DTOs.Achievement;
using Forge.Application.DTOs.Workout;
using Forge.Application.DTOs.XP;
using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Application.Services;
using Forge.Application.Validators;
using Forge.Domain.Entities;
using Forge.Domain.Enums;
using Xunit;

namespace Forge.Api.Tests.Services;

public class WorkoutServiceTemplateTests
{
    [Fact]
    public async Task StartAsync_CreatesExecutionFromDraftTemplate_AndPreservesTemplate()
    {
        var template = CreateTemplate();
        var repository = new FakeWorkoutRepository(template);
        var service = CreateService(repository);

        var response = await service.StartAsync(template.Id);

        Assert.NotNull(response);
        Assert.NotEqual(template.Id, response.Id);
        Assert.Equal(template.Id, response.TemplateWorkoutId);
        Assert.Equal("InProgress", response.Status);
        Assert.Equal(WorkoutStatus.Draft, template.Status);
        Assert.False(template.IsArchived);

        var execution = Assert.Single(repository.AddedWorkouts);
        Assert.Equal(template.Id, execution.TemplateWorkoutId);
        Assert.Equal(WorkoutStatus.InProgress, execution.Status);
        Assert.Equal(template.WorkoutExercises.Count, execution.WorkoutExercises.Count);
        Assert.All(execution.WorkoutExercises, workoutExercise => Assert.Equal(execution.Id, workoutExercise.WorkoutId));
    }

    [Fact]
    public async Task DeleteAsync_ArchivesDraftTemplate_WithoutPhysicalDelete()
    {
        var template = CreateTemplate();
        var repository = new FakeWorkoutRepository(template);
        var service = CreateService(repository);

        var deleted = await service.DeleteAsync(template.Id);

        Assert.True(deleted);
        Assert.True(template.IsArchived);
        Assert.Equal(1, repository.UpdateCount);
        Assert.Equal(0, repository.DeleteCount);
    }

    [Theory]
    [InlineData(WorkoutStatus.InProgress, "Workout in progress cannot be deleted.")]
    [InlineData(WorkoutStatus.Completed, "Completed workout history cannot be deleted.")]
    public async Task DeleteAsync_ThrowsConflict_ForExecutionOrHistory(WorkoutStatus status, string expectedMessage)
    {
        var workout = CreateTemplate(status);
        var service = CreateService(new FakeWorkoutRepository(workout));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(workout.Id));

        Assert.Equal(expectedMessage, exception.Message);
        Assert.False(workout.IsArchived);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsConflict_WhenWorkoutIsArchived()
    {
        var template = CreateTemplate();
        template.IsArchived = true;
        var service = CreateService(new FakeWorkoutRepository(template));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateAsync(template.Id, new UpdateWorkoutRequest(
                template.UserProfileId,
                "Treino editado",
                DateTime.UtcNow,
                null,
                null)));

        Assert.Equal("Archived workout cannot be edited.", exception.Message);
    }

    [Fact]
    public async Task CreatePlanAsync_CreatesWorkoutWithExercises_InSequentialOrder()
    {
        var firstExercise = CreateExercise();
        var secondExercise = CreateExercise();
        var repository = new FakeWorkoutRepository();
        var service = CreateService(repository, new FakeExerciseRepository(firstExercise, secondExercise));

        var response = await service.CreatePlanAsync(new CreateWorkoutPlanRequest(
            Guid.NewGuid(),
            "Treino completo",
            DateTime.UtcNow,
            null,
            null,
            [
                new CreateWorkoutPlanExerciseRequest(firstExercise.Id, 1, null),
                new CreateWorkoutPlanExerciseRequest(secondExercise.Id, 2, "Ajuste")
            ]));

        var workout = Assert.Single(repository.AddedWorkouts);
        Assert.Equal(response.Id, workout.Id);
        Assert.Equal(2, workout.WorkoutExercises.Count);
        Assert.Equal([1, 2], workout.WorkoutExercises.OrderBy(exercise => exercise.Order).Select(exercise => exercise.Order).ToArray());
        Assert.Equal([firstExercise.Id, secondExercise.Id], workout.WorkoutExercises.OrderBy(exercise => exercise.Order).Select(exercise => exercise.ExerciseId).ToArray());
        Assert.Equal(2, workout.WorkoutMuscleGroups.Count);
    }

    [Fact]
    public async Task CreatePlanAsync_RejectsDuplicatedExercise_BeforePersistingWorkout()
    {
        var exercise = CreateExercise();
        var repository = new FakeWorkoutRepository();
        var service = CreateService(repository, new FakeExerciseRepository(exercise));

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreatePlanAsync(new CreateWorkoutPlanRequest(
                Guid.NewGuid(),
                "Treino duplicado",
                DateTime.UtcNow,
                null,
                null,
                [
                    new CreateWorkoutPlanExerciseRequest(exercise.Id, 1, null),
                    new CreateWorkoutPlanExerciseRequest(exercise.Id, 2, null)
                ])));

        Assert.Contains("duplicated", exception.Message);
        Assert.Empty(repository.AddedWorkouts);
    }

    [Fact]
    public async Task CreatePlanAsync_RollsBack_WhenPersistenceFails()
    {
        var exercise = CreateExercise();
        var repository = new FakeWorkoutRepository { ThrowOnAdd = true };
        var transaction = new SnapshotTransaction(repository.RestoreSnapshot);
        var service = CreateService(repository, new FakeExerciseRepository(exercise), transaction);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreatePlanAsync(new CreateWorkoutPlanRequest(
                Guid.NewGuid(),
                "Treino rollback",
                DateTime.UtcNow,
                null,
                null,
                [new CreateWorkoutPlanExerciseRequest(exercise.Id, 1, null)])));

        Assert.Empty(repository.AddedWorkouts);
    }

    [Fact]
    public async Task ReorderAsync_NormalizesOrder_AndPersistsSavedWorkouts()
    {
        var userProfileId = Guid.NewGuid();
        var firstWorkout = CreateTemplate();
        var secondWorkout = CreateTemplate();
        firstWorkout.UserProfileId = userProfileId;
        firstWorkout.DisplayOrder = 1;
        secondWorkout.UserProfileId = userProfileId;
        secondWorkout.DisplayOrder = 2;
        var repository = new FakeWorkoutRepository(firstWorkout, secondWorkout);
        var service = CreateService(repository);

        await service.ReorderAsync(new ReorderWorkoutsRequest(
            userProfileId,
            [
                new ReorderWorkoutItemRequest(secondWorkout.Id, 10),
                new ReorderWorkoutItemRequest(firstWorkout.Id, 20)
            ]));

        Assert.Equal(2, repository.UpdateCount);
        Assert.Equal(2, firstWorkout.DisplayOrder);
        Assert.Equal(1, secondWorkout.DisplayOrder);
    }

    [Fact]
    public async Task ReorderAsync_RejectsDuplicatedWorkoutIds()
    {
        var userProfileId = Guid.NewGuid();
        var workout = CreateTemplate();
        workout.UserProfileId = userProfileId;
        var repository = new FakeWorkoutRepository(workout);
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ReorderAsync(new ReorderWorkoutsRequest(
                userProfileId,
                [
                    new ReorderWorkoutItemRequest(workout.Id, 1),
                    new ReorderWorkoutItemRequest(workout.Id, 2)
                ])));

        Assert.Contains("duplicated", exception.Message);
        Assert.Equal(0, repository.UpdateCount);
    }

    private static WorkoutService CreateService(
        IWorkoutRepository workoutRepository,
        IExerciseRepository? exerciseRepository = null,
        IApplicationTransaction? applicationTransaction = null)
    {
        return new WorkoutService(
            workoutRepository,
            new FakeUserProfileRepository(),
            exerciseRepository ?? new FakeExerciseRepository(),
            new FakeValidator<CreateWorkoutRequest>(),
            new FakeValidator<UpdateWorkoutRequest>(),
            new FakeXpService(),
            new FakeAchievementService(),
            applicationTransaction ?? new PassthroughTransaction());
    }

    private static Workout CreateTemplate(WorkoutStatus status = WorkoutStatus.Draft)
    {
        var now = DateTime.UtcNow;
        var workout = new Workout
        {
            Id = Guid.NewGuid(),
            UserProfileId = Guid.NewGuid(),
            Name = "Treino salvo",
            WorkoutDate = now,
            Status = status,
            CreatedAt = now,
            UpdatedAt = now
        };

        workout.WorkoutExercises.Add(new WorkoutExercise
        {
            Id = Guid.NewGuid(),
            WorkoutId = workout.Id,
            ExerciseId = Guid.NewGuid(),
            Order = 2,
            CreatedAt = now,
            UpdatedAt = now
        });
        workout.WorkoutExercises.Add(new WorkoutExercise
        {
            Id = Guid.NewGuid(),
            WorkoutId = workout.Id,
            ExerciseId = Guid.NewGuid(),
            Order = 1,
            CreatedAt = now,
            UpdatedAt = now
        });

        return workout;
    }

    private static Exercise CreateExercise()
    {
        return new Exercise
        {
            Id = Guid.NewGuid(),
            Name = Guid.NewGuid().ToString("N"),
            MuscleGroupId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private sealed class FakeWorkoutRepository(params Workout[] workouts) : IWorkoutRepository
    {
        private readonly List<Workout> workouts = workouts.ToList();

        public List<Workout> AddedWorkouts { get; } = [];
        public bool ThrowOnAdd { get; init; }
        public int UpdateCount { get; private set; }
        public int DeleteCount { get; private set; }

        public Task<IReadOnlyCollection<Workout>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Workout>>(workouts.ToArray());
        }

        public Task<Workout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(workouts.FirstOrDefault(workout => workout.Id == id));
        }

        public Task<Workout?> GetByIdWithExercisesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return GetByIdAsync(id, cancellationToken);
        }

        public Task<Workout?> GetByIdWithExercisesAndSetsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return GetByIdAsync(id, cancellationToken);
        }

        public Task AddAsync(Workout workout, CancellationToken cancellationToken = default)
        {
            if (ThrowOnAdd)
            {
                AddedWorkouts.Add(workout);
                workouts.Add(workout);
                throw new InvalidOperationException("Persistence failed.");
            }

            workouts.Add(workout);
            AddedWorkouts.Add(workout);
            return Task.CompletedTask;
        }

        public Task<int> GetNextDisplayOrderAsync(Guid userProfileId, CancellationToken cancellationToken = default)
        {
            var nextOrder = workouts
                .Where(workout => workout.UserProfileId == userProfileId)
                .Select(workout => workout.DisplayOrder)
                .DefaultIfEmpty(0)
                .Max() + 1;

            return Task.FromResult(nextOrder);
        }

        public Task<IReadOnlyCollection<Workout>> GetDraftsByUserProfileAsync(
            Guid userProfileId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Workout>>(
                workouts
                    .Where(workout => workout.UserProfileId == userProfileId && workout.Status == WorkoutStatus.Draft)
                    .ToArray());
        }

        public Task UpdateRangeAsync(IReadOnlyCollection<Workout> workoutsToUpdate, CancellationToken cancellationToken = default)
        {
            UpdateCount += workoutsToUpdate.Count;
            return Task.CompletedTask;
        }

        public void RestoreSnapshot()
        {
            foreach (var workout in AddedWorkouts)
            {
                workouts.Remove(workout);
            }

            AddedWorkouts.Clear();
        }

        public Task UpdateAsync(Workout workout, CancellationToken cancellationToken = default)
        {
            UpdateCount++;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Workout workout, CancellationToken cancellationToken = default)
        {
            DeleteCount++;
            workouts.Remove(workout);
            return Task.CompletedTask;
        }

        public Task<int> CountCompletedByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> CountCompletedByUserProfileSinceAsync(Guid userProfileId, DateTime utcStartDate, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Workout?> GetLatestCompletedByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Workout?> GetLatestInProgressByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<MobileWorkoutSummaryData>> GetMobileSummariesByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<MobileHistoryWorkoutData>> GetMobileHistoryWorkoutsByUserProfileAsync(Guid userProfileId, int skip, int take, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<decimal> SumCompletedVolumeByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<decimal> SumCompletedVolumeByUserProfileSinceAsync(Guid userProfileId, DateTime utcStartDate, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> SumCompletedExerciseCountByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> SumCompletedDurationMinutesByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeUserProfileRepository : IUserProfileRepository
    {
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<IReadOnlyCollection<UserProfile>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> EmailExistsAsync(string email, Guid? ignoredUserProfileId = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> HasCustomExercisesAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(UserProfile userProfile, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(UserProfile userProfile, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeExerciseRepository(params Exercise[] exercises) : IExerciseRepository
    {
        public Task<IReadOnlyCollection<Exercise>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Exercise>>(exercises);

        public Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(exercises.FirstOrDefault(exercise => exercise.Id == id));

        public Task<BackofficeExerciseListData> GetBackofficeAsync(BackofficeExerciseListQuery query, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<BackofficeExerciseData?> GetBackofficeByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> NameExistsAsync(string name, Guid? ignoredId = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> IsInUseAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task AddAsync(Exercise exercise, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Exercise exercise, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(Exercise exercise, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeValidator<TRequest> : IValidator<TRequest>
    {
        public ValidationResult Validate(TRequest request)
        {
            return ValidationResult.Success();
        }
    }

    private sealed class PassthroughTransaction : IApplicationTransaction
    {
        public Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            return operation(cancellationToken);
        }
    }

    private sealed class SnapshotTransaction(Action restore) : IApplicationTransaction
    {
        public async Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await operation(cancellationToken);
            }
            catch
            {
                restore();
                throw;
            }
        }
    }

    private sealed class FakeXpService : IXpService
    {
        public Task<XpSummaryResponse?> GetSummaryByUserAsync(Guid userProfileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<XpTransactionResponse>> GetTransactionsByUserAsync(Guid userProfileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<XpTransactionResponse?> AwardWorkoutCompletedAsync(Workout workout, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<XpTransactionResponse?> AwardAchievementUnlockedAsync(Guid userProfileId, Achievement achievement, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeAchievementService : IAchievementService
    {
        public Task<IReadOnlyCollection<AchievementResponse>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<UserAchievementResponse>> GetUnlockedByUserAsync(Guid userProfileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<UserAchievementResponse>> EvaluateWorkoutCompletedAsync(Guid userProfileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
