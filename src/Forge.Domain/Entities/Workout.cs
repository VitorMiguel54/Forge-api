using Forge.Domain.Enums;

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
    public WorkoutStatus Status { get; set; } = WorkoutStatus.Draft;
    public int DisplayOrder { get; set; }
    public Guid? TemplateWorkoutId { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
    public Workout? TemplateWorkout { get; set; }
    public ICollection<Workout> Executions { get; set; } = new List<Workout>();
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();
    public ICollection<WorkoutMuscleGroup> WorkoutMuscleGroups { get; set; } = new List<WorkoutMuscleGroup>();
}

