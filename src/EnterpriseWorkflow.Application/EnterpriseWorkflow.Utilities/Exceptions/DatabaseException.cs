using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Utilities.Exceptions
{
    public class DatabaseException : System.Exception
    {
        public string ErrorKey { get; } = string.Empty;
        public string Summary { get; } = string.Empty;
        public string Method { get; } = string.Empty;

        // Standard constructor with just a message
        public DatabaseException(string message)
            : base(message) { }

        // Constructor with message + inner exception
        public DatabaseException(string message, System.Exception inner)
            : base(message, inner) { }

        // Constructor with structured fields
        public DatabaseException(string errorKey, string summary, string method, System.Exception? inner = null)
            : base($"{errorKey}; Summary:{summary}; Method:{method}", inner)
        {
            ErrorKey = errorKey;
            Summary = summary;
            Method = method;
        }
    }
}
