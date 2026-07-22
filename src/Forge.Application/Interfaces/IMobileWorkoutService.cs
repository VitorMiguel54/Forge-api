using Forge.Application.DTOs.Mobile.Workouts;

namespace Forge.Application.Interfaces;

public interface IMobileWorkoutService
{
    Task<MobileWorkoutsResponse?> GetAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
