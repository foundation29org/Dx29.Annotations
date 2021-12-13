using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;

namespace Dx29.Services
{
    public class ServiceBus
    {
        public ServiceBus(string connectionString, string entityPath, ReceiveMode receiveMode = ReceiveMode.PeekLock)
        {
            Queue = new QueueClient(connectionString, entityPath, receiveMode);
        }

        public QueueClient Queue { get; }

        public async Task SendMessageAsync(object message)
        {
            string json = message.Serialize();
            await SendMessageAsync(json);
        }
        public async Task SendMessageAsync(string body)
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            var message = new Message(bytes);
            await Queue.SendAsync(message);
        }

        public void RegisterMessageHandler(MessageHandlerOptions options, Func<Message, CancellationToken, Task> processHandler, Func<ExceptionReceivedEventArgs, Task> exceptionHandler)
        {
            Queue.RegisterMessageHandler(processHandler, options);
        }

        public async Task CompleteAsync(Message message)
        {
            // Safetly complete message
            try
            {
                await Queue.CompleteAsync(message.SystemProperties.LockToken);
            }
            catch { }
        }

        public async Task AbandonAsync(Message message, IDictionary<string, object> propertiesToModify = null)
        {
            // Safetly abandon message
            try
            {
                await Queue.AbandonAsync(message.SystemProperties.LockToken, propertiesToModify);
            }
            catch { }
        }

        public async Task CloseAsync()
        {
            await Queue.CloseAsync();
        }
    }
}
