using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record SignOutParticipantCommand(Guid SessionId) : IRequest;
