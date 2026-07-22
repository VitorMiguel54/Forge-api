namespace Forge.Application.DTOs.Backoffice.Rarities;

public record UpdateBackofficeRarityRequest(
    string Name,
    string PrimaryColor,
    string? SecondaryColor,
    int DisplayOrder);
