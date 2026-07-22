namespace Forge.Application.DTOs.Backoffice.Exercises;

public record BackofficeExerciseResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid MuscleGroupId,
    string? MuscleGroupName,
    string? Difficulty,
    string? Equipment,
    bool IsCustom,
    bool IsActive,
    int? DisplayOrder,
    string? ImageUrl,
    string? GifUrl,
    string? VideoUrl,
    string? ThumbnailUrl);
