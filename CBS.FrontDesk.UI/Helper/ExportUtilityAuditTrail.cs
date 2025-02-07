using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using CBS.FrontDesk.Data.Entity.AuditTralP;
using ClosedXML.Excel;

namespace CBS.FrontDesk.UI.Helper
{
    public static class ExportUtilityAuditTrail
    {
        public static ExportFileResult ConvertToExcel(IEnumerable<AuditTrailDto> data, DateTime? startDate, DateTime? endDate, string searchItem)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Audit Trails");

            // Write title
            worksheet.Cell(1, 1).Value = "AuditTrail TSC";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.Italic = true; // Title in italic
            worksheet.Cell(1, 1).Style.Font.FontName = "Bahnschrift Light";
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;

            // Add subtitle
            worksheet.Cell(2, 1).Value = $"Extracted from {(startDate?.ToString("yyyy-MM-dd") ?? "N/A")} to {(endDate?.ToString("yyyy-MM-dd") ?? "N/A")} By {HttpContext.Current.Session["FullName"]}";
            worksheet.Cell(2, 1).Style.Font.FontName = "Bahnschrift Light";
            worksheet.Cell(2, 1).Style.Font.FontSize = 10;
            worksheet.Cell(2, 1).Style.Font.Italic = true; // Subtitle in italic
            worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            // Add Printed By under the title
            worksheet.Cell(3, 1).Value = $"Printed By: {HttpContext.Current.Session["FullName"]}";
            worksheet.Cell(3, 1).Style.Font.FontName = "Bahnschrift Light";
            worksheet.Cell(3, 1).Style.Font.FontSize = 10;
            worksheet.Cell(3, 1).Style.Font.Italic = true; // Printed By in italic
            worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(3, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            // Add Search Criteria
            worksheet.Cell(4, 1).Value = $"Search Criteria: Date From {(startDate?.ToString("yyyy-MM-dd") ?? "N/A")} To {(endDate?.ToString("yyyy-MM-dd") ?? "N/A")} [{searchItem}]";
            worksheet.Cell(4, 1).Style.Font.FontName = "Bahnschrift Light";
            worksheet.Cell(4, 1).Style.Font.FontSize = 10;
            worksheet.Cell(4, 1).Style.Font.Italic = true; // Search Criteria in italic
            worksheet.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(4, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            // Write headers (including ServiceName)
            var headers = new[] { "Id", "Action", "Timestamp", "ServiceName", "UserName", "FullName", "UserID", "Level", "IPAddress", "StatusCode", "StringifyObject", "DetailMessage", "BranchID", "BankID", "BranchName", "BranchCode", "CorrelationId" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(5, i + 1).Value = headers[i];
                worksheet.Cell(5, i + 1).Style.Font.Bold = true;
                worksheet.Cell(5, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(5, i + 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Cell(5, i + 1).Style.Font.FontSize = 10;
                worksheet.Cell(5, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Write data starting from the sixth row
            int row = 6;
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = item.Id ?? "N/A";
                worksheet.Cell(row, 2).Value = item.Action ?? "N/A";
                worksheet.Cell(row, 3).Value = item.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";  // Format the date
                worksheet.Cell(row, 4).Value = item.MicroServiceName ?? "N/A";  // Add ServiceName
                worksheet.Cell(row, 5).Value = item.UserName ?? "N/A";
                worksheet.Cell(row, 6).Value = item.FullName ?? "N/A";
                worksheet.Cell(row, 7).Value = item.UserID ?? "N/A";
                worksheet.Cell(row, 8).Value = item.Level ?? "N/A";
                worksheet.Cell(row, 9).Value = item.IPAddress ?? "N/A";
                worksheet.Cell(row, 10).Value = item.StatusCode?.ToString() ?? "N/A";
                worksheet.Cell(row, 11).Value = item.StringifyObject ?? "N/A";
                worksheet.Cell(row, 12).Value = item.DetailMessage ?? "N/A";
                worksheet.Cell(row, 13).Value = item.BranchID ?? "N/A";
                worksheet.Cell(row, 14).Value = item.BankID ?? "N/A";
                worksheet.Cell(row, 15).Value = item.BranchName ?? "N/A";
                worksheet.Cell(row, 16).Value = item.BranchCode ?? "N/A";
                worksheet.Cell(row, 17).Value = item.CorrolationId ?? "N/A";
                row++;
            }

            // Add border to data section
            var dataRange = worksheet.Range(5, 1, row - 1, headers.Length);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Add summary table with proper headers and spacing
            var summaryRow = row + 2;

            // SUMMARY HEADER
            worksheet.Cell(summaryRow, 1).Value = "SUMMARY";
            worksheet.Cell(summaryRow, 1).Style.Font.Bold = true;
            worksheet.Cell(summaryRow, 1).Style.Font.FontName = "Bahnschrift Light";
            worksheet.Cell(summaryRow, 1).Style.Font.FontSize = 10;
            worksheet.Cell(summaryRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(summaryRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            summaryRow++;

            // Summary Data (Total Logs, Total Errors, etc.)
            worksheet.Cell(summaryRow, 1).Value = "TOTAL LOGS";
            worksheet.Cell(summaryRow, 2).Value = data.Count();
            worksheet.Cell(summaryRow + 1, 1).Value = "TOTAL ERRORS";
            worksheet.Cell(summaryRow + 1, 2).Value = data.Count(d => d.Level == "Error");
            worksheet.Cell(summaryRow + 2, 1).Value = "TOTAL INFORMATION";
            worksheet.Cell(summaryRow + 2, 2).Value = data.Count(d => d.Level == "Information");
            worksheet.Cell(summaryRow + 3, 1).Value = "TOTAL WARNINGS";
            worksheet.Cell(summaryRow + 3, 2).Value = data.Count(d => d.Level == "Warning");

            // Add borders to summary table
            var summaryRange = worksheet.Range(summaryRow, 1, summaryRow + 3, 2);
            summaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            summaryRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Separate summary from next section
            summaryRow += 5;

            // User Logs and Group Statistics Section
            var userLogs = data.GroupBy(d => d.FullName)
                               .Select(g => new { User = $"{g.Key} [{g.First().BranchCode}] {g.First().BranchName}", Count = g.Count() })
                               .OrderByDescending(g => g.Count)
                               .ToList();

            worksheet.Cell(summaryRow, 1).Value = "USER LOGS SUMMARY";
            worksheet.Cell(summaryRow, 1).Style.Font.Bold = true;
            worksheet.Cell(summaryRow, 1).Style.Font.FontName = "Bahnschrift Light";
            worksheet.Cell(summaryRow, 1).Style.Font.FontSize = 10;

            int userSummaryRow = summaryRow + 1;
            foreach (var userLog in userLogs)
            {
                worksheet.Cell(userSummaryRow, 1).Value = userLog.User;
                worksheet.Cell(userSummaryRow, 2).Value = userLog.Count;
                userSummaryRow++;
            }

            // Add a border to the user logs summary
            var userSummaryRange = worksheet.Range(summaryRow, 1, userSummaryRow - 1, 2);
            userSummaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            userSummaryRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Service Logs Summary: Group by ServiceName and Level
            var serviceLogs = data.GroupBy(d => new { d.MicroServiceName, d.Level })
                                  .Select(g => new { Service = $"{g.Key.MicroServiceName} - {g.Key.Level}", Count = g.Count() })
                                  .OrderByDescending(g => g.Count)
                                  .ToList();

            worksheet.Cell(userSummaryRow + 1, 1).Value = "SERVICE LOGS SUMMARY";
            worksheet.Cell(userSummaryRow + 1, 1).Style.Font.Bold = true;
            worksheet.Cell(userSummaryRow + 1, 1).Style.Font.FontName = "Bahnschrift Light";
            worksheet.Cell(userSummaryRow + 1, 1).Style.Font.FontSize = 10;

            int serviceSummaryRow = userSummaryRow + 2;
            foreach (var serviceLog in serviceLogs)
            {
                worksheet.Cell(serviceSummaryRow, 1).Value = serviceLog.Service;
                worksheet.Cell(serviceSummaryRow, 2).Value = serviceLog.Count;
                serviceSummaryRow++;
            }

            // Add a border to the service logs summary
            var serviceSummaryRange = worksheet.Range(userSummaryRow + 1, 1, serviceSummaryRow - 1, 2);
            serviceSummaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            serviceSummaryRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Apply font and formatting to the entire worksheet
            worksheet.Cells().Style.Font.FontName = "Bahnschrift Light";
            worksheet.Cells().Style.Font.FontSize = 10;

            // Adjust column widths to fit content
            worksheet.Columns().AdjustToContents();

            // Create memory stream to hold the Excel file
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return new ExportFileResult
            {
                Content = content,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = $"Audit_Trail_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            };
        }
    }


    //public static class ExportUtilityAuditTrail
    //{
    //    public static ExportFileResult ConvertToExcel(IEnumerable<AuditTrailDto> data, DateTime? startDate, DateTime? endDate, string searchItem)
    //    {
    //        var workbook = new XLWorkbook();
    //        var worksheet = workbook.Worksheets.Add("Audit Trails");

    //        // Write title
    //        worksheet.Cell(1, 1).Value = "AuditTrail TSC";
    //        worksheet.Cell(1, 1).Style.Font.Bold = true;
    //        worksheet.Cell(1, 1).Style.Font.FontName = "Bahnschrift Light";
    //        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
    //        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    //        worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
    //        worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;

    //        // Add subtitle
    //        worksheet.Cell(2, 1).Value = $"Extracted from {(startDate?.ToString("yyyy-MM-dd") ?? "N/A")} to {(endDate?.ToString("yyyy-MM-dd") ?? "N/A")} By {HttpContext.Current.Session["FullName"]}";
    //        worksheet.Cell(2, 1).Style.Font.FontName = "Bahnschrift Light";
    //        worksheet.Cell(2, 1).Style.Font.FontSize = 10;
    //        worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
    //        worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

    //        // Add Printed By under the title
    //        worksheet.Cell(3, 1).Value = $"Printed By: {HttpContext.Current.Session["FullName"]}";
    //        worksheet.Cell(3, 1).Style.Font.FontName = "Bahnschrift Light";
    //        worksheet.Cell(3, 1).Style.Font.FontSize = 10;
    //        worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
    //        worksheet.Cell(3, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

    //        // Add Search Criteria
    //        worksheet.Cell(4, 1).Value = $"Search Criteria: Date From {(startDate?.ToString("yyyy-MM-dd") ?? "N/A")} To {(endDate?.ToString("yyyy-MM-dd") ?? "N/A")} [{searchItem}]";
    //        worksheet.Cell(4, 1).Style.Font.FontName = "Bahnschrift Light";
    //        worksheet.Cell(4, 1).Style.Font.FontSize = 10;
    //        worksheet.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
    //        worksheet.Cell(4, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

    //        // Write headers
    //        var headers = new[] { "Id", "Action", "Timestamp", "UserName", "FullName", "UserID", "Level", "IPAddress", "StatusCode", "StringifyObject", "DetailMessage", "BranchID", "BankID", "BranchName", "BranchCode", "CorrelationId" };
    //        for (int i = 0; i < headers.Length; i++)
    //        {
    //            worksheet.Cell(5, i + 1).Value = headers[i];
    //            worksheet.Cell(5, i + 1).Style.Font.Bold = true;
    //            worksheet.Cell(5, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
    //            worksheet.Cell(5, i + 1).Style.Font.FontName = "Bahnschrift Light";
    //            worksheet.Cell(5, i + 1).Style.Font.FontSize = 10;
    //            worksheet.Cell(5, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    //        }

    //        // Write data starting from the sixth row
    //        int row = 6;
    //        foreach (var item in data)
    //        {
    //            worksheet.Cell(row, 1).Value = item.Id ?? "N/A";
    //            worksheet.Cell(row, 2).Value = item.Action ?? "N/A";
    //            worksheet.Cell(row, 3).Value = item.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";  // Format the date
    //            worksheet.Cell(row, 4).Value = item.UserName ?? "N/A";
    //            worksheet.Cell(row, 5).Value = item.FullName ?? "N/A";
    //            worksheet.Cell(row, 6).Value = item.UserID ?? "N/A";
    //            worksheet.Cell(row, 7).Value = item.Level ?? "N/A";
    //            worksheet.Cell(row, 8).Value = item.IPAddress ?? "N/A";
    //            worksheet.Cell(row, 9).Value = item.StatusCode?.ToString() ?? "N/A";
    //            worksheet.Cell(row, 10).Value = item.StringifyObject ?? "N/A";
    //            worksheet.Cell(row, 11).Value = item.DetailMessage ?? "N/A";
    //            worksheet.Cell(row, 12).Value = item.BranchID ?? "N/A";
    //            worksheet.Cell(row, 13).Value = item.BankID ?? "N/A";
    //            worksheet.Cell(row, 14).Value = item.BranchName ?? "N/A";
    //            worksheet.Cell(row, 15).Value = item.BranchCode ?? "N/A";
    //            worksheet.Cell(row, 16).Value = item.CorrolationId ?? "N/A";
    //            row++;
    //        }

    //        // Add border to data section
    //        var dataRange = worksheet.Range(5, 1, row - 1, headers.Length);
    //        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    //        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

    //        // Add summary table at the end
    //        var summaryRow = row + 2;
    //        worksheet.Cell(summaryRow, 1).Value = "Summary:";
    //        worksheet.Cell(summaryRow, 1).Style.Font.Bold = true;
    //        worksheet.Cell(summaryRow, 1).Style.Font.FontName = "Bahnschrift Light";
    //        worksheet.Cell(summaryRow, 1).Style.Font.FontSize = 10;

    //        var totalLogs = data.Count();
    //        var totalErrors = data.Count(d => d.Level == "Error");
    //        var totalInformation = data.Count(d => d.Level == "Information");
    //        var totalWarnings = data.Count(d => d.Level == "Warning");

    //        worksheet.Cell(summaryRow + 1, 1).Value = "Total Logs";
    //        worksheet.Cell(summaryRow + 1, 2).Value = totalLogs;

    //        worksheet.Cell(summaryRow + 2, 1).Value = "Total Errors";
    //        worksheet.Cell(summaryRow + 2, 2).Value = totalErrors;

    //        worksheet.Cell(summaryRow + 3, 1).Value = "Total Information";
    //        worksheet.Cell(summaryRow + 3, 2).Value = totalInformation;

    //        worksheet.Cell(summaryRow + 4, 1).Value = "Total Warnings";
    //        worksheet.Cell(summaryRow + 4, 2).Value = totalWarnings;

    //        // User Logs and Group Statistics Section
    //        var userLogs = data.GroupBy(d => d.FullName).ToDictionary(g => g.Key, g => g.Count());
    //        worksheet.Cell(summaryRow + 6, 1).Value = "User Logs Summary";
    //        worksheet.Cell(summaryRow + 6, 1).Style.Font.Bold = true;

    //        int userSummaryRow = summaryRow + 7;
    //        foreach (var userLog in userLogs)
    //        {
    //            worksheet.Cell(userSummaryRow, 1).Value = userLog.Key;
    //            worksheet.Cell(userSummaryRow, 2).Value = userLog.Value;
    //            userSummaryRow++;
    //        }

    //        // Group Statistics: Count Logs per Level (Error, Info, Warning)
    //        var groupStats = data.GroupBy(d => d.Level).ToDictionary(g => g.Key, g => g.Count());
    //        worksheet.Cell(userSummaryRow + 1, 1).Value = "Group Logs Summary";
    //        worksheet.Cell(userSummaryRow + 1, 1).Style.Font.Bold = true;

    //        int groupSummaryRow = userSummaryRow + 2;
    //        foreach (var groupStat in groupStats)
    //        {
    //            worksheet.Cell(groupSummaryRow, 1).Value = groupStat.Key;
    //            worksheet.Cell(groupSummaryRow, 2).Value = groupStat.Value;
    //            groupSummaryRow++;
    //        }

    //        // Add borders to the summary sections
    //        var summaryRange = worksheet.Range(summaryRow, 1, groupSummaryRow - 1, 2);
    //        summaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    //        summaryRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

    //        // Apply font and formatting to the entire worksheet
    //        worksheet.Cells().Style.Font.FontName = "Bahnschrift Light";
    //        worksheet.Cells().Style.Font.FontSize = 10;

    //        // Adjust column widths to fit content
    //        worksheet.Columns().AdjustToContents();

    //        // Create memory stream to hold the Excel file
    //        var stream = new MemoryStream();
    //        workbook.SaveAs(stream);
    //        var content = stream.ToArray();

    //        return new ExportFileResult
    //        {
    //            Content = content,
    //            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    //            FileName = $"Audit_Trail_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
    //        };
    //    }
    //}




    //public static class ExportUtilityAuditTrail
    //{
    //    public static ExportFileResult ConvertToExcel(IEnumerable<AuditTrailDto> data, string fromDate, string toDate, string sessionFullName)
    //    {
    //        var workbook = new XLWorkbook();
    //        var worksheet = workbook.Worksheets.Add("Audit Trails");
    //        var currentRow = 1;

    //        // Title Section
    //        worksheet.Cell(currentRow, 1).Value = "AuditTrail TSC";
    //        worksheet.Range(currentRow, 1, currentRow, 5).Merge().Style.Font.SetBold().Font.FontSize = 14;
    //        currentRow++;
    //        worksheet.Cell(currentRow, 1).Value = $"Extracted from {fromDate} to {toDate} by {sessionFullName}";
    //        worksheet.Range(currentRow, 1, currentRow, 5).Merge().Style.Font.SetItalic().Font.FontSize = 10;
    //        currentRow += 2;

    //        // Headers
    //        var headers = new[] { "Id", "Action", "Timestamp", "UserName", "MicroServiceName", "FullName", "UserID", "Level", "IPAddress", "StatusCode", "StringifyObject", "DetailMessage", "BranchID", "BankID", "BranchName", "BranchCode", "CorrelationId" };
    //        for (int i = 0; i < headers.Length; i++)
    //        {
    //            worksheet.Cell(currentRow, i + 1).Value = headers[i];
    //        }

    //        var headerRange = worksheet.Range(currentRow, 1, currentRow, headers.Length);
    //        headerRange.Style.Font.SetBold();
    //        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
    //        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    //        headerRange.Style.Font.FontName = "Bahnschrift Light";
    //        headerRange.Style.Font.FontSize = 10;

    //        currentRow++;

    //        // Data Rows
    //        foreach (var item in data)
    //        {
    //            worksheet.Cell(currentRow, 1).Value = item.Id ?? "N/A";
    //            worksheet.Cell(currentRow, 2).Value = item.Action ?? "N/A";
    //            worksheet.Cell(currentRow, 3).Value = item.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
    //            worksheet.Cell(currentRow, 4).Value = item.UserName ?? "N/A";
    //            worksheet.Cell(currentRow, 5).Value = item.MicroServiceName ?? "N/A";
    //            worksheet.Cell(currentRow, 6).Value = item.FullName ?? "N/A";
    //            worksheet.Cell(currentRow, 7).Value = item.UserID ?? "N/A";
    //            worksheet.Cell(currentRow, 8).Value = item.Level ?? "N/A";
    //            worksheet.Cell(currentRow, 9).Value = item.IPAddress ?? "N/A";
    //            worksheet.Cell(currentRow, 10).Value = item.StatusCode?.ToString() ?? "N/A";
    //            worksheet.Cell(currentRow, 11).Value = item.StringifyObject ?? "N/A";
    //            worksheet.Cell(currentRow, 12).Value = item.DetailMessage ?? "N/A";
    //            worksheet.Cell(currentRow, 13).Value = item.BranchID ?? "N/A";
    //            worksheet.Cell(currentRow, 14).Value = item.BankID ?? "N/A";
    //            worksheet.Cell(currentRow, 15).Value = item.BranchName ?? "N/A";
    //            worksheet.Cell(currentRow, 16).Value = item.BranchCode ?? "N/A";
    //            worksheet.Cell(currentRow, 17).Value = item.CorrolationId ?? "N/A";
    //            currentRow++;
    //        }

    //        // Summary Section
    //        currentRow += 2;
    //        worksheet.Cell(currentRow, 1).Value = "Summary";
    //        worksheet.Cell(currentRow, 1).Style.Font.SetBold().Font.FontSize = 12;
    //        currentRow++;

    //        worksheet.Cell(currentRow, 1).Value = "Total Logs:";
    //        worksheet.Cell(currentRow, 2).Value = data.Count();
    //        currentRow++;

    //        worksheet.Cell(currentRow, 1).Value = "Total Errors:";
    //        worksheet.Cell(currentRow, 2).Value = data.Count(d => d.Level == "Error");
    //        currentRow++;

    //        worksheet.Cell(currentRow, 1).Value = "Total Information:";
    //        worksheet.Cell(currentRow, 2).Value = data.Count(d => d.Level == "Information");
    //        currentRow++;

    //        worksheet.Cell(currentRow, 1).Value = "Total Warnings:";
    //        worksheet.Cell(currentRow, 2).Value = data.Count(d => d.Level == "Warning");
    //        currentRow++;

    //        // Logs per user
    //        var groupedLogs = data.GroupBy(d => d.FullName)
    //                              .Select(g => new { User = g.Key, Count = g.Count() })
    //                              .OrderByDescending(g => g.Count);

    //        currentRow += 2;
    //        worksheet.Cell(currentRow, 1).Value = "Logs per User";
    //        worksheet.Cell(currentRow, 1).Style.Font.SetBold().Font.FontSize = 12;
    //        currentRow++;

    //        worksheet.Cell(currentRow, 1).Value = "User";
    //        worksheet.Cell(currentRow, 2).Value = "Log Count";
    //        var groupHeaderRange = worksheet.Range(currentRow, 1, currentRow, 2);
    //        groupHeaderRange.Style.Font.SetBold();
    //        groupHeaderRange.Style.Fill.BackgroundColor = XLColor.LightGray;
    //        currentRow++;

    //        foreach (var log in groupedLogs)
    //        {
    //            worksheet.Cell(currentRow, 1).Value = log.User ?? "Unknown";
    //            worksheet.Cell(currentRow, 2).Value = log.Count;
    //            currentRow++;
    //        }

    //        // Formatting
    //        worksheet.Columns().AdjustToContents();
    //        worksheet.Style.Font.FontName = "Bahnschrift Light";
    //        worksheet.Style.Font.FontSize = 10;

    //        // Create memory stream to hold the Excel file
    //        var stream = new MemoryStream();
    //        workbook.SaveAs(stream);
    //        var content = stream.ToArray();

    //        return new ExportFileResult
    //        {
    //            Content = content,
    //            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    //            FileName = $"Audit_Trail_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
    //        };
    //    }
    //}

    //public static class ExportUtilityAuditTrail
    //{
    //    public static ExportFileResult ConvertToExcel(IEnumerable<AuditTrailDto> data)
    //    {
    //        var workbook = new XLWorkbook();
    //        var worksheet = workbook.Worksheets.Add("Audit Trails");

    //        // Write headers
    //        worksheet.Cell(1, 1).Value = "Action";
    //        worksheet.Cell(1, 2).Value = "Timestamp";
    //        worksheet.Cell(1, 3).Value = "Full Name";
    //        worksheet.Cell(1, 4).Value = "MicroService";
    //        worksheet.Cell(1, 5).Value = "IP Address";
    //        worksheet.Cell(1, 6).Value = "Status Code";
    //        worksheet.Cell(1, 7).Value = "Details";

    //        // Apply header styling
    //        var headerRange = worksheet.Range(1, 1, 1, 7);
    //        headerRange.Style.Font.Bold = true;
    //        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

    //        // Write data starting from the second row
    //        int row = 2;
    //        foreach (var item in data)
    //        {
    //            worksheet.Cell(row, 1).Value = item.Action ?? "N/A";
    //            worksheet.Cell(row, 2).Value = item.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";  // Format the date
    //            worksheet.Cell(row, 3).Value = item.FullName ?? "N/A";
    //            worksheet.Cell(row, 4).Value = item.MicroServiceName ?? "N/A";
    //            worksheet.Cell(row, 5).Value = item.IPAddress ?? "N/A";
    //            worksheet.Cell(row, 6).Value = item.StatusCode?.ToString() ?? "N/A";
    //            worksheet.Cell(row, 7).Value = item.DetailMessage ?? "N/A";
    //            row++;
    //        }

    //        // Adjust column widths to fit content
    //        worksheet.Columns().AdjustToContents();

    //        // Create memory stream to hold the Excel file
    //        var stream = new MemoryStream();
    //        workbook.SaveAs(stream);
    //        var content = stream.ToArray();

    //        return new ExportFileResult
    //        {
    //            Content = content,
    //            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    //            FileName = $"Audit_Trail_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
    //        };
    //    }
    //}

    public class ExportFileResult
    {
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
    }


}