using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Application.Validation;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed class SetEventLogoHandler(ILogoDecoder decoder, IEventLogoStore store) : IRequestHandler<SetEventLogoCommand, EventDetail>
{
    public async Task<EventDetail> Handle(SetEventLogoCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Version)) throw new VersionRequiredException();
        if (request.Length is < 1 or > 2097152) throw new InputValidationException("logo", "Choose an image up to 2 MiB.");
        var expectedType = Path.GetExtension(request.FileName).ToLowerInvariant() switch
        { ".png" => "image/png", ".jpg" or ".jpeg" => "image/jpeg", ".webp" => "image/webp", _ => null };
        if (expectedType is null || request.MediaType != expectedType)
            throw new InputValidationException("logo", "Choose a PNG, JPEG or WebP file with a matching image type.");
        var bytes = new byte[(int)request.Length];
        await request.Content.ReadExactlyAsync(bytes, cancellationToken);
        var logo = decoder.Decode(bytes, request.MediaType);
        var version = request.Version.Trim('"');
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new {
            target = $"POST /api/admin/events/{request.EventId}/logo", version, request.FileName, request.MediaType,
            content = Convert.ToHexString(SHA256.HashData(bytes)) }))));
        return await store.Save(request.ActorId, request.EventId, request.OperationId, version, hash, logo, cancellationToken);
    }
}
