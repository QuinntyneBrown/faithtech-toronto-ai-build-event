using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed record ClearParticipantSessionCommand(string? Secret) : IRequest;
