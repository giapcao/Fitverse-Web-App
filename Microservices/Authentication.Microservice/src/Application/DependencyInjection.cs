using Application.Abstractions.Interface;
using Application.Behaviors;
using Application.Common;
using Application.Common.Mapper;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;
            services.AddScoped<IOtpStore, RedisOtpStore>();
            services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
            services.AddValidatorsFromAssembly(assembly);
            var config = TypeAdapterConfig.GlobalSettings;
            MappingConfig.RegisterMappings(config); 
            
            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
            services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
            return services;

        }
    }
}