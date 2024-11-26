using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Azure.AI.DocAnalyzer
{
    public class DocumentIntelligence
    {
        private readonly DocumentAnalysisClient _client;

        public DocumentIntelligence(string endpoint, string apiKey)
        {
            var credential = new AzureKeyCredential(apiKey);
            _client = new DocumentAnalysisClient(new Uri(endpoint), credential);
        }

        public async Task<Dictionary<string, object>> AnalyzeInvoiceWithSdkAsync(string publicUrl)
        {
            var operation = await _client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-document", new Uri(publicUrl));
            return ExtractInvoiceInsights(operation.Value);
        }

        private Dictionary<string, object> ExtractInvoiceInsights(AnalyzeResult analysisResult)
        {
            var insights = new Dictionary<string, object>
                        {
                            { "standard_fields", new Dictionary<string, object>() },
                            { "custom_fields", new Dictionary<string, object>() },
                            { "tables", new List<object>() },
                            { "images", new List<object>() },
                            { "barcodes", new List<object>() }
                        };

            // Extract standard fields (key-value pairs)
            if (analysisResult.KeyValuePairs != null)
            {
                var standardFields = insights["standard_fields"] as Dictionary<string, object>;

                foreach (var pair in analysisResult.KeyValuePairs)
                {
                    var key = pair.Key?.Content ?? "No key";
                    var value = pair.Value?.Content ?? "No value";
                    var confidence = pair.Confidence;

                    if (key != null && standardFields != null)
                    {
                        standardFields[key] = new
                        {
                            value,
                            confidence,
                            bounding_box = pair.Key?.BoundingRegions?.Count > 0 ? pair.Key.BoundingRegions[0].BoundingPolygon : null
                        };
                    }
                }
            }

            // Extract custom fields (documents)
            if (analysisResult.Documents != null)
            {
                var customFields = insights["custom_fields"] as Dictionary<string, object>;

                foreach (var document in analysisResult.Documents)
                {
                    foreach (var field in document.Fields)
                    {
                        var fieldName = field.Key;
                        var fieldValue = field.Value?.FieldType == DocumentFieldType.String ? field.Value.Content : null;
                        var confidence = field.Value?.Confidence ?? 0;

                        if (fieldName != null && customFields != null)
                        {
                            customFields[fieldName] = new
                            {
                                value = fieldValue,
                                confidence,
                                bounding_box = field.Value?.BoundingRegions?.Count > 0 ? field.Value.BoundingRegions[0].BoundingPolygon : null
                            };
                        }
                    }
                }
            }

            // Extract tables
            if (analysisResult.Tables != null)
            {
                var tables = insights["tables"] as List<object>;

                if (tables != null)
                {
                    foreach (var table in analysisResult.Tables)
                    {
                        var rows = new List<List<string>>();

                        for (int rowIndex = 0; rowIndex < table.RowCount; rowIndex++)
                        {
                            var rowData = new List<string>(new string[table.ColumnCount]);

                            foreach (var cell in table.Cells)
                            {
                                if (cell.RowIndex == rowIndex)
                                {
                                    rowData[cell.ColumnIndex] = cell.Content;
                                }
                            }

                            rows.Add(rowData);
                        }

                        tables.Add(new Dictionary<string, object>
                                    {
                                        { "row_count", table.RowCount },
                                        { "column_count", table.ColumnCount },
                                        { "data", rows }
                                    });
                    }
                }
            }

            // Extract images (figures) using reflection
            var figuresProperty = analysisResult.GetType().GetProperty("Figures");
            if (figuresProperty != null)
            {
                var figures = figuresProperty.GetValue(analysisResult) as IEnumerable<object>;
                if (figures != null)
                {
                    var images = insights["images"] as List<object>;

                    foreach (var figure in figures)
                    {
                        var captionProperty = figure.GetType().GetProperty("Caption");
                        var boundingRegionsProperty = figure.GetType().GetProperty("BoundingRegions");

                        var caption = captionProperty?.GetValue(figure)?.ToString() ?? "No caption";
                        var boundingRegions = boundingRegionsProperty?.GetValue(figure) as IEnumerable<object>;
                        var boundingBox = boundingRegions?.FirstOrDefault()?.GetType().GetProperty("BoundingPolygon")?.GetValue(boundingRegions.FirstOrDefault());

                        if (images != null)
                        {
                            images.Add(new
                            {
                                caption,
                                bounding_box = boundingBox
                            });
                        }
                    }
                }
            }

            // Extract barcodes using reflection
            var barcodesProperty = analysisResult.GetType().GetProperty("Barcodes");
            if (barcodesProperty != null)
            {
                var barcodes = barcodesProperty.GetValue(analysisResult) as IEnumerable<object>;
                if (barcodes != null)
                {
                    var barcodeList = insights["barcodes"] as List<object>;

                    foreach (var barcode in barcodes)
                    {
                        var kindProperty = barcode.GetType().GetProperty("Kind");
                        var valueProperty = barcode.GetType().GetProperty("Value");
                        var confidenceProperty = barcode.GetType().GetProperty("Confidence");

                        var kind = kindProperty?.GetValue(barcode)?.ToString() ?? "Unknown";
                        var value = valueProperty?.GetValue(barcode)?.ToString() ?? "No value";
                        var confidence = confidenceProperty?.GetValue(barcode) as float? ?? 0;

                        if (barcodeList != null)
                        {
                            barcodeList.Add(new
                            {
                                type = kind,
                                value,
                                confidence
                            });
                        }
                    }
                }
            }

            return insights;
        }
    }
}
