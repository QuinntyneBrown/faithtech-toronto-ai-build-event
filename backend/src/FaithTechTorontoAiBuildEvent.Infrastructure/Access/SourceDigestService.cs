using System.Security.Cryptography;
using System.Text;
using FaithTechTorontoAiBuildEvent.Application.Access;
using Microsoft.Extensions.Options;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Access;

public sealed class SourceDigestService : ISourceDigestService
{
    private readonly byte[] key;

    public SourceDigestService(IOptions<SecurityOptions> options)
    {
        try { key = Convert.FromBase64String(options.Value.DigestKey); }
        catch (FormatException exception) { throw new InvalidOperationException("Security:DigestKey must be base64.", exception); }
        if (key.Length < 32) throw new InvalidOperationException("Security:DigestKey must contain at least 32 random bytes.");
    }

    public string Digest(string source)
        => Convert.ToHexString(HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(source)));
}
