namespace Forge.Domain.Entities;

public class LevelDefinition
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MinimumXp { get; set; }
    public int DisplayOrder { get; set; }
    public Guid RarityId { get; set; }
    public string? BadgeImageUrl { get; set; }
    public string? GuardianImageUrl { get; set; }
    public string? GuardianImageStorageKey { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Rarity Rarity { get; set; } = null!;
}
