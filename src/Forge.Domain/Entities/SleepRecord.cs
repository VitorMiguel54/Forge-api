namespace Forge.Domain.Entities;

public class SleepRecord
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public DateTime SleepDate { get; set; }
    public decimal HoursSlept { get; set; }
    public decimal GoalInHours { get; set; }
    public bool GoalAchieved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
}
