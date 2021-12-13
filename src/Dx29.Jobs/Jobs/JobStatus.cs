using System;
using System.Collections.Generic;

namespace Dx29.Jobs
{
    public enum CommonStatus
    {
        Failed,
        Succeeded,
        Running,
        Preparing,
        Pending,
        Created,
        Unknown
    }

    public enum CommonErrorCodes
    {
        ERR_COMMON_0000, // Unexpected Error
    }

    public class JobStatus
    {
        public JobStatus()
        {
        }
        public JobStatus(string name, string token, string status = "Unknown")
        {
            Name = name;
            Token = token;
            DateTime date = DateTime.UtcNow;
            Status = status;
            CreatedOn = date;
            LastUpdate = date;
        }

        public string Name { get; set; }
        public string Token { get; set; }

        public string Status { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdate { get; set; }

        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }

        public List<JobStatusLog> Logs { get; set; }

        public void UpdateStatus(Result result, string errorCode = null)
        {
            var status = result.Success ? CommonStatus.Succeeded : CommonStatus.Failed;
            UpdateStatus(status.ToString(), errorCode, result.Message, result.Details);
        }
        public void UpdateStatus(Exception ex, string errorCode = null)
        {
            errorCode = errorCode ?? CommonErrorCodes.ERR_COMMON_0000.ToString();
            UpdateStatus("Failed", errorCode, $"{ex.GetType()}. {ex.Message}", ex.StackTrace);
        }
        public void UpdateStatus(string status, string errorCode, string message, string details)
        {
            UpdateStatus(status);
            ErrorCode = errorCode;
            Message = message;
            Details = details;
        }
        public void UpdateStatus(string status, string message = null)
        {
            Status = status;
            Message = message;
            LastUpdate = DateTime.UtcNow;
            Logs ??= new List<JobStatusLog>();
            Logs.Add(new JobStatusLog
            {
                DateTime = LastUpdate,
                Status = status,
                Message = message
            });
        }
    }

    public class JobStatusLog
    {
        public DateTime DateTime { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
