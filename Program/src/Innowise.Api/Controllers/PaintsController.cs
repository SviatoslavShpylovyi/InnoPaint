using Innowise.Application;
using Innowise.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Innowise.Api.Controllers;

[ApiController]
[Route("api/paints")]
public sealed class PaintsController(PaintStoreService store) : ControllerBase
{
    [HttpGet]
    public ActionResult<PaintPage> Search([FromQuery] PaintQuery query) => Ok(store.Search(query));

    [HttpGet("lookups")]
    public ActionResult<PaintLookups> Lookups() => Ok(store.Lookups());

    [HttpGet("{id:guid}")]
    public ActionResult<Paint> Get(Guid id) => Ok(store.GetPaint(id));

    [HttpPost]
    public ActionResult<Paint> Create(PaintRequest request)
    {
        var paint = store.CreatePaint(request);
        return CreatedAtAction(nameof(Get), new { id = paint.Id }, paint);
    }

    [HttpPut("{id:guid}")]
    public ActionResult<Paint> Update(Guid id, PaintRequest request) => Ok(store.UpdatePaint(id, request));

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        store.DeletePaint(id);
        return NoContent();
    }
}
