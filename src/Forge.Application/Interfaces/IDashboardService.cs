using Forge.Application.DTOs.Dashboard;

namespace Forge.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardResponse?> GetAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
