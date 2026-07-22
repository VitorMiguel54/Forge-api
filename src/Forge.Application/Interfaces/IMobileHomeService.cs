using Forge.Application.DTOs.Mobile.Home;

namespace Forge.Application.Interfaces;

public interface IMobileHomeService
{
    Task<MobileHomeResponse?> GetAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
