using Forge.Domain.Enums;

namespace Forge.Application.DTOs.Exercise;

public record ExerciseResponse(
    Guid Id,
    string Name,
    string? Description,
    MuscleGroup MuscleGroup,
    bool IsCustom,
    Guid? UserProfileId,
    Guid? MuscleGroupId,
    string? MuscleGroupDisplayName);
