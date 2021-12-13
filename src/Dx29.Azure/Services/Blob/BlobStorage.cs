using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace Dx29.Services
{
    public partial class BlobStorage
    {
        public BlobStorage(string connectionString)
        {
            BlobServiceClient = new BlobServiceClient(connectionString);
        }

        public BlobServiceClient BlobServiceClient { get; }

        public BlobContainerClient GetContainer(string blobContainerName, PublicAccessType accessType = PublicAccessType.None)
        {
            return BlobServiceClient.GetBlobContainerClient(blobContainerName);
        }

        public async Task<BlobContainerClient> CreateContainerAsync(string blobContainerName, PublicAccessType accessType = PublicAccessType.None)
        {
            var containerClient = BlobServiceClient.GetBlobContainerClient(blobContainerName);
            var response = await containerClient.CreateAsync(accessType);
            return containerClient;
        }

        public async Task<BlobContainerClient> GetOrCreateContainerAsync(string blobContainerName, PublicAccessType accessType = PublicAccessType.None)
        {
            var containerClient = BlobServiceClient.GetBlobContainerClient(blobContainerName);
            var response = await containerClient.CreateIfNotExistsAsync(accessType);
            return containerClient;
        }

        public async Task<(bool, BlobContainerClient)> EnsureContainerAsync(string blobContainerName, PublicAccessType accessType = PublicAccessType.None)
        {
            bool exists = false;
            var containerClient = BlobServiceClient.GetBlobContainerClient(blobContainerName);
            try
            {
                var response = await containerClient.CreateAsync(accessType);
            }
            catch (RequestFailedException ex)
            {
                exists = ex.ErrorCode == "ContainerAlreadyExists";
            }
            return (exists, containerClient);
        }

        //
        //  List Blobs
        //
        public IEnumerable<string> ListContainerNames(BlobContainerTraits traits = BlobContainerTraits.None, string prefix = null)
        {
            foreach (var item in BlobServiceClient.GetBlobContainers(traits, prefix: prefix))
            {
                yield return item.Name;
            }
        }

        //
        //  List Blobs
        //
        public async Task<IList<string>> ListBlobsAsync(string blobContainerName, string prefix)
        {
            var names = new List<string>();
            var containerClient = GetContainer(blobContainerName);
            var asyncBlobs = containerClient.GetBlobsAsync(prefix: prefix);
            await foreach (var item in asyncBlobs)
            {
                names.Add(item.Name);
            }
            return names;
        }

        //
        //  Upload Blobs
        //
        public async Task UploadBlobsAsync(string blobContainerName, string path, string folder)
        {
            foreach (var filename in Directory.GetFiles(folder))
            {
                using (var stream = new FileStream(filename, FileMode.Open))
                {
                    await UploadStreamAsync(blobContainerName, $"{path}/{Path.GetFileName(filename)}", stream);
                }
            }
        }

        //
        //  Download Blobs
        //
        public async Task<IList<string>> DownloadBlobsAsync(string blobContainerName, string prefix, string folder)
        {
            int len = prefix.Length + 1;
            var names = new List<string>();
            var containerClient = GetContainer(blobContainerName);
            var asyncBlobs = containerClient.GetBlobsAsync(prefix: prefix);
            await foreach (var item in asyncBlobs)
            {
                names.Add(item.Name);
            }

            var hash = new HashSet<string>();
            var filenames = new List<string>();
            foreach (var name in names)
            {
                string filename = Path.Combine(folder, name.Substring(len));
                filenames.Add(filename);
                string targetPath = Path.GetDirectoryName(filename);
                if (hash.Add(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }
                using (var stream = await DownloadStreamAsync(blobContainerName, name))
                {
                    using (var fileStream = new FileStream(filename, FileMode.Create))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }
            }

            return filenames;
        }

        //
        //  Upload
        //
        public async Task<BlobContentInfo> UploadObjectAsync(string blobContainerName, string blobName, object value, CancellationToken cancellationToken = default)
        {
            string json = value.Serialize(indented: true);
            return await UploadStringAsync(blobContainerName, blobName, json, cancellationToken);
        }

        public async Task<BlobContentInfo> UploadStringAsync(string blobContainerName, string blobName, string value, CancellationToken cancellationToken = default)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
            {
                return await UploadStreamAsync(blobContainerName, blobName, stream, cancellationToken);
            }
        }

        public async Task<BlobContentInfo> UploadFileAsync(string blobContainerName, string blobName, string filename, CancellationToken cancellationToken = default)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                return await UploadStreamAsync(blobContainerName, blobName, stream, cancellationToken);
            }
        }

        public async Task<BlobContentInfo> UploadStreamAsync(string blobContainerName, string blobName, Stream content, CancellationToken cancellationToken = default)
        {
            var containerClient = await GetOrCreateContainerAsync(blobContainerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            return await blobClient.UploadAsync(content, overwrite: true, cancellationToken: cancellationToken);
        }

        //
        //  Download
        //
        public async Task<TValue> DownloadObjectAsync<TValue>(string blobContainerName, string blobName, CancellationToken cancellationToken = default)
        {
            string json = await DownloadStringAsync(blobContainerName, blobName, cancellationToken);
            if (json != null)
            {
                return JsonSerializer.Deserialize<TValue>(json);
            }
            return default(TValue);
        }

        public async Task<string> DownloadStringAsync(string blobContainerName, string blobName, CancellationToken cancellationToken = default)
        {
            var stream = await DownloadStreamAsync(blobContainerName, blobName, cancellationToken);
            if (stream != null)
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            return null;
        }

        public async Task<Stream> DownloadStreamAsync(string blobContainerName, string blobName, CancellationToken cancellationToken = default)
        {
            try
            {
                var containerClient = BlobServiceClient.GetBlobContainerClient(blobContainerName);
                var blobClient = containerClient.GetBlockBlobClient(blobName);
                var response = await blobClient.DownloadAsync(cancellationToken);
                return response.Value.Content;
            }
            catch
            {
                return null;
            }
        }

        // Lease
        public BlobLeaseClient GetLease(string blobContainerName, string blobName, string leaseId = null)
        {
            var container = BlobServiceClient.GetBlobContainerClient(blobContainerName);
            var blob = container.GetBlobClient(blobName);
            var lease = blob.GetBlobLeaseClient(leaseId);
            return lease;
        }

        // Delete
        public async Task<Response> DeleteContainerAsync(string blobContainerName, CancellationToken cancellationToken = default)
        {
            var containerClient = BlobServiceClient.GetBlobContainerClient(blobContainerName);
            return await containerClient.DeleteAsync(cancellationToken: cancellationToken);
        }

        public async Task<Response> DeleteBlobAsync(string blobContainerName, string blobName, CancellationToken cancellationToken = default)
        {
            var containerClient = BlobServiceClient.GetBlobContainerClient(blobContainerName);
            var blobClient = containerClient.GetBlockBlobClient(blobName);
            return await blobClient.DeleteAsync(cancellationToken: cancellationToken);
        }
    }
}
