namespace Forge.Application.DTOs.XP;

public record XpSummaryResponse(
    Guid UserProfileId,
    int TotalXp,
    int Level,
    int XpToNextLevel,
    IReadOnlyCollection<XpTransactionResponse> Transactions,
    string? LevelName = null,
    string? LevelDescription = null,
    string? LevelBadgeImageUrl = null,
    string? GuardianImageUrl = null,
    string? Rarity = null,
    string? NextLevelName = null,
    decimal ProgressPercentage = 0,
    bool IsMaximumLevel = false);
