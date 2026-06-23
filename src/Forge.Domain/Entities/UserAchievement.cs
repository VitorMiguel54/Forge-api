namespace Forge.Domain.Entities;

public class UserAchievement
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public Guid AchievementId { get; set; }
    public DateTime UnlockedAt { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
    public Achievement Achievement { get; set; } = null!;
}
