using FaithTechTorontoAiBuildEvent.Application.EventFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController]
[Route("api/admin/event")]
public sealed class EventFlowController(ISender sender) : ControllerBase
{
    [HttpPost("advance")]
    public async Task<IActionResult> Advance(AdvanceScreenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await sender.Send(new AdvanceScreenCommand(request.OperationId, request.ExpectedVersion, request.FromScreen, request.ToScreen, Request.Cookies["faithtech-admin"]), cancellationToken);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Detail = exception.Message, Status = StatusCodes.Status409Conflict });
        }
    }
}
