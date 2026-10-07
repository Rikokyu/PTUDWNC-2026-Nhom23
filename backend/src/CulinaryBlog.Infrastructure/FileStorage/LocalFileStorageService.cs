using CulinaryBlog.Application.Common.Interfaces;

namespace CulinaryBlog.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadRoot;
    private readonly string _publicBasePath;

    public LocalFileStorageService(
        string uploadRoot,
        string publicBasePath)
    {
        _uploadRoot = Path.GetFullPath(uploadRoot);
        _publicBasePath = "/" + publicBasePath.Trim('/');
        Directory.CreateDirectory(_uploadRoot);
    }

    public async Task<byte[]> ReadAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        var filePath = ResolveFilePath(fileUrl);
        return await File.ReadAllBytesAsync(filePath, cancellationToken);
    }

    public async Task<string> UploadAsync(
        ReadOnlyMemory<byte> content,
        string extension,
        string folder,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(extension)
            || extension.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || extension.Contains('/') || extension.Contains('\\'))
        {
            throw new ArgumentException(
                "A valid file extension is required.",
                nameof(extension));
        }

        var relativeFolder = folder.Replace('\\', '/').Trim('/');
        var directory = ResolveFilePath(relativeFolder);
        Directory.CreateDirectory(directory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(directory, fileName);
        await File.WriteAllBytesAsync(
            filePath,
            content.ToArray(),
            cancellationToken);

        var relativePath = Path.GetRelativePath(_uploadRoot, filePath)
            .Replace('\\', '/');
        return $"{_publicBasePath}/{relativePath}";
    }

    public Task DeleteAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var filePath = ResolveFilePath(fileUrl);
        File.Delete(filePath);
        return Task.CompletedTask;
    }

    private string ResolveFilePath(string pathOrUrl)
    {
        var relativePath = pathOrUrl;
        if (Uri.TryCreate(pathOrUrl, UriKind.Absolute, out var uri))
        {
            relativePath = uri.AbsolutePath;
        }

        relativePath = Uri.UnescapeDataString(relativePath)
            .Replace('\\', '/');

        var publicPrefix = _publicBasePath + "/";
        if (relativePath.StartsWith(
                publicPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            relativePath = relativePath[publicPrefix.Length..];
        }

        var fullPath = Path.GetFullPath(
            Path.Combine(_uploadRoot, relativePath));
        var rootWithSeparator = _uploadRoot.EndsWith(
            Path.DirectorySeparatorChar)
            ? _uploadRoot
            : _uploadRoot + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(
                rootWithSeparator,
                StringComparison.OrdinalIgnoreCase)
            && !string.Equals(
                fullPath,
                _uploadRoot,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "The file path is outside the upload directory.",
                nameof(pathOrUrl));
        }

        return fullPath;
    }
}
