namespace Forge.Domain.Entities;

public class WorkoutMuscleGroup
{
    public Guid WorkoutId { get; set; }
    public Guid MuscleGroupId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Workout Workout { get; set; } = null!;
    public MuscleGroup MuscleGroup { get; set; } = null!;
}
