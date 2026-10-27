namespace CulinaryBlog.Application.Common.Interfaces;

public interface IWelcomeEmailJobScheduler
{
    void Enqueue(string email, string displayName);
}
