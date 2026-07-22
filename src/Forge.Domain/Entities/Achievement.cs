using Forge.Domain.Enums;

namespace Forge.Domain.Entities;

public class Achievement
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AchievementCategory Category { get; set; }
    public int RequiredValue { get; set; }
    public bool IsSecret { get; set; }
    public bool IsActive { get; set; } = true;
    public int XpReward { get; set; }
    public string? BadgeImageUrl { get; set; }
    public string? BadgeImageStorageKey { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
