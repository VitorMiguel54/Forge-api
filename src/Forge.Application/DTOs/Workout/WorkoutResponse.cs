namespace Forge.Application.DTOs.Workout;

public record WorkoutResponse(
    Guid Id,
    Guid UserProfileId,
    string Name,
    DateTime WorkoutDate,
    string? Location,
    string? Notes,
    decimal TotalVolume);
