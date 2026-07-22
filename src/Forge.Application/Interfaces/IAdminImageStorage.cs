using Forge.Application.Models;

namespace Forge.Application.Interfaces;

public interface IAdminImageStorage
{
    Task<StoredFileResult> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);

    string? GetStorageKeyFromPublicUrl(string? publicUrl);
}
