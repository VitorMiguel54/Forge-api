namespace Forge.Domain.Entities;

public class WaterIntake
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public DateTime IntakeDate { get; set; }
    public decimal Liters { get; set; }
    public decimal GoalInLiters { get; set; }
    public bool GoalAchieved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
}
