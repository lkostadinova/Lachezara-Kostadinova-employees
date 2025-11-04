using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace EmployeeIdentifier.Logging
{
    public class JsonLoggerProvider : ILoggerProvider
    {
        private readonly string _componentName;
        private readonly string _environment;

        private readonly ConcurrentDictionary<string, JsonLogger> _loggers = new(StringComparer.Ordinal);

        public JsonLoggerProvider(string componentName, string environment)
        {
            _componentName = componentName;
            _environment = environment;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, category => new JsonLogger(Console.Out, _componentName, _environment, categoryName));
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
