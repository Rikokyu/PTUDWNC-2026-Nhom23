namespace CulinaryBlog.Application.Common.Interfaces;

public interface IImageResizeJobScheduler
{
    void Enqueue(Guid imageId);
}
