using Innowise.Application.DTOs.Responses;
using Innowise.Application.Exceptions;
using Innowise.Domain.Models;

namespace Innowise.Application.Mappings;

public static class BasketMapper
{
    public static BasketResponse ToResponse(this Basket basket, IReadOnlyDictionary<Guid, Paint> paintsById)
    {
        ArgumentNullException.ThrowIfNull(basket);
        ArgumentNullException.ThrowIfNull(paintsById);

        var lines = basket.Items.Select(item => ToLineResponse(item, paintsById)).ToArray();

        return new BasketResponse(
            basket.Id,
            lines,
            lines.Sum(line => (long)line.Quantity),
            lines.Sum(line => line.LineTotal));
    }

    private static BasketLineResponse ToLineResponse(BasketItem item, IReadOnlyDictionary<Guid, Paint> paintsById)
    {
        if (!paintsById.TryGetValue(item.PaintId, out var paint))
        {
            throw new PaintNotFoundException(item.PaintId);
        }

        return new BasketLineResponse(paint.ToResponse(), item.Quantity, paint.Price * item.Quantity);
    }
}
