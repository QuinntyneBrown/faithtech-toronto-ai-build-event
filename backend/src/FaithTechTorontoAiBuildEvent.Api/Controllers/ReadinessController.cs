using FaithTechTorontoAiBuildEvent.Application.Operations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FaithTechTorontoAiBuildEvent.Api.Controllers;
[ApiController, Route("api/admin/readiness"), Authorize(Roles = "Administrator")]
public sealed class ReadinessController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ReadinessState> Read(CancellationToken cancellationToken) =>
        await sender.Send(new GetReadinessQuery(), cancellationToken);
}
