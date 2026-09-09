using FaithTechTorontoAiBuildEvent.Application.Participants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController]
[Route("api/participant")]
public sealed class ParticipantEntryController(ISender sender) : ControllerBase
{
    [HttpPost("entry-receipt")]
    public async Task<ActionResult<EntryReceiptResponse>> CreateReceipt(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateEntryReceiptCommand(Request.Cookies["faithtech-entry-receipt"]), cancellationToken);
        Response.Cookies.Append("faithtech-entry-receipt", result.Secret, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/participant",
            Expires = DateTimeOffset.UtcNow.AddHours(8),
            IsEssential = true
        });
        return Ok(new EntryReceiptResponse(result.OperationId));
    }

    [HttpPost("entries")]
    public async Task<ActionResult<EntryResultResponse>> Enter(EnterParticipantRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(
                new EnterParticipantCommand(request.OperationId, request.ExpectedVersion, request.Input, Request.Cookies["faithtech-entry-receipt"]),
                cancellationToken);
            Response.Cookies.Append("faithtech-participant", result.SessionSecret, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/api/participant",
                Expires = DateTimeOffset.UtcNow.AddHours(8),
                IsEssential = true
            });
            return Ok(new EntryResultResponse(result.ParticipantId, result.PublicLabel));
        }
        catch (EntryValidationException exception)
        {
            return BadRequest(new ProblemDetails { Detail = exception.Message, Status = StatusCodes.Status400BadRequest });
        }
    }
}
