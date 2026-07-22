using Forge.Application.Interfaces;
using Forge.Domain.Entities;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Repositories;

public class SleepRecordRepository(ForgeDbContext dbContext) : ISleepRecordRepository
{
    public async Task<IReadOnlyCollection<SleepRecord>> GetByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.SleepRecords
            .AsNoTracking()
            .Where(sleepRecord => sleepRecord.UserProfileId == userProfileId)
            .OrderByDescending(sleepRecord => sleepRecord.SleepDate)
            .ThenByDescending(sleepRecord => sleepRecord.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<SleepRecord?> GetLatestByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.SleepRecords
            .AsNoTracking()
            .Where(sleepRecord => sleepRecord.UserProfileId == userProfileId)
            .OrderByDescending(sleepRecord => sleepRecord.SleepDate)
            .ThenByDescending(sleepRecord => sleepRecord.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SleepRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.SleepRecords
            .FirstOrDefaultAsync(sleepRecord => sleepRecord.Id == id, cancellationToken);
    }

    public async Task AddAsync(SleepRecord sleepRecord, CancellationToken cancellationToken = default)
    {
        await dbContext.SleepRecords.AddAsync(sleepRecord, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(SleepRecord sleepRecord, CancellationToken cancellationToken = default)
    {
        dbContext.SleepRecords.Remove(sleepRecord);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
