using Xunit;

namespace EmployeeIdentifier.Api.Tests
{
    /// <summary>
    /// Collection definition for integration tests to ensure they don't run in parallel
    /// </summary>
    [CollectionDefinition("IntegrationTests")]
    public class IntegrationTestsCollection : ICollectionFixture<TestWebApplicationFactory>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}