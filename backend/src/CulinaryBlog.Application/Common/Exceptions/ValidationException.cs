namespace CulinaryBlog.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    public ValidationException(string message)
        : base(message)
    {
    }

    public ValidationException(string message, IReadOnlyDictionary<string, string[]> errors)
        : base(message) => Errors = errors;

    public ValidationException(string message, string field)
        : this(message, new Dictionary<string, string[]> { [field] = [message] })
    {
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
