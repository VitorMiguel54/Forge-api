using Forge.Domain.Entities;

namespace Forge.Application.Interfaces;

public interface IUserProfileRepository
{
    Task<IReadOnlyCollection<UserProfile>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, Guid? ignoredUserProfileId = null, CancellationToken cancellationToken = default);
    Task<bool> HasCustomExercisesAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserProfile userProfile, CancellationToken cancellationToken = default);
    Task DeleteAsync(UserProfile userProfile, CancellationToken cancellationToken = default);
}
