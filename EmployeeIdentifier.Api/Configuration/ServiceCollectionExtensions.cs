using EmployeeIdentifier.Api.RequestHandlers;
using EmployeeIdentifier.Api.RequestHandlers.Abstract;
using EmployeeIdentifier.Api.Shared.ExceptionHandling;
using EmployeeIdentifier.Services.Services;
using EmployeeIdentifier.Services.Services.Abstract;

namespace EmployeeIdentifier.Api.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfigSettings(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<ErrorHandlingSettings>(config.GetSection(ErrorHandlingSettings.CONFIG_SECTION));

            return services;
        }

        public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IEmployeeCollaborationService, EmployeeCollaborationService>();
            services.AddScoped<IGetEmployeeCollaborationRequestHandler, GetEmployeeCollaborationRequestHandler>();

            return services;
        }
    }
}
