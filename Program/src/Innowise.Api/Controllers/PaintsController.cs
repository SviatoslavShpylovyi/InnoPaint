using Microsoft.AspNetCore.Mvc;

using Innowise.Application.DTOs.Requests;
using Innowise.Application.DTOs.Responses;
using Innowise.Application.Interfaces;

namespace Innowise.Api.Controllers;

[ApiController]
[Route("api/paints")]
public sealed class PaintsController : ControllerBase
{
    private readonly IPaintService _paintService;

    public PaintsController(IPaintService paintService)
    {
        _paintService = paintService;
    }

    [HttpGet]
    public async Task<ActionResult<PaintPageResponse>> GetAll([FromQuery] PaintFilterRequest filter, CancellationToken cancellationToken)
    {
        var paints = await _paintService.GetAllAsync(filter, cancellationToken);

        return Ok(paints);
    }

    [HttpGet("lookups")]
    public async Task<ActionResult<PaintLookupsResponse>> GetLookups(CancellationToken cancellationToken)
    {
        var lookups = await _paintService.GetLookupsAsync(cancellationToken);

        return Ok(lookups);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaintResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var paint = await _paintService.GetByIdAsync(id, cancellationToken);

        return Ok(paint);
    }

    [HttpPost]
    public async Task<ActionResult<PaintResponse>> Create([FromBody] CreatePaintRequest request, CancellationToken cancellationToken)
    {
        var paint = await _paintService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = paint.Id }, paint);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PaintResponse>> Update(Guid id, [FromBody] UpdatePaintRequest request, CancellationToken cancellationToken)
    {
        var paint = await _paintService.UpdateAsync(id, request, cancellationToken);

        return Ok(paint);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _paintService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
