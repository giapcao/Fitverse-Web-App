using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharedLibrary.Storage;

public interface IFileStorageService
{
    Task<FileUploadResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default);

    Task<FileAccessUrls> GetFileUrlAsync(string key, TimeSpan? expiresIn = null, CancellationToken cancellationToken = default);

    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}

public sealed record FileUploadRequest(
    Guid OwnerId,
    byte[] Content,
    string FileName,
    string ContentType,
    string? Directory = null);

public sealed record FileUploadResult(string Key, string Url);

public sealed record FileAccessUrls(string InlineUrl, string DownloadUrl);
