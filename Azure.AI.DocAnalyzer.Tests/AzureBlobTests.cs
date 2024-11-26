using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Azure.AI.DocAnalyzer.Tests
{
    public class AzureBlobTests
    {
        private string _accountName = "";
        private string _accountKey = "";
        private string _containerName = "";
        private string _testFilePath = "";
        private string _blobName = "";

        public AzureBlobTests()
        {
            DotNetEnv.Env.Load();

            _accountName = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_NAME") ?? throw new InvalidOperationException("AZURE_STORAGE_ACCOUNT_NAME not set");
            _accountKey = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_KEY") ?? throw new InvalidOperationException("AZURE_STORAGE_ACCOUNT_KEY not set");
            _containerName = Environment.GetEnvironmentVariable("AZURE_STORAGE_BLOB_CONTAINER_NAME") ?? throw new InvalidOperationException("AZURE_STORAGE_BLOB_CONTAINER_NAME not set");
            _testFilePath = Path.Combine(AppContext.BaseDirectory, "Resources", "Invoice1.pdf");
            _blobName = "Invoice1.pdf";

            if (!File.Exists(_testFilePath))
            {
                throw new InvalidOperationException($"File not found - {_testFilePath}");
            }
        }

        [Fact]
        public async Task UploadFileWithSdkAsync_ShouldUploadSuccessfully()
        {
            // Arrange
            var azureBlob = new AzureBlob(_accountName, _accountKey, _containerName);

            // Act
            await azureBlob.UploadFileWithSdkAsync(_testFilePath, _blobName);

            // Assert
            // If no exception is thrown, the test passes.
        }        

        [Fact]
        public void GenerateBlobSasUri_ShouldReturnValidUrl()
        {
            // Arrange
            DateTimeOffset expiration = DateTimeOffset.UtcNow.AddHours(1);

            // Act
            var sasUri = SasGenerator.GenerateSasUri(_accountName, _accountKey, _containerName, _blobName, expiration, isBlob: true);

            // Assert
            Assert.Contains("sig=", sasUri.ToString());
            Assert.Contains(_blobName, sasUri.ToString());
        }
    }
}
