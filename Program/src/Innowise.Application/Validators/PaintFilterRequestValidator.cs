using FluentValidation;

using Innowise.Application.DTOs.Requests;

namespace Innowise.Application.Validators;

public sealed class PaintFilterRequestValidator : AbstractValidator<PaintFilterRequest>
{
    private static readonly string[] SortFields = ["name", "brand", "price", "volume"];

    public PaintFilterRequestValidator()
    {
        RuleFor(request => request.Search).MaximumLength(200);
        RuleFor(request => request.Brand).MaximumLength(100);
        RuleFor(request => request.Color).MaximumLength(80);
        RuleFor(request => request.Finish).MaximumLength(80);
        RuleFor(request => request.Surface).MaximumLength(80);
        RuleFor(request => request.MinPrice).InclusiveBetween(0m, 1000000m).When(request => request.MinPrice.HasValue);
        RuleFor(request => request.MaxPrice).InclusiveBetween(0m, 1000000m).When(request => request.MaxPrice.HasValue);
        RuleFor(request => request.MinVolumeLiters).InclusiveBetween(0.01m, 1000m).When(request => request.MinVolumeLiters.HasValue);
        RuleFor(request => request.MaxVolumeLiters).InclusiveBetween(0.01m, 1000m).When(request => request.MaxVolumeLiters.HasValue);
        RuleFor(request => request.SortBy).Must(BeASortField).WithMessage("SortBy must be one of: name, brand, price, volume.");
        RuleFor(request => request.Page).GreaterThanOrEqualTo(1);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 100);

        RuleFor(request => request.MinPrice)
            .LessThanOrEqualTo(request => request.MaxPrice)
            .When(request => request.MinPrice.HasValue && request.MaxPrice.HasValue)
            .WithMessage("MinPrice must not exceed MaxPrice.");

        RuleFor(request => request.MinVolumeLiters)
            .LessThanOrEqualTo(request => request.MaxVolumeLiters)
            .When(request => request.MinVolumeLiters.HasValue && request.MaxVolumeLiters.HasValue)
            .WithMessage("MinVolumeLiters must not exceed MaxVolumeLiters.");
    }

    private static bool BeASortField(string sortBy)
    {
        return SortFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase);
    }
}
