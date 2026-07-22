namespace Forge.Application.DTOs.Water;

public record CreateWaterIntakeRequest(
    DateTime IntakeDate,
    decimal Liters);
