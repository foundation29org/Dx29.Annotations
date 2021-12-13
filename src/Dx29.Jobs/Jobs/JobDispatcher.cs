using System;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

using Dx29.Services;

namespace Dx29.Jobs
{
    abstract public partial class JobDispatcher
    {
        public JobDispatcher(ServiceBus serviceBus, BlobStorage storage, ILogger logger)
        {
            ServiceBus = serviceBus;
            Storage = storage;
            Logger = logger;
            Options = new MessageHandlerOptions(ExceptionHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 1
            };
        }

        public ServiceBus ServiceBus { get; }
        public BlobStorage Storage { get; }
        public ILogger Logger { get; set; }

        public MessageHandlerOptions Options { get; }

        abstract public string JobName { get; }

        protected bool IsBusy { get; set; }
        protected DateTimeOffset LastMessageDateTime { get; set; }

        protected abstract Task<bool> OnMessageAsync(JobStorage storage, Message message, JobInfo jobInfo);

        virtual protected Task ExceptionHandler(ExceptionReceivedEventArgs args)
        {
            var context = args.ExceptionReceivedContext;
            Logger.LogError(@"Message handler encountered an exception.
    Endpoint: {endpoint}
    Entity Path: {entityPath}
    Executing Action: {action}
    Exception {exception}",
            context.Endpoint, context.EntityPath, context.Action, args.Exception);
            return Task.CompletedTask;
        }

        public async Task CloseAsync()
        {
            await ServiceBus.CloseAsync();
        }
    }
}
