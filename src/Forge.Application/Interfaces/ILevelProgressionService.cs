using Forge.Application.Models;

namespace Forge.Application.Interfaces;

public interface ILevelProgressionService
{
    Task<LevelProgressionData> GetProgressionAsync(int totalXp, CancellationToken cancellationToken = default);
}
