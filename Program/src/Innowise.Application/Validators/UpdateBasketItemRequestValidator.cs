using FluentValidation;

using Innowise.Application.DTOs.Requests;

namespace Innowise.Application.Validators;

public sealed class UpdateBasketItemRequestValidator : AbstractValidator<UpdateBasketItemRequest>
{
    public UpdateBasketItemRequestValidator()
    {
        RuleFor(request => request.Quantity).InclusiveBetween(1, 1000000);
    }
}
