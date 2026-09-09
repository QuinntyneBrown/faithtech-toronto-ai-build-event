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
        ProjectInputValidator.Validate(request.Input);
        var inputDigest = OperationInputDigest.Create(
            expectedVersion.ToString(CultureInfo.InvariantCulture),
            request.Input.Title,
            request.Input.Description,
            request.Input.RepositoryUrl,
            request.Input.DemoUrl);
        return await projectStore.AddAsync(request.OperationId, inputDigest, expectedVersion, request.Input, cancellationToken);
    }

}
