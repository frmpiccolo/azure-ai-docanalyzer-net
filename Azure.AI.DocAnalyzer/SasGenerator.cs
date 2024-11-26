using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Sas;
using System;

namespace Azure.AI.DocAnalyzer
{
    public static class SasGenerator
    {
        public static Uri GenerateSasUri(string accountName, string accountKey, string containerOrShareName, string resourcePath, DateTimeOffset expiresOn, bool isBlob = true)
        {
            var credentials = new Azure.Storage.StorageSharedKeyCredential(accountName, accountKey);

            if (isBlob)
            {
                var blobClient = new BlobClient(new Uri($"https://{accountName}.blob.core.windows.net/{containerOrShareName}/{resourcePath}"), credentials);
                return blobClient.GenerateSasUri(BlobSasPermissions.Read | BlobSasPermissions.Write, expiresOn);
            }
            else
            {
                var shareClient = new ShareClient(new Uri($"https://{accountName}.file.core.windows.net/{containerOrShareName}"), credentials);
                var fileClient = shareClient.GetRootDirectoryClient().GetFileClient(resourcePath);
                return fileClient.GenerateSasUri(ShareFileSasPermissions.Read | ShareFileSasPermissions.Write, expiresOn);
            }
        }
    }
}
