using FaithTechTorontoAiBuildEvent.Application.Participants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController]
[Route("api/admin/participants")]
public sealed class ParticipantsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AdministratorParticipant>> Add(AddAdministratorParticipantRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await sender.Send(new AddAdministratorParticipantCommand(request.OperationId, request.ExpectedVersion, request.Email, Request.Cookies["faithtech-admin"]), cancellationToken)); }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (EntryValidationException exception) { return BadRequest(new ProblemDetails { Detail = exception.Message, Status = StatusCodes.Status400BadRequest }); }
        catch (InvalidOperationException exception) { return Conflict(new ProblemDetails { Detail = exception.Message, Status = StatusCodes.Status409Conflict }); }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdministratorParticipant>>> Get(CancellationToken cancellationToken)
    {
        try { return Ok(await sender.Send(new GetAdministratorParticipantsQuery(Request.Cookies["faithtech-admin"]), cancellationToken)); }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
    }
}
