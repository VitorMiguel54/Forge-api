using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Application.Services;
using Forge.Domain.Entities;
using Xunit;

namespace Forge.Api.Tests.Services;

public class MobileHistoryServiceTests
{
    [Fact]
    public async Task GetAsync_ReturnsNull_WhenUserProfileDoesNotExist()
    {
        var workoutRepository = new FakeWorkoutRepository();
        var service = new MobileHistoryService(
            new FakeUserProfileRepository(userProfileExists: false),
            workoutRepository);

        var response = await service.GetAsync(Guid.NewGuid());

        Assert.Null(response);
        Assert.False(workoutRepository.HistoryWasRequested);
    }

    [Fact]
    public async Task GetAsync_ReturnsSummaryAndPagedWorkouts_WhenUserProfileExists()
    {
        var userProfileId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();
        var workoutDate = DateTime.UtcNow.AddDays(-1);
        var workoutRepository = new FakeWorkoutRepository(
            totalCompletedWorkouts: 3,
            totalDurationMinutes: 97,
            weeklyVolume: 1234.5m,
            new MobileHistoryWorkoutData(
                workoutId,
                "Superiores A",
                workoutDate,
                43,
                860m,
                4));
        var service = new MobileHistoryService(
            new FakeUserProfileRepository(userProfileExists: true),
            workoutRepository);

        var response = await service.GetAsync(userProfileId, page: 2, pageSize: 2);

        Assert.NotNull(response);
        Assert.Equal(3, response.Summary.Workouts);
        Assert.Equal(97, response.Summary.TotalDurationMinutes);
        Assert.Equal(1234.5m, response.Summary.WeeklyVolume);

        Assert.Equal(2, response.Page.Page);
        Assert.Equal(2, response.Page.PageSize);
        Assert.Equal(3, response.Page.TotalItems);
        Assert.Equal(2, response.Page.TotalPages);
        Assert.Equal(2, workoutRepository.LastSkip);
        Assert.Equal(2, workoutRepository.LastTake);

        var workout = Assert.Single(response.Workouts);
        Assert.Equal(workoutId, workout.Id);
        Assert.Equal("Superiores A", workout.Name);
        Assert.Equal(workoutDate, workout.Date);
        Assert.Equal(43, workout.DurationMinutes);
        Assert.Equal(860m, workout.Volume);
        Assert.Equal(4, workout.ExerciseCount);
    }

    [Fact]
    public async Task GetAsync_ClampsPageSizeToFifty_WhenRequestedPageSizeIsHigher()
    {
        var userProfileId = Guid.NewGuid();
        var workoutRepository = new FakeWorkoutRepository(totalCompletedWorkouts: 120);
        var service = new MobileHistoryService(
            new FakeUserProfileRepository(userProfileExists: true),
            workoutRepository);

        var response = await service.GetAsync(userProfileId, page: 1, pageSize: 100);

        Assert.NotNull(response);
        Assert.Equal(50, response.Page.PageSize);
        Assert.Equal(50, workoutRepository.LastTake);
        Assert.Equal(3, response.Page.TotalPages);
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

    private sealed class FakeWorkoutRepository(
        int totalCompletedWorkouts = 0,
        int totalDurationMinutes = 0,
        decimal weeklyVolume = 0,
        params MobileHistoryWorkoutData[] historyWorkouts) : IWorkoutRepository
    {
        public bool HistoryWasRequested { get; private set; }
        public int? LastSkip { get; private set; }
        public int? LastTake { get; private set; }

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
            return Task.FromResult(totalCompletedWorkouts);
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
            HistoryWasRequested = true;
            LastSkip = skip;
            LastTake = take;

            return Task.FromResult<IReadOnlyCollection<MobileHistoryWorkoutData>>(historyWorkouts);
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
            return Task.FromResult(weeklyVolume);
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
            return Task.FromResult(totalDurationMinutes);
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
