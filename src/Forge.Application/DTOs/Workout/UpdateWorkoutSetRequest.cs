namespace Forge.Application.DTOs.Workout;

public record UpdateWorkoutSetRequest(
    int SetNumber,
    int Repetitions,
    decimal Weight,
    string? Notes);
