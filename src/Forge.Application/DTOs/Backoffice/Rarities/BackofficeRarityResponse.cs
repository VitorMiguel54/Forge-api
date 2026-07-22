namespace Forge.Application.DTOs.Backoffice.Rarities;

public record BackofficeRarityResponse(
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
