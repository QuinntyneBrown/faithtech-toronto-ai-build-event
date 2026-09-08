using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed record GetEntryHeaderQuery(Guid EventId) : IRequest<EntryHeader?>;
