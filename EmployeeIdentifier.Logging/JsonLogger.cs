using EmployeeIdentifier.Logging.Metadata;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Dynamic;

namespace EmployeeIdentifier.Logging
{
    public class JsonLogger : ILogger
    {
        private readonly TextWriter _writer;
        private readonly string _componentName;
        private readonly string _environmentName;
        private readonly string _categoryName;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public JsonLogger(TextWriter writer, string componentName, string environmentName, string categoryName)
        {
            _writer = writer;
            _componentName = componentName;
            _environmentName = environmentName;
            _categoryName = categoryName;

            _jsonSerializerSettings = new JsonSerializerSettings();
            _jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            _jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        public IDisposable BeginScope<TState>(TState state) => default;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter is null)
                throw new ArgumentNullException(nameof(formatter));

            if (!IsEnabled(logLevel))
            {
                return;
            }

            dynamic message = new ExpandoObject();
            message.Timestamp = DateTime.UtcNow;
            message.Level = logLevel.ToString();
            message.EventId = eventId.Id;
            message.EventName = eventId.Name;
            message.Category = _categoryName;
            message.Exception = exception?.ToString();
            message.Message = formatter(state, exception);
            message.Component = _componentName;
            message.Environment = _environmentName;

            var properties = LoggerThreadContext.Properties.GetProperties(false);
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    var messageAsDictionary = message as IDictionary<string, object>;
                    messageAsDictionary.Add(property.Key, property.Value?.ToString());
                }
            }

            _writer.WriteLine(JsonConvert.SerializeObject(message, _jsonSerializerSettings));
        }
    }
}
