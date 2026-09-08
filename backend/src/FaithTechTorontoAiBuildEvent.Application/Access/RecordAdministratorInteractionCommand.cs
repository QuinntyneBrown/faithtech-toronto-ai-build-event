using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record RecordAdministratorInteractionCommand(Guid SessionId) : IRequest<bool>;
