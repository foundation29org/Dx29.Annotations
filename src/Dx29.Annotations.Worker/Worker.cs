using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace Dx29.Annotations.Worker
{
    public class Worker : BackgroundService
    {
        public Worker(AnnotationsDispatcher dispatcher, IHostApplicationLifetime applicationLifetime)
        {
            Dispatcher = dispatcher;
            ApplicationLifetime = applicationLifetime;
        }

        public AnnotationsDispatcher Dispatcher { get; }
        public IHostApplicationLifetime ApplicationLifetime { get; }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Executing version v0.13.01 ...");
            await Dispatcher.RunAsync(seconds: 60 * 60, cancellationToken);
            Console.WriteLine("Done!");
            ApplicationLifetime.StopApplication();
        }
    }
}
