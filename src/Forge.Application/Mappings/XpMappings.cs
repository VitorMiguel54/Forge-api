using Forge.Application.DTOs.XP;
using Forge.Domain.Entities;

namespace Forge.Application.Mappings;

public static class XpMappings
{
    public static XpTransactionResponse ToResponse(this XpTransaction xpTransaction)
    {
        return new XpTransactionResponse(
            xpTransaction.Id,
            xpTransaction.UserProfileId,
            xpTransaction.Amount,
            xpTransaction.Source,
            xpTransaction.Description,
            xpTransaction.ReferenceId,
            xpTransaction.CreatedAt);
    }
}
