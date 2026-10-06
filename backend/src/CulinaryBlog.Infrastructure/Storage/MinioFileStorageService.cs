using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Minio;
using Minio.DataModel.Args;

namespace CulinaryBlog.Infrastructure.Storage;

public sealed class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucket;
    private readonly string _publicBaseUrl;
    private readonly bool _publicRead;

    public MinioFileStorageService(IConfiguration configuration)
    {
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

    public async Task DeleteAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.RemoveObjectAsync(
                new RemoveObjectArgs().WithBucket(_bucket).WithObject(objectKey),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new StorageUnavailableException(
                "The image could not be deleted from MinIO.", exception);
        }
    }
}