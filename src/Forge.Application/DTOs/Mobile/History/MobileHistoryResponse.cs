namespace Forge.Application.DTOs.Mobile.History;

public record MobileHistoryResponse(
    MobileHistorySummaryResponse Summary,
    MobileHistoryPageResponse Page,
    IReadOnlyCollection<MobileHistoryWorkoutResponse> Workouts);

public record MobileHistorySummaryResponse(
    int Workouts,
    int TotalDurationMinutes,
    decimal WeeklyVolume);

public record MobileHistoryPageResponse(
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public record MobileHistoryWorkoutResponse(
    Guid Id,
    string Name,
    DateTime Date,
    int DurationMinutes,
    decimal Volume,
    int ExerciseCount,
    IReadOnlyCollection<string> MuscleGroups);
