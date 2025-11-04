namespace EmployeeIdentifier.Logging.Metadata
{
    public class LoggerThreadContext
    {
        private readonly static LoggerThreadContextProperties _properties = new();

        public static LoggerThreadContextProperties Properties
        {
            get { return _properties; }
        }
    }
}
