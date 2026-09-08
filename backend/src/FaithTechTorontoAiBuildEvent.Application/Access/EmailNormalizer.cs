using System.Net.Mail;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public static class EmailNormalizer
{
    public static (string Email, string NormalizedEmail)? Normalize(string? email)
    {
        var trimmed = email?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > 320 || !MailAddress.TryCreate(trimmed, out _)) return null;
        return (trimmed, trimmed.ToUpperInvariant());
    }
}
