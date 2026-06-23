using Forge.Domain.Enums;

namespace Forge.Application.DTOs.Exercise;

public record UpdateExerciseRequest(
    string Name,
    string? Description,
    MuscleGroup MuscleGroup,
    bool IsCustom,
    Guid? UserProfileId);
