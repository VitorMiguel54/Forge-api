namespace Forge.Application.DTOs.Water;

public record CreateWaterIntakeRequest(
    Guid UserProfileId,
    DateTime IntakeDate,
    decimal Liters,
    decimal GoalInLiters);
