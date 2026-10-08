using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BuildingBlocks.AspNetCore.Validation;

public sealed class GlobalValidateModelFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public GlobalValidateModelFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var cancellationToken = context.HttpContext.RequestAborted;

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            await ValidateAsync(argument, cancellationToken);
        }

        await next();
    }

    private async Task ValidateAsync(object argument, CancellationToken cancellationToken)
    {
        var validator = ResolveValidator(argument.GetType());

        if (validator is null)
        {
            return;
        }

        var result = await validator.ValidateAsync(new ValidationContext<object>(argument), cancellationToken);

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }

    private IValidator? ResolveValidator(Type argumentType)
    {
        return _serviceProvider.GetService(typeof(IValidator<>).MakeGenericType(argumentType)) as IValidator;
    }
}
