using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IRequestValidator<TRequest>> _validators;

    public ValidationBehavior(
        IEnumerable<IRequestValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var fieldErrors = _validators
            .OfType<IFieldRequestValidator<TRequest>>()
            .SelectMany(validator => validator.ValidateFields(request))
            .GroupBy(error => error.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.SelectMany(error => error.Value)
                    .Where(error => !string.IsNullOrWhiteSpace(error))
                    .Distinct()
                    .ToArray(),
                StringComparer.OrdinalIgnoreCase);

        var generalErrors = _validators
            .Where(validator => validator is not IFieldRequestValidator<TRequest>)
            .SelectMany(validator => validator.Validate(request))
            .Where(error => !string.IsNullOrWhiteSpace(error))
            .Distinct()
            .ToArray();

        if (generalErrors.Length > 0)
        {
            fieldErrors["request"] = generalErrors;
        }

        if (fieldErrors.Count > 0)
        {
            throw new ValidationException(fieldErrors);
        }

        return await next(cancellationToken);
    }
}
