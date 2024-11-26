# Azure.AI.DocAnalyzer

## Project Overview

`Azure.AI.DocAnalyzer` is a .NET-based application that integrates Azure Blob Storage and Azure Document Intelligence services. 
It provides functionality to securely upload documents, analyze their content, and extract meaningful insights. The project includes both a console application for CLI operations and unit tests for extensive validation.

---

> ⚠ **Disclaimer**: This project uses Azure services, which may incur costs. Be sure to review Azure's pricing and monitor your resource usage to avoid unexpected charges.

---

## Features

1. **Upload Files to Azure Blob Storage**:
   - Securely store documents using dynamically generated Shared Access Signatures (SAS).
2. **Analyze Documents**:
   - Extract metadata, tables, key-value pairs, and other structured data using Azure Document Intelligence.
3. **Extensive Testing**:
   - Comprehensive unit tests for all modules.
4. **Scalable Design**:
   - Structured project with modular components for easy extension and maintenance.

---

## Project Structure

```plaintext
Azure.AI.DocAnalyzer/
├── .env                            # Environment variables file (shared across all projects)
├── LICENSE                         # License file
├── README.md                       # Project documentation
├── Resources/                      # Folder containing example files for testing (shared across all projects)
│   └── Invoice1.pdf                # Sample invoice for testing
│   └── Invoice2.pdf                # Sample invoice for testing
│   └── Invoice3.pdf                # Sample invoice for testing
│   └── Invoice4.pdf                # Sample invoice for testing
│   └── Invoice5.pdf                # Sample invoice for testing
├── Azure.AI.DocAnalyzer/           # Core library for Azure integration
│   ├── AzureBlob.cs                # Handles Azure Blob Storage operations
│   ├── AzureFile.cs                # Handles Azure File Share operations
│   ├── DocumentIntelligence.cs     # Interacts with Azure Document Intelligence
│   ├── SasGenerator.cs             # Generates SAS tokens for Azure services
├── Azure.AI.DocAnalyzer.Console/   # Console application for document analysis
│   ├── Program.cs                  # Entry point for the console app
├── Azure.AI.DocAnalyzer.Tests/     # Unit tests
│   ├── AzureBlobTests.cs           # Tests for Azure Blob Storage operations
│   ├── AzureFileTests.cs           # Tests for Azure File Share operations
│   ├── SasGeneratorTests.cs        # Tests for SAS generator
│   ├── GlobalUsings.cs             # Global using statements for test project
```

---

## Prerequisites

1. **.NET SDK**:
   - Install the latest .NET SDK from [dotnet.microsoft.com](https://dotnet.microsoft.com/).
2. **Azure Subscription**:
   - An active Azure account to set up Blob Storage and Document Intelligence services.

---

## Installation

### Step 1: Clone the Repository
```bash
git clone https://github.com/frmpiccolo/azure-ai-docanalyzer-net.git
cd azure-ai-docanalyzer-net
```

### Step 2: Restore Dependencies
Run the following command in the solution directory:
```bash
dotnet restore
```

---

## Azure Services Configuration

### 1. Create a Resource Group
- Go to [Azure Portal](https://portal.azure.com/).
- Create a resource group (e.g., `azure-ai-docanalyzer-rg`).

### 2. Set Up Azure Blob Storage
- Create a Storage Account.
- Note the connection string from the **Access keys** section.

### 3. Set Up Azure Document Intelligence
- Create a Form Recognizer resource.
- Note the **Endpoint** and **API Key** from the **Keys and Endpoint** section.

---

## Configuration

1. Create a `.env` file in the root directory:
    ```plaintext
    AZURE_STORAGE_ACCOUNT_NAME=your_storage_account_name
    AZURE_STORAGE_ACCOUNT_KEY=your_storage_account_key
    AZURE_STORAGE_BLOB_CONTAINER_NAME=your_container_name
    AZURE_STORAGE_SHARE_NAME=your_share_name
    AZURE_DOCUMENT_INTELLIGENCE_ENDPOINT=https://your_document_intelligence_endpoint
    AZURE_DOCUMENT_INTELLIGENCE_KEY=your_document_intelligence_key
    ```

2. Replace placeholders with your actual Azure resource details.

---

## Usage

### Running the Console Application
To analyze documents using the console:
```bash
dotnet run --project Azure.AI.DocAnalyzer.Console
```

---

## Running Unit Tests

1. **Run Tests**:
   Use the following command to execute all tests:
   ```bash
   dotnet test
   ```

2. **Coverage Report**:
   Ensure your test project includes coverage settings for detailed insights.

---

## Key Features

1. **Document Insights**:
   - Extracts standard fields (key-value pairs), tables, barcodes, and images from documents.
2. **Blob Storage Integration**:
   - Securely uploads and retrieves documents from Azure Blob Storage.
3. **Extensive Testing**:
   - Comprehensive unit tests to ensure code quality and reliability.

---

## Known Limitations

- Some features (e.g., `Custom Query Fields`) require the `2024-07-31-preview` API version, which is only available via the Azure AI Document Intelligence Studio.

---

> ⚠ **Disclaimer**: Using Azure services incurs costs. Ensure you monitor your Azure resources and review billing details in the Azure Portal.

---

## License

This project is licensed under the [MIT License](LICENSE).
