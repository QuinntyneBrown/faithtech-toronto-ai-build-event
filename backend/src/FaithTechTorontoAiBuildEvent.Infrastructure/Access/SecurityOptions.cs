namespace FaithTechTorontoAiBuildEvent.Infrastructure.Access;

public sealed class SecurityOptions
{
    public const string SectionName = "Security";
    public string DigestKey { get; set; } = "";
}
