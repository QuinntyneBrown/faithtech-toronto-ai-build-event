using FaithTechTorontoAiBuildEvent.Application.Validation;

namespace FaithTechTorontoAiBuildEvent.Application.Projects;

public static class ProjectInputValidator
{
    public static void Validate(ProjectInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title)
            || !UnicodeText.IsWithinScalarLimit(input.Title, 200)
            || string.IsNullOrWhiteSpace(input.Description)
            || !UnicodeText.IsWithinScalarLimit(input.Description, 2000))
        {
            throw new ArgumentException("Project title and description are required and must be within their length limits.");
        }

        ValidateUrl(input.RepositoryUrl);
        ValidateUrl(input.DemoUrl);
    }

    private static void ValidateUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        var normalized = value.Trim();
        if (!UnicodeText.IsWithinScalarLimit(normalized, 2048)
            || !Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps
            || !string.IsNullOrEmpty(uri.UserInfo))
        {
            throw new ArgumentException("Project links must be absolute HTTPS URLs without credentials and no longer than 2,048 characters.");
        }
    }
}
