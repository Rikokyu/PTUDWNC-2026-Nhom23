using CulinaryBlog.Application.Common.Exceptions;
using FluentValidation;
using MediatR;
using ApplicationValidationException =
    CulinaryBlog.Application.Common.Exceptions.ValidationException;

namespace CulinaryBlog.Application.Common.Behaviors;

public sealed class FluentValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public FluentValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var results = await Task.WhenAll(
            _validators.Select(validator =>
                validator.ValidateAsync(
                    new ValidationContext<TRequest>(request),
                    cancellationToken)));
        var errors = results
            .SelectMany(result => result.Errors)
            .Select(error => error.ErrorMessage)
            .Distinct()
            .ToArray();

        if (errors.Length > 0)
        {
            throw new ApplicationValidationException(
                string.Join(" ", errors));
        }

        return await next(cancellationToken);
    }
}
