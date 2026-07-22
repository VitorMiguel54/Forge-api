using Forge.Application.DTOs.Workout;
using Forge.Application.Interfaces;
using Forge.Application.Mappings;
using Forge.Application.Validators;
using Forge.Application.Validators.Workout;
using Forge.Domain.Entities;
using Forge.Domain.Enums;

namespace Forge.Application.Services;

public class WorkoutExerciseService(
    IWorkoutExerciseRepository workoutExerciseRepository,
    IWorkoutMuscleGroupRepository workoutMuscleGroupRepository,
    IWorkoutRepository workoutRepository,
    IExerciseRepository exerciseRepository,
    IValidator<CreateWorkoutExerciseRequest> createWorkoutExerciseValidator) : IWorkoutExerciseService
{
    public async Task<IReadOnlyCollection<WorkoutExerciseResponse>?> GetByWorkoutAsync(
        Guid workoutId,
        CancellationToken cancellationToken = default)
    {
        if (workoutId == Guid.Empty)
        {
            return null;
        }

        if (await workoutRepository.GetByIdAsync(workoutId, cancellationToken) is null)
        {
            return null;
        }

        var workoutExercises = await workoutExerciseRepository.GetByWorkoutAsync(workoutId, cancellationToken);

        return workoutExercises
            .Select(workoutExercise => workoutExercise.ToResponse())
            .ToArray();
    }

    public async Task<WorkoutExerciseResponse> CreateAsync(
        Guid workoutId,
        CreateWorkoutExerciseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (workoutId == Guid.Empty)
        {
            throw new ArgumentException("Workout id is required.");
        }

        ValidateCreateRequest(request);

        var workout = await workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout is null)
        {
            throw new ArgumentException("Workout does not exist.");
        }

        ValidateWorkoutCanBeEdited(workout);

        if (await exerciseRepository.GetByIdAsync(request.ExerciseId, cancellationToken) is null)
        {
            throw new ArgumentException("Exercise does not exist.");
        }

        if (await workoutExerciseRepository.ExerciseExistsInWorkoutAsync(
                workoutId,
                request.ExerciseId,
                cancellationToken))
        {
            throw new ArgumentException("Exercise is already linked to this workout.");
        }

        var now = DateTime.UtcNow;
        var workoutExercise = new WorkoutExercise
        {
            Id = Guid.NewGuid(),
            WorkoutId = workoutId,
            ExerciseId = request.ExerciseId,
            Order = request.Order,
            Notes = NormalizeOptionalText(request.Notes),
            CreatedAt = now,
            UpdatedAt = now
        };

        await workoutExerciseRepository.AddAsync(workoutExercise, cancellationToken);
        await workoutMuscleGroupRepository.SyncFromWorkoutExercisesAsync(workoutId, cancellationToken);

        return workoutExercise.ToResponse();
    }

    public async Task<bool> DeleteAsync(
        Guid workoutId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (workoutId == Guid.Empty || id == Guid.Empty)
        {
            return false;
        }

        var workout = await workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout is null)
        {
            return false;
        }

        ValidateWorkoutCanBeEdited(workout);

        var workoutExercise = await workoutExerciseRepository.GetByWorkoutAndIdAsync(
            workoutId,
            id,
            cancellationToken);

        if (workoutExercise is null)
        {
            return false;
        }

        await workoutExerciseRepository.DeleteAsync(workoutExercise, cancellationToken);
        await workoutMuscleGroupRepository.SyncFromWorkoutExercisesAsync(workoutId, cancellationToken);

        return true;
    }

    private void ValidateCreateRequest(CreateWorkoutExerciseRequest request)
    {
        var validationResult = createWorkoutExerciseValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }

    private static void ValidateWorkoutCanBeEdited(Workout workout)
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

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
