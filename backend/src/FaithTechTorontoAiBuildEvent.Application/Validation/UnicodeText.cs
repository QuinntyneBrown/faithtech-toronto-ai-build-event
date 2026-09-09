using System.Text;

namespace FaithTechTorontoAiBuildEvent.Application.Validation;

public static class UnicodeText
{
    public static bool IsWithinScalarLimit(string? value, int maximum)
        => value is null || value.Trim().EnumerateRunes().Count() <= maximum;

    public static string Normalize(string value)
        => value.Trim().Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
}
