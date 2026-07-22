namespace Forge.Application.DTOs.Workout;

public record ReorderWorkoutsRequest(
    Guid UserProfileId,
    IReadOnlyCollection<ReorderWorkoutItemRequest>? Items);

public record ReorderWorkoutItemRequest(
    Guid WorkoutId,
    int DisplayOrder);
