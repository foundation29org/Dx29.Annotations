using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace Dx29.Services
{
    partial class BlobStorage
    {
        public BlobContainerClient CreateContainer(string blobContainerName, PublicAccessType accessType = PublicAccessType.None)
        {
            var containerClient = BlobServiceClient.GetBlobContainerClient(blobContainerName);
            var response = containerClient.Create(accessType);
            return containerClient;
        }

        public BlobContainerClient GetOrCreateContainer(string blobContainerName, PublicAccessType accessType = PublicAccessType.None)
        {
            var containerClient = BlobServiceClient.GetBlobContainerClient(blobContainerName);
            var response = containerClient.CreateIfNotExists(accessType);
            return containerClient;
        }

        //
        //  Upload
        //
        public BlobContentInfo UploadObject(string blobContainerName, string blobName, object value, CancellationToken cancellationToken = default)
        {
            string json = value.Serialize(indented: true);
            return UploadString(blobContainerName, blobName, json, cancellationToken);
        }

        public BlobContentInfo UploadString(string blobContainerName, string blobName, string value, CancellationToken cancellationToken = default)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
            {
                return UploadStream(blobContainerName, blobName, stream, cancellationToken);
            }
        }

        public BlobContentInfo UploadStream(string blobContainerName, string blobName, Stream content, CancellationToken cancellationToken = default)
        {
            var containerClient = GetOrCreateContainer(blobContainerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            return blobClient.Upload(content, overwrite: true, cancellationToken: cancellationToken);
        }

        //
        //  Download
        //
        public TValue DownloadObject<TValue>(string blobContainerName, string blobName, CancellationToken cancellationToken = default)
        {
            string json = DownloadString(blobContainerName, blobName, cancellationToken);
            return JsonSerializer.Deserialize<TValue>(json);
        }

        public string DownloadString(string blobContainerName, string blobName, CancellationToken cancellationToken = default)
        {
            try
            {
                var stream = DownloadStream(blobContainerName, blobName, cancellationToken);
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch
            {
                return null;
            }
        }

        public Stream DownloadStream(string blobContainerName, string blobName, CancellationToken cancellationToken = default)
        {
            var containerClient = BlobServiceClient.GetBlobContainerClient(blobContainerName);
            var blobClient = containerClient.GetBlockBlobClient(blobName);
            var response = blobClient.Download(cancellationToken);
            return response.Value.Content;
        }

        // Delete
        public Response DeleteContainer(string blobContainerName, CancellationToken cancellationToken = default)
        {
            var containerClient = BlobServiceClient.GetBlobContainerClient(blobContainerName);
            return containerClient.Delete(cancellationToken: cancellationToken);
        }

        public Response DeleteBlob(string blobContainerName, string blobName, CancellationToken cancellationToken = default)
        {
            var containerClient = BlobServiceClient.GetBlobContainerClient(blobContainerName);
            var blobClient = containerClient.GetBlockBlobClient(blobName);
            return blobClient.Delete(cancellationToken: cancellationToken);
        }
    }
}
