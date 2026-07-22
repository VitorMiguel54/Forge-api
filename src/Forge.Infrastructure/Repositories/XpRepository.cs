using Forge.Application.Interfaces;
using Forge.Domain.Entities;
using Forge.Domain.Enums;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Repositories;

public class XpRepository(ForgeDbContext dbContext) : IXpRepository
{
    public async Task<IReadOnlyCollection<XpTransaction>> GetByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.XpTransactions
            .AsNoTracking()
            .Where(xpTransaction => xpTransaction.UserProfileId == userProfileId)
            .OrderByDescending(xpTransaction => xpTransaction.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid userProfileId,
        XpSource source,
        Guid? referenceId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.XpTransactions
            .AnyAsync(
                xpTransaction =>
                    xpTransaction.UserProfileId == userProfileId
                    && xpTransaction.Source == source
                    && xpTransaction.ReferenceId == referenceId,
                cancellationToken);
    }

    public async Task AddAsync(XpTransaction xpTransaction, CancellationToken cancellationToken = default)
    {
        await dbContext.XpTransactions.AddAsync(xpTransaction, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
