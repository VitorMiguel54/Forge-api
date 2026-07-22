namespace Forge.Application.DTOs.Backoffice.Exercises;

public record UpdateBackofficeExerciseRequest(
    string Name,
    string? Description,
    Guid MuscleGroupId,
    string? Difficulty,
    string? Equipment,
    bool IsCustom,
    int? DisplayOrder,
    string? ImageUrl,
    string? GifUrl,
    string? VideoUrl,
    string? ThumbnailUrl);
