using HeyRed.ImageSharp.Heif.Formats.Avif;
using HeyRed.ImageSharp.Heif.Formats.Heif;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CulinaryBlog.Infrastructure.BackgroundJobs;

public static class ImageVariantProcessor
{
    public const long MaxDecodedPixels = 40_000_000;

    public static async Task<(byte[] Medium, byte[] Thumbnail)> CreateAsync(
        byte[] sourceBytes,
        CancellationToken cancellationToken = default)
    {
        var options = CreateDecoderOptions();
        var imageInfo = Image.Identify(options, sourceBytes);

        if (imageInfo is null
            || (long)imageInfo.Width * imageInfo.Height > MaxDecodedPixels)
        {
            throw new InvalidDataException(
                "The uploaded image is invalid or exceeds the pixel limit.");
        }

        var medium = await ResizeToWebpAsync(
            sourceBytes,
            800,
            600,
            cancellationToken);
        var thumbnail = await ResizeToWebpAsync(
            sourceBytes,
            300,
            300,
            cancellationToken);

        return (medium, thumbnail);
    }

    private static async Task<byte[]> ResizeToWebpAsync(
        byte[] sourceBytes,
        int width,
        int height,
        CancellationToken cancellationToken)
    {
        await using var sourceStream = new MemoryStream(sourceBytes);
        using var source = await Image.LoadAsync<Rgba32>(
            CreateDecoderOptions(),
            sourceStream,
            cancellationToken);
        using var resized = source.Clone(context => context
            .AutoOrient()
            .Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Crop
            }));

        await using var output = new MemoryStream();
        await resized.SaveAsWebpAsync(
            output,
            new WebpEncoder { Quality = 82 },
            cancellationToken);
        return output.ToArray();
    }

    private static DecoderOptions CreateDecoderOptions()
    {
        var configuration = Configuration.Default.Clone();
        configuration.Configure(new AvifConfigurationModule());
        configuration.Configure(new HeifConfigurationModule());

        return new DecoderOptions
        {
            Configuration = configuration,
            MaxFrames = 1
        };
    }
}
