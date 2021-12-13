using System;

namespace Dx29
{
    static public class ArgsParser
    {
        static public string GetJobToken(string[] args)
        {
            if (args.Length > 0)
            {
                string token = args[0];
                if (!String.IsNullOrEmpty(token))
                {
                    return token;
                }
            }
            throw new ArgumentException("Missing job Token parameter.");
        }

        static public int GetOptionalInteger(string[] args, int index, int defaultValue = 0)
        {
            return Int32.Parse(GetOptionalString(args, index, defaultValue.ToString()));
        }
        static public int GetRequiredInteger(string[] args, int index, string errorMessage)
        {
            return Int32.Parse(GetRequiredString(args, index, errorMessage));
        }

        static public string GetOptionalString(string[] args, int index, string defaultValue = "")
        {
            if (args.Length > index)
            {
                return args[index];
            }
            return defaultValue;
        }
        static public string GetRequiredString(string[] args, int index, string errorMessage)
        {
            if (args.Length > index)
            {
                return args[index];
            }
            throw new ArgumentException(errorMessage);
        }
    }
}
