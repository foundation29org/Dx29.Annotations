using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dx29.Services
{
    public class FileStorageClient2
    {
        public FileStorageClient2(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }

        public async Task<string> GetVersionAsync()
        {
            return await HttpClient.GETAsync($"About/version");
        }

        //
        //  Download
        //
        public async Task<string> DownloadStringAsync(string userId, string caseId, string path)
        {
            return await HttpClient.GETAsync($"FileStorage2/{userId}/{caseId}?path={path}");
        }
        public async Task<TValue> DownloadAsync<TValue>(string userId, string caseId, string path)
        {
            return await HttpClient.GETAsync<TValue>($"FileStorage2/{userId}/{caseId}?path={path}");
        }
        public async Task<Stream> DownloadAsync(string userId, string caseId, string path)
        {
            return await HttpClient.DownloadAsync($"FileStorage2/{userId}/{caseId}?path={path}");
        }

        //
        //  FileShare
        //
        public async Task<string> CreateFileShareAsync(string userId, string caseId, string path)
        {
            return await HttpClient.GETAsync($"FileStorage2/{userId}/{caseId}/share?path={path}");
        }

        //
        //  Upload
        //
        public async Task UploadFileAsync(string userId, string caseId, string path, object obj)
        {
            await HttpClient.POSTAsync($"FileStorage2/{userId}/{caseId}?path={path}", obj);
        }
        public async Task UploadFileAsync(string userId, string caseId, string path, string str)
        {
            await HttpClient.POSTAsync($"FileStorage2/{userId}/{caseId}?path={path}", str);
        }
        public async Task UploadFileAsync(string userId, string caseId, string path, Stream stream)
        {
            await HttpClient.POSTAsync($"FileStorage2/{userId}/{caseId}?path={path}", stream);
        }

        //
        //  Move
        //
        public async Task MoveFileAsync(string userId, string source, string target)
        {
            await HttpClient.PUTAsync($"FileStorage2/{userId}?source={source}&target={target}", "");
        }

        //
        //  Copy
        //
        public async Task CopyFileAsync(string userId, string source, string target)
        {
            await HttpClient.PUTAsync($"FileStorage2/{userId}?source={source}&target={target}&copy=true", "");
        }

        //
        //  Delete
        //
        public async Task DeleteFileAsync(string userId, string caseId, string path)
        {
            await HttpClient.DeleteAsync($"FileStorage2/{userId}/{caseId}?path={path}");
        }
    }
}
