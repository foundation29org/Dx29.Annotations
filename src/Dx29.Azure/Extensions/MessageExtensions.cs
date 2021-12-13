using System;
using System.Text;

using Microsoft.Azure.ServiceBus;

using Newtonsoft.Json;

namespace Dx29.Services
{
    static public class MessageExtensions
    {
        static public TValue Deserialize<TValue>(this Message message)
        {
            string json = Encoding.UTF8.GetString(message.Body);
            return JsonConvert.DeserializeObject<TValue>(json);
        }
    }
}
