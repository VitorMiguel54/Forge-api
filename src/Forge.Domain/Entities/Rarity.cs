namespace Forge.Domain.Entities;

public class Rarity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = string.Empty;
    public string? SecondaryColor { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<LevelDefinition> LevelDefinitions { get; set; } = new List<LevelDefinition>();
}
