namespace Forge.Application.Models;

public record LevelProgressionData(
    LevelDefinitionData? CurrentLevel,
    LevelDefinitionData? NextLevel,
    int TotalXp,
    int NumericLevel,
    int XpInCurrentLevel,
    int XpToNextLevel,
    decimal ProgressPercentage,
    bool IsMaximumLevel);
