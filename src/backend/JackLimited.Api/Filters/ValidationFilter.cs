using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace JackLimited.Api.Filters;

internal sealed class ValidationFilter<TRequest> : IEndpointFilter where TRequest : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService(typeof(IValidator<TRequest>)) as IValidator<TRequest>;
        if (validator is null)
        {
            return await next(context);
        }

        var request = context.Arguments.FirstOrDefault(arg => arg is TRequest) as TRequest;
        if (request is null)
        {
            return await next(context);
        }

        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        return await next(context);
    }
}
