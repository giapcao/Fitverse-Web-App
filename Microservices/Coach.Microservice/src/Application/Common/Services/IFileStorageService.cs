using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Services;

public interface IFileStorageService
{
    Task<FileUploadResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default);

    Task<string> GetFileUrlAsync(string key, TimeSpan? expiresIn = null, CancellationToken cancellationToken = default);
}

public sealed record FileUploadRequest(
    Guid CoachId,
    byte[] Content,
    string FileName,
    string ContentType,
    string? Directory = null);

public sealed record FileUploadResult(string Key, string Url);
