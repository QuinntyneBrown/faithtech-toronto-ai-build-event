using FaithTechTorontoAiBuildEvent.Application.Teams;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController]
[Route("api/admin/teams")]
public sealed class TeamsController(ISender sender) : ControllerBase
{
    [HttpPut("{teamId:guid}/project")]
    public async Task<IActionResult> AssignProject(Guid teamId, AssignTeamProjectRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await sender.Send(new AssignTeamProjectCommand(request.OperationId, teamId, request.ProjectId, request.ExpectedVersion, Request.Cookies["faithtech-admin"]), cancellationToken);
            return NoContent();
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (ArgumentException exception) { return BadRequest(new ProblemDetails { Detail = exception.Message, Status = StatusCodes.Status400BadRequest }); }
        catch (InvalidOperationException exception) { return Conflict(new ProblemDetails { Detail = exception.Message, Status = StatusCodes.Status409Conflict }); }
    }

    [HttpPost("moves")]
    public async Task<IActionResult> Move(MoveTeamMemberRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await sender.Send(new MoveTeamMemberCommand(request.OperationId, request.ParticipantId, request.Destination, request.TeamId, request.ExpectedVersion, Request.Cookies["faithtech-admin"]), cancellationToken);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Detail = exception.Message, Status = StatusCodes.Status400BadRequest });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Detail = exception.Message, Status = StatusCodes.Status409Conflict });
        }
    }
}
