namespace CulinaryBlog.Application.Common.Interfaces;

public interface IFileStorageService
{
	Task<string> UploadAsync(
		Stream content,
		string objectKey,
		long size,
		string contentType,
		CancellationToken cancellationToken = default);

	Task DeleteAsync(
		string objectKey,
		CancellationToken cancellationToken = default);
}
