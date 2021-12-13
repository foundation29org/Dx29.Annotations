using System;

namespace Dx29.Data
{
    public class LangDetection
    {
        public string Language { get; set; }
        public double Score { get; set; }

        public bool IsTranslationSupported { get; set; }
        public bool IsTransliterationSupported { get; set; }
    }
}
