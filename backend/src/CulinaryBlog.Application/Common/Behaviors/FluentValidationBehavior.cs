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
        var failures = results.SelectMany(result => result.Errors).ToArray();

        if (failures.Length > 0)
        {
            var errors = failures.GroupBy(error =>
                    error.PropertyName.Length == 0 ? "request" :
                    char.ToLowerInvariant(error.PropertyName[0]) + error.PropertyName[1..])
                .ToDictionary(group => group.Key,
                    group => group.Select(error => error.ErrorMessage).Distinct().ToArray());
            throw new ApplicationValidationException(
                string.Join(" ", failures.Select(error => error.ErrorMessage).Distinct()), errors);
        }

        return await next(cancellationToken);
    }
}
