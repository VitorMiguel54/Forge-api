using Forge.Application.DTOs.Backoffice.Levels;
using Forge.Application.DTOs.Levels;
using Forge.Application.Models;
using Forge.Domain.Constants;

namespace Forge.Application.Services;

internal static class LevelResponseFactory
{
    public static LevelDefinitionResponse ToPublicResponse(LevelDefinitionData level, bool isMaximumLevel)
    {
        return new LevelDefinitionResponse(
            level.Id,
            level.Name,
            level.Description,
            level.MinimumXp,
            level.DisplayOrder,
            ToRarityResponse(level.RarityId, level.RarityName, level.RarityPrimaryColor, level.RaritySecondaryColor),
            level.BadgeImageUrl,
            level.GuardianImageUrl,
            isMaximumLevel);
    }

    public static BackofficeLevelDefinitionResponse ToBackofficeResponse(BackofficeLevelDefinitionData level)
    {
        return new BackofficeLevelDefinitionResponse(
            level.Id,
            level.Name,
            level.Description,
            level.MinimumXp,
            level.DisplayOrder,
            level.RarityId,
            ToRarityResponse(level.RarityId, level.RarityName, level.RarityPrimaryColor, level.RaritySecondaryColor),
            level.BadgeImageUrl,
            level.GuardianImageUrl,
            level.IsActive,
            OfficialLevelDefinitionIds.Contains(level.Id),
            level.CurrentUserCount,
            level.CreatedAt,
            level.UpdatedAt);
    }

    private static LevelRarityResponse ToRarityResponse(Guid id, string name, string primaryColor, string? secondaryColor)
    {
        return new LevelRarityResponse(id, name, primaryColor, secondaryColor);
    }
}
