using Forge.Application.DTOs.Exercise;
using Forge.Application.Interfaces;
using Forge.Application.Mappings;
using Forge.Application.Models;
using Forge.Application.Validators;
using Forge.Application.Validators.Exercise;
using Forge.Domain.Entities;
using MuscleGroupEnum = Forge.Domain.Enums.MuscleGroup;

namespace Forge.Application.Services;

public class ExerciseService(
    IExerciseRepository exerciseRepository,
    IUserProfileRepository userProfileRepository,
    IMuscleGroupRepository muscleGroupRepository,
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
            .Where(exercise => exercise.IsActive)
            .OrderBy(exercise => exercise.MuscleGroupEntity?.DisplayOrder ?? int.MaxValue)
            .ThenBy(exercise => exercise.DisplayOrder ?? int.MaxValue)
            .ThenBy(exercise => exercise.Name)
            .Select(exercise => exercise.ToResponse())
            .ToArray();
    }

    public async Task<ExerciseResponse> CreateAsync(CreateExerciseRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);
        var userProfileId = NormalizeUserProfileId(request.IsCustom, request.UserProfileId);
        await ValidateUserProfileAsync(request.IsCustom, userProfileId, cancellationToken);
        var muscleGroupId = await ResolveMuscleGroupIdAsync(request.MuscleGroupId, request.MuscleGroup, cancellationToken);

        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = NormalizeOptionalText(request.Description),
            MuscleGroup = request.MuscleGroup,
            MuscleGroupId = muscleGroupId,
            IsCustom = request.IsCustom,
            UserProfileId = userProfileId,
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
        var userProfileId = NormalizeUserProfileId(request.IsCustom, request.UserProfileId);
        await ValidateUserProfileAsync(request.IsCustom, userProfileId, cancellationToken);
        var muscleGroupId = await ResolveMuscleGroupIdAsync(request.MuscleGroupId, request.MuscleGroup, cancellationToken);

        var exercise = await exerciseRepository.GetByIdAsync(id, cancellationToken);
        if (exercise is null)
        {
            return null;
        }

        exercise.Name = request.Name.Trim();
        exercise.Description = NormalizeOptionalText(request.Description);
        exercise.MuscleGroup = request.MuscleGroup;
        exercise.MuscleGroupId = muscleGroupId;
        exercise.IsCustom = request.IsCustom;
        exercise.UserProfileId = userProfileId;
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

    private async Task ValidateUserProfileAsync(
        bool isCustom,
        Guid? userProfileId,
        CancellationToken cancellationToken)
    {
        if (!isCustom)
        {
            return;
        }

        if (userProfileId is null || userProfileId == Guid.Empty)
        {
            throw new ArgumentException("Custom exercises must be linked to a user profile.");
        }

        if (!await userProfileRepository.ExistsAsync(userProfileId.Value, cancellationToken))
        {
            throw new ArgumentException("User profile does not exist.");
        }
    }

    private async Task<Guid> ResolveMuscleGroupIdAsync(
        Guid? muscleGroupId,
        MuscleGroupEnum legacyMuscleGroup,
        CancellationToken cancellationToken)
    {
        var resolvedMuscleGroupId = muscleGroupId ?? MuscleGroupCatalog.GetDefaultId(legacyMuscleGroup);

        if (!await muscleGroupRepository.ExistsAsync(resolvedMuscleGroupId, cancellationToken))
        {
            throw new ArgumentException("Muscle group does not exist.");
        }

        return resolvedMuscleGroupId;
    }

    private static Guid? NormalizeUserProfileId(bool isCustom, Guid? userProfileId)
    {
        return isCustom ? userProfileId : null;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
