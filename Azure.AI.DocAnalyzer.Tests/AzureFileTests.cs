using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Azure.AI.DocAnalyzer.Tests
{
    public class AzureFileTests
    {
        private string _accountName = "";
        private string _accountKey = "";
        private readonly string _shareName = "";
        private string _testFilePath = "";
        private string _fileName = "Invoice1.pdf";

        public AzureFileTests()
        {
            DotNetEnv.Env.Load();

            _accountName = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_NAME") ?? throw new InvalidOperationException("AZURE_STORAGE_ACCOUNT_NAME not set");
            _accountKey = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_KEY") ?? throw new InvalidOperationException("AZURE_STORAGE_ACCOUNT_KEY not set");
            _shareName = Environment.GetEnvironmentVariable("AZURE_STORAGE_SHARE_NAME") ?? throw new InvalidOperationException("AZURE_STORAGE_SHARE_NAME not set");
            _testFilePath = Path.Combine(AppContext.BaseDirectory, "Resources", "Invoice1.pdf");
            _fileName = "Invoice1.pdf";

            if (!File.Exists(_testFilePath))
            {
                throw new InvalidOperationException($"File not found - {_testFilePath}");
            }
        }

        [Fact]
        public async Task UploadFileWithSdkAsync_ShouldUploadSuccessfully()
        {
            // Arrange
            var azureFile = new AzureFile(_accountName, _accountKey, _shareName);

            // Act
            await azureFile.UploadFileWithSdkAsync(_testFilePath, _fileName);

            // Assert
            // If no exception is thrown, the test passes.
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
            Assert.Contains(_fileName, sasUri.ToString());
        }
    }
}
