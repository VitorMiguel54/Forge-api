namespace Forge.Application.DTOs.Weight;

public record CreateWeightRecordRequest(
    decimal Weight,
    DateTime RecordDate);
