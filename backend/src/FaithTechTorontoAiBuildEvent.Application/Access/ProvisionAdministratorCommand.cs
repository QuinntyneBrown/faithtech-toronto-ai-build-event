using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record ProvisionAdministratorCommand(string Username, string Password) : IRequest<Guid>;
