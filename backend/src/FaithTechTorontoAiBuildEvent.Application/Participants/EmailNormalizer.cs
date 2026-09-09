namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public static class EmailNormalizer
{
    public static string Normalize(string email)
    {
        var value = email.Trim();
        if (value.Length is 0 or > 254 || value.Any(char.IsWhiteSpace) || value.Any(char.IsControl))
        {
            throw new EntryValidationException("Email must be a valid address.");
        }

        var parts = value.Split('@');
        if (parts.Length != 2 || parts[0].Length == 0 || parts[1].Length == 0 || !parts[1].Contains('.'))
        {
            throw new EntryValidationException("Email must be a valid address.");
        }

        return value.ToUpperInvariant();
    }
}
