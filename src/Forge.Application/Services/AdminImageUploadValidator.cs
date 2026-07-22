namespace Forge.Application.Services;

internal static class AdminImageUploadValidator
{
    public const long MaxFileSize = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg",
        "image/webp"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".webp"
    };

    public static void Validate(string fileName, string contentType, long fileSize, Stream stream)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Image file is required.");
        }

        if (fileSize <= 0)
        {
            throw new ArgumentException("Image file cannot be empty.");
        }

        if (fileSize > MaxFileSize)
        {
            throw new ArgumentException("Image file exceeds the 10 MB limit.");
        }

        if (!AllowedContentTypes.Contains(contentType))
        {
            throw new ArgumentException("Image format is not supported. Use PNG, JPG or WebP.");
        }

        var extension = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException("Image extension is not supported. Use PNG, JPG or WebP.");
        }

        if (!HasValidSignature(contentType, stream))
        {
            throw new ArgumentException("Image content does not match the informed format.");
        }
    }

    private static bool HasValidSignature(string contentType, Stream stream)
    {
        if (!stream.CanSeek)
        {
            return true;
        }

        Span<byte> header = stackalloc byte[12];
        var originalPosition = stream.Position;
        stream.Position = 0;
        var bytesRead = stream.Read(header);
        stream.Position = originalPosition;

        return contentType.ToLowerInvariant() switch
        {
            "image/png" => bytesRead >= 8
                && header[0] == 0x89
                && header[1] == 0x50
                && header[2] == 0x4E
                && header[3] == 0x47
                && header[4] == 0x0D
                && header[5] == 0x0A
                && header[6] == 0x1A
                && header[7] == 0x0A,
            "image/jpeg" => bytesRead >= 3
                && header[0] == 0xFF
                && header[1] == 0xD8
                && header[2] == 0xFF,
            "image/webp" => bytesRead >= 12
                && header[0] == 0x52
                && header[1] == 0x49
                && header[2] == 0x46
                && header[3] == 0x46
                && header[8] == 0x57
                && header[9] == 0x45
                && header[10] == 0x42
                && header[11] == 0x50,
            _ => false
        };
    }
}
