namespace Forge.Domain.Entities;

public class WeightRecord
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public decimal Weight { get; set; }
    public DateTime RecordDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
}
