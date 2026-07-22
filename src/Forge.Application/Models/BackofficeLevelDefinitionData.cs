namespace Forge.Application.Models;

public record BackofficeLevelDefinitionData(
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
    int CurrentUserCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
