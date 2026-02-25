using BusinessServices;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using static CBS.FrontDesk.Data.Entity.SalaryManagement.StandingOrderDataTableQuery;


namespace CBS.BusinessService.Accounts
{
    public class StandingOrderExcelExportGenerator : BaseService
    {
        // Convert DataTable response to StandingOrderExport list
        public static List<StandingOrderExport> ConvertToStandingOrderData(List<dynamic> tableData)
        {
            var standingOrderList = new List<StandingOrderExport>();

            if (tableData == null || !tableData.Any())
            {
                Console.WriteLine("No table data provided for conversion");
                return standingOrderList;
            }

            Console.WriteLine($"Converting {tableData.Count} records to StandingOrderExport");

            foreach (var item in tableData)
            {
                try
                {
                    var standingOrder = new StandingOrderExport
                    {
                        Id = TryGetProperty(item, "id", "Id")?.ToString() ?? "",
                        MemberId = TryGetProperty(item, "memberId", "MemberId")?.ToString() ?? "",
                        MemberName = TryGetProperty(item, "memberName", "MemberName")?.ToString() ?? "",
                        Amount = Convert.ToDecimal(TryGetProperty(item, "amount", "Amount") ?? 0m),
                        SourceAccountType = TryGetProperty(item, "sourceAccountType", "SourceAccountType")?.ToString() ?? "",
                        DestinationAccountType = TryGetProperty(item, "destinationAccountType", "DestinationAccountType")?.ToString() ?? "",
                        Purpose = TryGetProperty(item, "purpose", "Purpose")?.ToString() ?? "",
                        StartDate = ParseDateTime(TryGetProperty(item, "startDate", "StartDate")),
                        EndDate = ParseDateTime(TryGetProperty(item, "endDate", "EndDate")),
                        IsActive = Convert.ToBoolean(TryGetProperty(item, "isActive", "IsActive") ?? false),
                        IsAutomatic = Convert.ToBoolean(TryGetProperty(item, "isAutomatic", "IsAutomatic") ?? false),
                        Frequency = TryGetProperty(item, "frequency", "Frequency")?.ToString() ?? "",
                        Priority = TryGetProperty(item, "priority", "Priority")?.ToString() ?? "",
                        BranchId = TryGetProperty(item, "branchId", "BranchId")?.ToString() ?? "",
                        BranchCode = TryGetProperty(item, "branchCode", "BranchCode")?.ToString() ?? "",
                        BranchName = TryGetProperty(item, "branchName", "BranchName")?.ToString() ?? "",
                        UserName = TryGetProperty(item, "userName", "UserName")?.ToString() ?? "",
                        ExternalAccount = Convert.ToBoolean(TryGetProperty(item, "externalAccount", "ExternalAccount") ?? false),
                        ExternalAccountNumber = TryGetProperty(item, "externalAccountNumber", "ExternalAccountNumber")?.ToString() ?? "",
                        ExternalAccountHolderName = TryGetProperty(item, "externalAccountHolderName", "ExternalAccountHolderName")?.ToString() ?? "",
                        PersonalNote = TryGetProperty(item, "personalNote", "PersonalNote")?.ToString() ?? "",
                        CreatedDate = ParseDateTime(TryGetProperty(item, "createdDate", "CreatedDate")) ?? DateTime.Now,
                        CreatedBy = TryGetProperty(item, "createdBy", "CreatedBy")?.ToString() ?? "",
                        ModifiedDate = ParseDateTime(TryGetProperty(item, "modifiedDate", "ModifiedDate")) ?? DateTime.Now,
                        ModifiedBy = TryGetProperty(item, "modifiedBy", "ModifiedBy")?.ToString() ?? ""
                    };

                    standingOrderList.Add(standingOrder);
                    Console.WriteLine($"Successfully converted record: {standingOrder.Id} - {standingOrder.MemberName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting standing order data: {ex.Message}");
                }
            }

            Console.WriteLine($"Successfully converted {standingOrderList.Count} out of {tableData.Count} records");
            return standingOrderList;
        }

        // Generate Excel with multiple sheets
        public void GenerateStandingOrderExcel(List<StandingOrderExport> standingOrderData, string filePath, string exportedBy, ExportOptions exportOptions)
        {
            string bank = GetBankName();
            string branchcode = GetBranchCode();
            string Branchid = GetBranchID();
            string branchname = GetBranchName();

            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                // ===== SHEET 1: SUMMARY & OVERVIEW =====
                CreateSummarySheet(package, standingOrderData, bank, branchcode, Branchid, branchname, exportedBy, exportOptions);

                // ===== SHEET 2: DETAILED DATA =====
                CreateDetailedSheet(package, standingOrderData, bank, branchcode, Branchid, branchname, exportedBy, exportOptions);

                // ===== SHEETS FOR EACH STATUS: Active/Inactive =====
                var activeOrders = standingOrderData.Where(x => x.IsActive).ToList();
                var inactiveOrders = standingOrderData.Where(x => !x.IsActive).ToList();

                if (activeOrders.Any())
                {
                    CreateStatusSheet(package, activeOrders, "Active Orders", bank, branchcode, Branchid, branchname, exportedBy, exportOptions);
                }

                if (inactiveOrders.Any())
                {
                    CreateStatusSheet(package, inactiveOrders, "Inactive Orders", bank, branchcode, Branchid, branchname, exportedBy, exportOptions);
                }

                // ===== SHEETS FOR FREQUENCY TYPES =====
                var frequencies = standingOrderData.GroupBy(x => x.Frequency)
                                                 .Select(g => g.Key)
                                                 .Where(f => !string.IsNullOrEmpty(f))
                                                 .ToList();

                foreach (var frequency in frequencies)
                {
                    var frequencyData = standingOrderData.Where(x => x.Frequency == frequency).ToList();
                    CreateFrequencySheet(package, frequencyData, frequency, bank, branchcode, Branchid, branchname, exportedBy, exportOptions);
                }

                // Save the file
                package.SaveAs(new FileInfo(filePath));

                Console.WriteLine($"Excel file generated successfully with {standingOrderData.Count} records");
                Console.WriteLine($"File saved to: {filePath}");
            }
        }

        private void CreateSummarySheet(ExcelPackage package, List<StandingOrderExport> data, string bank, string branchcode,
            string Branchid, string branchname, string exportedBy, ExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Summary & Overview");

            // Set font
            var fontName = "Bahnschrift SemiCondensed";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            CreateHeaderSection(worksheet, bank, branchcode, Branchid, branchname, exportedBy, exportOptions);

            int currentRow = 8; // Start after header

            // ===== GENERAL SUMMARY SECTION =====
            worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "STANDING ORDERS SUMMARY";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            currentRow += 2;

            // Calculate summary statistics
            var totalOrders = data.Count;
            var activeOrders = data.Count(x => x.IsActive);
            var inactiveOrders = data.Count(x => !x.IsActive);
            var automaticOrders = data.Count(x => x.IsAutomatic);
            var manualOrders = data.Count(x => !x.IsAutomatic);
            var totalAmount = data.Sum(x => x.Amount);
            var avgAmount = data.Any() ? data.Average(x => x.Amount) : 0;
            var externalAccounts = data.Count(x => x.ExternalAccount);
            var internalAccounts = data.Count(x => !x.ExternalAccount);

            // Frequency distribution
            var frequencyDist = data.GroupBy(x => x.Frequency)
                                   .Select(g => new { Frequency = g.Key, Count = g.Count() })
                                   .OrderByDescending(x => x.Count)
                                   .ToList();

            // Account type distribution
            var sourceTypes = data.GroupBy(x => x.SourceAccountType)
                                 .Select(g => new { Type = g.Key, Count = g.Count() })
                                 .OrderByDescending(x => x.Count)
                                 .ToList();

            var destTypes = data.GroupBy(x => x.DestinationAccountType)
                               .Select(g => new { Type = g.Key, Count = g.Count() })
                               .OrderByDescending(x => x.Count)
                               .ToList();

            // Summary table headers
            var summaryHeaders = new[] { "Metric", "Value", "Description" };
            for (int i = 0; i < summaryHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, 1 + i].Value = summaryHeaders[i];
                worksheet.Cells[currentRow, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[currentRow, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            // Summary data
            var summaryData = new[]
            {
                new { Metric = "Total Orders", Value = totalOrders.ToString("N0"), Description = "Total number of standing orders" },
                new { Metric = "Active Orders", Value = activeOrders.ToString("N0"), Description = "Currently active standing orders" },
                new { Metric = "Inactive Orders", Value = inactiveOrders.ToString("N0"), Description = "Inactive or expired orders" },
                new { Metric = "Automatic Orders", Value = automaticOrders.ToString("N0"), Description = "Orders processed automatically" },
                new { Metric = "Manual Orders", Value = manualOrders.ToString("N0"), Description = "Orders requiring manual processing" },
                new { Metric = "External Accounts", Value = externalAccounts.ToString("N0"), Description = "Orders to external accounts" },
                new { Metric = "Internal Accounts", Value = internalAccounts.ToString("N0"), Description = "Orders to internal accounts" },
                new { Metric = "Total Amount", Value = totalAmount.ToString("N2"), Description = "Sum of all standing order amounts" },
                new { Metric = "Average Amount", Value = avgAmount.ToString("N2"), Description = "Average amount per standing order" }
            };

            foreach (var item in summaryData)
            {
                worksheet.Cells[currentRow, 1].Value = item.Metric;
                worksheet.Cells[currentRow, 2].Value = item.Value;
                worksheet.Cells[currentRow, 3].Value = item.Description;

                for (int col = 1; col <= 3; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            currentRow += 2;

            // ===== FREQUENCY DISTRIBUTION =====
            if (frequencyDist.Any())
            {
                worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
                worksheet.Cells[$"A{currentRow}"].Value = "FREQUENCY DISTRIBUTION";
                worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
                worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
                worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                currentRow += 2;

                // Headers
                worksheet.Cells[currentRow, 1].Value = "Frequency";
                worksheet.Cells[currentRow, 2].Value = "Count";
                worksheet.Cells[currentRow, 3].Value = "Percentage";

                for (int col = 1; col <= 3; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[currentRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;

                foreach (var item in frequencyDist)
                {
                    worksheet.Cells[currentRow, 1].Value = item.Frequency ?? "Not Specified";
                    worksheet.Cells[currentRow, 2].Value = item.Count;
                    worksheet.Cells[currentRow, 3].Value = (double)item.Count / totalOrders;

                    for (int col = 1; col <= 3; col++)
                    {
                        worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    currentRow++;
                }

                // Format percentage column
                worksheet.Cells[$"C{currentRow - frequencyDist.Count}:C{currentRow - 1}"].Style.Numberformat.Format = "0.0%";
                currentRow += 2;
            }

            // ===== SOURCE ACCOUNT TYPE DISTRIBUTION =====
            if (sourceTypes.Any())
            {
                worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
                worksheet.Cells[$"A{currentRow}"].Value = "SOURCE ACCOUNT TYPES";
                worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
                worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
                worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                currentRow += 2;

                // Headers
                worksheet.Cells[currentRow, 1].Value = "Source Type";
                worksheet.Cells[currentRow, 2].Value = "Count";
                worksheet.Cells[currentRow, 3].Value = "Percentage";

                for (int col = 1; col <= 3; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[currentRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;

                foreach (var item in sourceTypes)
                {
                    worksheet.Cells[currentRow, 1].Value = item.Type ?? "Not Specified";
                    worksheet.Cells[currentRow, 2].Value = item.Count;
                    worksheet.Cells[currentRow, 3].Value = (double)item.Count / totalOrders;

                    for (int col = 1; col <= 3; col++)
                    {
                        worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    currentRow++;
                }

                // Format percentage column
                worksheet.Cells[$"C{currentRow - sourceTypes.Count}:C{currentRow - 1}"].Style.Numberformat.Format = "0.0%";
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void CreateDetailedSheet(ExcelPackage package, List<StandingOrderExport> data, string bank, string branchcode,
            string Branchid, string branchname, string exportedBy, ExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Detailed Data");

            // Set font
            var fontName = "Bahnschrift SemiCondensed";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            CreateHeaderSection(worksheet, bank, branchcode, Branchid, branchname, exportedBy, exportOptions);

            int currentRow = 8; // Start after header

            // ===== DETAILED DATA TABLE =====
            worksheet.Cells[$"A{currentRow}:Q{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "STANDING ORDERS DETAILED DATA";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
            currentRow += 2;

            // Detailed data headers
            var headers = new[]
            {
                "SN", "ID", "Member ID", "Member Name", "Amount", "Source Account",
                "Destination Account", "Purpose", "Start Date", "End Date", "Status",
                "Automatic", "Frequency", "Priority", "External", "Created Date", "Created By"
            };

            int headerRow = currentRow;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                worksheet.Cells[headerRow, i + 1].Style.Font.Bold = true;
                worksheet.Cells[headerRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[headerRow, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[headerRow, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Data rows
            int dataRow = headerRow + 1;
            int serialNumber = 1;

            foreach (var order in data.OrderByDescending(x => x.CreatedDate))
            {
                worksheet.Cells[dataRow, 1].Value = serialNumber++;
                worksheet.Cells[dataRow, 2].Value = order.Id;
                worksheet.Cells[dataRow, 3].Value = order.MemberId;
                worksheet.Cells[dataRow, 4].Value = order.MemberName;
                worksheet.Cells[dataRow, 5].Value = order.Amount;
                worksheet.Cells[dataRow, 6].Value = order.SourceAccountType;
                worksheet.Cells[dataRow, 7].Value = order.DestinationAccountType;
                worksheet.Cells[dataRow, 8].Value = order.Purpose;
                worksheet.Cells[dataRow, 9].Value = order.StartDate?.ToString("yyyy-MM-dd") ?? "-";
                worksheet.Cells[dataRow, 10].Value = order.EndDate?.ToString("yyyy-MM-dd") ?? "-";
                worksheet.Cells[dataRow, 11].Value = order.IsActive ? "Active" : "Inactive";
                worksheet.Cells[dataRow, 12].Value = order.IsAutomatic ? "Yes" : "No";
                worksheet.Cells[dataRow, 13].Value = order.Frequency;
                worksheet.Cells[dataRow, 14].Value = order.Priority;
                worksheet.Cells[dataRow, 15].Value = order.ExternalAccount ? "Yes" : "No";
                worksheet.Cells[dataRow, 16].Value = order.CreatedDate.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cells[dataRow, 17].Value = order.CreatedBy;

                // Apply borders
                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                dataRow++;
            }

            // Format numbers
            worksheet.Cells[$"E{headerRow + 1}:E{dataRow - 1}"].Style.Numberformat.Format = "#,##0.00";

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Freeze panes for easy scrolling
            worksheet.View.FreezePanes(headerRow + 1, 1);
        }

        private void CreateStatusSheet(ExcelPackage package, List<StandingOrderExport> data, string sheetName,
            string bank, string branchcode, string Branchid, string branchname, string exportedBy, ExportOptions exportOptions)
        {
            var cleanSheetName = CleanSheetName(sheetName);
            if (cleanSheetName.Length > 31) cleanSheetName = cleanSheetName.Substring(0, 31);

            var worksheet = package.Workbook.Worksheets.Add(cleanSheetName);

            // Set font
            var fontName = "Bahnschrift SemiCondensed";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            CreateHeaderSection(worksheet, bank, branchcode, Branchid, branchname, exportedBy, exportOptions);

            int currentRow = 8; // Start after header

            // ===== STATUS SHEET HEADER =====
            worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"{sheetName.ToUpper()} - {data.Count} ORDERS";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(sheetName.Contains("Active") ? Color.LightGreen : Color.LightGray);
            currentRow += 2;

            // Status summary
            var totalAmount = data.Sum(x => x.Amount);
            var avgAmount = data.Average(x => x.Amount);

            worksheet.Cells[currentRow, 1].Value = "Total Orders:";
            worksheet.Cells[currentRow, 2].Value = data.Count;
            worksheet.Cells[currentRow, 3].Value = "Total Amount:";
            worksheet.Cells[currentRow, 4].Value = totalAmount;
            worksheet.Cells[currentRow, 5].Value = "Average Amount:";
            worksheet.Cells[currentRow, 6].Value = avgAmount;

            for (int col = 1; col <= 6; col += 2)
            {
                worksheet.Cells[currentRow, col].Style.Font.Bold = true;
            }
            worksheet.Cells[currentRow, 4].Style.Numberformat.Format = "#,##0.00";
            worksheet.Cells[currentRow, 6].Style.Numberformat.Format = "#,##0.00";

            currentRow += 2;

            // Data headers
            var headers = new[]
            {
                "SN", "Member ID", "Member Name", "Amount", "Source", "Destination",
                "Purpose", "Start Date", "End Date", "Frequency", "Priority"
            };

            int headerRow = currentRow;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                worksheet.Cells[headerRow, i + 1].Style.Font.Bold = true;
                worksheet.Cells[headerRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[headerRow, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Data rows
            int dataRow = headerRow + 1;
            int serialNumber = 1;

            foreach (var order in data)
            {
                worksheet.Cells[dataRow, 1].Value = serialNumber++;
                worksheet.Cells[dataRow, 2].Value = order.MemberId;
                worksheet.Cells[dataRow, 3].Value = order.MemberName;
                worksheet.Cells[dataRow, 4].Value = order.Amount;
                worksheet.Cells[dataRow, 5].Value = order.SourceAccountType;
                worksheet.Cells[dataRow, 6].Value = order.DestinationAccountType;
                worksheet.Cells[dataRow, 7].Value = order.Purpose;
                worksheet.Cells[dataRow, 8].Value = order.StartDate?.ToString("yyyy-MM-dd") ?? "-";
                worksheet.Cells[dataRow, 9].Value = order.EndDate?.ToString("yyyy-MM-dd") ?? "-";
                worksheet.Cells[dataRow, 10].Value = order.Frequency;
                worksheet.Cells[dataRow, 11].Value = order.Priority;

                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                dataRow++;
            }

            worksheet.Cells[$"D{headerRow + 1}:D{dataRow - 1}"].Style.Numberformat.Format = "#,##0.00";
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            worksheet.View.FreezePanes(headerRow + 1, 1);
        }

        private void CreateFrequencySheet(ExcelPackage package, List<StandingOrderExport> data, string frequency,
            string bank, string branchcode, string Branchid, string branchname, string exportedBy, ExportOptions exportOptions)
        {
            var cleanSheetName = CleanSheetName($"{frequency} Orders");
            if (cleanSheetName.Length > 31) cleanSheetName = cleanSheetName.Substring(0, 31);

            var worksheet = package.Workbook.Worksheets.Add(cleanSheetName);

            // Set font
            var fontName = "Bahnschrift SemiCondensed";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            CreateHeaderSection(worksheet, bank, branchcode, Branchid, branchname, exportedBy, exportOptions);

            int currentRow = 8; // Start after header

            // ===== FREQUENCY SHEET HEADER =====
            worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"{frequency.ToUpper()} STANDING ORDERS - {data.Count} ORDERS";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            currentRow += 2;

            // Frequency summary
            var activeCount = data.Count(x => x.IsActive);
            var totalAmount = data.Sum(x => x.Amount);

            worksheet.Cells[currentRow, 1].Value = "Total Orders:";
            worksheet.Cells[currentRow, 2].Value = data.Count;
            worksheet.Cells[currentRow, 3].Value = "Active Orders:";
            worksheet.Cells[currentRow, 4].Value = activeCount;
            worksheet.Cells[currentRow, 5].Value = "Total Amount:";
            worksheet.Cells[currentRow, 6].Value = totalAmount;

            for (int col = 1; col <= 6; col += 2)
            {
                worksheet.Cells[currentRow, col].Style.Font.Bold = true;
            }
            worksheet.Cells[currentRow, 6].Style.Numberformat.Format = "#,##0.00";

            currentRow += 2;

            // Data headers
            var headers = new[]
            {
                "SN", "Member ID", "Member Name", "Amount", "Source", "Destination",
                "Purpose", "Start Date", "End Date", "Status", "Priority"
            };

            int headerRow = currentRow;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                worksheet.Cells[headerRow, i + 1].Style.Font.Bold = true;
                worksheet.Cells[headerRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[headerRow, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Data rows
            int dataRow = headerRow + 1;
            int serialNumber = 1;

            foreach (var order in data)
            {
                worksheet.Cells[dataRow, 1].Value = serialNumber++;
                worksheet.Cells[dataRow, 2].Value = order.MemberId;
                worksheet.Cells[dataRow, 3].Value = order.MemberName;
                worksheet.Cells[dataRow, 4].Value = order.Amount;
                worksheet.Cells[dataRow, 5].Value = order.SourceAccountType;
                worksheet.Cells[dataRow, 6].Value = order.DestinationAccountType;
                worksheet.Cells[dataRow, 7].Value = order.Purpose;
                worksheet.Cells[dataRow, 8].Value = order.StartDate?.ToString("yyyy-MM-dd") ?? "-";
                worksheet.Cells[dataRow, 9].Value = order.EndDate?.ToString("yyyy-MM-dd") ?? "-";
                worksheet.Cells[dataRow, 10].Value = order.IsActive ? "Active" : "Inactive";
                worksheet.Cells[dataRow, 11].Value = order.Priority;

                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                dataRow++;
            }

            worksheet.Cells[$"D{headerRow + 1}:D{dataRow - 1}"].Style.Numberformat.Format = "#,##0.00";
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            worksheet.View.FreezePanes(headerRow + 1, 1);
        }

        private void CreateHeaderSection(ExcelWorksheet worksheet, string bank, string branchcode, string Branchid,
            string branchname, string exportedBy, ExportOptions exportOptions)
        {
            const string lastColumnLetter = "Q";

            // ===== Bank Information at the TOP =====
            worksheet.Cells[$"A1:{lastColumnLetter}1"].Merge = true;
            worksheet.Cells["A1"].Value = bank;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.Font.Size = 18;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(1).Height = 30;
            worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 100, 0));

            // ===== Bank Code and Branch Information =====
            worksheet.Cells[$"A2:{lastColumnLetter}2"].Merge = true;
            worksheet.Cells["A2"].Value = $"Branch Code: {branchcode} | Branch: {branchname} | Branch ID: {Branchid}";
            worksheet.Cells["A2"].Style.Font.Bold = true;
            worksheet.Cells["A2"].Style.Font.Size = 12;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(2).Height = 22;
            worksheet.Cells["A2"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A2"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(70, 130, 180));

            // ===== Export meta =====
            worksheet.Cells[$"A3:{lastColumnLetter}3"].Merge = true;
            worksheet.Cells["A3"].Value = $"Exported By: {exportedBy}";
            worksheet.Cells["A3"].Style.Font.Bold = true;
            worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(3).Height = 18;

            worksheet.Cells[$"A4:{lastColumnLetter}4"].Merge = true;
            worksheet.Cells["A4"].Value = $"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            worksheet.Cells["A4"].Style.Font.Bold = true;
            worksheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(4).Height = 18;

            // ===== Report Title =====
            worksheet.Cells[$"A5:{lastColumnLetter}5"].Merge = true;
            worksheet.Cells["A5"].Value = "STANDING ORDERS REPORT";
            worksheet.Cells["A5"].Style.Font.Bold = true;
            worksheet.Cells["A5"].Style.Font.Size = 14;
            worksheet.Cells["A5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(5).Height = 25;
            worksheet.Cells["A5"].Style.Font.Color.SetColor(Color.Black);
            worksheet.Cells["A5"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A5"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 215, 0));

            // Optional date range row
            if (exportOptions != null && !string.IsNullOrEmpty(exportOptions.StartDate) && !string.IsNullOrEmpty(exportOptions.EndDate))
            {
                worksheet.Cells[$"A6:{lastColumnLetter}6"].Merge = true;
                worksheet.Cells["A6"].Value = $"Date Range: {exportOptions.StartDate} to {exportOptions.EndDate}";
                worksheet.Cells["A6"].Style.Font.Bold = true;
                worksheet.Cells["A6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells["A6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Row(6).Height = 18;
            }
        }

        private string CleanSheetName(string name)
        {
            // Excel sheet name restrictions
            var invalidChars = new char[] { '\\', '/', '*', '?', ':', '[', ']' };
            foreach (var invalidChar in invalidChars)
            {
                name = name.Replace(invalidChar, ' ');
            }

            // Remove extra spaces and trim
            name = string.Join(" ", name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            return name.Trim();
        }

        // Helper methods
        private static object TryGetProperty(dynamic obj, params string[] propertyNames)
        {
            if (obj == null) return null;

            foreach (var propName in propertyNames)
            {
                try
                {
                    if (obj is IDictionary<string, object> dict)
                    {
                        if (dict.ContainsKey(propName) && dict[propName] != null)
                            return dict[propName];
                    }
                    else
                    {
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

        private static DateTime? ParseDateTime(object dateValue)
        {
            if (dateValue == null) return null;

            try
            {
                if (dateValue is DateTime dt) return dt;
                if (dateValue is DateTimeOffset dto) return dto.DateTime;

                string dateString = dateValue.ToString();
                if (DateTime.TryParse(dateString, out DateTime result))
                    return result;

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}