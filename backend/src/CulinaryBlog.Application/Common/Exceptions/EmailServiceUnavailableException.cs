namespace CulinaryBlog.Application.Common.Exceptions;

public sealed class EmailServiceUnavailableException(string message)
    : Exception(message);
