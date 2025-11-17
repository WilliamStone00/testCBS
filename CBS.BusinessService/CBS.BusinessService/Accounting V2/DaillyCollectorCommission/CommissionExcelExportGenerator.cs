using CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorCommission;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using BusinessServices;

namespace CBS.BusinessService.Accounting_V2.Affiliate
{
    public  class CommissionExcelExportGenerator : BaseService
    {
        // ===================================================================
        // DATA TABLE EXPORT METHODS (The ones we're focusing on)
        // ===================================================================

        // Export Data table - IMPROVED VERSION
        public static List<DataTableResponse> ConvertToCommissionData(List<DataTableResponse> tableData)
        {
            var commissionList = new List<DataTableResponse>();

            if (tableData == null || !tableData.Any())
            {
                Console.WriteLine("No table data provided for conversion");
                return commissionList;
            }

            Console.WriteLine($"Converting {tableData.Count} records to CommissionData");

            foreach (var item in tableData)
            {
                try
                {
                    // Use dynamic access with fallbacks
                    var commission = new DataTableResponse
                    {
                        // Use TryGetProperty helper for safe property access
                       // Id = TryGetProperty(item, "id", "Id")?.ToString() ?? "",
                        CollectorId = TryGetProperty(item, "collectorId", "CollectorId")?.ToString() ?? "",
                        CollectorName = TryGetProperty(item, "collectorName", "CollectorName")?.ToString() ?? "",
                        CollectorPhoneNumber = TryGetProperty(item, "collectorPhoneNumber", "CollectorPhoneNumber")?.ToString() ?? "",
                        CollectorAccountNumber = TryGetProperty(item, "collectorAccountNumber", "CollectorAccountNumber")?.ToString() ?? "",
                        MemberReference = TryGetProperty(item, "memberReference", "MemberReference")?.ToString() ?? "",
                       // BranchId = TryGetProperty(item, "branchId", "BranchId")?.ToString() ?? "",
                        BranchCode = TryGetProperty(item, "branchCode", "BranchCode")?.ToString() ?? "",
                        BranchName = TryGetProperty(item, "branchName", "BranchName")?.ToString() ?? "",
                        Year = Convert.ToInt32(TryGetProperty(item, "year", "Year") ?? DateTime.Now.Year),
                        Month = Convert.ToInt32(TryGetProperty(item, "month", "Month") ?? DateTime.Now.Month),
                        ReferenceNumber = TryGetProperty(item, "referenceNumber", "ReferenceNumber")?.ToString() ?? "",
                        CollectorShareAmount = Convert.ToDecimal(TryGetProperty(item, "collectorShareAmount", "CollectorShareAmount") ?? 0m),
                        IncentiveAmount = Convert.ToDecimal(TryGetProperty(item, "incentiveAmount", "IncentiveAmount") ?? 0m),
                        TotalPaidAmountToCollector = Convert.ToDecimal(TryGetProperty(item, "totalPaidAmountToCollector", "TotalPaidAmountToCollector") ?? 0m),
                        TotalCommissionShared = Convert.ToDecimal(TryGetProperty(item, "totalCommissionShared", "TotalCommissionShared") ?? 0m),
                        AmountPaid = Convert.ToDecimal(TryGetProperty(item, "amountPaid", "AmountPaid") ?? 0m),
                        Currency = TryGetProperty(item, "currency", "Currency")?.ToString() ?? "XAF",
                        DatePaid = ParseDateTimeOffset(TryGetProperty(item, "datePaid", "DatePaid")) ?? DateTimeOffset.Now,
                        Description = TryGetProperty(item, "description", "Description")?.ToString() ?? "Monthly collector commission",
                        PaymentSource = TryGetProperty(item, "paymentSource", "PaymentSource")?.ToString() ?? "BackOffice_Operation",
                        ProcessedBy = TryGetProperty(item, "processedBy", "ProcessedBy")?.ToString() ?? "",
                       // ProcessedByUserId = TryGetProperty(item, "processedByUserId", "ProcessedByUserId")?.ToString() ?? "",
                        CreatedDate = ParseDateTimeOffset(TryGetProperty(item, "createdDate", "CreatedDate")) ?? DateTimeOffset.Now,
                        //CreatedBy = TryGetProperty(item, "createdBy", "CreatedBy")?.ToString() ?? "",
                        //ModifiedDate = ParseDateTimeOffset(TryGetProperty(item, "modifiedDate", "ModifiedDate")) ?? DateTimeOffset.MinValue,
                        //ModifiedBy = TryGetProperty(item, "modifiedBy", "ModifiedBy")?.ToString() ?? "",
                        //DeletedDate = ParseDateTimeOffset(TryGetProperty(item, "deletedDate", "DeletedDate")),
                        //DeletedBy = TryGetProperty(item, "deletedBy", "DeletedBy")?.ToString(),
                        //IsDeleted = Convert.ToBoolean(TryGetProperty(item, "isDeleted", "IsDeleted") ?? false)
                    };

                    commissionList.Add(commission);
                    Console.WriteLine($"Successfully converted record: {commission.Id} - {commission.CollectorName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting commission data: {ex.Message}");
                    // Log the problematic item for debugging
                    try
                    {
                        string itemJson = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                        Console.WriteLine($"Problematic item: {itemJson}");
                    }
                    catch
                    {
                        Console.WriteLine("Could not serialize problematic item");
                    }
                }
            }

            Console.WriteLine($"Successfully converted {commissionList.Count} out of {tableData.Count} records");
            return commissionList;
        }

        public void GenerateCommissionExcelFromTableData(List<DataTableResponse> commissionData, string filePath, string exportedBy, ExportOptions exportOptions)
        {
            string bank = GetBankName();
            string bankcode = GetBankCode();
            string Branchid = GetBankID();
            string branchname = GetBranchName();

            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                // Create main worksheet
                var worksheet = package.Workbook.Worksheets.Add("Commission Report");

                // Set font
                var fontName = "Bahnschrift SemiCondensed";
                worksheet.Cells.Style.Font.Name = fontName;

                // We'll be using 31 columns (A..AE)
                const string lastColumnLetter = "AE";

                // ===== Title =====
                worksheet.Cells[$"A1:{lastColumnLetter}1"].Merge = true;
                worksheet.Cells["A1"].Value = "DAILLY COLLECTOR COMMISSION REPORT";
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Row(1).Height = 28;
                worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
                worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 100, 0)); // dark green

                // ===== Export meta =====
                worksheet.Cells[$"A2:{lastColumnLetter}2"].Merge = true;
                worksheet.Cells["A2"].Value = $"Exported By: {exportedBy}";
                worksheet.Cells["A2"].Style.Font.Italic = true;
                worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Row(2).Height = 18;

                worksheet.Cells[$"A3:{lastColumnLetter}3"].Merge = true;
                worksheet.Cells["A3"].Value = $"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                worksheet.Cells["A3"].Style.Font.Italic = true;
                worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Row(3).Height = 18;

                // Optional date range row
                if (exportOptions != null && !string.IsNullOrEmpty(exportOptions.StartDate) && !string.IsNullOrEmpty(exportOptions.EndDate))
                {
                    worksheet.Cells[$"A4:{lastColumnLetter}4"].Merge = true;
                    worksheet.Cells["A4"].Value = $"Date Range: {exportOptions.StartDate} to {exportOptions.EndDate}";
                    worksheet.Cells["A4"].Style.Font.Italic = true;
                    worksheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    worksheet.Row(4).Height = 18;
                }

                // ===== Bank / Branch information =====
                worksheet.Cells[$"A5:{lastColumnLetter}5"].Merge = true;
                worksheet.Cells["A5"].Value = bank;
                worksheet.Cells["A5"].Style.Font.Bold = true;
                worksheet.Cells["A5"].Style.Font.Size = 12;
                worksheet.Cells["A5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells["A5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Row(5).Height = 20;
                worksheet.Cells["A5"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["A5"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(224, 235, 255)); // subtle light blue

                worksheet.Cells[$"A6:{lastColumnLetter}6"].Merge = true;
                worksheet.Cells["A6"].Value = $"Bank Code: {bankcode}    |    Branch: {branchname}    (ID: {Branchid})";
                worksheet.Cells["A6"].Style.Font.Italic = true;
                worksheet.Cells["A6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Row(6).Height = 18;

                // Small spacer row (optional)
                worksheet.Row(7).Height = 8;

                // Add a thin border around the top header block (A1:AE6)
                var headerBlock = worksheet.Cells[$"A1:{lastColumnLetter}6"];
                headerBlock.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                headerBlock.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                headerBlock.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                headerBlock.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // ===== COMPREHENSIVE Headers - Include ALL fields from DataTableResponse =====
                var headers = new[]
                {
            "SN", "Collector ID", "Collector Name", "Collector Phone", "Collector Account",
            "Member Reference","Branch Code", "Branch Name", "Year", "Month",
            "Reference Number", "Collector Share", "Incentive Amount", "Total Paid to Collector",
            "Total Commission Shared", "Amount Paid", "Currency", "Date Paid", "Description",
            "Payment Source", "Processed By", "Created Date"
            
        };

                // We'll use row 8 for the column headers so header area occupies rows 1..7
                int headerRow = 8;
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[headerRow, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                worksheet.Row(headerRow).Height = 20;

                // ===== Data rows =====
                int dataRow = headerRow + 1;
                decimal totalAmountPaid = 0;
                decimal totalIncentive = 0;
                decimal totalPaidToCollector = 0;
                decimal totalCommissionShared = 0;
                decimal totalCollectorShare = 0;
                int serialNumber = 1;

                foreach (var commission in commissionData)
                {
                    worksheet.Cells[dataRow, 1].Value = serialNumber++; // SN
                    //worksheet.Cells[dataRow, 2].Value = commission.Id;
                    worksheet.Cells[dataRow, 2].Value = commission.CollectorId;
                    worksheet.Cells[dataRow, 3].Value = commission.CollectorName;
                    worksheet.Cells[dataRow, 4].Value = commission.CollectorPhoneNumber;
                    worksheet.Cells[dataRow, 5].Value = commission.CollectorAccountNumber;
                    worksheet.Cells[dataRow, 6].Value = commission.MemberReference;
                    //worksheet.Cells[dataRow, 8].Value = commission.BranchId;
                    worksheet.Cells[dataRow, 7].Value = commission.BranchCode;
                    worksheet.Cells[dataRow, 8].Value = commission.BranchName;
                    worksheet.Cells[dataRow, 9].Value = commission.Year;
                    worksheet.Cells[dataRow, 10].Value = commission.Month;
                    worksheet.Cells[dataRow, 11].Value = commission.ReferenceNumber;
                    worksheet.Cells[dataRow, 12].Value = commission.CollectorShareAmount;
                    worksheet.Cells[dataRow, 13].Value = commission.IncentiveAmount;
                    worksheet.Cells[dataRow, 14].Value = commission.TotalPaidAmountToCollector;
                    worksheet.Cells[dataRow, 15].Value = commission.TotalCommissionShared;
                    worksheet.Cells[dataRow, 16].Value = commission.AmountPaid;
                    worksheet.Cells[dataRow, 17].Value = commission.Currency;
                    worksheet.Cells[dataRow, 18].Value = commission.DatePaid.ToString("yyyy-MM-dd");
                    worksheet.Cells[dataRow, 19].Value = commission.Description;
                    worksheet.Cells[dataRow, 20].Value = commission.PaymentSource;
                    worksheet.Cells[dataRow, 21].Value = commission.ProcessedBy;
                    //worksheet.Cells[dataRow, 24].Value = commission.ProcessedByUserId;
                    worksheet.Cells[dataRow, 22].Value = commission.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");
                    //worksheet.Cells[dataRow, 26].Value = commission.CreatedBy;
                    //worksheet.Cells[dataRow, 27].Value = commission.ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss");
                    //worksheet.Cells[dataRow, 28].Value = commission.ModifiedBy;
                    //worksheet.Cells[dataRow, 29].Value = commission.DeletedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
                    //worksheet.Cells[dataRow, 30].Value = commission.DeletedBy;
                    //worksheet.Cells[dataRow, 31].Value = commission.IsDeleted ? "Yes" : "No";

                    // Apply borders for the row
                    for (int col = 1; col <= headers.Length; col++)
                    {
                        worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        // Left align text columns for readability (but keep numeric alignment for known numeric cols)
                        if (col == 14 || col == 15 || col == 16 || col == 17 || col == 18)
                            worksheet.Cells[dataRow, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        else
                            worksheet.Cells[dataRow, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }

                    totalAmountPaid += commission.AmountPaid;
                    totalIncentive += commission.IncentiveAmount;
                    totalPaidToCollector += commission.TotalPaidAmountToCollector;
                    totalCommissionShared += commission.TotalCommissionShared;
                    totalCollectorShare += commission.CollectorShareAmount;
                    dataRow++;
                }

                // Totals Row
                if (commissionData.Any())
                {
                    worksheet.Cells[dataRow, 1].Value = "TOTALS:";
                    worksheet.Cells[dataRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[dataRow, 14].Value = totalCollectorShare;
                    worksheet.Cells[dataRow, 15].Value = totalIncentive;
                    worksheet.Cells[dataRow, 16].Value = totalPaidToCollector;
                    worksheet.Cells[dataRow, 17].Value = totalCommissionShared;
                    worksheet.Cells[dataRow, 18].Value = totalAmountPaid;

                    // Style totals row
                    for (int col = 1; col <= headers.Length; col++)
                    {
                        worksheet.Cells[dataRow, col].Style.Font.Bold = true;
                        worksheet.Cells[dataRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[dataRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                        worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                }

                // Format numeric columns (Collector Share, Incentive, Total Paid to Collector, Total Commission Shared, Amount Paid)
                if (commissionData.Any())
                {
                    // Columns 14 (N) through 18 (R)
                    string startCol = "N";
                    string endCol = "R";
                    int startRow = headerRow + 1;
                    int endRow = (commissionData.Any() ? dataRow : headerRow + 1);
                    worksheet.Cells[$"{startCol}{startRow}:{endCol}{endRow}"].Style.Numberformat.Format = "#,##0.00";
                }

                // Set comfortable default column widths for columns A..AE (1..31)
                for (int col = 1; col <= 31; col++)
                {
                    worksheet.Column(col).Width = 16;
                }

                // Freeze top rows so header remains visible while scrolling (freeze before data start)
                worksheet.View.FreezePanes(headerRow + 1, 1);

                // Auto-fit columns safely (only if there is a used range)
                if (worksheet.Dimension != null)
                {
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }

                // Save the file
                package.SaveAs(new FileInfo(filePath));

                Console.WriteLine($"Excel file generated successfully with {commissionData.Count} records");
                Console.WriteLine($"File saved to: {filePath}");
            }
        }


        // ===================================================================
        // HELPER METHODS FOR DATA TABLE EXPORT
        // ===================================================================

        // Helper method to safely try multiple property names
        private static object TryGetProperty(dynamic obj, params string[] propertyNames)
        {
            if (obj == null) return null;

            foreach (var propName in propertyNames)
            {
                try
                {
                    // Check if it's an ExpandoObject or similar
                    if (obj is IDictionary<string, object> dict)
                    {
                        if (dict.ContainsKey(propName) && dict[propName] != null)
                            return dict[propName];
                    }
                    else
                    {
                        // Try to get property via reflection
                        var property = obj.GetType().GetProperty(propName);
                        if (property != null)
                        {
                            var value = property.GetValue(obj, null);
                            if (value != null) return value;
                        }
                    }
                }
                catch
                {
                    // Continue to next property name
                }
            }
            return null;
        }

        // Improved date parsing
        private static DateTimeOffset? ParseDateTimeOffset(object dateValue)
        {
            if (dateValue == null) return null;

            try
            {
                if (dateValue is DateTimeOffset dto) return dto;
                if (dateValue is DateTime dt) return new DateTimeOffset(dt);

                string dateString = dateValue.ToString();
                if (DateTimeOffset.TryParse(dateString, out DateTimeOffset result))
                    return result;

                return null;
            }
            catch
            {
                return null;
            }
        }

        // Helper methods for safe property access
        private static string GetSafeString(dynamic obj, params string[] propertyNames)
        {
            foreach (var propName in propertyNames)
            {
                try
                {
                    var value = GetPropertyValue(obj, propName);
                    if (value != null && !string.IsNullOrEmpty(value.ToString()))
                        return value.ToString();
                }
                catch
                {
                    // Continue to next property name
                }
            }
            return null;
        }

        private static int? GetSafeInt(dynamic obj, params string[] propertyNames)
        {
            foreach (var propName in propertyNames)
            {
                try
                {
                    var value = GetPropertyValue(obj, propName);
                    if (value != null)
                    {
                        if (int.TryParse(value.ToString(), out int result))
                            return result;
                    }
                }
                catch
                {
                    // Continue to next property name
                }
            }
            return null;
        }

        private static decimal? GetSafeDecimal(dynamic obj, params string[] propertyNames)
        {
            foreach (var propName in propertyNames)
            {
                try
                {
                    var value = GetPropertyValue(obj, propName);
                    if (value != null)
                    {
                        if (decimal.TryParse(value.ToString(), out decimal result))
                            return result;
                    }
                }
                catch
                {
                    // Continue to next property name
                }
            }
            return null;
        }

        private static bool? GetSafeBool(dynamic obj, params string[] propertyNames)
        {
            foreach (var propName in propertyNames)
            {
                try
                {
                    var value = GetPropertyValue(obj, propName);
                    if (value != null)
                    {
                        if (bool.TryParse(value.ToString(), out bool result))
                            return result;
                        // Handle "true"/"false" strings and 1/0 integers
                        if (value.ToString().ToLower() == "true" || value.ToString() == "1")
                            return true;
                        if (value.ToString().ToLower() == "false" || value.ToString() == "0")
                            return false;
                    }
                }
                catch
                {
                    // Continue to next property name
                }
            }
            return null;
        }

        private static dynamic GetPropertyValue(dynamic obj, params string[] propertyNames)
        {
            foreach (var propName in propertyNames)
            {
                try
                {
                    // Check if property exists and has value
                    if (obj != null)
                    {
                        var property = obj.GetType().GetProperty(propName);
                        if (property != null)
                        {
                            var value = property.GetValue(obj, null);
                            if (value != null)
                                return value;
                        }

                        // Try as dictionary
                        if (obj is IDictionary<string, object> dict && dict.ContainsKey(propName))
                            return dict[propName];
                    }
                }
                catch
                {
                    // Continue to next property name
                }
            }
            return null;
        }

        // ===================================================================
        // ORIGINAL COMMISSION EXPORT METHODS (Leave as-is)
        // ===================================================================

        public static void GenerateCommissionExcel(CollectorComissionResponse commissionData,
            string filePath, string exportedBy, ExportOptions exportOptions = null)
        {
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // DEBUG: Check what data is received
            System.Diagnostics.Debug.WriteLine($"Commission Data Received:");
            System.Diagnostics.Debug.WriteLine($"- Collector: {commissionData?.CollectorName}");
            System.Diagnostics.Debug.WriteLine($"- SharedAmounts Count: {commissionData?.SharedAmounts?.Count}");

            if (commissionData?.SharedAmounts != null)
            {
                foreach (var item in commissionData.SharedAmounts)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {item.Stakeholder}: {item.Percentage}% = {item.Amount}");
                }
            }

            using (var package = new ExcelPackage())
            {
                // Apply Bahnschrift SemiCondensed font to all worksheets
                var fontName = "Bahnschrift SemiCondensed";

                // Create Summary Worksheet with Distribution Breakdown
                CreateSummaryWithDistributionWorksheet(package, commissionData, exportedBy, exportOptions, fontName);

                // Create Details Worksheet with date filtering and SN column
                CreateDetailsWorksheet(package, commissionData, exportOptions, fontName);

                // Save the Excel file
                package.SaveAs(new FileInfo(filePath));
            }
        }

        private static void CreateSummaryWithDistributionWorksheet(ExcelPackage package,
            CollectorComissionResponse data, string exportedBy, ExportOptions exportOptions, string fontName)
        {
            var worksheet = package.Workbook.Worksheets.Add("Summary & Distribution");

            // Apply font to all cells
            worksheet.Cells.Style.Font.Name = fontName;

            // Title Section
            worksheet.Cells["A1"].Value = "COMMISSION SUMMARY REPORT";
            worksheet.Cells["A1:F1"].Merge = true;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.Font.Size = 18;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.DarkGreen);

            // Export Information with Date Range
            worksheet.Cells["A2"].Value = "Exported By:";
            worksheet.Cells["B2"].Value = exportedBy;
            worksheet.Cells["A3"].Value = "Export Date:";
            worksheet.Cells["B3"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (exportOptions != null && !string.IsNullOrEmpty(exportOptions.StartDate) && !string.IsNullOrEmpty(exportOptions.EndDate))
            {
                worksheet.Cells["A4"].Value = "Date Range:";
                worksheet.Cells["B4"].Value = $"{exportOptions.StartDate} to {exportOptions.EndDate}";
                worksheet.Cells["A2:A4"].Style.Font.Bold = true;
            }
            else
            {
                worksheet.Cells["A2:A3"].Style.Font.Bold = true;
            }

            // Collector Information
            int row = 6;
            worksheet.Cells[$"A{row}"].Value = "COLLECTOR INFORMATION";
            worksheet.Cells[$"A{row}:F{row}"].Merge = true;
            worksheet.Cells[$"A{row}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{row}"].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            row++;

            worksheet.Cells[$"A{row}"].Value = "Collector Name:";
            worksheet.Cells[$"B{row}"].Value = data.CollectorName ?? "N/A";
            worksheet.Cells[$"D{row}"].Value = "Branch:";
            worksheet.Cells[$"E{row}"].Value = data.BranchName ?? "N/A";
            row++;

            worksheet.Cells[$"A{row}"].Value = "Period:";
            worksheet.Cells[$"B{row}"].Value = data.Month ?? "N/A";
            worksheet.Cells[$"D{row}"].Value = "Total Members:";
            worksheet.Cells[$"E{row}"].Value = data.TotalMembersWithActivity;
            row++;

            // Activity Overview
            row++;
            worksheet.Cells[$"A{row}"].Value = "ACTIVITY OVERVIEW";
            worksheet.Cells[$"A{row}:F{row}"].Merge = true;
            worksheet.Cells[$"A{row}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{row}"].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            row++;

            // Create overview table
            var overviewHeaders = new[] { "Description", "Amount" };
            for (int i = 0; i < overviewHeaders.Length; i++)
            {
                worksheet.Cells[row, 1 + i].Value = overviewHeaders[i];
                worksheet.Cells[row, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[row, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[row, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            row++;

            var overviewData = new[]
            {
                new { Description = "Total Savings Collected", Amount = data.TotalValueCollected },
                new { Description = "Total Fee Charged", Amount = data.TotalFeeCharged },
                new { Description = "Total Amount to Distribute", Amount = data.TotalAmountToDistribute }
            };

            foreach (var item in overviewData)
            {
                worksheet.Cells[row, 1].Value = item.Description;
                worksheet.Cells[row, 2].Value = item.Amount;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[row, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                row++;
            }

            // DISTRIBUTION BREAKDOWN SECTION
            row += 2;
            worksheet.Cells[$"A{row}"].Value = "DISTRIBUTION BREAKDOWN";
            worksheet.Cells[$"A{row}:F{row}"].Merge = true;
            worksheet.Cells[$"A{row}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{row}"].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            row++;

            // Distribution headers
            var distributionHeaders = new[] { "Stakeholder", "Percentage", "Amount" };
            for (int i = 0; i < distributionHeaders.Length; i++)
            {
                worksheet.Cells[row, 1 + i].Value = distributionHeaders[i];
                worksheet.Cells[row, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[row, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[row, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            row++;

            // Distribution data
            decimal totalDistributionAmount = 0;
            bool hasDistributionData = false;

            if (data?.SharedAmounts != null && data.SharedAmounts.Count > 0)
            {
                foreach (var distribution in data.SharedAmounts)
                {
                    if (distribution != null)
                    {
                        worksheet.Cells[row, 1].Value = distribution.Stakeholder ?? "N/A";
                        worksheet.Cells[row, 2].Value = (distribution.Percentage / 100m);
                        worksheet.Cells[row, 3].Value = distribution.Amount;

                        for (int col = 1; col <= 3; col++)
                        {
                            worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }

                        totalDistributionAmount += distribution.Amount;
                        row++;
                        hasDistributionData = true;
                    }
                }

                if (hasDistributionData)
                {
                    // Distribution totals row
                    worksheet.Cells[row, 1].Value = "TOTAL DISTRIBUTION:";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 3].Value = totalDistributionAmount;

                    for (int col = 1; col <= 3; col++)
                    {
                        worksheet.Cells[row, col].Style.Font.Bold = true;
                        worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                        worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    // Format numbers for distribution section
                    int distributionStartRow = row - data.SharedAmounts.Count(d => d != null);
                    worksheet.Cells[$"B{distributionStartRow}:B{row}"].Style.Numberformat.Format = "0.0%";
                    worksheet.Cells[$"C{distributionStartRow}:C{row}"].Style.Numberformat.Format = "#,##0.00";
                }
            }
            else
            {
                worksheet.Cells[row, 1].Value = "No distribution data available";
                worksheet.Cells[row, 1].Style.Font.Italic = true;
                worksheet.Cells[$"A{row}:C{row}"].Merge = true;
                worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private static void CreateDetailsWorksheet(ExcelPackage package, CollectorComissionResponse data,
            ExportOptions exportOptions, string fontName)
        {
            var worksheet = package.Workbook.Worksheets.Add("Savers Details");

            // Apply font to all cells
            worksheet.Cells.Style.Font.Name = fontName;

            // Title with date range info
            string title = "SAVERS COMMISSION DETAILS";
            if (exportOptions != null && !string.IsNullOrEmpty(exportOptions.StartDate) && !string.IsNullOrEmpty(exportOptions.EndDate))
            {
                title += $" ({exportOptions.StartDate} to {exportOptions.EndDate})";
            }

            worksheet.Cells["A1"].Value = title;
            worksheet.Cells["A1:E1"].Merge = true;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.DarkGreen);

            // Headers with SN column and Transaction Date
            var headers = new[] { "SN", "Transaction Date", "Member ID", "Member Name", "Total Activity Amount", "Fee Charged" };
            int headerRow = 4;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                worksheet.Cells[headerRow, i + 1].Style.Font.Bold = true;
                worksheet.Cells[headerRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[headerRow, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Data
            int dataRow = headerRow + 1;
            decimal totalActivity = 0;
            decimal totalFee = 0;
            int serialNumber = 1;

            // Filter member stats by date range if provided
            var filteredMemberStats = data?.MemberStats;
            if (exportOptions != null && !string.IsNullOrEmpty(exportOptions.StartDate) && !string.IsNullOrEmpty(exportOptions.EndDate))
            {
                var startDate = DateTime.Parse(exportOptions.StartDate);
                var endDate = DateTime.Parse(exportOptions.EndDate).AddDays(1).AddSeconds(-1); // Include entire end date

                filteredMemberStats = data?.MemberStats?
                    .Where(m => m.LastTransactionDate >= startDate && m.LastTransactionDate <= endDate)
                    .ToList();
            }

            if (filteredMemberStats != null && filteredMemberStats.Count > 0)
            {
                foreach (var member in filteredMemberStats)
                {
                    worksheet.Cells[dataRow, 1].Value = serialNumber++; // SN column
                    worksheet.Cells[dataRow, 2].Value = member.LastTransactionDate.ToString();
                    worksheet.Cells[dataRow, 3].Value = member.MemberId ?? "-";
                    worksheet.Cells[dataRow, 4].Value = member.MemberName ?? "-";
                    worksheet.Cells[dataRow, 5].Value = member.TotalActivityAmount;
                    worksheet.Cells[dataRow, 6].Value = member.FeeCharged;

                    // Apply borders to all cells in the row
                    for (int col = 1; col <= headers.Length; col++)
                    {
                        worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    totalActivity += member.TotalActivityAmount;
                    totalFee += member.FeeCharged;
                    dataRow++;
                }

                // Totals Row
                worksheet.Cells[dataRow, 1].Value = "TOTALS:";
                worksheet.Cells[dataRow, 1].Style.Font.Bold = true;
                worksheet.Cells[dataRow, 5].Value = totalActivity;
                worksheet.Cells[dataRow, 6].Value = totalFee;

                // Style totals row
                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cells[dataRow, col].Style.Font.Bold = true;
                    worksheet.Cells[dataRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[dataRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                    worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Add summary note for date range
                if (exportOptions != null && !string.IsNullOrEmpty(exportOptions.StartDate) && !string.IsNullOrEmpty(exportOptions.EndDate))
                {
                    dataRow += 2;
                    worksheet.Cells[dataRow, 1].Value = $"Note: Totals shown are for the selected date range only ({exportOptions.StartDate} to {exportOptions.EndDate})";
                    worksheet.Cells[dataRow, 1].Style.Font.Italic = true;
                    worksheet.Cells[$"A{dataRow}:E{dataRow}"].Merge = true;
                }
            }
            else
            {
                worksheet.Cells[dataRow, 1].Value = "No commission data available for the selected criteria";
                worksheet.Cells[dataRow, 1].Style.Font.Italic = true;
                worksheet.Cells[$"A{dataRow}:E{dataRow}"].Merge = true;
                worksheet.Cells[dataRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Format numbers
            if (filteredMemberStats != null && filteredMemberStats.Count > 0)
            {
                worksheet.Cells[$"E{headerRow + 1}:F{dataRow}"].Style.Numberformat.Format = "#,##0.00";
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }
    }

   
       
}