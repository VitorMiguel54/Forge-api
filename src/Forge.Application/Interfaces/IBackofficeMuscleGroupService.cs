using Forge.Application.DTOs.Backoffice.MuscleGroups;

namespace Forge.Application.Interfaces;

public interface IBackofficeMuscleGroupService
{
    Task<IReadOnlyCollection<BackofficeMuscleGroupResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BackofficeMuscleGroupResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BackofficeMuscleGroupResponse> CreateAsync(CreateBackofficeMuscleGroupRequest request, CancellationToken cancellationToken = default);
    Task<BackofficeMuscleGroupResponse?> UpdateAsync(Guid id, UpdateBackofficeMuscleGroupRequest request, CancellationToken cancellationToken = default);
    Task<BackofficeMuscleGroupResponse?> UpdateStatusAsync(Guid id, UpdateBackofficeMuscleGroupStatusRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
