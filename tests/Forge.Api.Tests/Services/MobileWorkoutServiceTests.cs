using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Application.Services;
using Forge.Domain.Entities;
using Forge.Domain.Enums;
using Xunit;

namespace Forge.Api.Tests.Services;

public class MobileWorkoutServiceTests
{
    [Fact]
    public async Task GetAsync_ReturnsNull_WhenUserProfileDoesNotExist()
    {
        var workoutRepository = new FakeWorkoutRepository();
        var service = new MobileWorkoutService(
            new FakeUserProfileRepository(userProfileExists: false),
            workoutRepository);

        var response = await service.GetAsync(Guid.NewGuid());

        Assert.Null(response);
        Assert.False(workoutRepository.MobileSummariesWereRequested);
    }

    [Fact]
    public async Task GetAsync_ReturnsActiveWorkoutAndSavedWorkouts_WhenUserProfileExists()
    {
        var userProfileId = Guid.NewGuid();
        var activeWorkoutId = Guid.NewGuid();
        var completedWorkoutId = Guid.NewGuid();
        var availableWorkoutId = Guid.NewGuid();
        var utcNow = DateTime.UtcNow;

        var service = new MobileWorkoutService(
            new FakeUserProfileRepository(userProfileExists: true),
            new FakeWorkoutRepository(
                new MobileWorkoutSummaryData(
                    completedWorkoutId,
                    "Forca A",
                    utcNow.AddDays(-2),
                    utcNow.AddDays(-2),
                    1,
                    ["Chest", "Shoulders"],
                    2,
                    50,
                    WorkoutStatus.Completed),
                new MobileWorkoutSummaryData(
                    activeWorkoutId,
                    "Inferiores",
                    utcNow,
                    utcNow,
                    2,
                    ["Glutes", "Legs"],
                    3,
                    12,
                    WorkoutStatus.InProgress),
                new MobileWorkoutSummaryData(
                    availableWorkoutId,
                    "Costas",
                    utcNow.AddDays(-1),
                    utcNow.AddDays(-1),
                    3,
                    ["Back"],
                    1,
                    0,
                    WorkoutStatus.Draft)));

        var response = await service.GetAsync(userProfileId);

        Assert.NotNull(response);
        Assert.NotNull(response.ActiveWorkout);
        Assert.Equal(activeWorkoutId, response.ActiveWorkout.Id);
        Assert.Equal("Inferiores", response.ActiveWorkout.Name);
        Assert.Equal(["Glutes", "Legs"], response.ActiveWorkout.MuscleGroups);
        Assert.Equal(3, response.ActiveWorkout.ExerciseCount);
        Assert.Equal(12, response.ActiveWorkout.DurationMinutes);
        Assert.Equal(12, response.ActiveWorkout.EstimatedDurationMinutes);
        Assert.Equal("inProgress", response.ActiveWorkout.Status);

        var savedWorkout = Assert.Single(response.SavedWorkouts);
        Assert.Equal(availableWorkoutId, savedWorkout.Id);
        Assert.Equal("available", savedWorkout.Status);
        Assert.Equal(0, savedWorkout.DurationMinutes);
        Assert.Equal(0, savedWorkout.EstimatedDurationMinutes);
    }

    private sealed class FakeUserProfileRepository(bool userProfileExists) : IUserProfileRepository
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
            return Task.FromResult(userProfileExists);
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

    private sealed class FakeWorkoutRepository(params MobileWorkoutSummaryData[] summaries) : IWorkoutRepository
    {
        public bool MobileSummariesWereRequested { get; private set; }

        public Task<IReadOnlyCollection<Workout>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Workout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Workout?> GetByIdWithExercisesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Workout?> GetByIdWithExercisesAndSetsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountCompletedByUserProfileAsync(
            Guid userProfileId,
            CancellationToken cancellationToken = default)
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
            MobileSummariesWereRequested = true;
            return Task.FromResult<IReadOnlyCollection<MobileWorkoutSummaryData>>(summaries);
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
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Workout workout, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
