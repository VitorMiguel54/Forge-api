using Forge.Application.DTOs.Mobile.MuscleGroups;
using Forge.Application.Interfaces;

namespace Forge.Application.Services;

public class MobileMuscleGroupService(IMuscleGroupRepository muscleGroupRepository) : IMobileMuscleGroupService
{
    public async Task<IReadOnlyCollection<MobileMuscleGroupResponse>> GetAsync(CancellationToken cancellationToken = default)
    {
        var muscleGroups = await muscleGroupRepository.GetActiveAsync(cancellationToken);

        return muscleGroups
            .OrderBy(muscleGroup => muscleGroup.DisplayOrder)
            .ThenBy(muscleGroup => muscleGroup.DisplayName)
            .Select(muscleGroup => new MobileMuscleGroupResponse(
                muscleGroup.Id,
                muscleGroup.Name,
                muscleGroup.DisplayName,
                muscleGroup.Icon,
                muscleGroup.DisplayOrder))
            .ToArray();
    }
}
