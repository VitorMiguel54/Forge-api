using Forge.Application.DTOs.Levels;

namespace Forge.Application.DTOs.Backoffice.Levels;

public record BackofficeLevelDefinitionResponse(
    Guid Id,
    string Name,
    string Description,
    int MinimumXp,
    int DisplayOrder,
    Guid RarityId,
    LevelRarityResponse Rarity,
    string? BadgeImageUrl,
    string? GuardianImageUrl,
    bool IsActive,
    bool IsOfficial,
    int CurrentUserCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
