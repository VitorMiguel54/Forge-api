using MuscleGroupEnum = Forge.Domain.Enums.MuscleGroup;

namespace Forge.Domain.Entities;

public class Exercise
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MuscleGroupEnum MuscleGroup { get; set; }
    public Guid? MuscleGroupId { get; set; }
    public string? Difficulty { get; set; }
    public string? Equipment { get; set; }
    public bool IsCustom { get; set; }
    public bool IsActive { get; set; } = true;
    public int? DisplayOrder { get; set; }
    public string? ImageUrl { get; set; }
    public string? GifUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public Guid? UserProfileId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserProfile? UserProfile { get; set; }
    public MuscleGroup? MuscleGroupEntity { get; set; }
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();
}


