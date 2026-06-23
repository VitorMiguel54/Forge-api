using Forge.Application.DTOs.XP;

namespace Forge.Application.Interfaces;

public interface IXpService
{
    Task<IReadOnlyCollection<XpTransactionResponse>> GetTransactionsByUserAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
