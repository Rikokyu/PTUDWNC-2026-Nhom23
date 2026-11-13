namespace CulinaryBlog.Application.Common.Exceptions;

public sealed class GoogleProviderUnavailableException(Exception innerException)
    : Exception("Google identity provider is unavailable.", innerException);
