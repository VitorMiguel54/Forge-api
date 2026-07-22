using Forge.Application.Interfaces;
using Forge.Domain.Entities;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Repositories;

public class UserProfileRepository(ForgeDbContext dbContext) : IUserProfileRepository
{
    public async Task<IReadOnlyCollection<UserProfile>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.UserProfiles
            .AsNoTracking()
            .OrderBy(userProfile => userProfile.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserProfiles
            .FirstOrDefaultAsync(userProfile => userProfile.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserProfiles
            .AsNoTracking()
            .AnyAsync(userProfile => userProfile.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        Guid? ignoredUserProfileId = null,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.UserProfiles
            .AsNoTracking()
            .AnyAsync(
                userProfile =>
                    userProfile.Email == email
                    && (ignoredUserProfileId == null || userProfile.Id != ignoredUserProfileId),
                cancellationToken);
    }

    public async Task<bool> HasCustomExercisesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Exercises
            .AsNoTracking()
            .AnyAsync(exercise => exercise.UserProfileId == id, cancellationToken);
    }

    public async Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken = default)
    {
        await dbContext.UserProfiles.AddAsync(userProfile, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserProfile userProfile, CancellationToken cancellationToken = default)
    {
        dbContext.UserProfiles.Update(userProfile);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(UserProfile userProfile, CancellationToken cancellationToken = default)
    {
        dbContext.UserProfiles.Remove(userProfile);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
