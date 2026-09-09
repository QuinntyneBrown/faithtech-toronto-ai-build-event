using System.Security.Cryptography;
using FaithTechTorontoAiBuildEvent.Application.Participants;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Participants;

public sealed class EntryReceiptSecretService : IEntryReceiptSecretService
{
    public string CreateSecret() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        .TrimEnd('=')
        .Replace('+', '-')
        .Replace('/', '_');

    public byte[] Digest(string secret) => SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(secret));

    public bool Matches(byte[] expectedDigest, string secret)
        => CryptographicOperations.FixedTimeEquals(expectedDigest, Digest(secret));
}
