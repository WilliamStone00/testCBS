using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using CBS.FrontDesk.Data.Entity.AuditTralP;
using CBS.FrontDesk.Data.Entity.RequestManagement;
using ClosedXML.Excel;

namespace CBS.FrontDesk.UI.Helper
{
    public static class ExportUtilityRateLimitDashboardLogs
    {
        public static ExportFileResult ExportRateLimitLogsToExcel(List<RateLimiteTrackerLogger> data, DateTime? startDate, DateTime? endDate)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Rate Limit Logs");

            // === Title Section ===
            worksheet.Cell(1, 1).Value = "Rate Limit Logs - TSC Monitoring";
            worksheet.Range(1, 1, 1, 19).Merge().Style
                .Font.SetFontName("Bahnschrift").Font.SetFontSize(14).Font.SetBold()
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

            worksheet.Cell(2, 1).Value = $"Date Range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}";
            worksheet.Cell(2, 1).Style.Font.SetFontName("Bahnschrift");

            worksheet.Cell(3, 1).Value = $"Printed By: {HttpContext.Current?.Session?["FullName"] ?? "System"}";
            worksheet.Cell(3, 1).Style.Font.SetFontName("Bahnschrift");

            var headers = new[] {
            "Timestamp", "IP Address", "Username", "Full Name", "Phone Number", "Branch Code", "Branch Name",
            "Country", "Region", "City", "Latitude", "Longitude", "Location", "Path", "HTTP Code",
            "Status", "Reason", "Block End", "Block Type"
        };

            int startRow = 5;
            int row = startRow + 1;

            // === Grouping by Branch ===
            var grouped = data.OrderBy(x => x.BranchName).GroupBy(x => x.BranchName ?? "Unknown");

            foreach (var branchGroup in grouped)
            {
                worksheet.Cell(row++, 1).Value = $"Branch: {branchGroup.Key}";
                worksheet.Cell(row - 1, 1).Style.Font.SetBold().Font.SetFontName("Bahnschrift");

                // Headers
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(row, i + 1).Value = headers[i];
                    worksheet.Cell(row, i + 1).Style
                        .Font.SetFontName("Bahnschrift").Font.SetBold()
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                        .Fill.SetBackgroundColor(XLColor.LightGray);
                }

                row++;

                foreach (var log in branchGroup.OrderByDescending(x => x.Timestamp))
                {
                    worksheet.Cell(row, 1).Value = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                    worksheet.Cell(row, 2).Value = log.IpAddress;
                    worksheet.Cell(row, 3).Value = log.UserName;
                    worksheet.Cell(row, 4).Value = log.FullName;
                    worksheet.Cell(row, 5).Value = log.PhoneNumber;
                    worksheet.Cell(row, 6).Value = log.BranchCode;
                    worksheet.Cell(row, 7).Value = log.BranchName;
                    worksheet.Cell(row, 8).Value = log.Country;
                    worksheet.Cell(row, 9).Value = log.Region;
                    worksheet.Cell(row, 10).Value = log.City;
                    worksheet.Cell(row, 11).Value = log.Latitude;
                    worksheet.Cell(row, 12).Value = log.Longitude;
                    worksheet.Cell(row, 13).Value = log.Location;
                    worksheet.Cell(row, 14).Value = log.Path;
                    worksheet.Cell(row, 15).Value = log.StatusCode;
                    worksheet.Cell(row, 16).Value = log.IsBlocked ? "Blocked" : "Allowed";
                    worksheet.Cell(row, 17).Value = log.Reason ?? "-";
                    worksheet.Cell(row, 18).Value = log.BlockEndTime.ToString("yyyy-MM-dd HH:mm:ss");
                    worksheet.Cell(row, 19).Value = log.BlockType ?? "-";

                    for (int i = 1; i <= 19; i++)
                    {
                        worksheet.Cell(row, i).Style
                            .Font.SetFontName("Bahnschrift").Font.SetFontSize(10)
                            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                            .Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    row++;
                }

                // Footer for branch
                worksheet.Cell(row++, 1).Value = $"Total Requests for {branchGroup.Key}: {branchGroup.Count()}";
                worksheet.Cell(row - 1, 1).Style.Font.SetFontName("Bahnschrift").Font.SetBold();

                row++; // spacing between groups
            }

            worksheet.Columns().AdjustToContents();

            // === Summary Tables ===
            row += 2;
            AddSummary(worksheet, "Top IPs", data.GroupBy(x => x.IpAddress), row);
            row += 13;
            AddSummary(worksheet, "Top Users", data.GroupBy(x => x.UserName), row);
            row += 13;
            AddSummary(worksheet, "Top Branches", data.GroupBy(x => x.BranchName), row);
            row += 13;
            AddSummary(worksheet, "Blocked Users", data.Where(x => x.IsBlocked).GroupBy(x => x.UserName), row);

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return new ExportFileResult
            {
                Content = stream.ToArray(),
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = $"RateLimitLogs_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };
        }

        private static void AddSummary(IXLWorksheet ws, string title, IEnumerable<IGrouping<string, RateLimiteTrackerLogger>> groups, int startRow)
        {
            ws.Cell(startRow, 1).Value = title;
            ws.Cell(startRow, 1).Style.Font.SetFontName("Bahnschrift").Font.SetBold();
            int r = startRow + 1;

            foreach (var g in groups.OrderByDescending(x => x.Count()).Take(10))
            {
                ws.Cell(r, 1).Value = g.Key ?? "N/A";
                ws.Cell(r, 2).Value = g.Count();

                ws.Cell(r, 1).Style.Font.SetFontName("Bahnschrift");
                ws.Cell(r, 2).Style.Font.SetFontName("Bahnschrift");

                ws.Range(r, 1, r, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                r++;
            }
        }
    }



}