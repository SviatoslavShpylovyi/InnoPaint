using Innowise.Api.Assistant;
using Microsoft.AspNetCore.Mvc;

namespace Innowise.Api.Controllers;

[ApiController]
[Route("api/assistant")]
public sealed class AssistantController(PaletteAssistantService assistant) : ControllerBase
{
    [HttpPost("messages")]
    public async Task<ActionResult<AssistantReply>> Send(AssistantRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message must not be blank");
        var reply = await assistant.ReplyAsync(
            request,
            cancellationToken);
        return Ok(reply);
    }
}