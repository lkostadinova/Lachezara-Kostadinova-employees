using EmployeeIdentifier.Api;
using EmployeeIdentifier.Logging;
using Microsoft.AspNetCore.Hosting;
using System.Reflection;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.ClearProviders();

                var componentName = Assembly.GetAssembly(typeof(Program)).GetName().Name;
                var environment = hostingContext.Configuration.GetSection("Environment").Value;
                using var loggerProvider = new JsonLoggerProvider(componentName, environment);

                logging.AddProvider(loggerProvider);

            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}