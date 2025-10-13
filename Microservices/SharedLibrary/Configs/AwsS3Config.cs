using System;

namespace SharedLibrary.Configs
{
    /// <summary>
    /// Strongly typed options for AWS S3 settings.
    /// </summary>
    public class AwsS3Config
    {
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public string Region { get; set; } = "ap-southeast-1";
        public string? ServiceUrl { get; set; }
        public bool UseForcePathStyle { get; set; } = true;

        public bool HasCredentials =>
            !string.IsNullOrWhiteSpace(AccessKey) &&
            !string.IsNullOrWhiteSpace(SecretKey);
    }
}
