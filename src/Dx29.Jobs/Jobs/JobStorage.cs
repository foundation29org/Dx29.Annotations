using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azure;
using Azure.Storage.Blobs.Specialized;

using Dx29.Services;

namespace Dx29.Jobs
{
    public interface IJobStorage
    {
        Task UploadOutputAsync(string name, object obj);
        Task UploadOutputAsync(string name, string str);
        Task UpdateStatusAsync(string status, string message = null);
        Task<JobStatus> GetJobStatusAsync();
    }

    public partial class JobStorage: IJobStorage
    {
        const string JOB_LOCK = "lock";
        const string JOB_INFO = "job.info";
        const string JOB_STATUS = "status.json";

        const string LOG_FOLDER = "logs";
        const string INPUT_FOLDER = "input";
        const string OUTPUT_FOLDER = "output";

        public JobStorage(BlobStorage blobStorage, string name, string token)
        {
            Name = name;
            Token = token;
            Storage = blobStorage;
            (string container, string path) = GetBlobPath(LOG_FOLDER, "job.log");

            Logger = new BlobLogger(Storage, container, path);
        }

        public string Name { get; }
        public string Token { get; }

        public BlobStorage Storage { get; }
        public BlobLogger Logger { get; }

        public async Task InitializeJobAsync(JobInfo jobInfo)
        {
            DateTime date = DateTime.UtcNow;
            await UploadStringAsync(null, JOB_LOCK, date.ToString("yyyy/MM/dd HH:mm:ss"));
            await UploadJobInfoAsync(jobInfo);
            await UploadJobStatusAsync(new JobStatus(Name, Token, "Created") { CreatedOn = date, LastUpdate = date });
        }

        //
        // Write Log
        //
        public async Task WriteLogAsync(string filename, object obj)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            await WriteLogAsync(filename, json);
        }
        public async Task WriteLogAsync(string filename, string str)
        {
            await UploadStringAsync(LOG_FOLDER, filename, str);
        }

        //
        // Get JobInfo
        //
        public async Task<JobInfo> GetJobInfoAsync()
        {
            return await GetJobInfoAsync<JobInfo>();
        }
        public async Task<T> GetJobInfoAsync<T>() where T : JobInfo
        {
            try
            {
                return await DownloadJobInfoAsync<T>();
            }
            catch (RequestFailedException ex)
            {
                if (ex.Status == 404) // Not Found
                {
                    return default(T);
                }
                throw;
            }
        }

        //
        // Update Status
        //
        public async Task UpdateStatusAsync(string status, string message = null)
        {
            var jobStatus = await GetJobStatusAsync();
            jobStatus.UpdateStatus(status, message);
            await UploadJobStatusAsync(jobStatus);
        }
        public async Task UpdateStatusAsync(Result result, string errorCode = null)
        {
            var jobStatus = await GetJobStatusAsync();
            jobStatus.UpdateStatus(result, errorCode);
            await UploadJobStatusAsync(jobStatus);
        }
        public async Task UpdateStatusAsync(Exception ex, string errorCode = null)
        {
            var jobStatus = await GetJobStatusAsync();
            jobStatus.UpdateStatus(ex, errorCode);
            await UploadJobStatusAsync(jobStatus);
        }

        public async Task<JobStatus> GetJobStatusAsync()
        {
            try
            {
                return await DownloadJobStatusAsync<JobStatus>();
            }
            catch (RequestFailedException ex)
            {
                if (ex.Status == 404) // Not Found
                {
                    return null;
                }
                throw;
            }
        }

        //
        // Lock
        //
        public async Task<BlobLeaseClient> AcquireLockAsync(int secs = 30)
        {
            (string container, string path) = GetBlobPath(null, JOB_LOCK);
            return await AcquireLeaseAsync(container, path, secs);
        }

        public async Task<BlobLeaseClient> AcquireLeaseAsync(string blobContainerName, string blobName, int secs = 30)
        {
            try
            {
                var lease = Storage.GetLease(blobContainerName, blobName);
                await lease.AcquireAsync(TimeSpan.FromSeconds(secs));
                return lease;
            }
            catch (RequestFailedException ex)
            {
                if (ex.Status != 404) return null; // Not Found
            }
            catch { return null; }
            try
            {
                await UploadStringAsync(null, JOB_LOCK, DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss"));
                var lease = Storage.GetLease(blobContainerName, blobName);
                await lease.AcquireAsync(TimeSpan.FromSeconds(secs));
                return lease;
            }
            catch { return null; }
        }

        //
        //  List
        //
        public async Task<IList<string>> ListOutputFilesAsync()
        {
            (string container, string path) = GetBlobPath(OUTPUT_FOLDER);
            return await Storage.ListBlobsAsync(container, path);
        }

        //
        // Upload
        //
        public async Task UploadJobInfoAsync(JobInfo jobInfo) => await UploadObjectAsync(null, JOB_INFO, jobInfo);
        public async Task UploadJobStatusAsync(JobStatus jobStatus) => await UploadObjectAsync(null, JOB_STATUS, jobStatus);

        public async Task UploadInputAsync(string name, object obj) => await UploadObjectAsync(INPUT_FOLDER, name, obj);
        public async Task UploadInputAsync(string name, string str) => await UploadStringAsync(INPUT_FOLDER, name, str);
        public async Task UploadInputAsync(string name, Stream stream) => await UploadStreamAsync(INPUT_FOLDER, name, stream);

        public async Task UploadOutputAsync(string name, object obj) => await UploadObjectAsync(OUTPUT_FOLDER, name, obj);
        public async Task UploadOutputAsync(string name, string str) => await UploadStringAsync(OUTPUT_FOLDER, name, str);
        public async Task UploadOutputAsync(string name, Stream stream) => await UploadStreamAsync(OUTPUT_FOLDER, name, stream);

        public async Task UploadOuputFolderAsync(string sourcePath)
        {
            (string container, string path) = GetBlobPath(OUTPUT_FOLDER);
            await Storage.UploadBlobsAsync(container, path, sourcePath);
        }

        //
        // Download
        //
        public async Task<JobInfo> DownloadJobInfoAsync() => await DownloadJobInfoAsync<JobInfo>();
        public async Task<T> DownloadJobInfoAsync<T>() where T : JobInfo => await DownloadObjectAsync<T>(null, JOB_INFO);

        public async Task<JobStatus> DownloadJobStatusAsync() => await DownloadJobStatusAsync<JobStatus>();
        public async Task<T> DownloadJobStatusAsync<T>() where T : JobStatus => await DownloadObjectAsync<T>(null, JOB_STATUS);

        public async Task<T> DownloadInputObjectAsync<T>(string name) => await DownloadObjectAsync<T>(INPUT_FOLDER, name);
        public async Task<string> DownloadInputStringAsync(string name) => await DownloadStringAsync(INPUT_FOLDER, name);
        public async Task<Stream> DownloadInputStreamAsync(string name) => await DownloadStreamAsync(INPUT_FOLDER, name);

        public async Task<T> DownloadOutputObjectAsync<T>(string name) => await DownloadObjectAsync<T>(OUTPUT_FOLDER, name);
        public async Task<string> DownloadOutputStringAsync(string name) => await DownloadStringAsync(OUTPUT_FOLDER, name);
        public async Task<Stream> DownloadOutputStreamAsync(string name) => await DownloadStreamAsync(OUTPUT_FOLDER, name);

        public async Task<IList<string>> DownloadInputFolderAsync(string targetPath)
        {
            (string container, string path) = GetBlobPath(INPUT_FOLDER);
            return await Storage.DownloadBlobsAsync(container, path, targetPath);
        }

        #region Upload
        public async Task<string> UploadObjectAsync(string folder, string filename, object obj)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            return await UploadStringAsync(folder, filename, json);
        }
        public async Task<string> UploadStringAsync(string folder, string filename, string str)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            {
                return await UploadStreamAsync(folder, filename, stream);
            }
        }
        public async Task<string> UploadStreamAsync(string folder, string filename, Stream stream)
        {
            (string container, string path) = GetBlobPath(folder, filename);
            await Storage.UploadStreamAsync(container, path, stream);
            return path;
        }
        #endregion

        #region Download
        public async Task<T> DownloadObjectAsync<T>(string folder, string filename)
        {
            string json = await DownloadStringAsync(folder, filename);
            if (json != null)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            return default(T);
        }
        public async Task<string> DownloadStringAsync(string folder, string filename)
        {
            (string container, string path) = GetBlobPath(folder, filename);
            return await Storage.DownloadStringAsync(container, path);
        }
        public async Task<Stream> DownloadStreamAsync(string folder, string filename)
        {
            (string container, string path) = GetBlobPath(folder, filename);
            return await Storage.DownloadStreamAsync(container, path);
        }
        #endregion

        #region GetBlobPath
        private (string, string) GetBlobPath(string folder = null, string filename = null)
        {
            string container = $"jobs-{Name.ToLower()}";
            string yymd = Token.Substring(0, "yyyy-mm-dd".Length).Replace('-', '/');
            string path = $"{yymd}/{Token}";
            if (!String.IsNullOrEmpty(folder))
            {
                path = $"{path}/{folder}";
            }
            if (!String.IsNullOrEmpty(filename))
            {
                path = $"{path}/{filename}";
            }
            return (container, path);
        }
        #endregion
    }
}
