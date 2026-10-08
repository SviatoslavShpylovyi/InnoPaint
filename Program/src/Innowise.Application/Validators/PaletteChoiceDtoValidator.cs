using FluentValidation;

using Innowise.Application.DTOs.Requests;

namespace Innowise.Application.Validators;

public sealed class PaletteChoiceDtoValidator : AbstractValidator<PaletteChoiceDto>
{
    public PaletteChoiceDtoValidator()
    {
        RuleFor(choice => choice.PaintId).NotEmpty();
        RuleFor(choice => choice.Role).NotEmpty().MaximumLength(80);
        RuleFor(choice => choice.Reason).NotEmpty().MaximumLength(500);
    }
}
