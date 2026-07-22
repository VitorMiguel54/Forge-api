namespace Forge.Domain.Entities;

public class WorkoutExercise
{
    public Guid Id { get; set; }
    public Guid WorkoutId { get; set; }
    public Guid ExerciseId { get; set; }
    public int Order { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Workout Workout { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
    public ICollection<WorkoutSet> WorkoutSets { get; set; } = new List<WorkoutSet>();
}
