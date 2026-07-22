namespace Forge.Application.DTOs.Levels;

public record LevelDefinitionResponse(
    Guid Id,
    string Name,
    string Description,
    int MinimumXp,
    int DisplayOrder,
    LevelRarityResponse Rarity,
    string? BadgeImageUrl,
    string? GuardianImageUrl,
    bool IsMaximumLevel);
