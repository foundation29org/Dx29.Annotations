using System;
using System.Collections.Generic;

namespace Dx29.Annotations
{
    public class TranslateRequest : List<TranslateText>
    {
    }

    public class TranslateResponse : List<TranslateItem>
    {
    }

    public class TranslateItem
    {
        public List<TranslateText> Translations { get; set; }
    }

    public class TranslateText
    {
        public string Text { get; set; }
    }
}
