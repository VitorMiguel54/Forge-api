using Forge.Application.DTOs.Workout;
using Forge.Application.Interfaces;
using Forge.Application.Mappings;
using Forge.Application.Validators;
using Forge.Application.Validators.Workout;
using Forge.Domain.Entities;

namespace Forge.Application.Services;

public class WorkoutSetService(
    IWorkoutSetRepository workoutSetRepository,
    IWorkoutExerciseRepository workoutExerciseRepository,
    IValidator<CreateWorkoutSetRequest> createWorkoutSetValidator,
    IValidator<UpdateWorkoutSetRequest> updateWorkoutSetValidator) : IWorkoutSetService
{
    public async Task<IReadOnlyCollection<WorkoutSetResponse>?> GetByWorkoutExerciseAsync(
        Guid workoutExerciseId,
        CancellationToken cancellationToken = default)
    {
        if (workoutExerciseId == Guid.Empty)
        {
            return null;
        }

        if (!await workoutExerciseRepository.ExistsAsync(workoutExerciseId, cancellationToken))
        {
            return null;
        }

        var workoutSets = await workoutSetRepository.GetByWorkoutExerciseAsync(
            workoutExerciseId,
            cancellationToken);

        return workoutSets
            .Select(workoutSet => workoutSet.ToResponse())
            .ToArray();
    }

    public async Task<WorkoutSetResponse> CreateAsync(
        Guid workoutExerciseId,
        CreateWorkoutSetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (workoutExerciseId == Guid.Empty)
        {
            throw new ArgumentException("Workout exercise id is required.");
        }

        ValidateCreateRequest(request);

        if (!await workoutExerciseRepository.ExistsAsync(workoutExerciseId, cancellationToken))
        {
            throw new ArgumentException("Workout exercise does not exist.");
        }

        var now = DateTime.UtcNow;
        var workoutSet = new WorkoutSet
        {
            Id = Guid.NewGuid(),
            WorkoutExerciseId = workoutExerciseId,
            SetNumber = request.SetNumber,
            Repetitions = request.Repetitions,
            Weight = request.Weight,
            Volume = 0,
            Notes = NormalizeOptionalText(request.Notes),
            CreatedAt = now,
            UpdatedAt = now
        };

        await workoutSetRepository.AddAsync(workoutSet, cancellationToken);

        return workoutSet.ToResponse();
    }

    public async Task<WorkoutSetResponse?> UpdateAsync(
        Guid id,
        UpdateWorkoutSetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        ValidateUpdateRequest(request);

        var workoutSet = await workoutSetRepository.GetByIdAsync(id, cancellationToken);
        if (workoutSet is null)
        {
            return null;
        }

        workoutSet.SetNumber = request.SetNumber;
        workoutSet.Repetitions = request.Repetitions;
        workoutSet.Weight = request.Weight;
        workoutSet.Volume = 0;
        workoutSet.Notes = NormalizeOptionalText(request.Notes);
        workoutSet.UpdatedAt = DateTime.UtcNow;

        await workoutSetRepository.UpdateAsync(workoutSet, cancellationToken);

        return workoutSet.ToResponse();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        var workoutSet = await workoutSetRepository.GetByIdAsync(id, cancellationToken);
        if (workoutSet is null)
        {
            return false;
        }

        await workoutSetRepository.DeleteAsync(workoutSet, cancellationToken);

        return true;
    }

    private void ValidateCreateRequest(CreateWorkoutSetRequest request)
    {
        var validationResult = createWorkoutSetValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }

    private void ValidateUpdateRequest(UpdateWorkoutSetRequest request)
    {
        var validationResult = updateWorkoutSetValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
