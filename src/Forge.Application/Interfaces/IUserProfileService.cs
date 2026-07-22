using Forge.Application.DTOs.UserProfile;

namespace Forge.Application.Interfaces;

public interface IUserProfileService
{
    Task<UserProfileResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserProfileResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserProfileResponse> CreateAsync(CreateUserProfileRequest request, CancellationToken cancellationToken = default);
    Task<UserProfileResponse?> UpdateAsync(Guid id, UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
