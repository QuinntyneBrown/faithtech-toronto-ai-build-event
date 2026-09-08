using FaithTechTorontoAiBuildEvent.Domain.Access;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record AuthenticateAdministratorCommand(string Username, string Password) : IRequest<AdministratorSession?>;
