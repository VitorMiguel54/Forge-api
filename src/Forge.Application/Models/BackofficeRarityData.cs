namespace Forge.Application.Models;

public record BackofficeRarityData(
    Guid Id,
    string Name,
    string PrimaryColor,
    string? SecondaryColor,
    int DisplayOrder,
    bool IsActive,
    int AchievementCount,
    int LevelCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
