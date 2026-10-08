using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;

namespace CulinaryBlog.Infrastructure.Services;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadsRoot;

    public LocalFileStorageService(IHostEnvironment environment)
    {
        _uploadsRoot = Path.GetFullPath(
            Path.Combine(
                environment.ContentRootPath,
                "wwwroot",
                "uploads"));
    }

    public async Task<byte[]> ReadAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(fileUrl);
        return await File.ReadAllBytesAsync(path, cancellationToken);
    }

    public async Task<string> UploadAsync(
        ReadOnlyMemory<byte> content,
        string extension,
        string folder,
        CancellationToken cancellationToken = default)
    {
        var safeFolder = SanitizeSegment(folder);
        var safeExtension = SanitizeExtension(extension);
        var destinationDirectory = Path.Combine(_uploadsRoot, safeFolder);
        Directory.CreateDirectory(destinationDirectory);

        var fileName = $"{Guid.NewGuid():N}{safeExtension}";
        var destinationPath = Path.Combine(destinationDirectory, fileName);
        await File.WriteAllBytesAsync(
            destinationPath,
            content.ToArray(),
            cancellationToken);

        return $"/uploads/{safeFolder}/{fileName}";
    }

    public Task DeleteAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = ResolvePath(fileUrl);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private string ResolvePath(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            throw new ArgumentException(
                "A file URL is required.",
                nameof(fileUrl));
        }

        var relativePath = fileUrl
            .Replace('\\', '/')
            .TrimStart('/')
            .Split('/');
        if (relativePath.Length < 3
            || !string.Equals(
                relativePath[0],
                "uploads",
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The file URL must point to an uploads path.",
                nameof(fileUrl));
        }

        var fullPath = Path.GetFullPath(
            Path.Combine(
                _uploadsRoot,
                Path.Combine(relativePath.Skip(1).ToArray())));
        var relativeToRoot = Path.GetRelativePath(_uploadsRoot, fullPath);
        if (relativeToRoot == "."
            || relativeToRoot.StartsWith(
                $"..{Path.DirectorySeparatorChar}",
                StringComparison.Ordinal)
            || Path.IsPathRooted(relativeToRoot))
        {
            throw new ArgumentException(
                "The file URL points outside the uploads directory.",
                nameof(fileUrl));
        }

        return fullPath;
    }

    private static string SanitizeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value is "." or ".."
            || value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || value.Contains('/') || value.Contains('\\'))
        {
            throw new ArgumentException(
                "Folder must be a single valid path segment.",
                nameof(value));
        }

        return value;
    }

    private static string SanitizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException(
                "A file extension is required.",
                nameof(extension));
        }

        var normalized = extension.StartsWith('.')
            ? extension
            : $".{extension}";
        if (normalized.Length > 12
            || normalized[1..].Any(character =>
                !char.IsAsciiLetterOrDigit(character)))
        {
            throw new ArgumentException(
                "File extension contains unsupported characters.",
                nameof(extension));
        }

        return normalized.ToLowerInvariant();
    }
}
