using MediatR;
namespace FaithTechTorontoAiBuildEvent.Application.Operations;
public sealed record ReviewEventImportQuery(Guid EventId, bool Create, string? Version, byte[] Bytes) : IRequest<EventImportReview>;
