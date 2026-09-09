using FaithTechTorontoAiBuildEvent.Application.Participants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController]
[Route("api/admin/participants")]
public sealed class ParticipantsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdministratorParticipant>>> Get(CancellationToken cancellationToken)
    {
        try { return Ok(await sender.Send(new GetAdministratorParticipantsQuery(Request.Cookies["faithtech-admin"]), cancellationToken)); }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
    }
}
