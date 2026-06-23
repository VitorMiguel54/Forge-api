namespace Forge.Domain.Entities;

public class Workout
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime WorkoutDate { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public decimal TotalVolume { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();
}
