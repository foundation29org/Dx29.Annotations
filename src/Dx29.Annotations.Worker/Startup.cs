using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Dx29.Services;

namespace Dx29.Annotations.Worker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            services.AddSingleton((c) => new BlobStorage(Configuration.GetConnectionString("BlobStorage")));
            services.AddSingleton((c) => new ServiceBus(Configuration["ServiceBus:ConnectionString"], Configuration["ServiceBus:QueueName"]));
            services.AddSingleton<AnnotationsDispatcher>();

            services.AddAnnotations(Configuration);
        }
    }
}
