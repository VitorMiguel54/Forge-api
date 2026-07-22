using Forge.Domain.Enums;

namespace Forge.Domain.Entities;

public class XpTransaction
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public int Amount { get; set; }
    public XpSource Source { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
}
