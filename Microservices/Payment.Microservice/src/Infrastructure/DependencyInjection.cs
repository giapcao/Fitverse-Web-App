using System;
using System.IO;
using Application.Sagas;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Common;
using Infrastructure.Context;
using Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using SharedLibrary.Common;
using SharedLibrary.Configs;
using SharedLibrary.Contracts.Payments;
using SharedLibrary.Utils;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletBalanceRepository, WalletBalanceRepository>();
        services.AddScoped<IWalletJournalRepository, WalletJournalRepository>();
        services.AddScoped<IWalletLedgerEntryRepository, WalletLedgerEntryRepository>();
        services.AddScoped<IWithdrawalRequestRepository, WithdrawalRequestRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddSingleton<EnvironmentConfig>();

        services.AddDbContext<FitverseDbContext>((serviceProvider, options) =>
        {
            var dbConfig = serviceProvider.GetRequiredService<IOptions<DatabaseConfig>>().Value;
            var dataSource = serviceProvider.GetRequiredService<NpgsqlDataSource>();
            options.UseNpgsql(dataSource, npgsqlOptions =>
            {
                npgsqlOptions.MapEnum<Dc>("dc_enum");
                npgsqlOptions.MapEnum<Gateway>("gateway_enum");
                npgsqlOptions.MapEnum<PaymentStatus>("payment_status_enum");
                npgsqlOptions.MapEnum<WalletAccountType>("wallet_account_type_enum");
                npgsqlOptions.MapEnum<WalletJournalStatus>("wallet_journal_status_enum");
                npgsqlOptions.MapEnum<WalletJournalType>("wallet_journal_type_enum");
                npgsqlOptions.MapEnum<WalletStatus>("wallet_status_enum");
                npgsqlOptions.MapEnum<WithdrawalRequestStatus>("withdrawal_request_status_enum");

                if (dbConfig.MaxRetryCount > 0)
                {
                    npgsqlOptions.EnableRetryOnFailure(dbConfig.MaxRetryCount);
                }

                if (dbConfig.CommandTimeout > 0)
                {
                    npgsqlOptions.CommandTimeout(dbConfig.CommandTimeout);
                }
            });

            var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
            if (environment.IsDevelopment())
            {
                if (dbConfig.EnableDetailedErrors)
                {
                    options.EnableDetailedErrors();
                }

                if (dbConfig.EnableSensitiveDataLogging)
                {
                    options.EnableSensitiveDataLogging();
                }
            }
        });

        using var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<AutoScaffold>>();
        var environmentConfig = serviceProvider.GetRequiredService<EnvironmentConfig>();
        var scaffold = new AutoScaffold(logger)
            .Configure(
                environmentConfig.DatabaseHost,
                environmentConfig.DatabasePort,
                environmentConfig.DatabaseName,
                environmentConfig.DatabaseUser,
                environmentConfig.DatabasePassword,
                environmentConfig.DatabaseProvider);

        scaffold.UpdateAppSettings();

        var solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(solutionDirectory))
        {
            DotNetEnv.Env.Load(Path.Combine(solutionDirectory, ".env"));
        }

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();

            busConfigurator.AddSagaStateMachine<PendingSubscriptionPackageSaga, PendingSubscriptionPackageSagaData>()
                .RedisRepository(repository =>
                {
                    repository.DatabaseConfiguration($"{environmentConfig.RedisHost}:{environmentConfig.RedisPort},password={environmentConfig.RedisPassword}");
                    repository.KeyPrefix = "pending-subscription-package-saga";
                    repository.Expiry = TimeSpan.FromMinutes(30);
                });

            busConfigurator.UsingRabbitMq((context, configurator) =>
            {
                if (environmentConfig.IsRabbitMqCloud)
                {
                    configurator.Host(environmentConfig.RabbitMqUrl);
                }
                else
                {
                    configurator.Host(new Uri($"rabbitmq://{environmentConfig.RabbitMqHost}:{environmentConfig.RabbitMqPort}/"), host =>
                    {
                        host.Username(environmentConfig.RabbitMqUser);
                        host.Password(environmentConfig.RabbitMqPassword);
                    });
                }

                configurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
