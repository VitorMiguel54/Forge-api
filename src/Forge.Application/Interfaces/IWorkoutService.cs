using Forge.Application.DTOs.Workout;

namespace Forge.Application.Interfaces;

public interface IWorkoutService
{
    Task<WorkoutResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkoutAnalysisResponse?> GetAnalysisByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<WorkoutResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WorkoutResponse> CreateAsync(CreateWorkoutRequest request, CancellationToken cancellationToken = default);
    Task<WorkoutResponse> CreatePlanAsync(CreateWorkoutPlanRequest request, CancellationToken cancellationToken = default);
    Task<WorkoutResponse?> UpdateAsync(Guid id, UpdateWorkoutRequest request, CancellationToken cancellationToken = default);
    Task<WorkoutResponse?> StartAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkoutResponse?> FinishAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkoutResponse?> CancelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task ReorderAsync(ReorderWorkoutsRequest request, CancellationToken cancellationToken = default);
}
