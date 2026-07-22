using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Domain.Entities;
using Forge.Domain.Enums;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Repositories;

public class WorkoutRepository(ForgeDbContext dbContext) : IWorkoutRepository
{
    public async Task<IReadOnlyCollection<Workout>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Workouts
            .AsNoTracking()
            .OrderBy(workout => workout.DisplayOrder)
            .ThenBy(workout => workout.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Workout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Workouts
            .FirstOrDefaultAsync(workout => workout.Id == id, cancellationToken);
    }

    public async Task<Workout?> GetByIdWithExercisesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Workouts
            .Include(workout => workout.WorkoutExercises)
            .FirstOrDefaultAsync(workout => workout.Id == id, cancellationToken);
    }

    public async Task<Workout?> GetByIdWithExercisesAndSetsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Workouts
            .Include(workout => workout.WorkoutExercises)
            .ThenInclude(workoutExercise => workoutExercise.WorkoutSets)
            .FirstOrDefaultAsync(workout => workout.Id == id, cancellationToken);
    }

    public async Task<WorkoutAnalysisData?> GetAnalysisByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var workout = await dbContext.Workouts
            .AsNoTracking()
            .Where(workout => workout.Id == id)
            .Select(workout => new
            {
                workout.Id,
                workout.UserProfileId,
                workout.Name,
                workout.WorkoutDate,
                workout.StartedAt,
                workout.FinishedAt,
                workout.Status,
                Exercises = workout.WorkoutExercises
                    .Select(workoutExercise => new
                    {
                        WorkoutExerciseId = workoutExercise.Id,
                        workoutExercise.ExerciseId,
                        ExerciseName = workoutExercise.Exercise.Name,
                        MuscleGroupDisplayName = workoutExercise.Exercise.MuscleGroupEntity != null
                            ? workoutExercise.Exercise.MuscleGroupEntity.DisplayName
                            : null,
                        workoutExercise.Exercise.MuscleGroup,
                        workoutExercise.Exercise.Equipment,
                        workoutExercise.Order,
                        workoutExercise.Notes,
                        Sets = workoutExercise.WorkoutSets
                            .Select(workoutSet => new
                            {
                                workoutSet.Id,
                                workoutSet.SetNumber,
                                workoutSet.Repetitions,
                                workoutSet.Weight,
                                workoutSet.Volume,
                                workoutSet.Notes
                            })
                            .ToArray()
                    })
                    .ToArray()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (workout is null)
        {
            return null;
        }

        var exerciseIds = workout.Exercises
            .Select(exercise => exercise.ExerciseId)
            .Distinct()
            .ToArray();

        var previousBestWeights = exerciseIds.Length == 0
            ? new Dictionary<Guid, decimal>()
            : (await dbContext.WorkoutExercises
                .AsNoTracking()
                .Where(workoutExercise =>
                    exerciseIds.Contains(workoutExercise.ExerciseId)
                    && workoutExercise.Workout.UserProfileId == workout.UserProfileId
                    && workoutExercise.Workout.Status == WorkoutStatus.Completed
                    && workoutExercise.Workout.Id != workout.Id
                    && workoutExercise.Workout.WorkoutDate < workout.WorkoutDate
                    && workoutExercise.WorkoutSets.Any())
                .Select(workoutExercise => new
                {
                    workoutExercise.ExerciseId,
                    workoutExercise.WorkoutId,
                    workoutExercise.Workout.WorkoutDate,
                    workoutExercise.Workout.CreatedAt,
                    BestWeight = workoutExercise.WorkoutSets.Max(workoutSet => workoutSet.Weight)
                })
                .ToArrayAsync(cancellationToken))
            .GroupBy(previousExercise => previousExercise.ExerciseId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(previousExercise => previousExercise.WorkoutDate)
                    .ThenByDescending(previousExercise => previousExercise.CreatedAt)
                    .First()
                    .BestWeight);

        var exercises = workout.Exercises
            .OrderBy(exercise => exercise.Order)
            .ThenBy(exercise => exercise.ExerciseName)
            .Select(exercise =>
            {
                var sets = exercise.Sets
                    .OrderBy(set => set.SetNumber)
                    .Select(set => new WorkoutAnalysisSetData(
                        set.Id,
                        set.SetNumber,
                        set.Repetitions,
                        set.Weight,
                        set.Weight * set.Repetitions,
                        set.Notes))
                    .ToArray();
                var totalSets = sets.Length;
                var totalRepetitions = sets.Sum(set => set.Repetitions);
                var bestWeight = sets.Length == 0 ? 0 : sets.Max(set => set.Weight);
                var totalVolume = sets.Sum(set => set.Volume);
                var previousBestWeight = previousBestWeights.GetValueOrDefault(exercise.ExerciseId);
                decimal? nullablePreviousBestWeight = previousBestWeight == 0 ? null : previousBestWeight;
                decimal? weightDifference = nullablePreviousBestWeight is null
                    ? null
                    : bestWeight - nullablePreviousBestWeight.Value;
                decimal? weightDifferencePercentage = nullablePreviousBestWeight is null or 0
                    ? null
                    : weightDifference / nullablePreviousBestWeight.Value * 100;

                return new WorkoutAnalysisExerciseData(
                    exercise.WorkoutExerciseId,
                    exercise.ExerciseId,
                    exercise.ExerciseName,
                    exercise.MuscleGroupDisplayName ?? exercise.MuscleGroup.ToString(),
                    exercise.Equipment,
                    exercise.Order,
                    exercise.Notes,
                    totalSets,
                    totalRepetitions,
                    bestWeight,
                    totalVolume,
                    nullablePreviousBestWeight,
                    weightDifference,
                    weightDifferencePercentage,
                    sets);
            })
            .ToArray();
        var totalVolume = exercises.Sum(exercise => exercise.TotalVolume);

        return new WorkoutAnalysisData(
            workout.Id,
            workout.Name,
            workout.WorkoutDate,
            workout.StartedAt,
            workout.FinishedAt,
            CalculateCompletedDurationMinutes(workout.StartedAt, workout.FinishedAt),
            totalVolume,
            exercises.Length,
            exercises.Sum(exercise => exercise.TotalSets),
            exercises.Sum(exercise => exercise.TotalRepetitions),
            workout.Status.ToString(),
            exercises);
    }

    public async Task<int> CountCompletedByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        return await GetCompletedWorkoutsByUserProfile(userProfileId)
            .CountAsync(cancellationToken);
    }

    public async Task<int> CountCompletedByUserProfileSinceAsync(
        Guid userProfileId,
        DateTime utcStartDate,
        CancellationToken cancellationToken = default)
    {
        return await GetCompletedWorkoutsByUserProfile(userProfileId)
            .Where(workout => workout.WorkoutDate >= utcStartDate)
            .CountAsync(cancellationToken);
    }

    public async Task<Workout?> GetLatestCompletedByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        return await GetCompletedWorkoutsByUserProfile(userProfileId)
            .OrderByDescending(workout => workout.WorkoutDate)
            .ThenByDescending(workout => workout.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Workout?> GetLatestInProgressByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Workouts
            .AsNoTracking()
            .Include(workout => workout.WorkoutExercises)
            .ThenInclude(workoutExercise => workoutExercise.WorkoutSets)
            .Where(workout =>
                workout.UserProfileId == userProfileId
                && workout.Status == WorkoutStatus.InProgress)
            .OrderByDescending(workout => workout.WorkoutDate)
            .ThenByDescending(workout => workout.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<MobileWorkoutSummaryData>> GetMobileSummariesByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        var workouts = await dbContext.Workouts
            .AsNoTracking()
            .Where(workout =>
                workout.UserProfileId == userProfileId
                && !workout.IsArchived
                && (workout.Status == WorkoutStatus.Draft || workout.Status == WorkoutStatus.InProgress))
            .OrderBy(workout => workout.DisplayOrder)
            .ThenBy(workout => workout.CreatedAt)
            .Select(workout => new
            {
                workout.Id,
                workout.Name,
                workout.WorkoutDate,
                workout.CreatedAt,
                workout.DisplayOrder,
                workout.StartedAt,
                workout.FinishedAt,
                workout.Status,
                WorkoutMuscleGroups = workout.WorkoutMuscleGroups
                    .OrderBy(workoutMuscleGroup => workoutMuscleGroup.MuscleGroup.DisplayOrder)
                    .ThenBy(workoutMuscleGroup => workoutMuscleGroup.MuscleGroup.DisplayName)
                    .Select(workoutMuscleGroup => workoutMuscleGroup.MuscleGroup.DisplayName)
                    .ToArray(),
                ExerciseMuscleGroups = workout.WorkoutExercises
                    .Select(workoutExercise => workoutExercise.Exercise.MuscleGroupEntity != null
                        ? new
                        {
                            DisplayOrder = workoutExercise.Exercise.MuscleGroupEntity.DisplayOrder,
                            DisplayName = (string?)workoutExercise.Exercise.MuscleGroupEntity.DisplayName,
                            workoutExercise.Exercise.MuscleGroup
                        }
                        : new
                        {
                            DisplayOrder = 999,
                            DisplayName = (string?)null,
                            workoutExercise.Exercise.MuscleGroup
                        })
                    .ToArray(),
                ExerciseCount = workout.WorkoutExercises.Count
            })
            .ToArrayAsync(cancellationToken);
        var utcNow = DateTime.UtcNow;

        return workouts
            .Select(workout => new MobileWorkoutSummaryData(
                workout.Id,
                workout.Name,
                workout.WorkoutDate,
                workout.CreatedAt,
                workout.DisplayOrder,
                SelectWorkoutMuscleGroups(
                    workout.WorkoutMuscleGroups,
                    workout.ExerciseMuscleGroups
                        .OrderBy(muscleGroup => muscleGroup.DisplayOrder)
                        .ThenBy(muscleGroup => muscleGroup.DisplayName ?? muscleGroup.MuscleGroup.ToString())
                        .Select(muscleGroup => muscleGroup.DisplayName ?? muscleGroup.MuscleGroup.ToString())
                        .ToArray()),
                workout.ExerciseCount,
                CalculateDurationMinutes(workout.StartedAt, workout.FinishedAt, workout.Status, utcNow),
                workout.Status))
            .ToArray();
    }

    public async Task<IReadOnlyCollection<Workout>> GetDraftsByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Workouts
            .Where(workout =>
                workout.UserProfileId == userProfileId
                && !workout.IsArchived
                && workout.Status == WorkoutStatus.Draft)
            .OrderBy(workout => workout.DisplayOrder)
            .ThenBy(workout => workout.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<int> GetNextDisplayOrderAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var maxDisplayOrder = await dbContext.Workouts
            .Where(workout => workout.UserProfileId == userProfileId)
            .Select(workout => (int?)workout.DisplayOrder)
            .MaxAsync(cancellationToken);

        return (maxDisplayOrder ?? 0) + 1;
    }

    public async Task<decimal> SumCompletedVolumeByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        return await GetCompletedWorkoutsByUserProfile(userProfileId)
            .SumAsync(workout => workout.TotalVolume, cancellationToken);
    }

    public async Task<decimal> SumCompletedVolumeByUserProfileSinceAsync(
        Guid userProfileId,
        DateTime utcStartDate,
        CancellationToken cancellationToken = default)
    {
        return await GetCompletedWorkoutsByUserProfile(userProfileId)
            .Where(workout => workout.WorkoutDate >= utcStartDate)
            .SumAsync(workout => workout.TotalVolume, cancellationToken);
    }

    public async Task<int> SumCompletedExerciseCountByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        return await GetCompletedWorkoutsByUserProfile(userProfileId)
            .SumAsync(workout => workout.WorkoutExercises.Count, cancellationToken);
    }

    public async Task<int> SumCompletedDurationMinutesByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        var completedWorkoutDurations = await GetCompletedWorkoutsByUserProfile(userProfileId)
            .Select(workout => new
            {
                workout.StartedAt,
                workout.FinishedAt,
                workout.Status
            })
            .ToArrayAsync(cancellationToken);

        return completedWorkoutDurations.Sum(workout =>
            CalculateDurationMinutes(workout.StartedAt, workout.FinishedAt, workout.Status, DateTime.UtcNow));
    }

    public async Task<IReadOnlyCollection<MobileHistoryWorkoutData>> GetMobileHistoryWorkoutsByUserProfileAsync(
        Guid userProfileId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var workouts = await GetCompletedWorkoutsByUserProfile(userProfileId)
            .OrderByDescending(workout => workout.WorkoutDate)
            .ThenByDescending(workout => workout.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(workout => new
            {
                workout.Id,
                workout.Name,
                workout.WorkoutDate,
                workout.StartedAt,
                workout.FinishedAt,
                workout.TotalVolume,
                ExerciseCount = workout.WorkoutExercises.Count,
                WorkoutMuscleGroups = workout.WorkoutMuscleGroups
                    .OrderBy(workoutMuscleGroup => workoutMuscleGroup.MuscleGroup.DisplayOrder)
                    .ThenBy(workoutMuscleGroup => workoutMuscleGroup.MuscleGroup.DisplayName)
                    .Select(workoutMuscleGroup => workoutMuscleGroup.MuscleGroup.DisplayName)
                    .ToArray(),
                ExerciseMuscleGroups = workout.WorkoutExercises
                    .Select(workoutExercise => workoutExercise.Exercise.MuscleGroupEntity != null
                        ? new
                        {
                            DisplayOrder = workoutExercise.Exercise.MuscleGroupEntity.DisplayOrder,
                            DisplayName = (string?)workoutExercise.Exercise.MuscleGroupEntity.DisplayName,
                            workoutExercise.Exercise.MuscleGroup
                        }
                        : new
                        {
                            DisplayOrder = 999,
                            DisplayName = (string?)null,
                            workoutExercise.Exercise.MuscleGroup
                        })
                    .ToArray()
            })
            .ToArrayAsync(cancellationToken);

        return workouts
            .Select(workout => new MobileHistoryWorkoutData(
                workout.Id,
                workout.Name,
                workout.WorkoutDate,
                CalculateCompletedDurationMinutes(workout.StartedAt, workout.FinishedAt),
                workout.TotalVolume,
                workout.ExerciseCount,
                SelectWorkoutMuscleGroups(
                    workout.WorkoutMuscleGroups,
                    workout.ExerciseMuscleGroups
                        .OrderBy(muscleGroup => muscleGroup.DisplayOrder)
                        .ThenBy(muscleGroup => muscleGroup.DisplayName ?? muscleGroup.MuscleGroup.ToString())
                        .Select(muscleGroup => muscleGroup.DisplayName ?? muscleGroup.MuscleGroup.ToString())
                        .ToArray())))
            .ToArray();
    }

    public async Task AddAsync(Workout workout, CancellationToken cancellationToken = default)
    {
        await dbContext.Workouts.AddAsync(workout, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Workout workout, CancellationToken cancellationToken = default)
    {
        dbContext.Workouts.Update(workout);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(IReadOnlyCollection<Workout> workouts, CancellationToken cancellationToken = default)
    {
        dbContext.Workouts.UpdateRange(workouts);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Workout workout, CancellationToken cancellationToken = default)
    {
        dbContext.Workouts.Remove(workout);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyCollection<string> SelectWorkoutMuscleGroups(
        IReadOnlyCollection<string> workoutMuscleGroups,
        IReadOnlyCollection<string> exerciseMuscleGroups)
    {
        var source = workoutMuscleGroups.Count > 0
            ? workoutMuscleGroups
            : exerciseMuscleGroups;

        return source
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private IQueryable<Workout> GetCompletedWorkoutsByUserProfile(Guid userProfileId)
    {
        return dbContext.Workouts
            .AsNoTracking()
            .Where(workout =>
                workout.UserProfileId == userProfileId
                && workout.Status == WorkoutStatus.Completed);
    }

    private static int CalculateDurationMinutes(
        DateTime? startedAt,
        DateTime? finishedAt,
        WorkoutStatus status,
        DateTime utcNow)
    {
        if (startedAt is null)
        {
            return 0;
        }

        var effectiveFinishedAt = status == WorkoutStatus.InProgress
            ? utcNow
            : finishedAt;

        return CalculateCompletedDurationMinutes(startedAt, effectiveFinishedAt);
    }

    private static int CalculateCompletedDurationMinutes(DateTime? startedAt, DateTime? finishedAt)
    {
        if (startedAt is null || finishedAt is null || finishedAt <= startedAt)
        {
            return 0;
        }

        return (int)Math.Floor((finishedAt.Value - startedAt.Value).TotalMinutes);
    }
}


