using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Dx29.Services;

namespace Dx29.Jobs
{
    abstract public class JobClient
    {
        public JobClient(ServiceBus serviceBus, BlobStorage blobStorage, string token)
        {
            ServiceBus = serviceBus;
            Storage = new JobStorage(blobStorage, JobName, token);
            Token = token;
        }

        abstract public string JobName { get; }

        public string Token { get; }
        public ServiceBus ServiceBus { get; }
        public JobStorage Storage { get; }

        protected JobInfo CreateJobInfo(string command, params (string, string)[] args)
        {
            var argumens = args?.Select(r => new KeyValuePair<string, string>(r.Item1, r.Item2));
            return new JobInfo { Name = JobName, Token = Token, Command = command, Args = new Dictionary<string, string>(argumens) };
        }

        protected async Task InitializeAsync(JobInfo jobInfo)
        {
            await Storage.InitializeJobAsync(jobInfo);
        }

        public async Task<JobStatus> SendMessageAsync(JobInfo jobInfo)
        {
            await ServiceBus.SendMessageAsync(jobInfo);
            return await GetStatusAsync();
        }

        public async Task<JobStatus> GetStatusAsync()
        {
            return await Storage.GetJobStatusAsync();
        }

        public async Task UploadInputAsync(string name, object obj) => await Storage.UploadInputAsync(name, obj);
        public async Task UploadInputAsync(string name, string str) => await Storage.UploadInputAsync(name, str);
        public async Task UploadInputAsync(string name, Stream stream) => await Storage.UploadInputAsync(name, stream);

        public async Task<T> DownloadOutputObjectAsync<T>(string name) => await Storage.DownloadOutputObjectAsync<T>(name);
        public async Task<string> DownloadOutputStringAsync(string name) => await Storage.DownloadOutputStringAsync(name);
        public async Task<Stream> DownloadOutputStreamAsync(string name) => await Storage.DownloadOutputStreamAsync(name);
    }
}
