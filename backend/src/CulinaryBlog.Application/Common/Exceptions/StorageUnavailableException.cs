namespace CulinaryBlog.Application.Common.Exceptions;

public sealed class StorageUnavailableException : Exception
{
    public StorageUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}