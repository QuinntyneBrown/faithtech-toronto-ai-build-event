using FaithTechTorontoAiBuildEvent.Application.Raffle;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController]
[Route("api/admin/raffle/draws")]
public sealed class RaffleController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<DrawWinnerResult>> Draw(DrawRaffleWinnerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await sender.Send(new DrawWinnerCommand(request.OperationId, request.ExpectedVersion, Request.Cookies["faithtech-admin"]), cancellationToken));
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
