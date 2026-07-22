namespace Forge.Application.DTOs.Weight;

public record WeightRecordResponse(
    Guid Id,
    Guid UserProfileId,
    decimal Weight,
    DateTime RecordDate);
