using Microsoft.AspNetCore.Mvc;

using Innowise.Application.DTOs.Requests;
using Innowise.Application.DTOs.Responses;
using Innowise.Application.Interfaces;

namespace Innowise.Api.Controllers;

[ApiController]
[Route("api/baskets")]
public sealed class BasketsController : ControllerBase
{
    private readonly IBasketService _basketService;

    public BasketsController(IBasketService basketService)
    {
        _basketService = basketService;
    }

    [HttpPost]
    public async Task<ActionResult<BasketResponse>> Create(CancellationToken cancellationToken)
    {
        var basket = await _basketService.CreateAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = basket.Id }, basket);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BasketResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var basket = await _basketService.GetByIdAsync(id, cancellationToken);

        return Ok(basket);
    }

    [HttpPut("{id:guid}/items/{paintId:guid}")]
    public async Task<ActionResult<BasketResponse>> SetItem(Guid id, Guid paintId, [FromBody] UpdateBasketItemRequest request, CancellationToken cancellationToken)
    {
        var basket = await _basketService.SetItemQuantityAsync(id, paintId, request, cancellationToken);

        return Ok(basket);
    }

    [HttpDelete("{id:guid}/items/{paintId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid id, Guid paintId, CancellationToken cancellationToken)
    {
        await _basketService.RemoveItemAsync(id, paintId, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}/items")]
    public async Task<IActionResult> Clear(Guid id, CancellationToken cancellationToken)
    {
        await _basketService.ClearAsync(id, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _basketService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
