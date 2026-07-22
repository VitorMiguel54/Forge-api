using Forge.Application.DTOs.Mobile.MuscleGroups;

namespace Forge.Application.Interfaces;

public interface IMobileMuscleGroupService
{
    Task<IReadOnlyCollection<MobileMuscleGroupResponse>> GetAsync(CancellationToken cancellationToken = default);
}
