using Forge.Application.Interfaces;
using Forge.Domain.Entities;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Repositories;

public class WeightRecordRepository(ForgeDbContext dbContext) : IWeightRecordRepository
{
    public async Task<IReadOnlyCollection<WeightRecord>> GetByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.WeightRecords
            .AsNoTracking()
            .Where(weightRecord => weightRecord.UserProfileId == userProfileId)
            .OrderByDescending(weightRecord => weightRecord.RecordDate)
            .ThenByDescending(weightRecord => weightRecord.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<WeightRecord?> GetLatestByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.WeightRecords
            .AsNoTracking()
            .Where(weightRecord => weightRecord.UserProfileId == userProfileId)
            .OrderByDescending(weightRecord => weightRecord.RecordDate)
            .ThenByDescending(weightRecord => weightRecord.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<WeightRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.WeightRecords
            .FirstOrDefaultAsync(weightRecord => weightRecord.Id == id, cancellationToken);
    }

    public async Task AddAsync(WeightRecord weightRecord, CancellationToken cancellationToken = default)
    {
        await dbContext.WeightRecords.AddAsync(weightRecord, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(WeightRecord weightRecord, CancellationToken cancellationToken = default)
    {
        dbContext.WeightRecords.Remove(weightRecord);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
