using Azure.AI.DocAnalyzer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotNetEnv;
using Microsoft.CSharp.RuntimeBinder;

namespace Azure.AI.DocAnalyzer.ConsoleApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Azure AI Document Analyzer - Console Application");

            string resourcesPath = Path.Combine(AppContext.BaseDirectory, "Resources");
            string filePath = Path.Combine(resourcesPath, "Invoice1.pdf");

            // Ensure the file exists
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File not found - {filePath}");
                return;
            }

            // Environment variables
            DotNetEnv.Env.Load();

            string? endpoint = Environment.GetEnvironmentVariable("AZURE_DOCUMENT_INTELLIGENCE_ENDPOINT");
            string? apiKey = Environment.GetEnvironmentVariable("AZURE_DOCUMENT_INTELLIGENCE_KEY");

            string? accountName = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_NAME");
            string? accountKey = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_KEY");
            string? containerName = Environment.GetEnvironmentVariable("AZURE_STORAGE_BLOB_CONTAINER_NAME");

            if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(accountName) || string.IsNullOrWhiteSpace(accountKey) || string.IsNullOrWhiteSpace(containerName))
            {
                Console.WriteLine("Error: Required environment variables are not set.");
                return;
            }

            try
            {
                // Upload file to Azure Blob Storage
                var azureBlob = new AzureBlob(accountName, accountKey, containerName);
                await azureBlob.UploadFileWithSdkAsync(filePath, Path.GetFileName(filePath));

                string blobUrl = SasGenerator.GenerateSasUri(accountName,
                                                             accountKey,
                                                             containerName,
                                                             Path.GetFileName(filePath),
                                                             DateTimeOffset.UtcNow.AddHours(1),
                                                             isBlob: true).ToString();

                Console.WriteLine($"Blob uploaded successfully. URL: {blobUrl}");

                // Analyze document with Azure Document Intelligence
                var documentAnalyzer = new DocumentIntelligence(endpoint, apiKey);
                var analysisResult = await documentAnalyzer.AnalyzeInvoiceWithSdkAsync(blobUrl);

                // Print organized results
                PrintSection("File Information", new Dictionary<string, object>
                                                                {
                                                                    { "File Name", Path.GetFileName(filePath) },
                                                                    { "File URL", blobUrl },
                                                                    { "Document Type", "Invoice" }
                                                                });

                var standardFields = analysisResult["standard_fields"];
                if (standardFields != null)
                    PrintSection("Standard Fields", standardFields);

                // Handle and print tables
                var tables = analysisResult["tables"] as List<object>;
                if (tables != null)                
                    PrintSection("Tables", tables, isTable: true);                                    

                Console.WriteLine("\nThe features below are only available when using apiVersion=2024-07-31.\nThis version is currently in preview and only accessible via Azure AI Document Intelligence Studio.");

                var customFields = analysisResult["custom_fields"]; 
                if (customFields != null)
                    PrintSection("Custom Fields", customFields);

                // Handle and save images
                var images = analysisResult["images"] as List<object>;
                if (images != null)
                {
                    SaveImages(images);
                    PrintSection("Images", images);
                }

                // Handle and save barcodes
                var barcodes = analysisResult["barcodes"] as List<object>;
                if (barcodes != null)
                {
                    SaveBarcodes(barcodes);
                    PrintSection("Barcodes", barcodes);
                }                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine(""); 
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static void PrintSection(string title, object data, bool isTable = false)
        {
            Console.WriteLine($"\n{new string('=', 140)}\n{title}\n{new string('=', 140)}");
            if (isTable)
            {
                if (data is List<object> tables)
                {
                    PrintTables(tables);
                }
                else
                {
                    Console.WriteLine("No tables found.");
                }
            }
            else
            {
                if (data is Dictionary<string, object> dictData)
                {
                    foreach (var (key, value) in dictData)
                    {
                        if (value is Dictionary<string, object> subDict)
                        {
                            Console.WriteLine($"{key}:");
                            foreach (var (subKey, subValue) in subDict)
                            {
                                Console.WriteLine($"  - {subKey}: {subValue}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"{key}: {value}");
                        }
                    }
                }
                else if (data is List<object> listData)
                {
                    for (int i = 0; i < listData.Count; i++)
                    {
                        Console.WriteLine($"Item {i + 1}: {listData[i]}");
                    }
                }
                else
                {
                    Console.WriteLine(data);
                }
            }
        }

        private static void PrintTables(List<object> tables)
        {
            if (tables == null || tables.Count == 0)
            {
                Console.WriteLine("No tables found.");
                return;
            }

            for (int i = 0; i < tables.Count; i++)
            {
                Console.WriteLine($"\nTable {i + 1}:");

                // Tenta converter para Dictionary<string, object>
                if (tables[i] is Dictionary<string, object> table &&
                    table.TryGetValue("row_count", out var rowCountObj) &&
                    table.TryGetValue("column_count", out var columnCountObj) &&
                    table.TryGetValue("data", out var dataObj))
                {
                    var rows = dataObj as List<List<string>>;
                    if (rows != null && rows.Count > 0)
                    {
                        Console.WriteLine(Tabulate(rows));
                    }
                    else
                    {
                        Console.WriteLine("No data available in table.");
                    }
                }
                else
                {
                    Console.WriteLine("Unexpected table format.");
                }
            }
        }

        private static string Tabulate(List<List<string>> rows)
        {
            var output = new System.Text.StringBuilder();
            var columnWidths = new int[rows[0].Count];

            // Calcula a largura máxima de cada coluna
            foreach (var row in rows)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    columnWidths[i] = Math.Max(columnWidths[i], row[i]?.Length ?? 0);
                }
            }

            // Linha separadora
            string separator = "+" + string.Join("+", columnWidths.Select(w => new string('-', w + 2))) + "+";
            output.AppendLine(separator);

            // Adiciona as linhas da tabela
            foreach (var row in rows)
            {
                output.Append("|");
                for (int i = 0; i < row.Count; i++)
                {
                    output.Append($" {row[i]?.PadRight(columnWidths[i])} |");
                }
                output.AppendLine();
                output.AppendLine(separator);
            }

            return output.ToString();
        }

        private static void SaveImages(List<object> images, string outputDir = "images")
        {
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            for (int i = 0; i < images.Count; i++)
            {
                var filename = Path.Combine(outputDir, $"image_{i + 1}.png");
                File.WriteAllText(filename, $"Placeholder for image {i + 1}");
                Console.WriteLine($"Image saved: {filename}");
            }
        }

        private static void SaveBarcodes(List<object> barcodes, string outputDir = "barcodes")
        {
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            for (int i = 0; i < barcodes.Count; i++)
            {
                var filename = Path.Combine(outputDir, $"barcode_{i + 1}.png");
                File.WriteAllText(filename, $"Placeholder for barcode {i + 1}");
                Console.WriteLine($"Barcode saved: {filename}");
            }
        }
    }
}
