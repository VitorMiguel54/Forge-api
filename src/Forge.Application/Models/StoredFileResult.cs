namespace Forge.Application.Models;

public record StoredFileResult(
    string StorageKey,
    string PublicUrl,
    string StoredFileName,
    string ContentType,
    long FileSize);
