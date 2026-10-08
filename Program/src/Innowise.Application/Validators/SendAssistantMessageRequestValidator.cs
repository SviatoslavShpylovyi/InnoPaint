using FluentValidation;

using Innowise.Application.DTOs.Requests;

namespace Innowise.Application.Validators;

public sealed class SendAssistantMessageRequestValidator : AbstractValidator<SendAssistantMessageRequest>
{
    public SendAssistantMessageRequestValidator()
    {
        RuleFor(request => request.Message).NotEmpty().MaximumLength(2000);
    }
}
