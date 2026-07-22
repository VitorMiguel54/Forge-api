namespace Forge.Application.DTOs.Workout;

public record UpdateWorkoutRequest(
    Guid UserProfileId,
    string Name,
    DateTime WorkoutDate,
    string? Location,
    string? Notes);
