using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.Helper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CBS.FrontDesk.Data.UserManagement;
    using ClosedXML.Excel;



    public static class ExportUtilityUser
    {
        public static ExportFileResult ConvertToExcel(IEnumerable<UserDownloadDto> data, DateTime? startDate, DateTime? endDate, string searchItem)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Users");

            // Write title
            worksheet.Cell(1, 1).Value = "User Data Export";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.Italic = true;
            worksheet.Cell(1, 1).Style.Font.FontName = "Bahnschrift Light";
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Range(1, 1, 1, 17).Merge();
            worksheet.Range(1, 1, 1, 17).Style.Fill.BackgroundColor = XLColor.LightBlue;

            // Add subtitle
            worksheet.Cell(2, 1).Value = $"Extracted from {(startDate?.ToString("yyyy-MM-dd") ?? "N/A")} to {(endDate?.ToString("yyyy-MM-dd") ?? "N/A")}";
            worksheet.Cell(2, 1).Style.Font.FontName = "Bahnschrift Light";
            worksheet.Cell(2, 1).Style.Font.FontSize = 10;
            worksheet.Cell(2, 1).Style.Font.Italic = true;
            worksheet.Range(2, 1, 2, 17).Merge();

            // Write headers
            var headers = new[]
            {
            "UserName", "FirstName", "LastName", "FullName", "RoleName",
            "PhoneNumber", "IsVerified", "IsBlocked", "ReasonForBlockingAccount",
            "LoginAttempts", "ChangePasswordOnFirstLogin", "LastLoginDate",
            "IsActive", "SessionRecoveryCode", "CreatedDate", "BranchCode",
            "BranchName", "NumberOfDaysSinceLastLogin"
        };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(4, i + 1).Value = headers[i];
                worksheet.Cell(4, i + 1).Style.Font.Bold = true;
                worksheet.Cell(4, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(4, i + 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Cell(4, i + 1).Style.Font.FontSize = 10;
                worksheet.Cell(4, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Write data
            int row = 5;
            foreach (var user in data)
            {
                worksheet.Cell(row, 1).Value = user.UserName ?? "N/A";
                worksheet.Cell(row, 2).Value = user.FirstName ?? "N/A";
                worksheet.Cell(row, 3).Value = user.LastName ?? "N/A";
                worksheet.Cell(row, 4).Value = user.FullName ?? "N/A";
                worksheet.Cell(row, 5).Value = user.RoleName ?? "N/A";
                worksheet.Cell(row, 6).Value = user.PhoneNumber ?? "N/A";
                worksheet.Cell(row, 7).Value = user.IsVerified;
                worksheet.Cell(row, 8).Value = user.IsBlocked;
                worksheet.Cell(row, 9).Value = user.ReasonForBlockingAccount ?? "N/A";
                worksheet.Cell(row, 10).Value = user.LoginAttempts;
                worksheet.Cell(row, 11).Value = user.ChangePasswordOnFirstLogin;
                worksheet.Cell(row, 12).Value = user.LastLoginDate?.ToString("yyyy-MM-dd") ?? "N/A";
                worksheet.Cell(row, 13).Value = user.IsActive;
                worksheet.Cell(row, 14).Value = user.SessionRecoveryCode ?? "N/A";
                worksheet.Cell(row, 15).Value = user.CreatedDate.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 16).Value = user.BranchCode ?? "N/A";
                worksheet.Cell(row, 17).Value = user.BranchName ?? "N/A";
                worksheet.Cell(row, 18).Value = user.NumberOfDaysSinceLastLogin;
                row++;
            }

            // Adjust column widths
            worksheet.Columns().AdjustToContents();

            // Create memory stream to hold the Excel file
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return new ExportFileResult
            {
                Content = content,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = $"User_Data_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            };
        }
    }

}