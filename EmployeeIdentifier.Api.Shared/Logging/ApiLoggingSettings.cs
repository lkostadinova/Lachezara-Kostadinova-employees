using System.Reflection;

namespace EmployeeIdentifier.Api.Shared.Logging
{
    public class ApiLoggingSettings
    {
        public bool LogRequestsAndResponses { get; private set; }

        public ApiLoggingSettings(bool logRequestResponses = true, Assembly modelsAssembly = null)
        {
            LogRequestsAndResponses = logRequestResponses;
        }
    }
}
