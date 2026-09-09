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
}
