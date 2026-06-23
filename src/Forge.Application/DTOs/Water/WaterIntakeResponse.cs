namespace Forge.Application.DTOs.Water;

public record WaterIntakeResponse(
    Guid Id,
    Guid UserProfileId,
    DateTime IntakeDate,
    decimal Liters,
    decimal GoalInLiters,
    bool GoalAchieved);
