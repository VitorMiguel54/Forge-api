using Forge.Application.DTOs.Backoffice.Exercises;

namespace Forge.Application.Interfaces;

public interface IBackofficeExerciseService
{
    Task<BackofficeExerciseListResponse> GetAsync(
        string? search = null,
        Guid? muscleGroupId = null,
        bool? isActive = null,
        string? sortBy = null,
        string? sortDirection = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<BackofficeExerciseResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BackofficeExerciseResponse> CreateAsync(CreateBackofficeExerciseRequest request, CancellationToken cancellationToken = default);
    Task<BackofficeExerciseResponse?> UpdateAsync(Guid id, UpdateBackofficeExerciseRequest request, CancellationToken cancellationToken = default);
    Task<BackofficeExerciseResponse?> UpdateStatusAsync(Guid id, UpdateBackofficeExerciseStatusRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
