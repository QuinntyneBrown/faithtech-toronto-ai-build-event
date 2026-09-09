using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record RecordAdministratorInteractionCommand(string? SessionSecret) : IRequest<bool>;
