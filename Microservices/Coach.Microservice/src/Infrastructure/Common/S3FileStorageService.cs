using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedLibrary.Configs;
using SharedLibrary.Storage;

namespace Infrastructure.Common;

public sealed class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly AwsS3Config _config;
    private readonly ILogger<S3FileStorageService> _logger;

    public S3FileStorageService(
        IAmazonS3 s3Client,
        IOptions<AwsS3Config> config,
        ILogger<S3FileStorageService> logger)
    {
        _s3Client = s3Client;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<FileUploadResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Content.Length == 0)
        {
            throw new ArgumentException("File content must not be empty.", nameof(request));
        }

        var key = BuildObjectKey(request);

        using var stream = new MemoryStream(request.Content);
        var putRequest = new PutObjectRequest
        {
            BucketName = _config.BucketName,
            Key = key,
            InputStream = stream,
            ContentType = request.ContentType,
            AutoCloseStream = false
        };

        await _s3Client.PutObjectAsync(putRequest, cancellationToken).ConfigureAwait(false);

        var url = BuildFileUrl(key);
        _logger.LogInformation("Uploaded file {FileName} to S3 bucket {Bucket} with key {Key}", request.FileName, _config.BucketName, key);
        return new FileUploadResult(key, url);
    }

    public Task<string> GetFileUrlAsync(string key, TimeSpan? expiresIn = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Object key must not be empty.", nameof(key));
        }

        key = NormalizeKey(key);

        if (expiresIn is { } ttl && ttl > TimeSpan.Zero)
        {
            var presignRequest = new GetPreSignedUrlRequest
            {
                BucketName = _config.BucketName,
                Key = key,
                Expires = DateTime.UtcNow.Add(ttl)
            };

            if (!string.IsNullOrWhiteSpace(_config.ServiceUrl))
            {
                presignRequest.Protocol = _config.ServiceUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase)
                    ? Protocol.HTTPS
                    : Protocol.HTTP;
            }

            var url = _s3Client.GetPreSignedURL(presignRequest);
            return Task.FromResult(url);
        }

        return Task.FromResult(BuildFileUrl(key));
    }

    private string BuildObjectKey(FileUploadRequest request)
    {
        var sanitizedFileName = Path.GetFileName(request.FileName)
            .Replace("/", "_", StringComparison.Ordinal)
            .Replace("\\", "_", StringComparison.Ordinal);
        var directorySegment = string.IsNullOrWhiteSpace(request.Directory)
            ? "coach"
            : request.Directory.Trim().Trim('/').Replace("..", string.Empty);

        if (string.IsNullOrWhiteSpace(directorySegment))
        {
            directorySegment = "coach";
        }

        var ownerSegment = request.OwnerId.ToString("D");
        return $"{ownerSegment}/{directorySegment}/{Guid.NewGuid():N}-{sanitizedFileName}";
    }

    private string BuildFileUrl(string key)
    {
        if (!string.IsNullOrWhiteSpace(_config.ServiceUrl) && _config.UseForcePathStyle)
        {
            var baseUri = _config.ServiceUrl.TrimEnd('/');
            return $"{baseUri}/{_config.BucketName}/{key}";
        }

        return $"https://{_config.BucketName}.s3.{_config.Region}.amazonaws.com/{key}";
    }

    public static IAmazonS3 CreateS3Client(AwsS3Config config)
    {
        var region = RegionEndpoint.GetBySystemName(string.IsNullOrWhiteSpace(config.Region) ? "us-east-1" : config.Region);

        var s3Config = new AmazonS3Config
        {
            RegionEndpoint = region,
            ForcePathStyle = config.UseForcePathStyle
        };

        if (!string.IsNullOrWhiteSpace(config.ServiceUrl))
        {
            s3Config.ServiceURL = config.ServiceUrl;
        }

        AWSCredentials credentials;
        if (config.HasCredentials)
        {
            credentials = new BasicAWSCredentials(config.AccessKey, config.SecretKey);
        }
        else
        {
            try
            {
                credentials = FallbackCredentialsFactory.GetCredentials();
            }
            catch (AmazonClientException)
            {
                credentials = new AnonymousAWSCredentials();
            }
        }

        return new AmazonS3Client(credentials, s3Config);
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        var normalizedKey = NormalizeKey(key);

        try
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _config.BucketName,
                Key = normalizedKey
            };

            await _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Deleted file with key {Key} from S3 bucket {Bucket}", normalizedKey, _config.BucketName);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete file with key {Key} from S3 bucket {Bucket}", normalizedKey, _config.BucketName);
        }
    }

    private string NormalizeKey(string key)
    {
        var trimmed = key.Trim();
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            var path = uri.AbsolutePath.TrimStart('/');

            if (!string.IsNullOrWhiteSpace(_config.BucketName) &&
                path.StartsWith($"{_config.BucketName}/", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(_config.BucketName.Length + 1);
            }

            return path;
        }

        return trimmed.TrimStart('/');
    }
}
