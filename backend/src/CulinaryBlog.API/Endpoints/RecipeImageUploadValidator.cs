using CulinaryBlog.Application.Common.Exceptions;

namespace CulinaryBlog.API.Endpoints;

public static class RecipeImageUploadValidator
{
    private static readonly IReadOnlyDictionary<string, string> AllowedTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp",
            ["image/avif"] = ".avif"
        };

    public static string Validate(
        string fileName,
        string contentType,
        long fileSize,
        long maxFileSize,
        ReadOnlySpan<byte> signature)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 255)
        {
            throw new ValidationException("A valid file name is required.");
        }

        if (fileSize <= 0)
        {
            throw new ValidationException("The file cannot be empty.");
        }

        if (maxFileSize <= 0 || fileSize > maxFileSize)
        {
            throw new ValidationException("Image size cannot exceed the configured limit.");
        }

        if (!AllowedTypes.TryGetValue(contentType, out var expectedExtension))
        {
            throw new ValidationException(
                "Only JPEG, PNG, WebP, and AVIF images are accepted.");
        }

        var suppliedExtension = Path.GetExtension(fileName);
        var isJpegExtension = expectedExtension == ".jpg"
            && string.Equals(suppliedExtension, ".jpeg", StringComparison.OrdinalIgnoreCase);
        if (!string.Equals(
                suppliedExtension,
                expectedExtension,
                StringComparison.OrdinalIgnoreCase)
            && !isJpegExtension)
        {
            throw new ValidationException(
                "The file extension does not match its MIME type.");
        }

        if (!HasValidImageSignature(contentType, signature))
        {
            throw new ValidationException(
                "The uploaded file content does not match its MIME type.");
        }

        return isJpegExtension ? ".jpeg" : expectedExtension;
    }

    private static bool HasValidImageSignature(
        string contentType,
        ReadOnlySpan<byte> bytes) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" => bytes.Length >= 3
            && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF,
        "image/png" => bytes.Length >= 8
            && bytes[..8].SequenceEqual(
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
        "image/webp" => bytes.Length >= 12
            && bytes[..4].SequenceEqual("RIFF"u8)
            && bytes.Slice(8, 4).SequenceEqual("WEBP"u8),
        "image/avif" => bytes.Length >= 12
            && bytes.Slice(4, 4).SequenceEqual("ftyp"u8)
            && (bytes.Slice(8, 4).SequenceEqual("avif"u8)
                || bytes.Slice(8, 4).SequenceEqual("avis"u8)),
        _ => false
    };
}
