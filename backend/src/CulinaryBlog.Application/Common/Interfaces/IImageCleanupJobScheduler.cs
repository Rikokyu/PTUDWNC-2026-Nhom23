namespace CulinaryBlog.Application.Common.Interfaces;

public interface IImageCleanupJobScheduler
{
    void Enqueue(string fileUrl);
}
