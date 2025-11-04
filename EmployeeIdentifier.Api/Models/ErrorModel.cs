using EmployeeIdentifier.Api.Models.Enums;
using EmployeeIdentifier.Logging.Metadata;

namespace EmployeeIdentifier.Api.Models
{
    public class ErrorModel
    {
        public ErrorModel()
        {
            Messages = new List<string>();
        }

        public ErrorModel(ErrorCodes code, string message) : this(code, message, null)
        {
        }

        public ErrorModel(ErrorCodes code, string message, Exception ex) : this()
        {
            Code = code;
            RequestId = LoggerThreadContext.Properties["trackingId"]?.ToString();
            Messages.Add(message);
            Details = ex;
        }

        public ErrorModel(ErrorCodes code, List<string> messages) : this(code, messages, null)
        {
        }

        public ErrorModel(ErrorCodes code, List<string> messages, Exception ex) : this()
        {
            Code = code;
            RequestId = LoggerThreadContext.Properties["trackingId"]?.ToString();
            Messages = messages;
            Details = ex;
        }

        public ErrorCodes Code { get; set; }
        public string RequestId { get; set; }
        public List<string> Messages { get; set; }

        /// <summary>
        ///  Contains the exception if any. Should be NOT available in production environment.
        /// </summary>
        public Exception Details { get; set; }
    }
}
