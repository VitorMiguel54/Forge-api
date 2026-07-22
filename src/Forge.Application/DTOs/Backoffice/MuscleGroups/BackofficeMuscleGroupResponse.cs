namespace Forge.Application.DTOs.Backoffice.MuscleGroups;

public record BackofficeMuscleGroupResponse(
    Guid Id,
    string Name,
    string DisplayName,
    string? Icon,
    int DisplayOrder,
    bool IsActive,
    int ExerciseCount);
