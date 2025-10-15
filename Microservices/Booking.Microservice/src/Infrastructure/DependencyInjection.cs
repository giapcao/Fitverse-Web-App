using System;
using Domain.IRepositories;
using Domain.Persistence;
using Domain.Persistence.Enums;
using Infrastructure.Common;
using Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
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
        
        services.AddDbContext<FitverseBookingDbContext>((serviceProvider, options) =>
        {
            var connString = configuration.GetConnectionString("BookingDatabase")
                ?? configuration["Database:ConnectionString"]
                ?? configuration["ConnectionStrings:BookingDatabase"]
                ?? throw new InvalidOperationException("Booking database connection string is not configured.");

            options.UseNpgsql(connString, npgsqlOptions =>
            {
                npgsqlOptions.MapEnum<BookingStatus>("booking_status_enum");
                npgsqlOptions.MapEnum<SlotStatus>("slot_status_enum");
                npgsqlOptions.MapEnum<SubscriptionStatus>("subscription_status_enum");
                npgsqlOptions.MapEnum<SubscriptionEventType>("subscription_event_type_enum");
            });
        });

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAvailabilityRuleRepository, AvailabilityRuleRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ICoachTimeoffRepository, CoachTimeoffRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ISubscriptionEventRepository, SubscriptionEventRepository>();
        services.AddScoped<ITimeslotRepository, TimeslotRepository>();
        services.AddSingleton<EnvironmentConfig>();

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
                busConfigurator.SetKebabCaseEndpointNameFormatter();
                // busConfigurator.AddConsumer<UserCreatedConsumer>();
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
