using Microsoft.AspNetCore.Mvc.Testing;

namespace EmployeeIdentifier.Api.Tests
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>{});
        }
    }
}