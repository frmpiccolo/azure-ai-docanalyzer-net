﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Azure.AI.DocAnalyzer
{
    /// <summary>
    /// Azure Blob Storage helper class.
    /// </summary>
    public class AzureBlob
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="accountName">Azure Storage account name.</param>
        /// <param name="accountKey">Azure Storage account key.</param>
        /// <param name="containerName">Azure Blob Container name.</param>
        public AzureBlob(string accountName, string accountKey, string containerName)
        {
            var blobUri = new Uri($"https://{accountName}.blob.core.windows.net");
            var credential = new Azure.Storage.StorageSharedKeyCredential(accountName, accountKey);
            _blobServiceClient = new BlobServiceClient(blobUri, credential);
            _containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        }

        /// <summary>
        /// Ensure the container exists.
        /// </summary>
        /// <param name="makePublic">Whether to make the container public.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task EnsureContainerExistsAsync(bool makePublic = false)
        {
            await _containerClient.CreateIfNotExistsAsync();
            if (makePublic)
            {
                await _containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);
            }
        }

        /// <summary>
        /// Upload a file to Azure Blob Storage using the Azure SDK.
        /// </summary>
        /// <param name="filePath">The full path of the file to upload.</param>
        /// <param name="fileName">The name of the file in Azure Blob Storage.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
        public async Task UploadFileWithSdkAsync(string filePath, string fileName)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            await EnsureContainerExistsAsync();

            var blobClient = _containerClient.GetBlobClient(fileName);
            using var fileStream = File.OpenRead(filePath);
            await blobClient.UploadAsync(fileStream, overwrite: true);
        }      
    }
}
