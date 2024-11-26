using System;
using Xunit;

namespace Azure.AI.DocAnalyzer.Tests
{
    public class SasGeneratorTests
    {
        private string _accountName = "";
        private string _accountKey = "";
        private string _containerName = "";
        private string _blobName = "";
        private string _shareName = "";
        private string _fileName = "";

        public SasGeneratorTests()
        {
            DotNetEnv.Env.Load();

            _accountName = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_NAME") ?? throw new InvalidOperationException("AZURE_STORAGE_ACCOUNT_NAME not set");
            _accountKey = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_KEY") ?? throw new InvalidOperationException("AZURE_STORAGE_ACCOUNT_KEY not set");
            _containerName = Environment.GetEnvironmentVariable("AZURE_STORAGE_BLOB_CONTAINER_NAME") ?? throw new InvalidOperationException("AZURE_STORAGE_BLOB_CONTAINER_NAME not set");
            _blobName = "Invoice1.pdf";
            _shareName = Environment.GetEnvironmentVariable("AZURE_STORAGE_SHARE_NAME") ?? throw new InvalidOperationException("AZURE_STORAGE_SHARE_NAME not set");
            _fileName = "Invoice1.pdf";

            if (string.IsNullOrWhiteSpace(_accountName) || string.IsNullOrWhiteSpace(_accountKey) || string.IsNullOrWhiteSpace(_containerName) || string.IsNullOrWhiteSpace(_shareName))
            {
                throw new InvalidOperationException("Required environment variables are not set.");
            }
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
            Assert.Contains(_containerName, sasUri.ToString());
            Assert.Contains(_blobName, sasUri.ToString());
        }

        [Fact]
        public void GenerateFileSasUri_ShouldReturnValidUrl()
        {
            // Arrange
            DateTimeOffset expiration = DateTimeOffset.UtcNow.AddHours(1);

            // Act
            var sasUri = SasGenerator.GenerateSasUri(_accountName, _accountKey, _shareName, _fileName, expiration, isBlob: false);

            // Assert
            Assert.Contains("sig=", sasUri.ToString());
            Assert.Contains(_shareName, sasUri.ToString());
            Assert.Contains(_fileName, sasUri.ToString());
        }
    }
}
