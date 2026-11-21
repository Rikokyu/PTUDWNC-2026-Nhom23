namespace CulinaryBlog.Application.Common.Exceptions;

public sealed class GoogleProfileUnavailableException()
    : Exception("Google account must provide a verified email address.");
