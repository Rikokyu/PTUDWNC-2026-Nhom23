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
        var errors = _validators
            .SelectMany(validator => validator.Validate(request))
            .Where(error => !string.IsNullOrWhiteSpace(error))
            .Distinct()
            .ToList();

        if (errors.Count > 0)
        {
            throw new ValidationException(string.Join(" ", errors));
        }

        return await next(cancellationToken);
    }
}
