using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Application.Validation;
using SkiaSharp;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Events;

public sealed class SkiaLogoDecoder : ILogoDecoder
{
    public LogoContent Decode(byte[] bytes, string mediaType)
    {
        using var data = SKData.CreateCopy(bytes);
        using var codec = SKCodec.Create(data);
        if (codec is null) throw new InputValidationException("logo", "The file is not a valid image.");
        var detected = codec.EncodedFormat switch
        { SKEncodedImageFormat.Png => "image/png", SKEncodedImageFormat.Jpeg => "image/jpeg", SKEncodedImageFormat.Webp => "image/webp", _ => null };
        if (detected != mediaType) throw new InputValidationException("logo", "The image content does not match its declared type.");
        var info = codec.Info;
        if (info.Width is < 1 or > 4096 || info.Height is < 1 or > 4096)
            throw new InputValidationException("logo", "Choose an image no larger than 4,096 pixels per dimension.");
        using var bitmap = new SKBitmap(new SKImageInfo(info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Premul));
        if (codec.GetPixels(bitmap.Info, bitmap.GetPixels()) != SKCodecResult.Success)
            throw new InputValidationException("logo", "The image is incomplete or cannot be decoded.");
        var swapsDimensions = codec.EncodedOrigin >= SKEncodedOrigin.LeftTop;
        var width = swapsDimensions ? info.Height : info.Width;
        var height = swapsDimensions ? info.Width : info.Height;
        using var oriented = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        using (var canvas = new SKCanvas(oriented))
        {
            canvas.SetMatrix(codec.EncodedOrigin switch {
                SKEncodedOrigin.TopRight => new(-1, 0, width, 0, 1, 0, 0, 0, 1),
                SKEncodedOrigin.BottomRight => new(-1, 0, width, 0, -1, height, 0, 0, 1),
                SKEncodedOrigin.BottomLeft => new(1, 0, 0, 0, -1, height, 0, 0, 1),
                SKEncodedOrigin.LeftTop => new(0, 1, 0, 1, 0, 0, 0, 0, 1),
                SKEncodedOrigin.RightTop => new(0, -1, width, 1, 0, 0, 0, 0, 1),
                SKEncodedOrigin.RightBottom => new(0, -1, width, -1, 0, height, 0, 0, 1),
                SKEncodedOrigin.LeftBottom => new(0, 1, 0, -1, 0, height, 0, 0, 1),
                _ => SKMatrix.Identity
            });
            canvas.DrawBitmap(bitmap, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest));
        }
        using var image = SKImage.FromBitmap(oriented);
        using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
        return new(encoded.ToArray(), "image/png", width, height);
    }
}
