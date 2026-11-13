using CulinaryBlog.Infrastructure.BackgroundJobs;
using CulinaryBlog.Infrastructure.Storage;
using Minio;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class MinioStorageIntegrationTests
{
    [Fact]
    public async Task IsolatedMinio_UploadReadVariantsAndDelete()
    {
        var endpoint = Environment.GetEnvironmentVariable("CULINARY_TEST_MINIO_ENDPOINT");
        if (endpoint is null) return;

        var origin = new Uri(endpoint);
        var client = new MinioClient()
            .WithEndpoint(origin.Host, origin.Port)
            .WithCredentials("testaccess", "testsecret123")
            .WithSSL(origin.Scheme == "https")
            .Build();
        var storage = new MinioObjectStorageService(
            client, "culinary-storage-test", endpoint);
        using var image = new Image<Rgba32>(1200, 900);
        await using var png = new MemoryStream();
        await image.SaveAsPngAsync(png);
        var original = png.ToArray();
        var folder = $"recipes/{Guid.NewGuid():N}";
        var url = await storage.UploadAsync(original, ".png", folder);
        Assert.Equal(original, await storage.ReadAsync(url));

        using var http = new HttpClient();
        using var response = await http.GetAsync(url);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);

        var (medium, thumbnail) = await ImageVariantProcessor.CreateAsync(original);
        var mediumUrl = await storage.UploadAsync(medium, ".webp", folder);
        var thumbnailUrl = await storage.UploadAsync(thumbnail, ".webp", folder);
        using var mediumImage = Image.Load(await storage.ReadAsync(mediumUrl));
        using var thumbnailImage = Image.Load(await storage.ReadAsync(thumbnailUrl));
        Assert.Equal((800, 600), (mediumImage.Width, mediumImage.Height));
        Assert.Equal((300, 300), (thumbnailImage.Width, thumbnailImage.Height));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            storage.DeleteAsync(endpoint + "/other-bucket/recipes/a.png"));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            storage.DeleteAsync(endpoint + "/culinary-storage-test/../other-bucket/a.png"));
        foreach (var item in new[] { url, mediumUrl, thumbnailUrl })
        {
            await storage.DeleteAsync(item);
            await storage.DeleteAsync(item);
        }
    }
}
