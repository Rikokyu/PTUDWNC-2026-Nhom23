namespace CulinaryBlog.Application.Common.Interfaces;

public interface IRequestValidator<in TRequest>
{
    IReadOnlyList<string> Validate(TRequest request);
}
