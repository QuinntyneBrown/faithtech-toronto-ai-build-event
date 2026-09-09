using FaithTechTorontoAiBuildEvent.Application.Raffle;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController]
[Route("api/event/raffle")]
public sealed class PublicRaffleController(ISender sender) : ControllerBase
{
    [HttpGet]
    public Task<RaffleSnapshot> Get(CancellationToken cancellationToken)
        => sender.Send(new GetRaffleSnapshotQuery(), cancellationToken);
}
