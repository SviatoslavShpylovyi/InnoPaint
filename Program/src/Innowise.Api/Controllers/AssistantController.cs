using Microsoft.AspNetCore.Mvc;

using Innowise.Application.DTOs.Requests;
using Innowise.Application.DTOs.Responses;
using Innowise.Application.Interfaces;

namespace Innowise.Api.Controllers;

[ApiController]
[Route("api/assistant")]
public sealed class AssistantController : ControllerBase
{
    private readonly IPaletteAssistantService _paletteAssistantService;

    public AssistantController(IPaletteAssistantService paletteAssistantService)
    {
        _paletteAssistantService = paletteAssistantService;
    }

    [HttpPost("messages")]
    public async Task<ActionResult<AssistantReplyResponse>> Create([FromBody] SendAssistantMessageRequest request, CancellationToken cancellationToken)
    {
        var reply = await _paletteAssistantService.ReplyAsync(request, cancellationToken);

        return Ok(reply);
    }
}
