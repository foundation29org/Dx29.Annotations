using System;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dx29.Annotations
{
    public class TokenParams
    {
        [BindRequired]
        public string Token { get; set; }
    }
}
