using FaithTechTorontoAiBuildEvent.Application.Projects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController]
[Route("api/admin/projects")]
public sealed class ProjectsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Add(SaveProjectRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await sender.Send(new SaveProjectCommand(request.OperationId, request.ExpectedVersion, request.Input, Request.Cookies["faithtech-admin"]), cancellationToken));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Detail = exception.Message, Status = StatusCodes.Status400BadRequest });
        }
    }
}
