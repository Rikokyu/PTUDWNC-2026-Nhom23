namespace CulinaryBlog.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<byte[]> ReadAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);

    Task<string> UploadAsync(
        ReadOnlyMemory<byte> content,
        string extension,
        string folder,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);
}
