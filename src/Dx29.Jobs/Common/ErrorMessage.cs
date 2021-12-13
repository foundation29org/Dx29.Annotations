using System;

namespace Dx29
{
    public enum ErrorType
    {
        Info,
        Warning,
        Error
    }

    public class ErrorMessage
    {
        public ErrorMessage()
        {
            Language = "es";
        }
        public ErrorMessage(ErrorType type, string code, string message, string details) : this()
        {
            Type = type.ToString();
            Code = code;
            Message = message;
            Details = details;
        }

        public string Type { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public string Language { get; set; }

        static public ErrorMessage Exception(Exception ex)
        {
            return Error("ERR_COMMON_500", "Internal Server Error", ex.Message);
        }

        static public ErrorMessage Info(string code, string message, string details)
        {
            return new ErrorMessage(ErrorType.Info, code, message, details);
        }

        static public ErrorMessage Warning(string code, string message, string details)
        {
            return new ErrorMessage(ErrorType.Warning, code, message, details);
        }

        static public ErrorMessage Error(string code, string message, string details)
        {
            return new ErrorMessage(ErrorType.Error, code, message, details);
        }
    }
}
