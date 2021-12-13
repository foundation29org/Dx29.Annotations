using System;
using System.Collections.Generic;

namespace Dx29.Services
{
    public class NCRAnnotation
    {
        public string Id { get; set; }
        public string Text { get; set; }

        public IList<NCRAnnotationPhen> Phens { get; set; }
    }

    public class NCRAnnotationPhen
    {
        public string Id { get; set; }
        public string Concept { get; set; }
        public IList<int> Characters { get; set; }
        public double Probability { get; set; }
    }
}
