using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedLibrary.Configs
{
    public class EnvironmentConfig
    {
        public string DatabaseHost => Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "localhost";
        public int DatabasePort => int.TryParse(Environment.GetEnvironmentVariable("DATABASE_PORT"), out var port) ? port : 5432;
        public string DatabaseName => Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "defaultdb";
        public string DatabaseUser => Environment.GetEnvironmentVariable("DATABASE_USERNAME") ?? "postgres";
        public string DatabasePassword => Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "password";
        public string DatabaseProvider => Environment.GetEnvironmentVariable("DATABASE_PROVIDER") ?? "postgres";
        
        // RabbitMQ Cloud Configuration (priority)
        public string? RabbitMqUrl => Environment.GetEnvironmentVariable("RABBITMQ_URL");
        
        // RabbitMQ Local Configuration (fallback)
        public string RabbitMqHost => Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbit-mq";
        public int RabbitMqPort  => int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out var port) ? port : 5672;
        public string RabbitMqUser => Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "username";
        public string RabbitMqPassword => Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "password";
        
        // Helper property to determine if using cloud RabbitMQ
        public bool IsRabbitMqCloud => !string.IsNullOrEmpty(RabbitMqUrl);

        public string RedisHost => Environment.GetEnvironmentVariable("REDIS_HOST") ?? "redis";
        public string RedisPassword => Environment.GetEnvironmentVariable("REDIS_PASSWORD") ?? "default";
        public int RedisPort => int.TryParse(Environment.GetEnvironmentVariable("REDIS_PORT"), out var port) ? port : 6379;

        //AWS S3 Configuration
        public string AwsS3AccessKey => GetEnvironmentValue("AwsS3__AccessKey", "AWS_S3_ACCESS_KEY") ?? string.Empty;
        public string AwsS3SecretKey => GetEnvironmentValue("AwsS3__SecretKey", "AWS_S3_SECRET_KEY") ?? string.Empty;
        public string AwsS3BucketName => GetEnvironmentValue("AwsS3__BucketName", "AWS_S3_BUCKET_NAME") ?? "fitverse-files";
        public string AwsS3Region => GetEnvironmentValue("AwsS3__Region", "AWS_S3_REGION") ?? "ap-southeast-1";
        public string? AwsS3ServiceUrl => GetEnvironmentValue("AwsS3__ServiceUrl", "AWS_S3_SERVICE_URL");
        public bool AwsS3UseForcePathStyle =>
            !bool.TryParse(GetEnvironmentValue("AwsS3__UseForcePathStyle", "AWS_S3_USE_FORCE_PATH_STYLE"), out var value) || value;

        private static string? GetEnvironmentValue(params string[] keys)
        {
            foreach (var key in keys)
            {
                var value = Environment.GetEnvironmentVariable(key);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }
    }
} 
