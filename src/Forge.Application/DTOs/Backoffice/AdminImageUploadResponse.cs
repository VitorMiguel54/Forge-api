namespace Forge.Application.DTOs.Backoffice;

public record AdminImageUploadResponse(
    Guid EntityId,
    string FieldName,
    string Url,
    string ContentType,
    long FileSize,
    DateTime UpdatedAt);
