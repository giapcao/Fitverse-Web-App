using System;
using System.IO;
using Domain.IRepositories;
using Infrastructure.Common;
using Infrastructure.Repositories;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedLibrary.Common;
using SharedLibrary.Configs;
using SharedLibrary.Utils;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        _ = configuration;

        services.AddScoped<IAvailabilityRuleRepository, AvailabilityRuleRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ISubscriptionEventRepository, SubscriptionEventRepository>();
        services.AddScoped<ITimeslotRepository, TimeslotRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddSingleton<EnvironmentConfig>();

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

        var solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName;
        if (!string.IsNullOrWhiteSpace(solutionDirectory))
        {
            DotNetEnv.Env.Load(Path.Combine(solutionDirectory, ".env"));
        }

        services.AddMassTransit(busConfigurator =>
        {
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
