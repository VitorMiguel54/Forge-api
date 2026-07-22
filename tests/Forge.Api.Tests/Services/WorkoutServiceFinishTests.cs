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

public class WorkoutServiceFinishTests
{
    [Fact]
    public async Task FinishAsync_CompletesWorkoutAndAwardsXpAndAchievements_WhenWorkoutCanBeFinished()
    {
        var workout = CreateFinishableWorkout();
        var workoutRepository = new FakeWorkoutRepository(workout);
        var xpService = new FakeXpService();
        var achievementService = new FakeAchievementService();
        var transaction = new FakeApplicationTransaction(
            () => Snapshot.Capture(workoutRepository, xpService, achievementService),
            snapshot => snapshot.Restore(workoutRepository, xpService, achievementService));
        var service = CreateService(workoutRepository, xpService, achievementService, transaction);

        var response = await service.FinishAsync(workout.Id);

        Assert.NotNull(response);
        Assert.Equal(WorkoutStatus.Completed.ToString(), response.Status);
        Assert.Equal(140m, response.TotalVolume);
        Assert.NotNull(response.StartedAt);
        Assert.NotNull(response.FinishedAt);
        Assert.Equal(1, workoutRepository.UpdateCount);
        Assert.Equal(1, xpService.WorkoutCompletedAwardCount);
        Assert.Equal(1, achievementService.EvaluateWorkoutCompletedCount);
        Assert.True(transaction.Committed);
        Assert.False(transaction.RolledBack);
    }

    [Fact]
    public async Task FinishAsync_ReturnsCurrentWorkoutWithoutEffects_WhenWorkoutIsAlreadyCompleted()
    {
        var workout = CreateFinishableWorkout(WorkoutStatus.Completed);
        workout.TotalVolume = 140m;
        workout.FinishedAt = DateTime.UtcNow;
        var workoutRepository = new FakeWorkoutRepository(workout);
        var xpService = new FakeXpService();
        var achievementService = new FakeAchievementService();
        var transaction = new FakeApplicationTransaction(
            () => Snapshot.Capture(workoutRepository, xpService, achievementService),
            snapshot => snapshot.Restore(workoutRepository, xpService, achievementService));
        var service = CreateService(workoutRepository, xpService, achievementService, transaction);

        var firstResponse = await service.FinishAsync(workout.Id);
        var secondResponse = await service.FinishAsync(workout.Id);

        Assert.NotNull(firstResponse);
        Assert.NotNull(secondResponse);
        Assert.Equal(WorkoutStatus.Completed.ToString(), secondResponse.Status);
        Assert.Equal(0, workoutRepository.UpdateCount);
        Assert.Equal(0, xpService.WorkoutCompletedAwardCount);
        Assert.Equal(0, achievementService.EvaluateWorkoutCompletedCount);
        Assert.Equal(0, transaction.ExecutionCount);
    }

    [Fact]
    public async Task FinishAsync_RollsBackWorkoutAndXp_WhenAchievementEvaluationFails()
    {
        var workout = CreateFinishableWorkout();
        var workoutRepository = new FakeWorkoutRepository(workout);
        var xpService = new FakeXpService();
        var achievementService = new FakeAchievementService { ThrowOnEvaluate = true };
        var transaction = new FakeApplicationTransaction(
            () => Snapshot.Capture(workoutRepository, xpService, achievementService),
            snapshot => snapshot.Restore(workoutRepository, xpService, achievementService));
        var service = CreateService(workoutRepository, xpService, achievementService, transaction);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.FinishAsync(workout.Id));

        Assert.Equal(WorkoutStatus.InProgress, workout.Status);
        Assert.Null(workout.FinishedAt);
        Assert.Equal(0m, workout.TotalVolume);
        Assert.All(
            workout.WorkoutExercises.SelectMany(workoutExercise => workoutExercise.WorkoutSets),
            workoutSet => Assert.Equal(0m, workoutSet.Volume));
        Assert.Equal(0, xpService.WorkoutCompletedAwardCount);
        Assert.Equal(0, achievementService.EvaluateWorkoutCompletedCount);
        Assert.True(transaction.RolledBack);
        Assert.False(transaction.Committed);
    }

    [Fact]
    public async Task FinishAsync_DoesNotDuplicateXpOrAchievements_WhenCalledTwice()
    {
        var workout = CreateFinishableWorkout();
        var workoutRepository = new FakeWorkoutRepository(workout);
        var xpService = new FakeXpService();
        var achievementService = new FakeAchievementService();
        var transaction = new FakeApplicationTransaction(
            () => Snapshot.Capture(workoutRepository, xpService, achievementService),
            snapshot => snapshot.Restore(workoutRepository, xpService, achievementService));
        var service = CreateService(workoutRepository, xpService, achievementService, transaction);

        await service.FinishAsync(workout.Id);
        await service.FinishAsync(workout.Id);

        Assert.Equal(WorkoutStatus.Completed, workout.Status);
        Assert.Equal(1, workoutRepository.UpdateCount);
        Assert.Equal(1, xpService.WorkoutCompletedAwardCount);
        Assert.Equal(1, achievementService.EvaluateWorkoutCompletedCount);
        Assert.Equal(1, transaction.ExecutionCount);
    }

    private static WorkoutService CreateService(
        IWorkoutRepository workoutRepository,
        IXpService xpService,
        IAchievementService achievementService,
        IApplicationTransaction applicationTransaction)
    {
        return new WorkoutService(
            workoutRepository,
            new FakeUserProfileRepository(),
            new FakeExerciseRepository(),
            new FakeValidator<CreateWorkoutRequest>(),
            new FakeValidator<UpdateWorkoutRequest>(),
            xpService,
            achievementService,
            applicationTransaction);
    }

    private static Workout CreateFinishableWorkout(WorkoutStatus status = WorkoutStatus.InProgress)
    {
        var now = DateTime.UtcNow;
        var workout = new Workout
        {
            Id = Guid.NewGuid(),
            UserProfileId = Guid.NewGuid(),
            Name = "Treino transacional",
            WorkoutDate = now.AddMinutes(-30),
            StartedAt = now.AddMinutes(-30),
            TotalVolume = 0,
            Status = status,
            CreatedAt = now.AddMinutes(-30),
            UpdatedAt = now.AddMinutes(-30)
        };

        var workoutExercise = new WorkoutExercise
        {
            Id = Guid.NewGuid(),
            WorkoutId = workout.Id,
            Order = 1,
            CreatedAt = now,
            UpdatedAt = now
        };

        workoutExercise.WorkoutSets.Add(new WorkoutSet
        {
            Id = Guid.NewGuid(),
            WorkoutExerciseId = workoutExercise.Id,
            SetNumber = 1,
            Repetitions = 10,
            Weight = 10m,
            Volume = 0,
            CreatedAt = now,
            UpdatedAt = now
        });
        workoutExercise.WorkoutSets.Add(new WorkoutSet
        {
            Id = Guid.NewGuid(),
            WorkoutExerciseId = workoutExercise.Id,
            SetNumber = 2,
            Repetitions = 8,
            Weight = 5m,
            Volume = 0,
            CreatedAt = now,
            UpdatedAt = now
        });
        workout.WorkoutExercises.Add(workoutExercise);

        return workout;
    }

    private sealed class FakeApplicationTransaction(
        Func<Snapshot> capture,
        Action<Snapshot> restore) : IApplicationTransaction
    {
        public int ExecutionCount { get; private set; }
        public bool Committed { get; private set; }
        public bool RolledBack { get; private set; }

        public async Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            ExecutionCount++;
            var snapshot = capture();

            try
            {
                var result = await operation(cancellationToken);
                Committed = true;

                return result;
            }
            catch
            {
                RolledBack = true;
                restore(snapshot);
                throw;
            }
        }
    }

    private sealed record Snapshot(
        Workout Workout,
        int UpdateCount,
        int XpAwardCount,
        int AchievementEvaluateCount)
    {
        public static Snapshot Capture(
            FakeWorkoutRepository workoutRepository,
            FakeXpService xpService,
            FakeAchievementService achievementService)
        {
            return new Snapshot(
                CloneWorkout(workoutRepository.Workout),
                workoutRepository.UpdateCount,
                xpService.WorkoutCompletedAwardCount,
                achievementService.EvaluateWorkoutCompletedCount);
        }

        public void Restore(
            FakeWorkoutRepository workoutRepository,
            FakeXpService xpService,
            FakeAchievementService achievementService)
        {
            Copy(Workout, workoutRepository.Workout);
            workoutRepository.UpdateCount = UpdateCount;
            xpService.WorkoutCompletedAwardCount = XpAwardCount;
            achievementService.EvaluateWorkoutCompletedCount = AchievementEvaluateCount;
        }

        private static Workout CloneWorkout(Workout workout)
        {
            var clone = new Workout
            {
                Id = workout.Id,
                UserProfileId = workout.UserProfileId,
                Name = workout.Name,
                WorkoutDate = workout.WorkoutDate,
                Location = workout.Location,
                Notes = workout.Notes,
                TotalVolume = workout.TotalVolume,
                Status = workout.Status,
                TemplateWorkoutId = workout.TemplateWorkoutId,
                IsArchived = workout.IsArchived,
                StartedAt = workout.StartedAt,
                FinishedAt = workout.FinishedAt,
                CreatedAt = workout.CreatedAt,
                UpdatedAt = workout.UpdatedAt
            };

            foreach (var workoutExercise in workout.WorkoutExercises)
            {
                var workoutExerciseClone = new WorkoutExercise
                {
                    Id = workoutExercise.Id,
                    WorkoutId = workoutExercise.WorkoutId,
                    ExerciseId = workoutExercise.ExerciseId,
                    Order = workoutExercise.Order,
                    Notes = workoutExercise.Notes,
                    CreatedAt = workoutExercise.CreatedAt,
                    UpdatedAt = workoutExercise.UpdatedAt
                };

                foreach (var workoutSet in workoutExercise.WorkoutSets)
                {
                    workoutExerciseClone.WorkoutSets.Add(new WorkoutSet
                    {
                        Id = workoutSet.Id,
                        WorkoutExerciseId = workoutSet.WorkoutExerciseId,
                        SetNumber = workoutSet.SetNumber,
                        Repetitions = workoutSet.Repetitions,
                        Weight = workoutSet.Weight,
                        Volume = workoutSet.Volume,
                        Notes = workoutSet.Notes,
                        CreatedAt = workoutSet.CreatedAt,
                        UpdatedAt = workoutSet.UpdatedAt
                    });
                }

                clone.WorkoutExercises.Add(workoutExerciseClone);
            }

            return clone;
        }

        private static void Copy(Workout source, Workout target)
        {
            target.UserProfileId = source.UserProfileId;
            target.Name = source.Name;
            target.WorkoutDate = source.WorkoutDate;
            target.Location = source.Location;
            target.Notes = source.Notes;
            target.TotalVolume = source.TotalVolume;
            target.Status = source.Status;
            target.TemplateWorkoutId = source.TemplateWorkoutId;
            target.IsArchived = source.IsArchived;
            target.StartedAt = source.StartedAt;
            target.FinishedAt = source.FinishedAt;
            target.CreatedAt = source.CreatedAt;
            target.UpdatedAt = source.UpdatedAt;

            target.WorkoutExercises.Clear();
            foreach (var workoutExercise in source.WorkoutExercises)
            {
                var targetWorkoutExercise = new WorkoutExercise
                {
                    Id = workoutExercise.Id,
                    WorkoutId = workoutExercise.WorkoutId,
                    ExerciseId = workoutExercise.ExerciseId,
                    Order = workoutExercise.Order,
                    Notes = workoutExercise.Notes,
                    CreatedAt = workoutExercise.CreatedAt,
                    UpdatedAt = workoutExercise.UpdatedAt
                };

                foreach (var workoutSet in workoutExercise.WorkoutSets)
                {
                    targetWorkoutExercise.WorkoutSets.Add(new WorkoutSet
                    {
                        Id = workoutSet.Id,
                        WorkoutExerciseId = workoutSet.WorkoutExerciseId,
                        SetNumber = workoutSet.SetNumber,
                        Repetitions = workoutSet.Repetitions,
                        Weight = workoutSet.Weight,
                        Volume = workoutSet.Volume,
                        Notes = workoutSet.Notes,
                        CreatedAt = workoutSet.CreatedAt,
                        UpdatedAt = workoutSet.UpdatedAt
                    });
                }

                target.WorkoutExercises.Add(targetWorkoutExercise);
            }
        }
    }

    private sealed class FakeWorkoutRepository(Workout workout) : IWorkoutRepository
    {
        public Workout Workout { get; } = workout;
        public int UpdateCount { get; set; }

        public Task<IReadOnlyCollection<Workout>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Workout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(id == Workout.Id ? Workout : null);
        }

        public Task<Workout?> GetByIdWithExercisesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(id == Workout.Id ? Workout : null);
        }

        public Task<Workout?> GetByIdWithExercisesAndSetsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(id == Workout.Id ? Workout : null);
        }

        public Task<int> CountCompletedByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountCompletedByUserProfileSinceAsync(
            Guid userProfileId,
            DateTime utcStartDate,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Workout?> GetLatestCompletedByUserProfileAsync(
            Guid userProfileId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Workout?> GetLatestInProgressByUserProfileAsync(
            Guid userProfileId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<MobileWorkoutSummaryData>> GetMobileSummariesByUserProfileAsync(
            Guid userProfileId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<MobileHistoryWorkoutData>> GetMobileHistoryWorkoutsByUserProfileAsync(
            Guid userProfileId,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<decimal> SumCompletedVolumeByUserProfileAsync(
            Guid userProfileId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<decimal> SumCompletedVolumeByUserProfileSinceAsync(
            Guid userProfileId,
            DateTime utcStartDate,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> SumCompletedExerciseCountByUserProfileAsync(
            Guid userProfileId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> SumCompletedDurationMinutesByUserProfileAsync(
            Guid userProfileId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task AddAsync(Workout workout, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Workout workout, CancellationToken cancellationToken = default)
        {
            UpdateCount++;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Workout workout, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class FakeXpService : IXpService
    {
        public int WorkoutCompletedAwardCount { get; set; }

        public Task<XpSummaryResponse?> GetSummaryByUserAsync(
            Guid userProfileId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<XpTransactionResponse>> GetTransactionsByUserAsync(
            Guid userProfileId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<XpTransactionResponse?> AwardWorkoutCompletedAsync(
            Workout workout,
            CancellationToken cancellationToken = default)
        {
            WorkoutCompletedAwardCount++;
            return Task.FromResult<XpTransactionResponse?>(null);
        }

        public Task<XpTransactionResponse?> AwardAchievementUnlockedAsync(
            Guid userProfileId,
            Achievement achievement,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class FakeAchievementService : IAchievementService
    {
        public int EvaluateWorkoutCompletedCount { get; set; }
        public bool ThrowOnEvaluate { get; init; }

        public Task<IReadOnlyCollection<AchievementResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<UserAchievementResponse>> GetUnlockedByUserAsync(
            Guid userProfileId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<UserAchievementResponse>> EvaluateWorkoutCompletedAsync(
            Guid userProfileId,
            CancellationToken cancellationToken = default)
        {
            EvaluateWorkoutCompletedCount++;

            if (ThrowOnEvaluate)
            {
                throw new InvalidOperationException("Achievement evaluation failed.");
            }

            return Task.FromResult<IReadOnlyCollection<UserAchievementResponse>>(Array.Empty<UserAchievementResponse>());
        }
    }

    private sealed class FakeUserProfileRepository : IUserProfileRepository
    {
        public Task<IReadOnlyCollection<UserProfile>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> EmailExistsAsync(
            string email,
            Guid? ignoredUserProfileId = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasCustomExercisesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(UserProfile userProfile, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(UserProfile userProfile, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class FakeExerciseRepository : IExerciseRepository
    {
        public Task<IReadOnlyCollection<Exercise>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
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
}
