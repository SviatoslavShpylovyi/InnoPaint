using FluentValidation;

using Innowise.Application.DTOs.Requests;

namespace Innowise.Application.Validators;

public sealed class UpdatePaintRequestValidator : AbstractValidator<UpdatePaintRequest>
{
    public UpdatePaintRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(150);
        RuleFor(request => request.Brand).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Description).NotEmpty().MaximumLength(4000);
        RuleFor(request => request.Color).NotEmpty().MaximumLength(80);
        RuleFor(request => request.Finish).NotEmpty().MaximumLength(80);
        RuleFor(request => request.Surface).NotEmpty().MaximumLength(80);
        RuleFor(request => request.VolumeLiters).InclusiveBetween(0.01m, 1000m);
        RuleFor(request => request.Price).InclusiveBetween(0.01m, 1000000m);
        RuleFor(request => request.StockQuantity).InclusiveBetween(0, 1000000);
    }
}
