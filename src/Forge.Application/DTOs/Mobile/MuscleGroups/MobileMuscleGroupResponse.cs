namespace Forge.Application.DTOs.Mobile.MuscleGroups;

public record MobileMuscleGroupResponse(
    Guid Id,
    string Name,
    string DisplayName,
    string? Icon,
    int DisplayOrder);
