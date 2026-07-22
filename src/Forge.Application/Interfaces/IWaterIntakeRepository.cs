using Forge.Domain.Entities;

namespace Forge.Application.Interfaces;

public interface IWaterIntakeRepository
{
    Task<IReadOnlyCollection<WaterIntake>> GetByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default);

    Task<WaterIntake?> GetTodayByUserProfileAsync(
        Guid userProfileId,
        DateTime utcToday,
        CancellationToken cancellationToken = default);

    Task<decimal> SumTodayByUserProfileAsync(
        Guid userProfileId,
        DateTime utcToday,
        CancellationToken cancellationToken = default);

    Task<WaterIntake?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(WaterIntake waterIntake, CancellationToken cancellationToken = default);
    Task DeleteAsync(WaterIntake waterIntake, CancellationToken cancellationToken = default);
}
