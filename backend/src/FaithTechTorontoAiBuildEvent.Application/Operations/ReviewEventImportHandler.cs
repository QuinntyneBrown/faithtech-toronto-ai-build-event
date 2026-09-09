using MediatR;
namespace FaithTechTorontoAiBuildEvent.Application.Operations;
public sealed class ReviewEventImportHandler(IEventImportStore store) : IRequestHandler<ReviewEventImportQuery, EventImportReview>
{
    public Task<EventImportReview> Handle(ReviewEventImportQuery request, CancellationToken token) =>
        store.Review(request.EventId, request.Create, request.Version, EventSeedReader.Read(request.Bytes), token);
}
