namespace Forge.Application.Services;

internal static class AdminExerciseMediaUploadValidator
{
    private static readonly IReadOnlyDictionary<string, MediaRule> Rules = new Dictionary<string, MediaRule>(StringComparer.OrdinalIgnoreCase)
    {
        ["image"] = new(
            MaxFileSize: 5 * 1024 * 1024,
            AllowedContentTypes: ["image/png", "image/jpeg", "image/webp"],
            AllowedExtensions: [".png", ".jpg", ".jpeg", ".webp"],
            Label: "Image"),
        ["thumbnail"] = new(
            MaxFileSize: 2 * 1024 * 1024,
            AllowedContentTypes: ["image/png", "image/jpeg", "image/webp"],
            AllowedExtensions: [".png", ".jpg", ".jpeg", ".webp"],
            Label: "Thumbnail"),
        ["gif"] = new(
            MaxFileSize: 15 * 1024 * 1024,
            AllowedContentTypes: ["image/gif"],
            AllowedExtensions: [".gif"],
            Label: "GIF"),
        ["video"] = new(
            MaxFileSize: 50 * 1024 * 1024,
            AllowedContentTypes: ["video/mp4", "video/webm"],
            AllowedExtensions: [".mp4", ".webm"],
            Label: "Video")
    };

    public static void Validate(string mediaType, string fileName, string contentType, long fileSize, Stream stream)
    {
        var rule = GetRule(mediaType);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException($"{rule.Label} file is required.");
        }

        if (fileSize <= 0)
        {
            throw new ArgumentException($"{rule.Label} file cannot be empty.");
        }

        if (fileSize > rule.MaxFileSize)
        {
            throw new ArgumentException($"{rule.Label} file exceeds the {rule.MaxFileSize / 1024 / 1024} MB limit.");
        }

        if (!rule.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"{rule.Label} format is not supported.");
        }

        var extension = Path.GetExtension(fileName);
        if (!rule.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"{rule.Label} extension is not supported.");
        }

        if (!HasValidSignature(contentType, stream))
        {
            throw new ArgumentException($"{rule.Label} content does not match the informed format.");
        }
    }

    public static string NormalizeMediaType(string mediaType)
    {
        if (!Rules.ContainsKey(mediaType))
        {
            throw new ArgumentException("Exercise media type is not supported.");
        }

        return mediaType.ToLowerInvariant();
    }

    private static MediaRule GetRule(string mediaType)
    {
        if (!Rules.TryGetValue(mediaType, out var rule))
        {
            throw new ArgumentException("Exercise media type is not supported.");
        }

        return rule;
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
            "image/gif" => bytesRead >= 6
                && header[0] == 0x47
                && header[1] == 0x49
                && header[2] == 0x46
                && header[3] == 0x38
                && (header[4] == 0x37 || header[4] == 0x39)
                && header[5] == 0x61,
            "video/mp4" => bytesRead >= 12
                && header[4] == 0x66
                && header[5] == 0x74
                && header[6] == 0x79
                && header[7] == 0x70,
            "video/webm" => bytesRead >= 4
                && header[0] == 0x1A
                && header[1] == 0x45
                && header[2] == 0xDF
                && header[3] == 0xA3,
            _ => false
        };
    }

    private sealed record MediaRule(
        long MaxFileSize,
        IReadOnlyCollection<string> AllowedContentTypes,
        IReadOnlyCollection<string> AllowedExtensions,
        string Label);
}
