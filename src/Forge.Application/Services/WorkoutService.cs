using Forge.Application.DTOs.Workout;
using Forge.Application.Interfaces;
using Forge.Application.Mappings;
using Forge.Application.Validators;
using Forge.Application.Validators.Workout;
using Forge.Domain.Entities;
using Forge.Domain.Enums;

namespace Forge.Application.Services;

public class WorkoutService(
    IWorkoutRepository workoutRepository,
    IUserProfileRepository userProfileRepository,
    IExerciseRepository exerciseRepository,
    IValidator<CreateWorkoutRequest> createWorkoutValidator,
    IValidator<UpdateWorkoutRequest> updateWorkoutValidator,
    IXpService xpService,
    IAchievementService achievementService,
    IApplicationTransaction applicationTransaction) : IWorkoutService
{
    public async Task<WorkoutResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var workout = await workoutRepository.GetByIdAsync(id, cancellationToken);

        return workout?.ToResponse();
    }

    public async Task<WorkoutAnalysisResponse?> GetAnalysisByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var workout = await workoutRepository.GetAnalysisByIdAsync(id, cancellationToken);

        return workout is null
            ? null
            : new WorkoutAnalysisResponse(
                workout.Id,
                workout.Name,
                workout.WorkoutDate,
                workout.StartedAt,
                workout.FinishedAt,
                workout.DurationMinutes,
                workout.TotalVolume,
                workout.TotalExercises,
                workout.TotalSets,
                workout.TotalRepetitions,
                workout.Status,
                workout.Exercises
                    .Select(exercise => new WorkoutAnalysisExerciseResponse(
                        exercise.WorkoutExerciseId,
                        exercise.ExerciseId,
                        exercise.Name,
                        exercise.MuscleGroup,
                        exercise.Equipment,
                        exercise.Order,
                        exercise.Notes,
                        exercise.TotalSets,
                        exercise.TotalRepetitions,
                        exercise.BestWeight,
                        exercise.TotalVolume,
                        exercise.PreviousBestWeight,
                        exercise.WeightDifference,
                        exercise.WeightDifferencePercentage,
                        exercise.Sets
                            .Select(set => new WorkoutAnalysisSetResponse(
                                set.Id,
                                set.SetNumber,
                                set.Repetitions,
                                set.Weight,
                                set.Volume,
                                set.Notes))
                            .ToArray()))
                    .ToArray());
    }

    public async Task<IReadOnlyCollection<WorkoutResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var workouts = await workoutRepository.GetAllAsync(cancellationToken);

        return workouts
            .Select(workout => workout.ToResponse())
            .ToArray();
    }

    public async Task<WorkoutResponse> CreateAsync(CreateWorkoutRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);
        await ValidateUserProfileExistsAsync(request.UserProfileId, cancellationToken);
        var displayOrder = await workoutRepository.GetNextDisplayOrderAsync(request.UserProfileId, cancellationToken);

        var now = DateTime.UtcNow;
        var workout = new Workout
        {
            Id = Guid.NewGuid(),
            UserProfileId = request.UserProfileId,
            Name = request.Name.Trim(),
            WorkoutDate = request.WorkoutDate,
            Location = NormalizeOptionalText(request.Location),
            Notes = NormalizeOptionalText(request.Notes),
            TotalVolume = 0,
            Status = WorkoutStatus.Draft,
            DisplayOrder = displayOrder,
            CreatedAt = now,
            UpdatedAt = now
        };

        await workoutRepository.AddAsync(workout, cancellationToken);

        return workout.ToResponse();
    }

    public async Task<WorkoutResponse> CreatePlanAsync(
        CreateWorkoutPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(new CreateWorkoutRequest(
            request.UserProfileId,
            request.Name,
            request.WorkoutDate,
            request.Location,
            request.Notes));
        ValidatePlanExercises(request.Exercises);
        await ValidateUserProfileExistsAsync(request.UserProfileId, cancellationToken);
        var displayOrder = await workoutRepository.GetNextDisplayOrderAsync(request.UserProfileId, cancellationToken);

        var orderedExercises = request.Exercises!
            .OrderBy(exercise => exercise.Order)
            .ToArray();
        var exerciseIds = orderedExercises
            .Select(exercise => exercise.ExerciseId)
            .ToArray();
        var exercisesById = new Dictionary<Guid, Exercise>();

        foreach (var exerciseId in exerciseIds)
        {
            var exercise = await exerciseRepository.GetByIdAsync(exerciseId, cancellationToken);
            if (exercise is null)
            {
                throw new ArgumentException($"Exercise does not exist: {exerciseId}.");
            }

            exercisesById[exerciseId] = exercise;
        }

        return await applicationTransaction.ExecuteAsync(
            async transactionCancellationToken =>
            {
                var now = DateTime.UtcNow;
                var workoutId = Guid.NewGuid();
                var muscleGroupIds = orderedExercises
                    .Select(exercise => exercisesById[exercise.ExerciseId].MuscleGroupId)
                    .Where(muscleGroupId => muscleGroupId is not null)
                    .Select(muscleGroupId => muscleGroupId!.Value)
                    .Distinct()
                    .ToArray();

                var workout = new Workout
                {
                    Id = workoutId,
                    UserProfileId = request.UserProfileId,
                    Name = request.Name.Trim(),
                    WorkoutDate = request.WorkoutDate,
                    Location = NormalizeOptionalText(request.Location),
                    Notes = NormalizeOptionalText(request.Notes),
                    TotalVolume = 0,
                    Status = WorkoutStatus.Draft,
                    DisplayOrder = displayOrder,
                    CreatedAt = now,
                    UpdatedAt = now,
                    WorkoutExercises = orderedExercises
                        .Select(exercise => new WorkoutExercise
                        {
                            Id = Guid.NewGuid(),
                            WorkoutId = workoutId,
                            ExerciseId = exercise.ExerciseId,
                            Order = exercise.Order,
                            Notes = NormalizeOptionalText(exercise.Notes),
                            CreatedAt = now,
                            UpdatedAt = now
                        })
                        .ToArray(),
                    WorkoutMuscleGroups = muscleGroupIds
                        .Select(muscleGroupId => new WorkoutMuscleGroup
                        {
                            WorkoutId = workoutId,
                            MuscleGroupId = muscleGroupId,
                            CreatedAt = now
                        })
                        .ToArray()
                };

                await workoutRepository.AddAsync(workout, transactionCancellationToken);

                return workout.ToResponse();
            },
            cancellationToken);
    }

    public async Task<WorkoutResponse?> UpdateAsync(Guid id, UpdateWorkoutRequest request, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        ValidateUpdateRequest(request);
        await ValidateUserProfileExistsAsync(request.UserProfileId, cancellationToken);

        var workout = await workoutRepository.GetByIdAsync(id, cancellationToken);
        if (workout is null)
        {
            return null;
        }

        ValidateWorkoutCanBeManaged(workout);

        workout.UserProfileId = request.UserProfileId;
        workout.Name = request.Name.Trim();
        workout.WorkoutDate = request.WorkoutDate;
        workout.Location = NormalizeOptionalText(request.Location);
        workout.Notes = NormalizeOptionalText(request.Notes);
        workout.UpdatedAt = DateTime.UtcNow;

        await workoutRepository.UpdateAsync(workout, cancellationToken);

        return workout.ToResponse();
    }

    public async Task<WorkoutResponse?> StartAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var workout = await workoutRepository.GetByIdWithExercisesAsync(id, cancellationToken);
        if (workout is null)
        {
            return null;
        }

        if (workout.IsArchived)
        {
            throw new InvalidOperationException("Archived workout cannot be started.");
        }

        if (workout.Status == WorkoutStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled workout cannot be started.");
        }

        if (workout.Status == WorkoutStatus.Completed)
        {
            throw new InvalidOperationException("Completed workout cannot be started.");
        }

        if (workout.Status == WorkoutStatus.InProgress)
        {
            return workout.ToResponse();
        }

        if (workout.WorkoutExercises.Count == 0)
        {
            throw new ArgumentException("Workout must have at least one exercise before it can be started.");
        }

        var now = DateTime.UtcNow;
        var executionId = Guid.NewGuid();
        var execution = new Workout
        {
            Id = executionId,
            UserProfileId = workout.UserProfileId,
            Name = workout.Name,
            WorkoutDate = now,
            Location = workout.Location,
            Notes = workout.Notes,
            TotalVolume = 0,
            Status = WorkoutStatus.InProgress,
            DisplayOrder = workout.DisplayOrder,
            TemplateWorkoutId = workout.Id,
            IsArchived = false,
            StartedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
            WorkoutExercises = workout.WorkoutExercises
                .OrderBy(workoutExercise => workoutExercise.Order)
                .ThenBy(workoutExercise => workoutExercise.CreatedAt)
                .Select(workoutExercise => new WorkoutExercise
                {
                    Id = Guid.NewGuid(),
                    WorkoutId = executionId,
                    ExerciseId = workoutExercise.ExerciseId,
                    Order = workoutExercise.Order,
                    Notes = workoutExercise.Notes,
                    CreatedAt = now,
                    UpdatedAt = now
                })
                .ToArray()
        };

        await workoutRepository.AddAsync(execution, cancellationToken);

        return execution.ToResponse();
    }

    public async Task<WorkoutResponse?> FinishAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var workout = await workoutRepository.GetByIdWithExercisesAndSetsAsync(id, cancellationToken);
        if (workout is null)
        {
            return null;
        }

        if (workout.Status == WorkoutStatus.Completed)
        {
            return workout.ToResponse();
        }

        return await applicationTransaction.ExecuteAsync(
            async transactionCancellationToken =>
            {
                ValidateWorkoutCanBeFinished(workout);

                var now = DateTime.UtcNow;
                workout.StartedAt ??= GetFallbackStartedAt(workout, now);
                workout.FinishedAt = now;
                workout.TotalVolume = CalculateTotalVolume(workout, now);
                workout.Status = WorkoutStatus.Completed;
                workout.UpdatedAt = now;

                await workoutRepository.UpdateAsync(workout, transactionCancellationToken);
                await xpService.AwardWorkoutCompletedAsync(workout, transactionCancellationToken);
                await achievementService.EvaluateWorkoutCompletedAsync(
                    workout.UserProfileId,
                    transactionCancellationToken);

                return workout.ToResponse();
            },
            cancellationToken);
    }

    public async Task<WorkoutResponse?> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var workout = await workoutRepository.GetByIdAsync(id, cancellationToken);
        if (workout is null)
        {
            return null;
        }

        if (workout.Status == WorkoutStatus.Completed)
        {
            throw new InvalidOperationException("Completed workout cannot be cancelled.");
        }

        var now = DateTime.UtcNow;
        if (workout.StartedAt is not null)
        {
            workout.FinishedAt ??= now;
        }

        workout.Status = WorkoutStatus.Cancelled;
        workout.UpdatedAt = now;

        await workoutRepository.UpdateAsync(workout, cancellationToken);

        return workout.ToResponse();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        var workout = await workoutRepository.GetByIdAsync(id, cancellationToken);
        if (workout is null)
        {
            return false;
        }

        if (workout.Status == WorkoutStatus.InProgress)
        {
            throw new InvalidOperationException("Workout in progress cannot be deleted.");
        }

        if (workout.Status == WorkoutStatus.Completed)
        {
            throw new InvalidOperationException("Completed workout history cannot be deleted.");
        }

        if (!workout.IsArchived)
        {
            workout.IsArchived = true;
            workout.UpdatedAt = DateTime.UtcNow;
            await workoutRepository.UpdateAsync(workout, cancellationToken);
        }

        return true;
    }

    public async Task ReorderAsync(
        ReorderWorkoutsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.UserProfileId == Guid.Empty)
        {
            throw new ArgumentException("User profile id is required.");
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ArgumentException("Workout reorder items are required.");
        }

        await ValidateUserProfileExistsAsync(request.UserProfileId, cancellationToken);
        ValidateReorderItems(request.Items);

        await applicationTransaction.ExecuteAsync(
            async transactionCancellationToken =>
            {
                var workouts = await workoutRepository.GetDraftsByUserProfileAsync(
                    request.UserProfileId,
                    transactionCancellationToken);
                var workoutsById = workouts.ToDictionary(workout => workout.Id);
                var requestedIds = request.Items.Select(item => item.WorkoutId).ToHashSet();

                if (requestedIds.Count != workouts.Count || workouts.Any(workout => !requestedIds.Contains(workout.Id)))
                {
                    throw new ArgumentException("Reorder must include all saved workouts for the user.");
                }

                var orderedItems = request.Items
                    .OrderBy(item => item.DisplayOrder)
                    .ThenBy(item => item.WorkoutId)
                    .ToArray();

                for (var index = 0; index < orderedItems.Length; index++)
                {
                    var workout = workoutsById[orderedItems[index].WorkoutId];
                    workout.DisplayOrder = index + 1;
                    workout.UpdatedAt = DateTime.UtcNow;
                }

                await workoutRepository.UpdateRangeAsync(workouts, transactionCancellationToken);
                return true;
            },
            cancellationToken);
    }

    private void ValidateCreateRequest(CreateWorkoutRequest request)
    {
        var validationResult = createWorkoutValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }

    private static void ValidatePlanExercises(IReadOnlyCollection<CreateWorkoutPlanExerciseRequest>? exercises)
    {
        if (exercises is null || exercises.Count == 0)
        {
            throw new ArgumentException("Workout must have at least one exercise.");
        }

        var duplicateExerciseId = exercises
            .GroupBy(exercise => exercise.ExerciseId)
            .FirstOrDefault(group => group.Count() > 1)
            ?.Key;

        if (duplicateExerciseId is not null)
        {
            throw new ArgumentException($"Exercise is duplicated in workout: {duplicateExerciseId}.");
        }

        var expectedOrder = 1;
        foreach (var exercise in exercises.OrderBy(exercise => exercise.Order))
        {
            if (exercise.ExerciseId == Guid.Empty)
            {
                throw new ArgumentException("Exercise id is required.");
            }

            if (exercise.Order != expectedOrder)
            {
                throw new ArgumentException("Exercise order must be sequential and start at one.");
            }

            expectedOrder++;
        }
    }

    private static void ValidateReorderItems(IReadOnlyCollection<ReorderWorkoutItemRequest> items)
    {
        var duplicateId = items
            .GroupBy(item => item.WorkoutId)
            .FirstOrDefault(group => group.Count() > 1)
            ?.Key;

        if (duplicateId is not null)
        {
            throw new ArgumentException($"Workout is duplicated in reorder request: {duplicateId}.");
        }

        foreach (var item in items)
        {
            if (item.WorkoutId == Guid.Empty)
            {
                throw new ArgumentException("Workout id is required.");
            }

            if (item.DisplayOrder <= 0)
            {
                throw new ArgumentException("Workout display order must be greater than zero.");
            }
        }
    }

    private void ValidateUpdateRequest(UpdateWorkoutRequest request)
    {
        var validationResult = updateWorkoutValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }

    private static void ValidateWorkoutCanBeFinished(Workout workout)
    {
        if (workout.Status == WorkoutStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled workout cannot be finished.");
        }

        if (workout.WorkoutExercises.Count == 0)
        {
            throw new ArgumentException("Workout must have at least one exercise before it can be finished.");
        }

        if (workout.WorkoutExercises.Any(workoutExercise => workoutExercise.WorkoutSets.Count == 0))
        {
            throw new ArgumentException("Every workout exercise must have at least one set before the workout can be finished.");
        }
    }

    private static void ValidateWorkoutCanBeManaged(Workout workout)
    {
        if (workout.IsArchived)
        {
            throw new InvalidOperationException("Archived workout cannot be edited.");
        }

        if (workout.Status == WorkoutStatus.InProgress)
        {
            throw new InvalidOperationException("Workout in progress cannot be edited.");
        }

        if (workout.Status == WorkoutStatus.Completed)
        {
            throw new InvalidOperationException("Completed workout history cannot be edited.");
        }
    }

    private static decimal CalculateTotalVolume(Workout workout, DateTime utcNow)
    {
        var totalVolume = 0m;

        foreach (var workoutSet in workout.WorkoutExercises.SelectMany(workoutExercise => workoutExercise.WorkoutSets))
        {
            workoutSet.Volume = workoutSet.Weight * workoutSet.Repetitions;
            workoutSet.UpdatedAt = utcNow;
            totalVolume += workoutSet.Volume;
        }

        return totalVolume;
    }

    private static DateTime GetFallbackStartedAt(Workout workout, DateTime utcNow)
    {
        return workout.WorkoutDate <= utcNow ? workout.WorkoutDate : utcNow;
    }

    private async Task ValidateUserProfileExistsAsync(Guid userProfileId, CancellationToken cancellationToken)
    {
        if (!await userProfileRepository.ExistsAsync(userProfileId, cancellationToken))
        {
            throw new ArgumentException("User profile does not exist.");
        }
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
