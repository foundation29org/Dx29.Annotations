using System;
using System.Collections.Generic;

namespace Dx29.Data
{
    public class ReportInfo
    {
        public string UserId { get; set; }
        public string CaseId { get; set; }
        public string ReportId { get; set; }

        static public ReportInfo FromArgs(IDictionary<string, string> args)
        {
            if (args != null)
            {
                if (args.ContainsKey("userId"))
                {
                    return new ReportInfo
                    {
                        UserId = args.TryGetValue("userId"),
                        CaseId = args.TryGetValue("caseId"),
                        ReportId = args.TryGetValue("reportId"),
                    };
                }
            }
            return null;
        }
    }
}
