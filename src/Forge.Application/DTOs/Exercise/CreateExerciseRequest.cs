using Forge.Domain.Enums;

namespace Forge.Application.DTOs.Exercise;

public record CreateExerciseRequest(
    string Name,
    string? Description,
    MuscleGroup MuscleGroup,
    bool IsCustom,
    Guid? UserProfileId,
    Guid? MuscleGroupId = null);
