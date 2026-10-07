using CulinaryBlog.Infrastructure.BackgroundJobs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class ImageVariantProcessorTests
{
    [Fact]
    public async Task CreateAsync_ProducesExpectedWebpDimensions()
    {
        using var source = new Image<Rgba32>(1200, 900);
        await using var sourceStream = new MemoryStream();
        await source.SaveAsPngAsync(sourceStream);

        var (medium, thumbnail) = await ImageVariantProcessor.CreateAsync(
            sourceStream.ToArray());

        var mediumInfo = Image.Identify(medium);
        var thumbnailInfo = Image.Identify(thumbnail);

        Assert.NotNull(mediumInfo);
        Assert.Equal(800, mediumInfo.Width);
        Assert.Equal(600, mediumInfo.Height);
        Assert.NotNull(thumbnailInfo);
        Assert.Equal(300, thumbnailInfo.Width);
        Assert.Equal(300, thumbnailInfo.Height);
        Assert.Equal("Webp", mediumInfo.Metadata.DecodedImageFormat?.Name);
        Assert.Equal("Webp", thumbnailInfo.Metadata.DecodedImageFormat?.Name);
    }
}
