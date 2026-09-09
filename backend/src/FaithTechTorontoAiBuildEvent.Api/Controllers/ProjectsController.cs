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

    [HttpPut("{projectId:guid}")]
    public async Task<IActionResult> Update(Guid projectId, SaveProjectRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await sender.Send(new UpdateProjectCommand(request.OperationId, projectId, request.ExpectedVersion, request.Input, Request.Cookies["faithtech-admin"]), cancellationToken);
            return Ok();
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

    [HttpDelete("{projectId:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, DeleteProjectRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await sender.Send(new DeleteProjectCommand(request.OperationId, projectId, request.ExpectedVersion, Request.Cookies["faithtech-admin"]), cancellationToken);
            return NoContent();
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
