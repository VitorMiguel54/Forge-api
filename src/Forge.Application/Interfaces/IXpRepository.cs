using Forge.Domain.Entities;
using Forge.Domain.Enums;

namespace Forge.Application.Interfaces;

public interface IXpRepository
{
    Task<IReadOnlyCollection<XpTransaction>> GetByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid userProfileId,
        XpSource source,
        Guid? referenceId,
        CancellationToken cancellationToken = default);

    Task AddAsync(XpTransaction xpTransaction, CancellationToken cancellationToken = default);
}
