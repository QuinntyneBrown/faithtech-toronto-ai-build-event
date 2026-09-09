using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.EventState;

public sealed record GetPublicEventSnapshotQuery : IRequest<PublicEventSnapshot>;
