using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace CulinaryBlog.Infrastructure.Storage;

public sealed class MinioFileStorageService : IFileStorageService
{
    private const int MaxDeleteRetries = 3;
    private static readonly TimeSpan InitialRetryDelay = TimeSpan.FromMilliseconds(200);

    private readonly IMinioClient _client;
    private readonly string _bucket;
    private readonly string _publicBaseUrl;
    private readonly bool _publicRead;
    private readonly ILogger<MinioFileStorageService> _logger;

    public MinioFileStorageService(
        IConfiguration configuration,
        ILogger<MinioFileStorageService> logger)
    {
        _logger = logger;
        var endpoint = configuration["Minio:Endpoint"];
        var accessKey = configuration["Minio:AccessKey"];
        var secretKey = configuration["Minio:SecretKey"];
        _bucket = configuration["Minio:BucketName"]
            ?? configuration["Minio:Bucket"]
            ?? string.Empty;
        _publicBaseUrl = configuration["Minio:PublicBaseUrl"] ?? string.Empty;
        _publicRead = bool.TryParse(configuration["Minio:PublicRead"], out var publicRead)
            && publicRead;

        if (string.IsNullOrWhiteSpace(endpoint)
            || string.IsNullOrWhiteSpace(accessKey)
            || string.IsNullOrWhiteSpace(secretKey)
            || string.IsNullOrWhiteSpace(_bucket)
            || string.IsNullOrWhiteSpace(_publicBaseUrl))
        {
            throw new StorageUnavailableException(
                "MinIO endpoint, credentials, bucket, and public base URL must be configured.");
        }

        var useSsl = bool.TryParse(configuration["Minio:UseSSL"], out var ssl) && ssl;
        _client = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(useSsl)
            .Build();
    }

    /// <summary>
    /// Uploads a file to the configured MinIO bucket and returns its public URL.
    /// </summary>
    public async Task<string> UploadAsync(
        Stream content,
        string objectKey,
        long size,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var bucketExists = await _client.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_bucket), cancellationToken);
            if (!bucketExists)
            {
                await _client.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_bucket), cancellationToken);
            }

            if (_publicRead)
            {
                var policy = JsonSerializer.Serialize(new
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
                await _client.SetPolicyAsync(
                    new SetPolicyArgs().WithBucket(_bucket).WithPolicy(policy),
                    cancellationToken);
            }

            await _client.PutObjectAsync(
                new PutObjectArgs()
                    .WithBucket(_bucket)
                    .WithObject(objectKey)
                    .WithStreamData(content)
                    .WithObjectSize(size)
                    .WithContentType(contentType),
                cancellationToken);

            var encodedKey = string.Join(
                "/",
                objectKey.Split('/').Select(Uri.EscapeDataString));
            return $"{_publicBaseUrl.TrimEnd('/')}/{encodedKey}";
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new StorageUnavailableException(
                "The image could not be uploaded to MinIO.", exception);
        }
    }

    /// <summary>
    /// Deletes an object from MinIO, retrying transient failures with exponential backoff.
    /// </summary>
    /// <param name="objectKey">The object key of the file to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <exception cref="StorageUnavailableException">
    /// MinIO could not delete the object after the configured retries.
    /// </exception>
    public async Task DeleteAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        objectKey = ResolveObjectKey(objectKey);

        for (var attempt = 0; ; attempt++)
        {
            try
            {
                await _client.RemoveObjectAsync(
                    new RemoveObjectArgs().WithBucket(_bucket).WithObject(objectKey),
                    cancellationToken);
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                if (IsTransient(exception, cancellationToken) && attempt < MaxDeleteRetries)
                {
                    var retryNumber = attempt + 1;
                    var delay = TimeSpan.FromMilliseconds(
                        InitialRetryDelay.TotalMilliseconds * Math.Pow(2, attempt));

                    _logger.LogWarning(
                        exception,
                        "Transient MinIO delete failure for object {ObjectKey}; retry {RetryNumber}/{MaxRetries} in {RetryDelay}.",
                        objectKey,
                        retryNumber,
                        MaxDeleteRetries,
                        delay);

                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                _logger.LogError(
                    exception,
                    "MinIO delete failed for object {ObjectKey} after {AttemptCount} attempt(s).",
                    objectKey,
                    attempt + 1);
                throw new StorageUnavailableException(
                    "The image could not be deleted from MinIO.", exception);
            }
        }
    }

    private static bool IsTransient(
        Exception exception,
        CancellationToken cancellationToken)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if ((current is HttpRequestException
                or IOException
                or SocketException
                or TimeoutException)
                || current is OperationCanceledException && !cancellationToken.IsCancellationRequested)
            {
                return true;
            }

            if (current is MinioException
                && !current.GetType().Name.Contains("Authorization", StringComparison.OrdinalIgnoreCase)
                && !current.GetType().Name.Contains("Invalid", StringComparison.OrdinalIgnoreCase)
                && !current.GetType().Name.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public async Task<byte[]> ReadAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var key = ResolveObjectKey(fileUrl);
        using var buffer = new MemoryStream();
        try
        {
            await _client.GetObjectAsync(new GetObjectArgs().WithBucket(_bucket)
                .WithObject(key).WithCallbackStream(stream => stream.CopyTo(buffer)), cancellationToken);
            return buffer.ToArray();
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { throw new StorageUnavailableException("The image could not be read from MinIO.", ex); }
    }

    public async Task<string> UploadAsync(ReadOnlyMemory<byte> content, string extension,
        string folder, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(extension) || extension.Contains('/') || extension.Contains('\\'))
            throw new ArgumentException("A valid extension is required.", nameof(extension));
        var key = $"{folder.Trim('/')}/{Guid.NewGuid():N}{extension}";
        using var stream = new MemoryStream(content.ToArray());
        var contentType = extension.ToLowerInvariant() switch
        {
            ".webp" => "image/webp", ".png" => "image/png", ".gif" => "image/gif", ".avif" => "image/avif",
            _ => "image/jpeg"
        };
        return await UploadAsync(stream, key, content.Length, contentType, cancellationToken);
    }

    private string ResolveObjectKey(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)) return value;
        var baseUri = new Uri(_publicBaseUrl.TrimEnd('/') + "/");
        if (!baseUri.IsBaseOf(uri)) throw new ArgumentException("The file URL is outside the configured bucket.", nameof(value));
        return Uri.UnescapeDataString(baseUri.MakeRelativeUri(uri).ToString());
    }
}
