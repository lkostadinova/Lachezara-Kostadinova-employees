namespace EmployeeIdentifier.Api.Shared.ExceptionHandling
{
    public class ErrorHandlingSettings
    {
        public const string CONFIG_SECTION = "ErrorHandlingSettings";

        public bool ShowException { get; set; }
    }
}
