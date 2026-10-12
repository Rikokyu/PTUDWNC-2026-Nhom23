using CulinaryBlog.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CulinaryBlog.API.Tests;

public sealed class MinioFileStorageServiceTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public async Task DeleteAsync_RejectsEmptyObjectKey(string objectKey)
    {
        var storage = new MinioFileStorageService(
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Minio:Endpoint"] = "localhost:9000",
                    ["Minio:AccessKey"] = "test-access-key",
                    ["Minio:SecretKey"] = "test-secret-key",
                    ["Minio:BucketName"] = "test-bucket",
                    ["Minio:PublicBaseUrl"] = "http://localhost:9000/test-bucket"
                })
                .Build(),
            NullLogger<MinioFileStorageService>.Instance);

        await Assert.ThrowsAsync<ArgumentException>(
            () => storage.DeleteAsync(objectKey));
    }
}
