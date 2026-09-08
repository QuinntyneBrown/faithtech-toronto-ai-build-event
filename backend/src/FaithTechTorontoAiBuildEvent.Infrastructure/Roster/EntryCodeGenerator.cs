using System.Security.Cryptography;
using System.Text;
using FaithTechTorontoAiBuildEvent.Application.Roster;
using FaithTechTorontoAiBuildEvent.Infrastructure.Access;
using Microsoft.Extensions.Options;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Roster;

public sealed class EntryCodeGenerator(IOptions<SecurityOptions> options) : IEntryCodeGenerator
{
    public string Generate() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
    public string Digest(Guid eventId, string code) => Convert.ToHexString(HMACSHA256.HashData(
        Convert.FromBase64String(options.Value.DigestKey), Encoding.UTF8.GetBytes($"entry:{eventId:N}:{code.Trim().ToUpperInvariant()}")));
}
