using BusinessServices;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Translations;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CBS.BusinessService.Accounting_V2.Translations
{
    public class TranslationPlaceholderExportGenerator : BaseService
    {
        public byte[] GenerateExcel(List<TranslationPlaceholder> data, string exportedBy, DateTime? startDate = null, DateTime? endDate = null)
        {
            // Get current user's full name for export info
            exportedBy = GetUserFullName();
            Console.WriteLine($"Starting Excel generation for {data?.Count ?? 0} translation placeholders");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Translation Placeholders");
                Console.WriteLine("Worksheet created: Translation Placeholders");

                // Apply default font
                worksheet.Cells.Style.Font.Name = "Calibri";

                // Create header section
                CreateHeaderSection(worksheet, exportedBy, startDate, endDate);

                // Add data table
                AddDataTable(worksheet, data);

                // Add summary section
                AddSummarySection(worksheet, data);

                var excelBytes = package.GetAsByteArray();
                Console.WriteLine($"Excel generation complete: {excelBytes.Length} bytes");
                return excelBytes;
            }
        }

        private void AddDataTable(ExcelWorksheet worksheet, List<TranslationPlaceholder> data)
        {
            int currentRow = 5;

            // Column headers
            var headers = new[]
            {
                "Placeholder",
                "English Translation",
                "French Translation",
                "View",
                "Created By",
                "Created Date"
            };

            Console.WriteLine("Adding column headers");
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cells[currentRow, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            currentRow++;

            // Data rows
            Console.WriteLine($"Adding {data.Count} data rows");
            foreach (var item in data.OrderBy(x => x.Placeholder))
            {
                Console.WriteLine($"Adding row for: {item.Placeholder}");

                worksheet.Cells[currentRow, 1].Value = item.Placeholder;
                worksheet.Cells[currentRow, 2].Value = item.English;
                worksheet.Cells[currentRow, 3].Value = item.French;
                worksheet.Cells[currentRow, 4].Value = item.View;
                worksheet.Cells[currentRow, 5].Value = item.CreatedBy;
                worksheet.Cells[currentRow, 6].Value = item.CreatedDate;
                worksheet.Cells[currentRow, 6]
                            .Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";

                // Apply borders to all cells in the row
                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                currentRow++;
            }

            // Auto-fit columns
            Console.WriteLine("Auto-fitting columns");
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void AddSummarySection(ExcelWorksheet worksheet, List<TranslationPlaceholder> data)
        {
            int currentRow = worksheet.Dimension.End.Row + 2;

            // Summary header
            worksheet.Cells[currentRow, 1].Value = "SUMMARY";
            worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
            worksheet.Cells[currentRow, 1].Style.Font.Size = 12;
            worksheet.Cells[currentRow, 1, currentRow, 2].Merge = true;
            worksheet.Cells[currentRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[currentRow, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            currentRow++;

            // Total Records
            worksheet.Cells[currentRow, 1].Value = "Total Records:";
            worksheet.Cells[currentRow, 2].Value = data.Count;
            worksheet.Cells[currentRow, 2].Style.Font.Bold = true;
            currentRow++;

            // Views Distribution header
            worksheet.Cells[currentRow, 1].Value = "Views Distribution:";
            worksheet.Cells[currentRow, 1, currentRow, 2].Merge = true;
            worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
            currentRow++;

            var viewGroups = data.GroupBy(x => x.View)
                               .Select(g => new { View = g.Key, Count = g.Count() })
                               .OrderByDescending(g => g.Count)
                               .ToList();

            Console.WriteLine($"Found {viewGroups.Count} view groups");

            if (viewGroups.Any())
            {
                foreach (var group in viewGroups)
                {
                    worksheet.Cells[currentRow, 1].Value = $"{group.View}:";
                    worksheet.Cells[currentRow, 2].Value = group.Count;
                    currentRow++;
                }
            }
            else
            {
                worksheet.Cells[currentRow, 1].Value = "No view data";
                worksheet.Cells[currentRow, 1].Style.Font.Italic = true;
                currentRow++;
            }

            // Add borders to summary cells
            int summaryStartRow = worksheet.Dimension.End.Row - currentRow + 6;
            for (int row = summaryStartRow; row < currentRow; row++)
            {
                for (int col = 1; col <= 2; col++)
                {
                    if (worksheet.Cells[row, col].Value != null)
                    {
                        worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                }
            }
        }

        private void CreateHeaderSection(ExcelWorksheet worksheet, string exportedBy, DateTime? startDate = null, DateTime? endDate = null)
        {
            // Get current user's full name
            exportedBy = GetUserFullName();

            // Main title row
            worksheet.Cells["A1:F1"].Merge = true;
            worksheet.Cells["A1"].Value = "TRANSLATION PLACEHOLDERS REPORT";
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 100, 0));

            // Export information rows
            worksheet.Cells["A2:F2"].Merge = true;
            worksheet.Cells["A2"].Value = $"Exported By: {exportedBy}";
            worksheet.Cells["A2"].Style.Font.Bold = true;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            worksheet.Cells["A3:F3"].Merge = true;
            worksheet.Cells["A3"].Value = $"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            worksheet.Cells["A3"].Style.Font.Bold = true;
            worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            // Date range if provided
            if (startDate.HasValue && endDate.HasValue)
            {
                worksheet.Cells["A4:F4"].Merge = true;
                worksheet.Cells["A4"].Value = $"Date Range: {startDate.Value:yyyy-MM-dd} to {endDate.Value:yyyy-MM-dd}";
                worksheet.Cells["A4"].Style.Font.Bold = true;
                worksheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            }

            // Empty row for spacing (row 6)
            worksheet.Row(6).Height = 22;
        }

       
    }
}