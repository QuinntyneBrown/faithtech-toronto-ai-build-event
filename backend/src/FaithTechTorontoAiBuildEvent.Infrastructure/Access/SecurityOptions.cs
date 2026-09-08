namespace FaithTechTorontoAiBuildEvent.Infrastructure.Access;

public sealed class SecurityOptions
{
    public string DigestKey { get; set; } = "";

    public bool HasValidDigestKey() => Convert.TryFromBase64String(DigestKey, new byte[128], out var length) && length >= 32;
}
