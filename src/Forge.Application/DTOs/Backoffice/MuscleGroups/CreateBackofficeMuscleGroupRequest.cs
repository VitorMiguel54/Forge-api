namespace Forge.Application.DTOs.Backoffice.MuscleGroups;

public record CreateBackofficeMuscleGroupRequest(
    string Name,
    string DisplayName,
    string? Icon,
    int DisplayOrder);
