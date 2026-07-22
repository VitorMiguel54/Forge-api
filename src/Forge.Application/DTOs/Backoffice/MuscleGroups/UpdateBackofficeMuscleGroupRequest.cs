namespace Forge.Application.DTOs.Backoffice.MuscleGroups;

public record UpdateBackofficeMuscleGroupRequest(
    string Name,
    string DisplayName,
    string? Icon,
    int DisplayOrder);
