namespace FaithTechTorontoAiBuildEvent.Application.Validation;

public static class TextValidation
{
    public static string? Text(string? value, string field, int limit)
    {
        var normalized = value?.ReplaceLineEndings("\n").Trim();
        if (normalized?.EnumerateRunes().Count() > limit) throw new InputValidationException(field, $"Use at most {limit} characters.");
        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }
    public static string? HttpsUrl(string? value, string field)
    {
        var normalized = Text(value, field, 2048);
        if (normalized is not null && (!Uri.TryCreate(normalized, UriKind.Absolute, out var url) ||
            url.Scheme != Uri.UriSchemeHttps || string.IsNullOrEmpty(url.Host) || url.UserInfo.Length > 0))
            throw new InputValidationException(field, "Use an absolute HTTPS URL without credentials.");
        return normalized;
    }
}
