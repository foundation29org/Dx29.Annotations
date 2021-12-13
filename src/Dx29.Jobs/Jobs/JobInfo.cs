using System;
using System.Collections.Generic;

namespace Dx29.Jobs
{
    public class JobInfo
    {
        public JobInfo()
        {
        }

        public string Name { get; set; }
        public string Token { get; set; }

        public string Command { get; set; }
        public IDictionary<string, string> Args { get; set; }

        public double GetThreshold()
        {
            if (Double.TryParse(Args.TryGetValue("threshold"), out double threshold))
            {
                return threshold;
            }
            return 0.89;
        }
    }
}
