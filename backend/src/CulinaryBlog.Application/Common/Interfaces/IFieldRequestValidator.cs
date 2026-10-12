namespace CulinaryBlog.Application.Common.Interfaces;

public interface IFieldRequestValidator<in TRequest>
    : IRequestValidator<TRequest>
{
    IReadOnlyDictionary<string, string[]> ValidateFields(TRequest request);
}
