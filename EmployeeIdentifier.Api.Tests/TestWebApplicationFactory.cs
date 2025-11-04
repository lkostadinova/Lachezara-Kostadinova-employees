using EmployeeIdentifier.Api.RequestHandlers.Abstract;
using EmployeeIdentifier.Services.Services.Abstract;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EmployeeIdentifier.Api.Tests
{
    /// <summary>
    /// Custom WebApplicationFactory for integration testing
    /// </summary>
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Configure any test-specific services here if needed
                // For example, replace dependencies with test doubles
            });
        }
    }
}