using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.EventState;

public sealed record GetServerTimeQuery : IRequest<DateTimeOffset>;
