using Forge.Application.Interfaces;
using Forge.Domain.Entities;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Repositories;

public class WaterIntakeRepository(ForgeDbContext dbContext) : IWaterIntakeRepository
{
    public async Task<IReadOnlyCollection<WaterIntake>> GetByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.WaterIntakes
            .AsNoTracking()
            .Where(waterIntake => waterIntake.UserProfileId == userProfileId)
            .OrderByDescending(waterIntake => waterIntake.IntakeDate)
            .ThenByDescending(waterIntake => waterIntake.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<WaterIntake?> GetTodayByUserProfileAsync(
        Guid userProfileId,
        DateTime utcToday,
        CancellationToken cancellationToken = default)
    {
        var utcTomorrow = utcToday.AddDays(1);

        return await dbContext.WaterIntakes
            .AsNoTracking()
            .Where(waterIntake =>
                waterIntake.UserProfileId == userProfileId
                && waterIntake.IntakeDate >= utcToday
                && waterIntake.IntakeDate < utcTomorrow)
            .OrderByDescending(waterIntake => waterIntake.IntakeDate)
            .ThenByDescending(waterIntake => waterIntake.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<decimal> SumTodayByUserProfileAsync(
        Guid userProfileId,
        DateTime utcToday,
        CancellationToken cancellationToken = default)
    {
        var utcTomorrow = utcToday.AddDays(1);

        return await dbContext.WaterIntakes
            .AsNoTracking()
            .Where(waterIntake =>
                waterIntake.UserProfileId == userProfileId
                && waterIntake.IntakeDate >= utcToday
                && waterIntake.IntakeDate < utcTomorrow)
            .SumAsync(waterIntake => waterIntake.Liters, cancellationToken);
    }

    public async Task<WaterIntake?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.WaterIntakes
            .FirstOrDefaultAsync(waterIntake => waterIntake.Id == id, cancellationToken);
    }

    public async Task AddAsync(WaterIntake waterIntake, CancellationToken cancellationToken = default)
    {
        await dbContext.WaterIntakes.AddAsync(waterIntake, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(WaterIntake waterIntake, CancellationToken cancellationToken = default)
    {
        dbContext.WaterIntakes.Remove(waterIntake);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
