namespace Forge.Application.DTOs.Backoffice.Exercises;

public record BackofficeExerciseMediaUploadResponse(
    Guid ExerciseId,
    string MediaType,
    string? Url,
    string? ContentType,
    long? FileSize,
    DateTime UpdatedAt);
