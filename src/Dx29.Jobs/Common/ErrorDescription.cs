using System;

namespace Dx29
{
    public enum ErrorSeverity
    {
        Info,
        Warning,
        Error
    }

    public class ErrorDescription
    {
        public ErrorDescription()
        {
            Language = "en";
        }
        public ErrorDescription(ErrorSeverity severity, string code, string message, string description) : this()
        {
            Code = code;
            Severity = severity.ToString();
            Message = message;
            Description = description;
        }

        public string Code { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
    }
}
