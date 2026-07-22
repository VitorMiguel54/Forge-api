namespace Forge.Application.DTOs.Levels;

public record LevelRarityResponse(
    Guid Id,
    string Name,
    string PrimaryColor,
    string? SecondaryColor);
