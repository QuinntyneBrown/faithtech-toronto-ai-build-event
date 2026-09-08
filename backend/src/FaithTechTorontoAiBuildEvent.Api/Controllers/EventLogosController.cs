using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using FaithTechTorontoAiBuildEvent.Application.Events;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController, Route("api/admin/events/{eventId:guid}/logo"), Authorize(Roles = "Administrator")]
public sealed class EventLogosController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid eventId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEventLogoQuery(eventId), cancellationToken);
        return result is null ? NotFound() : File(result.Bytes, result.MediaType);
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(3145728), RequestFormLimits(MultipartBodyLengthLimit = 3145728)]
    public async Task<IActionResult> Upload(Guid eventId, [FromForm] IFormFile file,
        [FromHeader(Name = "Idempotency-Key"), Required] Guid? operationId,
        [FromHeader(Name = "If-Match")] string? version, CancellationToken cancellationToken)
    {
        await using var content = file.OpenReadStream();
        return Ok(await sender.Send(new SetEventLogoCommand(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            eventId, operationId!.Value, version, content, file.Length, file.ContentType, file.FileName), cancellationToken));
    }
}
