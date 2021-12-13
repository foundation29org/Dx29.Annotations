using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dx29.Services
{
    public class FileStorageService
    {
        public FileStorageService(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }

        public async Task UploadAsync(string userId, string caseId, string resourceId, string name, object obj)
        {
            string path = GetPath(userId, caseId, resourceId, name);
            await HttpClient.POSTAsync(path, obj);
        }
        public async Task UploadAsync(string userId, string caseId, string resourceId, string name, string str)
        {
            string path = GetPath(userId, caseId, resourceId, name);
            await HttpClient.POSTAsync(path, str);
        }
        public async Task UploadAsync(string userId, string caseId, string resourceId, string name, Stream stream)
        {
            string path = GetPath(userId, caseId, resourceId, name);
            await HttpClient.POSTAsync(path, stream);
        }

        public async Task<string> DownloadAsync(string userId, string caseId, string resourceId, string name)
        {
            string path = GetPath(userId, caseId, resourceId, name);
            return await HttpClient.GetStringAsync(path);
        }
        public async Task<TValue> DownloadAsync<TValue>(string userId, string caseId, string resourceId, string name)
        {
            string path = GetPath(userId, caseId, resourceId, name);
            return await HttpClient.GETAsync<TValue>(path);
        }

        private static string GetPath(string userId, string caseId, string resourceId, string name)
        {
            return $"FileStorage/file/{userId}/{caseId}/{resourceId}/{name}";
        }
    }
}
