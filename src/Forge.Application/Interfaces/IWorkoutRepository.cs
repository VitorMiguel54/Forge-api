using Forge.Domain.Entities;
using Forge.Application.Models;

namespace Forge.Application.Interfaces;

public interface IWorkoutRepository
{
    Task<IReadOnlyCollection<Workout>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Workout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Workout?> GetByIdWithExercisesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Workout?> GetByIdWithExercisesAndSetsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkoutAnalysisData?> GetAnalysisByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
    Task<int> CountCompletedByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default);
    Task<int> CountCompletedByUserProfileSinceAsync(
        Guid userProfileId,
        DateTime utcStartDate,
        CancellationToken cancellationToken = default);
    Task<Workout?> GetLatestCompletedByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default);
    Task<Workout?> GetLatestInProgressByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MobileWorkoutSummaryData>> GetMobileSummariesByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Workout>> GetDraftsByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();
    Task<int> GetNextDisplayOrderAsync(Guid userProfileId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
    Task<IReadOnlyCollection<MobileHistoryWorkoutData>> GetMobileHistoryWorkoutsByUserProfileAsync(
        Guid userProfileId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
    Task<decimal> SumCompletedVolumeByUserProfileAsync(Guid userProfileId, CancellationToken cancellationToken = default);
    Task<decimal> SumCompletedVolumeByUserProfileSinceAsync(
        Guid userProfileId,
        DateTime utcStartDate,
        CancellationToken cancellationToken = default);
    Task<int> SumCompletedExerciseCountByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default);
    Task<int> SumCompletedDurationMinutesByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default);
    Task AddAsync(Workout workout, CancellationToken cancellationToken = default);
    Task UpdateAsync(Workout workout, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IReadOnlyCollection<Workout> workouts, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
    Task DeleteAsync(Workout workout, CancellationToken cancellationToken = default);
}
