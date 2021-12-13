using System;

using Dx29.Jobs;

namespace Dx29.Annotations
{
    static public class AnnotationsErrors
    {
        static public string GetErrorCode(Result result)
        {
            if (!result.Success)
            {
                return "ERR_ANNOTATIONS_000";
            }
            return null;
        }

        static public ErrorDescription GetErrorDescription(JobStatus jobStatus, string lan)
        {
            string code = jobStatus.ErrorCode;
            string severity = "Error";
            string message = jobStatus.Message;
            string description = jobStatus.Details;

            return new ErrorDescription
            {
                Code = code,
                Severity = severity,
                Message = message,
                Description = description,
                Language = lan
            };
        }
    }
}
