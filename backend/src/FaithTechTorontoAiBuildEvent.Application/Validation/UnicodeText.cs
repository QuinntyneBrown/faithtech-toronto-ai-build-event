using System.Text;

namespace FaithTechTorontoAiBuildEvent.Application.Validation;

public static class UnicodeText
{
    public static bool IsWithinScalarLimit(string? value, int maximum)
        => value is null || value.Trim().EnumerateRunes().Count() <= maximum;
}
