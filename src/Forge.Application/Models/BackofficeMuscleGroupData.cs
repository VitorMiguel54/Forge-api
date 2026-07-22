namespace Forge.Application.Models;

public record BackofficeMuscleGroupData(
    Guid Id,
    string Name,
    string DisplayName,
    string? Icon,
    int DisplayOrder,
    bool IsActive,
    int ExerciseCount);
