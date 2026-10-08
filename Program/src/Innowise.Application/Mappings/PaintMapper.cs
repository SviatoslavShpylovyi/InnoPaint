using Innowise.Application.DTOs.Requests;
using Innowise.Application.DTOs.Responses;
using Innowise.Domain.Models;

namespace Innowise.Application.Mappings;

public static class PaintMapper
{
    public static Paint ToEntity(this CreatePaintRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return Paint.Create(
            request.Name.Trim(),
            request.Brand.Trim(),
            request.Description.Trim(),
            request.Color.Trim(),
            request.Finish.Trim(),
            request.Surface.Trim(),
            request.IsExterior,
            request.VolumeLiters,
            request.Price,
            request.StockQuantity);
    }

    public static void ApplyTo(this UpdatePaintRequest request, Paint paint)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(paint);

        paint.SetDetails(
            request.Name.Trim(),
            request.Brand.Trim(),
            request.Description.Trim(),
            request.Color.Trim(),
            request.Finish.Trim(),
            request.Surface.Trim(),
            request.IsExterior,
            request.VolumeLiters,
            request.Price,
            request.StockQuantity);
    }

    public static PaintResponse ToResponse(this Paint paint)
    {
        ArgumentNullException.ThrowIfNull(paint);

        return new PaintResponse(
            paint.Id,
            paint.Name,
            paint.Brand,
            paint.Description,
            paint.Color,
            paint.Finish,
            paint.Surface,
            paint.IsExterior,
            paint.VolumeLiters,
            paint.Price,
            paint.StockQuantity);
    }

    public static IReadOnlyList<PaintResponse> ToResponses(this IEnumerable<Paint> paints)
    {
        ArgumentNullException.ThrowIfNull(paints);

        return [.. paints.Select(paint => paint.ToResponse())];
    }
}
