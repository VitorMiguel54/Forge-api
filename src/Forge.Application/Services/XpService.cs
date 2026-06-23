using Forge.Application.DTOs.XP;
using Forge.Application.Interfaces;

namespace Forge.Application.Services;

public class XpService : IXpService
{
    public Task<IReadOnlyCollection<XpTransactionResponse>> GetTransactionsByUserAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
