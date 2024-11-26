using Azure.Storage.Files.Shares;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;

namespace Azure.AI.DocAnalyzer
{
    /// <summary>
    /// Handles operations with Azure File Share, including file uploads via SDK or HTTP.
    /// </summary>
    public class AzureFile
    {
        private readonly ShareServiceClient _shareServiceClient;
        private readonly ShareClient _shareClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFile"/> class.
        /// </summary>
        /// <param name="accountName">Azure Storage account name.</param>
        /// <param name="accountKey">Azure Storage account key.</param>
        /// <param name="shareName">Azure File Share name.</param>
        public AzureFile(string accountName, string accountKey, string shareName)
        {
            var serviceUri = new Uri($"https://{accountName}.file.core.windows.net");
            var credential = new Azure.Storage.StorageSharedKeyCredential(accountName, accountKey);

            _shareServiceClient = new ShareServiceClient(serviceUri, credential);
            _shareClient = _shareServiceClient.GetShareClient(shareName);
        }

        /// <summary>
        /// Ensures the Azure File Share exists. If it does not exist, it will be created.
        /// </summary>
        public async Task EnsureShareExistsAsync()
        {
            await _shareClient.CreateIfNotExistsAsync();
        }

        /// <summary>
        /// Uploads a file to Azure File Share using the Azure SDK.
        /// </summary>
        /// <param name="filePath">The full path of the file to upload.</param>
        /// <param name="fileName">The name of the file to be created in the Azure File Share.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
        public async Task UploadFileWithSdkAsync(string filePath, string fileName)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            await EnsureShareExistsAsync();

            var fileClient = _shareClient.GetRootDirectoryClient().GetFileClient(fileName);
            using var fileStream = File.OpenRead(filePath);

            await fileClient.CreateAsync(fileStream.Length);
            await fileClient.UploadRangeAsync(new HttpRange(0, fileStream.Length), fileStream);
        }      
    }
}
