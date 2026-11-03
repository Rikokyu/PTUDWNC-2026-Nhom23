namespace CulinaryBlog.Application.Common.Exceptions;

public sealed class PaginationException(string message) : Exception(message);
