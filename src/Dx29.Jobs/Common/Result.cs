using System;

namespace Dx29
{
    public class Result
    {
        public Result(bool success = true)
        {
            Success = success;
        }

        public bool Success { get; }
        public string Message { get; set; }
        public string Details { get; set; }

        static public Result Ok()
        {
            return new Result();
        }
        static public Result Failed(string message, string description)
        {
            return new Result(success: false)
            {
                Message = message,
                Details = description
            };
        }
    }
}
