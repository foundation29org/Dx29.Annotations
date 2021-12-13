using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Management;

namespace Dx29.Services
{
    public class SignalRService
    {
        public SignalRService(string connectionString, string hubName)
        {
            ServiceManager = new ServiceManagerBuilder()
                .WithOptions(option =>
                {
                    option.ConnectionString = connectionString;
                })
                .Build();
            HubName = hubName;
        }

        public IServiceManager ServiceManager { get; }
        public string HubName { get; }

        public async Task SendAllAsync(string method, object arg1, CancellationToken cancellationToken = default)
        {
            var hubContext = await ServiceManager.CreateHubContextAsync(HubName);
            await hubContext.Clients.All.SendAsync(method, arg1, cancellationToken);
            await hubContext.DisposeAsync();
        }
        public async Task SendAllAsync(string method, object arg1, object arg2, CancellationToken cancellationToken = default)
        {
            var hubContext = await ServiceManager.CreateHubContextAsync(HubName);
            await hubContext.Clients.All.SendAsync(method, arg1, arg2, cancellationToken);
            await hubContext.DisposeAsync();
        }
        public async Task SendAllAsync(string method, object arg1, object arg2, object arg3, CancellationToken cancellationToken = default)
        {
            var hubContext = await ServiceManager.CreateHubContextAsync(HubName);
            await hubContext.Clients.All.SendAsync(method, arg1, arg2, arg3, cancellationToken);
            await hubContext.DisposeAsync();
        }
        public async Task SendAllAsync(string method, object arg1, object arg2, object arg3, object arg4, CancellationToken cancellationToken = default)
        {
            var hubContext = await ServiceManager.CreateHubContextAsync(HubName);
            await hubContext.Clients.All.SendAsync(method, arg1, arg2, arg3, arg4, cancellationToken);
            await hubContext.DisposeAsync();
        }

        public async Task SendUserAsync(string user, string method, object arg1, CancellationToken cancellationToken = default)
        {
            var hubContext = await ServiceManager.CreateHubContextAsync(HubName);
            await hubContext.Clients.User(user).SendAsync(method, arg1, cancellationToken);
            await hubContext.DisposeAsync();
        }
        public async Task SendUserAsync(string user, string method, object arg1, object arg2, CancellationToken cancellationToken = default)
        {
            var hubContext = await ServiceManager.CreateHubContextAsync(HubName);
            await hubContext.Clients.User(user).SendAsync(method, arg1, arg2, cancellationToken);
            await hubContext.DisposeAsync();
        }
        public async Task SendUserAsync(string user, string method, object arg1, object arg2, object arg3, CancellationToken cancellationToken = default)
        {
            var hubContext = await ServiceManager.CreateHubContextAsync(HubName);
            await hubContext.Clients.User(user).SendAsync(method, arg1, arg2, arg3, cancellationToken);
            await hubContext.DisposeAsync();
        }
        public async Task SendUserAsync(string user, string method, object arg1, object arg2, object arg3, object arg4, CancellationToken cancellationToken = default)
        {
            var hubContext = await ServiceManager.CreateHubContextAsync(HubName);
            await hubContext.Clients.User(user).SendAsync(method, arg1, arg2, arg3, arg4, cancellationToken);
            await hubContext.DisposeAsync();
        }
    }
}
