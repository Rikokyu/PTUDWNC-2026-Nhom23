using CulinaryBlog.Infrastructure.BackgroundJobs;
using Hangfire;
using System.Reflection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class ImageVariantProcessorTests
{
    [Fact]
    public void ResizeJob_RetriesThreeTimesThenMovesToFailedState()
    {
        var method = typeof(ImageResizeJob).GetMethod(
            nameof(ImageResizeJob.ProcessAsync));
        var retry = method?.GetCustomAttribute<AutomaticRetryAttribute>();

        Assert.NotNull(retry);
        Assert.Equal(3, retry.Attempts);
        Assert.Equal(AttemptsExceededAction.Fail, retry.OnAttemptsExceeded);
        Assert.True(retry.LogEvents);
    }

    [Fact]
    public async Task CreateAsync_ProducesExactMediumAndThumbnailDimensions()
    {
        using var source = new Image<Rgba32>(
            Configuration.Default,
            1280,
            720,
            new Rgba32(100, 149, 237));
        await using var sourceStream = new MemoryStream();
        await source.SaveAsPngAsync(sourceStream);

        var (mediumBytes, thumbnailBytes) = await ImageVariantProcessor.CreateAsync(
            sourceStream.ToArray());

        using var medium = Image.Load<Rgba32>(mediumBytes);
        using var thumbnail = Image.Load<Rgba32>(thumbnailBytes);

        Assert.Equal(800, medium.Width);
        Assert.Equal(600, medium.Height);
        Assert.Equal(300, thumbnail.Width);
        Assert.Equal(300, thumbnail.Height);
    }
}
