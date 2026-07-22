namespace Forge.Domain.Entities;

public class WorkoutSet
{
    public Guid Id { get; set; }
    public Guid WorkoutExerciseId { get; set; }
    public int SetNumber { get; set; }
    public int Repetitions { get; set; }
    public decimal Weight { get; set; }
    public decimal Volume { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public WorkoutExercise WorkoutExercise { get; set; } = null!;
}
