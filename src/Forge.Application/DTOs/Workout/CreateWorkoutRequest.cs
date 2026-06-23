namespace Forge.Application.DTOs.Workout;

public record CreateWorkoutRequest(
    Guid UserProfileId,
    string Name,
    DateTime WorkoutDate,
    string? Location,
    string? Notes);
