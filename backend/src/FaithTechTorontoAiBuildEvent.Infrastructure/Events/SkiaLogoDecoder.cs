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
        using var image = SKImage.FromBitmap(bitmap);
        using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
        return new(encoded.ToArray(), "image/png", bitmap.Width, bitmap.Height);
    }
}
