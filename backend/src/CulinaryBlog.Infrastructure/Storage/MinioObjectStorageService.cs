using System.Text.Json;
using CulinaryBlog.Application.Common.Interfaces;
using Minio;
using Minio.DataModel.Args;

namespace CulinaryBlog.Infrastructure.Storage;

/// <summary>
/// Stores recipe images in MinIO and returns stable, browser-readable URLs.
/// Recipe images are intentionally public because published recipes display them
/// without an authenticated image proxy.
/// </summary>
public sealed class MinioObjectStorageService : IFileStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucket;
    private readonly Uri _publicEndpoint;
    private readonly SemaphoreSlim _bucketLock = new(1, 1);
    private bool _bucketReady;

    public MinioObjectStorageService(
        IMinioClient client,
        string bucket,
        string publicEndpoint)
    {
        _client = client;
        _bucket = ValidateBucket(bucket);
        _publicEndpoint = new Uri(
            publicEndpoint.EndsWith('/') ? publicEndpoint : $"{publicEndpoint}/",
            UriKind.Absolute);
        if (_publicEndpoint.Scheme is not ("http" or "https")
            || _publicEndpoint.AbsolutePath != "/"
            || _publicEndpoint.UserInfo.Length > 0
            || _publicEndpoint.Query.Length > 0
            || _publicEndpoint.Fragment.Length > 0)
        {
            throw new ArgumentException("The MinIO public endpoint must be an HTTP(S) origin.", nameof(publicEndpoint));
        }
    }

    public async Task<byte[]> ReadAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        var objectName = GetObjectName(fileUrl);
        await using var output = new MemoryStream();
        var args = new GetObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithCallbackStream((source, token) =>
                source.CopyToAsync(output, token));

        await _client.GetObjectAsync(args, cancellationToken);
        return output.ToArray();
    }

    public async Task<string> UploadAsync(
        ReadOnlyMemory<byte> content,
        string extension,
        string folder,
        CancellationToken cancellationToken = default)
    {
        if (content.IsEmpty)
        {
            throw new ArgumentException("File content cannot be empty.", nameof(content));
        }

        if (string.IsNullOrWhiteSpace(extension)
            || extension.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || extension.Contains('/')
            || extension.Contains('\\')
            || GetContentType(extension) == "application/octet-stream")
        {
            throw new ArgumentException("A valid file extension is required.", nameof(extension));
        }

        var objectName = BuildObjectName(folder, $"{Guid.NewGuid():N}{extension}");
        await EnsureBucketAsync(cancellationToken);

        await using var stream = new MemoryStream(content.ToArray(), writable: false);
        var args = new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(GetContentType(extension));

        await _client.PutObjectAsync(args, cancellationToken);
        return BuildPublicUrl(objectName);
    }

    public async Task DeleteAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        var objectName = GetObjectName(fileUrl);
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await _client.RemoveObjectAsync(
                    new RemoveObjectArgs()
                        .WithBucket(_bucket)
                        .WithObject(objectName),
                    cancellationToken);
                return;
            }
            catch (Exception) when (attempt < 3 && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), cancellationToken);
            }
        }
    }

    private async Task EnsureBucketAsync(CancellationToken cancellationToken)
    {
        if (_bucketReady)
        {
            return;
        }

        await _bucketLock.WaitAsync(cancellationToken);
        try
        {
            if (_bucketReady)
            {
                return;
            }

            var exists = await _client.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_bucket),
                cancellationToken);
            if (!exists)
            {
                await _client.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_bucket),
                    cancellationToken);
            }

            await _client.SetPolicyAsync(
                new SetPolicyArgs()
                    .WithBucket(_bucket)
                    .WithPolicy(CreatePublicReadPolicy()),
                cancellationToken);

            _bucketReady = true;
        }
        finally
        {
            _bucketLock.Release();
        }
    }

    private string BuildPublicUrl(string objectName)
    {
        var escapedObjectName = string.Join(
            '/',
            objectName.Split('/').Select(Uri.EscapeDataString));
        var baseUrl = _publicEndpoint.AbsoluteUri.TrimEnd('/');
        return $"{baseUrl}/{Uri.EscapeDataString(_bucket)}/{escapedObjectName}";
    }

    private string GetObjectName(string fileUrl)
    {
        if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri)
            || !string.Equals(uri.Scheme, _publicEndpoint.Scheme, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(uri.Host, _publicEndpoint.Host, StringComparison.OrdinalIgnoreCase)
            || uri.Port != _publicEndpoint.Port
            || uri.UserInfo.Length > 0
            || uri.Query.Length > 0
            || uri.Fragment.Length > 0)
        {
            throw new ArgumentException(
                "The image URL does not belong to the configured MinIO public endpoint.",
                nameof(fileUrl));
        }

        var path = uri.AbsolutePath.Trim('/');
        var bucketPrefix = $"{_bucket}/";
        if (!path.StartsWith(bucketPrefix, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The image URL does not belong to the configured MinIO bucket.",
                nameof(fileUrl));
        }

        var objectName = Uri.UnescapeDataString(path[bucketPrefix.Length..]);
        if (string.IsNullOrWhiteSpace(objectName)
            || !objectName.StartsWith("recipes/", StringComparison.Ordinal)
            || objectName.Split('/').Any(segment => segment is "" or "." or ".."))
        {
            throw new ArgumentException("The image URL contains an invalid object key.", nameof(fileUrl));
        }

        return objectName;
    }

    private static string BuildObjectName(string folder, string fileName)
    {
        var normalizedFolder = folder.Replace('\\', '/').Trim('/');
        if (string.IsNullOrWhiteSpace(normalizedFolder)
            || normalizedFolder.Split('/').Any(segment => segment is "" or "." or ".."))
        {
            throw new ArgumentException("A valid object folder is required.", nameof(folder));
        }

        return $"{normalizedFolder}/{fileName}";
    }

    private static string ValidateBucket(string bucket)
    {
        if (string.IsNullOrWhiteSpace(bucket)
            || bucket.Length is < 3 or > 63
            || bucket.Any(character =>
                !(char.IsAsciiLetterLower(character)
                    || char.IsAsciiDigit(character)
                    || character is '-' or '.'))
            || bucket[0] is '-' or '.'
            || bucket[^1] is '-' or '.')
        {
            throw new ArgumentException("A valid MinIO bucket name is required.", nameof(bucket));
        }

        return bucket;
    }

    private string CreatePublicReadPolicy() => JsonSerializer.Serialize(new
    {
        Version = "2012-10-17",
        Statement = new[]
        {
            new
            {
                Effect = "Allow",
                Principal = new { AWS = new[] { "*" } },
                Action = new[] { "s3:GetObject" },
                Resource = new[] { $"arn:aws:s3:::{_bucket}/*" }
            }
        }
    });

    private static string GetContentType(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".avif" => "image/avif",
            _ => "application/octet-stream"
        };
}
