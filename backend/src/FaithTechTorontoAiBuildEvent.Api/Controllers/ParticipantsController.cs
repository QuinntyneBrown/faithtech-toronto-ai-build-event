using FaithTechTorontoAiBuildEvent.Application.Participants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController]
[Route("api/admin/participants")]
public sealed class ParticipantsController(ISender sender) : ControllerBase
{
    [HttpPut("{participantId:guid}")]
    public async Task<ActionResult<AdministratorParticipant>> Update(Guid participantId, UpdateAdministratorParticipantRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await sender.Send(new UpdateAdministratorParticipantCommand(request.OperationId, participantId, request.ExpectedVersion, request.Input, Request.Cookies["faithtech-admin"]), cancellationToken)); }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (EntryValidationException exception) { return BadRequest(new ProblemDetails { Detail = exception.Message, Status = StatusCodes.Status400BadRequest }); }
        catch (InvalidOperationException exception) { return Conflict(new ProblemDetails { Detail = exception.Message, Status = StatusCodes.Status409Conflict }); }
    }

    [HttpDelete("{participantId:guid}")]
    public async Task<IActionResult> Delete(Guid participantId, DeleteAdministratorParticipantRequest request, CancellationToken cancellationToken)
    {
        try { await sender.Send(new DeleteAdministratorParticipantCommand(request.OperationId, participantId, request.ExpectedVersion, Request.Cookies["faithtech-admin"]), cancellationToken); return NoContent(); }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (EntryValidationException exception) { return BadRequest(new ProblemDetails { Detail = exception.Message, Status = StatusCodes.Status400BadRequest }); }
        catch (InvalidOperationException exception) { return Conflict(new ProblemDetails { Detail = exception.Message, Status = StatusCodes.Status409Conflict }); }
    }

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
