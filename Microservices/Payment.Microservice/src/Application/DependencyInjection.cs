using Application.Behaviors;
using Application.Common.Mapper;
using Application.Momo.Returns;
using Application.Payments.Returns;
using Application.PayOs.Returns;
using Application.VNPay.Returns;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Mapster;
using MapsterMapper;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;

            services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
            services.AddValidatorsFromAssembly(assembly);
            
            var config = TypeAdapterConfig.GlobalSettings;
            MappingConfig.RegisterMappings(config); 
            
            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkPipelineBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
            services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
            services.AddScoped<IPaymentGatewayReturnHandler, VnPayReturnHandler>();
            services.AddScoped<IPaymentGatewayReturnHandler, MomoReturnHandler>();
            services.AddScoped<IPaymentGatewayReturnHandler, PayOsReturnHandler>();

            return services;
        }
    }
}
