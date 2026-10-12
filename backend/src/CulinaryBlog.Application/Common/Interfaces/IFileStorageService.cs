using CulinaryBlog.Application.Common.Exceptions;

namespace CulinaryBlog.Application.Common.Interfaces;

public interface IFileStorageService
{
	/// <summary>
	/// Uploads a file to the configured object storage.
	/// </summary>
	Task<string> UploadAsync(
		Stream content,
		string objectKey,
		long size,
		string contentType,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes a file from the configured object storage.
	/// </summary>
	/// <remarks>
	/// Completes successfully only when the delete succeeds; storage failures are
	/// reported by throwing <see cref="StorageUnavailableException"/>.
	/// </remarks>
	/// <param name="objectKey">The object key of the file to delete.</param>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <exception cref="StorageUnavailableException">
	/// The object storage operation failed.
	/// </exception>
	Task DeleteAsync(
		string objectKey,
		CancellationToken cancellationToken = default);
}
