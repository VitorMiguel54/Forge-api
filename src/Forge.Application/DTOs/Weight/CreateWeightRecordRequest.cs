namespace Forge.Application.DTOs.Weight;

public record CreateWeightRecordRequest(
    Guid UserProfileId,
    decimal Weight,
    DateTime RecordDate);
