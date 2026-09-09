using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Application.Participants;
using MediatR;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using System.Globalization;

namespace FaithTechTorontoAiBuildEvent.Application.Projects;

public sealed class SaveProjectHandler(
    IAdministratorAuthorizationStore authorizationStore,
    IEntryReceiptSecretService secretService,
    IProjectStore projectStore)
    : IRequestHandler<SaveProjectCommand, Guid>
{
    public async Task<Guid> Handle(SaveProjectCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AdministratorSecret) || !long.TryParse(request.ExpectedVersion, out var expectedVersion)
            || !await authorizationStore.IsAuthorizedAsync(secretService.Digest(request.AdministratorSecret), DateTimeOffset.UtcNow, cancellationToken))
        {
            throw new UnauthorizedAccessException();
        }
        Validate(request.Input);
        var inputDigest = OperationInputDigest.Create(
            expectedVersion.ToString(CultureInfo.InvariantCulture),
            request.Input.Title,
            request.Input.Description,
            request.Input.RepositoryUrl,
            request.Input.DemoUrl);
        return await projectStore.AddAsync(request.OperationId, inputDigest, expectedVersion, request.Input, cancellationToken);
    }

    private static void Validate(ProjectInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title) || input.Title.Trim().Length > 200 || string.IsNullOrWhiteSpace(input.Description) || input.Description.Trim().Length > 2000)
        {
            throw new ArgumentException("Project title and description are required.");
        }
        ValidateUrl(input.RepositoryUrl);
        ValidateUrl(input.DemoUrl);
    }

    private static void ValidateUrl(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo)))
        {
            throw new ArgumentException("Project links must be absolute HTTPS URLs without credentials.");
        }
    }
}
