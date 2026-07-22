namespace Forge.Application.DTOs.Workout;

public record WorkoutResponse(
    Guid Id,
    Guid UserProfileId,
    string Name,
    DateTime WorkoutDate,
    string? Location,
    string? Notes,
    decimal TotalVolume,
    string Status,
    Guid? TemplateWorkoutId,
    bool IsArchived,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    int DurationMinutes);
