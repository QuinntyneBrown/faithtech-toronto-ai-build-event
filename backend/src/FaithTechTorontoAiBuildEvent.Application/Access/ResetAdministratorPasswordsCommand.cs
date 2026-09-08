using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record ResetAdministratorPasswordsCommand(string? Username, string Password) : IRequest<string[]>;
