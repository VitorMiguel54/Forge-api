namespace Forge.Domain.Entities;

public class MuscleGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    public ICollection<WorkoutMuscleGroup> WorkoutMuscleGroups { get; set; } = new List<WorkoutMuscleGroup>();
}
