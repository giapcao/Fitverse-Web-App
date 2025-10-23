using System;
using Amazon.S3;
using Application.Sagas;
using Domain.IRepositories;
using Infrastructure.Common;
using Infrastructure.Repositories;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedLibrary.Common;
using SharedLibrary.Configs;
using SharedLibrary.Storage;
using SharedLibrary.Utils;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICoachProfileRepository, CoachProfileRepository>();
        services.AddScoped<ICoachCertificationRepository, CoachCertificationRepository>();
        services.AddScoped<ICoachMediaRepository, CoachMediaRepository>();
        services.AddScoped<ICoachServiceRepository, CoachServiceRepository>();
        services.AddScoped<IKycRecordRepository, KycRecordRepository>();
        services.AddScoped<ISportRepository, SportRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<EnvironmentConfig>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AwsS3Config>>();
            return S3FileStorageService.CreateS3Client(options.Value);
        });
        services.AddSingleton<IFileStorageService, S3FileStorageService>();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            using var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<AutoScaffold>>();
            var config = serviceProvider.GetRequiredService<EnvironmentConfig>();
            var scaffold = new AutoScaffold(logger)
                .Configure(
                    config.DatabaseHost,
                    config.DatabasePort,
                    config.DatabaseName,
                    config.DatabaseUser,
                    config.DatabasePassword,
                    config.DatabaseProvider);

            scaffold.UpdateAppSettings();
            string solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? "";
            if (solutionDirectory != null)
            {
                DotNetEnv.Env.Load(Path.Combine(solutionDirectory, ".env"));
            }
            services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.AddSagaStateMachine<CoachProfileCreatingSaga, CoachProfileCreatingSagaData>()
                    .RedisRepository(r =>
                    {
                        r.DatabaseConfiguration($"{config.RedisHost}:{config.RedisPort},password={config.RedisPassword}");
                        r.KeyPrefix = "coach-profile-creating-saga";
                        r.Expiry = TimeSpan.FromMinutes(10);
                    });

                busConfigurator.SetKebabCaseEndpointNameFormatter();

                busConfigurator.UsingRabbitMq((context, configurator) =>
                {
                    if (config.IsRabbitMqCloud)
                    {
                        configurator.Host(config.RabbitMqUrl);
                    }
                    else
                    {
                        configurator.Host(new Uri($"rabbitmq://{config.RabbitMqHost}:{config.RabbitMqPort}/"), h =>
                        {
                            h.Username(config.RabbitMqUser);
                            h.Password(config.RabbitMqPassword);
                        });
                    }
                    configurator.ConfigureEndpoints(context);
                });
            });
            return services;
        
    }
}
