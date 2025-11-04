using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeIdentifier.Api.Shared.ExceptionHandling
{
    public class ErrorHandlingSettings
    {
        public const string CONFIG_SECTION = "ErrorHandlingSettings";

        public bool ShowException { get; set; }
    }
}
