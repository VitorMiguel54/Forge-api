namespace Forge.Application.Models;

public record LevelDefinitionData(
    Guid Id,
    string Name,
    string Description,
    int MinimumXp,
    int DisplayOrder,
    Guid RarityId,
    string RarityName,
    string RarityPrimaryColor,
    string? RaritySecondaryColor,
    string? BadgeImageUrl,
    string? GuardianImageUrl,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
