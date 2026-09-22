using Innowise.Application;
using Microsoft.AspNetCore.Mvc;

namespace Innowise.Api.Controllers;

[ApiController]
[Route("api/baskets")]
public sealed class BasketsController(PaintStoreService store) : ControllerBase
{
    [HttpPost]
    public ActionResult<BasketView> Create()
    {
        var basket = store.CreateBasket();
        return CreatedAtAction(nameof(Get), new { id = basket.Id }, basket);
    }

    [HttpGet("{id:guid}")]
    public ActionResult<BasketView> Get(Guid id) => Ok(store.GetBasket(id));

    [HttpPut("{id:guid}/items/{paintId:guid}")]
    public ActionResult<BasketView> SetItem(Guid id, Guid paintId, QuantityRequest request) => Ok(store.SetItem(id, paintId, request.Quantity));

    [HttpDelete("{id:guid}/items/{paintId:guid}")]
    public IActionResult RemoveItem(Guid id, Guid paintId)
    {
        store.RemoveItem(id, paintId);
        return NoContent();
    }

    [HttpDelete("{id:guid}/items")]
    public IActionResult Clear(Guid id)
    {
        store.ClearBasket(id);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        store.DeleteBasket(id);
        return NoContent();
    }
}
