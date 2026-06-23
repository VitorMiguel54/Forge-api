using Forge.Application.DTOs.Exercise;
using Forge.Application.Interfaces;
using Forge.Application.Mappings;
using Forge.Application.Validators;
using Forge.Application.Validators.Exercise;
using Forge.Domain.Entities;

namespace Forge.Application.Services;

public class ExerciseService(
    IExerciseRepository exerciseRepository,
    IValidator<CreateExerciseRequest> createExerciseValidator,
    IValidator<UpdateExerciseRequest> updateExerciseValidator) : IExerciseService
{
    public async Task<ExerciseResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var exercise = await exerciseRepository.GetByIdAsync(id, cancellationToken);

        return exercise?.ToResponse();
    }

    public async Task<IReadOnlyCollection<ExerciseResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var exercises = await exerciseRepository.GetAllAsync(cancellationToken);

        return exercises
            .Select(exercise => exercise.ToResponse())
            .ToArray();
    }

    public async Task<ExerciseResponse> CreateAsync(CreateExerciseRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = NormalizeOptionalText(request.Description),
            MuscleGroup = request.MuscleGroup,
            IsCustom = request.IsCustom,
            UserProfileId = request.IsCustom ? request.UserProfileId : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await exerciseRepository.AddAsync(exercise, cancellationToken);

        return exercise.ToResponse();
    }

    public async Task<ExerciseResponse?> UpdateAsync(Guid id, UpdateExerciseRequest request, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        ValidateUpdateRequest(request);

        var exercise = await exerciseRepository.GetByIdAsync(id, cancellationToken);
        if (exercise is null)
        {
            return null;
        }

        exercise.Name = request.Name.Trim();
        exercise.Description = NormalizeOptionalText(request.Description);
        exercise.MuscleGroup = request.MuscleGroup;
        exercise.IsCustom = request.IsCustom;
        exercise.UserProfileId = request.IsCustom ? request.UserProfileId : null;
        exercise.UpdatedAt = DateTime.UtcNow;

        await exerciseRepository.UpdateAsync(exercise, cancellationToken);

        return exercise.ToResponse();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        var exercise = await exerciseRepository.GetByIdAsync(id, cancellationToken);
        if (exercise is null)
        {
            return false;
        }

        if (await exerciseRepository.IsInUseAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Exercise cannot be deleted because it is linked to a workout.");
        }

        await exerciseRepository.DeleteAsync(exercise, cancellationToken);

        return true;
    }

    private void ValidateCreateRequest(CreateExerciseRequest request)
    {
        var validationResult = createExerciseValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }

    private void ValidateUpdateRequest(UpdateExerciseRequest request)
    {
        var validationResult = updateExerciseValidator.Validate(request);
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
