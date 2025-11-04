using EmployeeIdentifier.Api.Configuration;
using EmployeeIdentifier.Api.Middleware;
using EmployeeIdentifier.Api.Models;
using EmployeeIdentifier.Api.Models.Enums;
using EmployeeIdentifier.Api.Shared.ExceptionHandling;
using EmployeeIdentifier.Api.Shared.Logging;
using EmployeeIdentifier.Api.Swagger;
using EmployeeIdentifier.Logging.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json.Serialization;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.Filters;

namespace EmployeeIdentifier.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConfigSettings(Configuration);
            services.AddDependencies(Configuration);

            services.AddControllers(options =>
            {
                options.OutputFormatters.RemoveType<StringOutputFormatter>();
                options.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var error = new ErrorResponseModel(new ErrorModel
                    {
                        Code = Models.Enums.ErrorCodes.VALIDATION_ERROR,
                        RequestId = LoggerThreadContext.Properties["trackingId"]?.ToString(),
                        Messages = context.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList()
                    });
                    var result = new BadRequestObjectResult(error);

                    return result;
                };
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddOpenApi();
            services.AddSwaggerGen(options =>
            {
                options.IncludeXmlComments(XmlCommentsFilePath.ControllerComments);
                options.IncludeXmlComments(XmlCommentsFilePath.ApiModelsComments);
            });

            services.AddSwaggerExamplesFromAssemblyOf<Startup>();
            services.AddSwaggerGenNewtonsoftSupport();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();

            //// Custom middlewares >>
            var loggingSettings = new ApiLoggingSettings(logRequestResponses: true, modelsAssembly: typeof(ErrorModel).Assembly);
            app.UseMiddleware<RequestResponseLoggingMiddleware>(loggingSettings);
            app.UseMiddleware<ErrorHandlerMiddleware>();
            //// << Custom middlewares

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // Configure Scalar 
                endpoints.MapOpenApi();
                endpoints.MapScalarApiReference();
            });
        }
    }
}
