using Forge.Application.Interfaces;
using Forge.Application.Models;
using Microsoft.Extensions.Options;

namespace Forge.Infrastructure.Storage;

public class LocalAdminImageStorage(IOptions<AdminImageStorageOptions> options) : IAdminImageStorage
{
    private readonly AdminImageStorageOptions options = options.Value;

    public async Task<StoredFileResult> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.RootPath))
        {
            throw new InvalidOperationException("Admin image storage root path is not configured.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var safeFolder = NormalizeRelativePath(folder);
        var storageKey = $"{safeFolder}/{storedFileName}";
        var absolutePath = GetSafeAbsolutePath(storageKey);
        var directory = Path.GetDirectoryName(absolutePath)
            ?? throw new InvalidOperationException("Invalid storage directory.");

        Directory.CreateDirectory(directory);

        await using var output = File.Create(absolutePath);
        await stream.CopyToAsync(output, cancellationToken);

        var publicUrl = $"{options.PublicBasePath.TrimEnd('/')}/{storageKey}";

        return new StoredFileResult(storageKey, publicUrl, storedFileName, contentType, output.Length);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return Task.CompletedTask;
        }

        var absolutePath = GetSafeAbsolutePath(storageKey);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }

    private string GetSafeAbsolutePath(string storageKey)
    {
        var root = Path.GetFullPath(options.RootPath);
        var relativePath = NormalizeRelativePath(storageKey).Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath));

        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid storage path.");
        }

        return fullPath;
    }

    private static string NormalizeRelativePath(string path)
    {
        var normalized = path.Replace('\\', '/').Trim('/');
        if (string.IsNullOrWhiteSpace(normalized)
            || normalized.Contains("..", StringComparison.Ordinal)
            || Path.IsPathRooted(normalized))
        {
            throw new InvalidOperationException("Invalid storage path.");
        }

        return normalized;
    }
}
