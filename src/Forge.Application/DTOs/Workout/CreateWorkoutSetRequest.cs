namespace Forge.Application.DTOs.Workout;

public record CreateWorkoutSetRequest(
    int SetNumber,
    int Repetitions,
    decimal Weight,
    string? Notes);
