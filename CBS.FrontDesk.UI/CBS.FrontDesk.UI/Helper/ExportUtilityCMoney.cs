using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using CBS.FrontDesk.Data.Entity.AuditTralP;
using CBS.FrontDesk.Data.Entity.CMoney;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CBS.FrontDesk.UI.Helper
{
    public static class ExportUtilityCMoney
    {
        public static ExportFileResult GenerateCMoneyMemberActivationsExcel(List<CMoneyMembersActivationAccount> activations, string exportedBy, string startDate, string endDate)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("C-Money Activations");

                worksheet.Cell(1, 1).Value = "C-MONEY MEMBER ACTIVATIONS EXPORT";
                worksheet.Cell(1, 1).Style.Font.SetBold().Font.SetFontSize(14).Font.SetFontName("Bahnschrift");
                worksheet.Range(1, 1, 1, 30).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                worksheet.Cell(2, 1).Value = $"Export Date: {DateTime.Now:dd/MM/yyyy HH:mm:ss} | Exported By: {exportedBy}";
                worksheet.Range(2, 1, 2, 30).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left).Font.SetFontName("Bahnschrift");

                string dateRangeText = (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                    ? $"Date Range: {startDate} to {endDate}"
                    : "Date Range: ALL";
                worksheet.Cell(3, 1).Value = dateRangeText;
                worksheet.Range(3, 1, 3, 30).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left).Font.SetFontName("Bahnschrift");

                worksheet.Cell(4, 1).Value = $"Overall Total Activations: {activations.Count:N0}";
                worksheet.Range(4, 1, 4, 30).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left).Font.SetBold().Font.SetFontName("Bahnschrift");

                worksheet.Cell(6, 1).Value = "SUMMARY OF BRANCH ACTIVATIONS";
                worksheet.Range(6, 1, 6, 2).Merge().Style.Font.SetBold().Font.SetFontName("Bahnschrift").Border.OutsideBorder = XLBorderStyleValues.Thin;
                int summaryRow = 7;
                worksheet.Cell(summaryRow, 1).Value = "Branch Name [Code]";
                worksheet.Cell(summaryRow, 2).Value = "Number of Activations";
                worksheet.Range(summaryRow, 1, summaryRow, 2).Style.Font.SetBold().Font.SetFontName("Bahnschrift").Border.OutsideBorder = XLBorderStyleValues.Thin;
                summaryRow++;

                foreach (var branch in activations.GroupBy(a => new { a.ActivatingBranchName, a.ActivatingBranchCode }).OrderByDescending(x => x.Count()))
                {
                    worksheet.Cell(summaryRow, 1).Value = $"{branch.Key.ActivatingBranchName} [Code: {branch.Key.ActivatingBranchCode}]";
                    worksheet.Cell(summaryRow, 2).Value = branch.Count();
                    worksheet.Range(summaryRow, 1, summaryRow, 2).Style.Font.SetFontName("Bahnschrift").Border.OutsideBorder = XLBorderStyleValues.Thin;
                    summaryRow++;
                }

                var headers = new[]
                {
                    "NAME", "M.REFERENCE", "LOGIN", "PHONE", "S.BCDE", "A.DATE",
                    "ACTIVATED BY", "A.STATUS", "CH.D.PIN", "SUBSCRIBED?", "L.L.DATE", "F.ATTEMPTS",
                    "DEACTIVATION REASON", "LANGUAGE", "A.BRANCH NAME", "A.BCDE"
                 };

                int currentRow = summaryRow + 2;

                foreach (var branchGroup in activations
                    .OrderByDescending(x => x.ActivationDate)
                    .ThenBy(x => x.BranchCode)
                    .GroupBy(a => a.ActivatingBranchName))
                {
                    worksheet.Cell(currentRow, 1).Value = $"ACTIVATING BRANCH: {branchGroup.Key.ToUpper()} [{branchGroup.FirstOrDefault().ActivatingBranchCode}]";
                    worksheet.Range(currentRow, 1, currentRow, headers.Length).Merge().Style.Font.SetBold().Font.SetFontName("Bahnschrift").Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Style.Font.SetBold().Font.SetFontName("Bahnschrift").Fill.SetBackgroundColor(XLColor.LightGray)
                            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                            .Border.OutsideBorder = XLBorderStyleValues.Thin;

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(currentRow, i + 1).Value = headers[i];
                        worksheet.Cell(currentRow, i + 1).Style.Font.SetBold().Font.SetFontName("Bahnschrift").Fill.SetBackgroundColor(XLColor.LightGray)
                            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                            .Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    currentRow++;

                    foreach (var item in branchGroup.OrderByDescending(a => a.ActivationDate).ThenBy(a => a.BranchCode))
                    {
                        worksheet.Cell(currentRow, 1).Value = item.Name;
                        worksheet.Cell(currentRow, 2).Value = item.CustomerId;
                        worksheet.Cell(currentRow, 3).Value = item.LoginId;
                        worksheet.Cell(currentRow, 4).Value = item.PhoneNumber;
                        worksheet.Cell(currentRow, 5).Value = item.BranchCode;
                        worksheet.Cell(currentRow, 6).Value = item.ActivationDate.ToString("dd/MM/yyyy HH:mm:ss");
                        worksheet.Cell(currentRow, 7).Value = item.ActivatedBy;
                        worksheet.Cell(currentRow, 8).Value = item.IsActive ? "Active" : "Deactivated";
                        worksheet.Cell(currentRow, 9).Value = item.HasChangeDefaultPin ? "Yes" : "No";
                        worksheet.Cell(currentRow, 10).Value = item.IsSubcribed ? "Yes" : "No";
                        worksheet.Cell(currentRow, 11).Value = item.LastLoginDate?.ToString("dd/MM/yyyy HH:mm:ss");
                        worksheet.Cell(currentRow, 12).Value = item.FailedAttempts;
                        worksheet.Cell(currentRow, 13).Value = item.DeactivationReason;
                        worksheet.Cell(currentRow, 14).Value = item.Language;
                        worksheet.Cell(currentRow, 15).Value = item.ActivatingBranchName;
                        worksheet.Cell(currentRow, 16).Value = item.ActivatingBranchCode;

                        for (int col = 1; col <= 16; col++)
                        {
                            worksheet.Cell(currentRow, col).Style.Font.SetFontName("Bahnschrift").Font.SetFontSize(10).Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        currentRow++;
                    }

                    worksheet.Cell(currentRow, 1).Value = $"TOTAL ACTIVATIONS FOR {branchGroup.Key.ToUpper()}: {branchGroup.Count()} [{branchGroup.FirstOrDefault().ActivatingBranchCode}]";
                    worksheet.Range(currentRow, 1, currentRow, headers.Length).Merge().Style.Font.SetBold().Font.SetFontName("Bahnschrift")
                        .Fill.SetBackgroundColor(XLColor.LightGray).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left).Border.OutsideBorder = XLBorderStyleValues.Thin;
                    currentRow += 2;
                }

                worksheet.Cell(currentRow, 1).Value = "SUMMARY: ACTIVATIONS PER USER";
                worksheet.Range(currentRow, 1, currentRow, 2).Merge().Style.Font.SetBold().Font.SetFontName("Bahnschrift");
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = "User [Branch]";
                worksheet.Cell(currentRow, 2).Value = "Activations Count";
                worksheet.Row(currentRow).Style.Font.SetBold().Font.SetFontName("Bahnschrift");
                currentRow++;

                foreach (var userGroup in activations
                             .GroupBy(a => new { a.ActivatedBy, a.ActivatingBranchName })
                             .OrderByDescending(g => g.Count()))
                {
                    worksheet.Cell(currentRow, 1).Value = $"{userGroup.Key.ActivatedBy} [{userGroup.Key.ActivatingBranchName}]";
                    worksheet.Cell(currentRow, 2).Value = userGroup.Count();
                    worksheet.Range(currentRow, 1, currentRow, 2).Style.Font.SetFontName("Bahnschrift").Font.SetFontSize(10).Border.OutsideBorder = XLBorderStyleValues.Thin;
                    currentRow++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return new ExportFileResult
                    {
                        Content = stream.ToArray(),
                        ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        FileName = $"C_Money_Activations_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                    };
                }
            }
        }
        //public static ExportFileResult GenerateCMoneyMemberActivationsExcel(List<CMoneyMembersActivationAccount> activations, string exportedBy, string startDate, string endDate)
        //{
        //    using (var workbook = new XLWorkbook())
        //    {
        //        var worksheet = workbook.Worksheets.Add("C-Money Activations");

        //        worksheet.Cell(1, 1).Value = $"C-MONEY MEMBER ACTIVATIONS EXPORT";
        //        worksheet.Cell(1, 1).Style.Font.Bold = true;
        //        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        //        worksheet.Cell(1, 1).Style.Font.FontName = "Bahnschrift";
        //        worksheet.Range(1, 1, 1, 30).Merge();
        //        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        //        worksheet.Cell(2, 1).Value = $"Export Date: {DateTime.Now:dd/MM/yyyy HH:mm:ss} | Exported By: {exportedBy}";
        //        worksheet.Range(2, 1, 2, 30).Merge();
        //        worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        //        worksheet.Cell(2, 1).Style.Font.FontName = "Bahnschrift";

        //        string dateRangeText = (startDate != null && endDate != null)? $"Date Range: {startDate:dd/MM/yyyy} to {endDate:dd/MM/yyyy}": "Date Range: ALL";

        //        worksheet.Cell(3, 1).Value = dateRangeText;

        //        worksheet.Range(3, 1, 3, 30).Merge();
        //        worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        //        worksheet.Cell(3, 1).Style.Font.FontName = "Bahnschrift";

        //        worksheet.Cell(4, 1).Value = $"Overall Total Activations: {activations.Count:N0}";
        //        worksheet.Range(4, 1, 4, 30).Merge();
        //        worksheet.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        //        worksheet.Cell(4, 1).Style.Font.Bold = true;
        //        worksheet.Cell(4, 1).Style.Font.FontName = "Bahnschrift";

        //        worksheet.Cell(6, 1).Value = "SUMMARY OF BRANCH ACTIVATIONS";
        //        worksheet.Cell(6, 1).Style.Font.Bold = true;
        //        worksheet.Range(6, 1, 6, 4).Merge();
        //        worksheet.Cell(6, 1).Style.Font.FontName = "Bahnschrift";
        //        int summaryRow = 7;
        //        worksheet.Cell(summaryRow, 1).Value = "Branch Name";
        //        worksheet.Cell(summaryRow, 2).Value = "Number of Activations";
        //        worksheet.Row(summaryRow).Style.Font.Bold = true;
        //        worksheet.Row(summaryRow).Style.Font.FontName = "Bahnschrift";
        //        summaryRow++;

        //        foreach (var branch in activations.GroupBy(a => a.ActivatingBranchName))
        //        {
        //            worksheet.Cell(summaryRow, 1).Value = branch.Key;
        //            worksheet.Cell(summaryRow, 2).Value = branch.Count();
        //            worksheet.Cell(summaryRow, 1).Style.Font.FontName = "Bahnschrift";
        //            worksheet.Cell(summaryRow, 2).Style.Font.FontName = "Bahnschrift";
        //            worksheet.Range(summaryRow, 1, summaryRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //            summaryRow++;
        //        }

        //        var headers = new[]
        //        {
        //            "NAME", "M.REFERENCE", "LOGIN", "PHONE", "S.BCDE", "A.DATE",
        //            "ACTIVATED BY", "A.STATUS", "CH.D.PIN", "SUBSCRIBED?", "L.L.DATE", "F.ATTEMPTS",
        //            "DEACTIVATION REASON", "LANGUAGE", "A.BRANCH NAME", "A.BCDE"
        //        };


        //        int currentRow = 6;

        //        foreach (var branchGroup in activations.OrderByDescending(x =>x.ActivationDate).GroupBy(a => a.ActivatingBranchName))
        //        {
        //            string bName = $"Activating Branch: {branchGroup.Key}";
        //            worksheet.Cell(currentRow, 1).Value = bName.ToUpper();
        //            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
        //            worksheet.Cell(currentRow, 1).Style.Font.FontName = "Bahnschrift";
        //            worksheet.Range(currentRow, 1, currentRow, headers.Length).Merge();
        //            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        //            currentRow++;

        //            for (int i = 0; i < headers.Length; i++)
        //            {
        //                worksheet.Cell(currentRow, i + 1).Value = headers[i];
        //                worksheet.Cell(currentRow, i + 1).Style.Font.Bold = true;
        //                worksheet.Cell(currentRow, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        //                worksheet.Cell(currentRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //                worksheet.Cell(currentRow, i + 1).Style.Font.FontName = "Bahnschrift";
        //                worksheet.Cell(currentRow, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //            }
        //            currentRow++;

        //            foreach (var item in branchGroup.OrderBy(a => a.CustomerId))
        //            {
        //                worksheet.Cell(currentRow, 1).Value = item.Name;
        //                worksheet.Cell(currentRow, 2).Value = item.CustomerId;
        //                worksheet.Cell(currentRow, 3).Value = item.LoginId;
        //                worksheet.Cell(currentRow, 4).Value = item.PhoneNumber;
        //                worksheet.Cell(currentRow, 5).Value = item.BranchCode;
        //                worksheet.Cell(currentRow, 6).Value = item.ActivationDate.ToString("dd/MM/yyyy HH:mm:ss");
        //                worksheet.Cell(currentRow, 7).Value = item.ActivatedBy;
        //                worksheet.Cell(currentRow, 8).Value = item.IsActive ? "Active" : "Deactivated";
        //                worksheet.Cell(currentRow, 9).Value = item.HasChangeDefaultPin ? "Yes" : "No";
        //                worksheet.Cell(currentRow, 10).Value = item.IsSubcribed ? "Yes" : "No";
        //                worksheet.Cell(currentRow, 11).Value = item.LastLoginDate?.ToString("dd/MM/yyyy HH:mm:ss");
        //                worksheet.Cell(currentRow, 12).Value = item.FailedAttempts;
        //                worksheet.Cell(currentRow, 13).Value = item.DeactivationReason;
        //                worksheet.Cell(currentRow, 14).Value = item.Language;
        //                worksheet.Cell(currentRow, 15).Value = item.ActivatingBranchName;
        //                worksheet.Cell(currentRow, 16).Value = item.ActivatingBranchCode;


        //                for (int col = 1; col <= 16; col++)
        //                {
        //                    worksheet.Cell(currentRow, col).Style.Font.FontName = "Bahnschrift";
        //                    worksheet.Cell(currentRow, col).Style.Font.FontSize = 10;
        //                    worksheet.Cell(currentRow, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //                }

        //                currentRow++;
        //            }
        //            string footer = $"Total Activations for {branchGroup.Key}: {branchGroup.Count()}";
        //            worksheet.Cell(currentRow, 1).Value = footer.ToUpper();
        //            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
        //            worksheet.Range(currentRow, 1, currentRow, headers.Length).Merge();
        //            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        //            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        //            worksheet.Cell(currentRow, 1).Style.Font.FontName = "Bahnschrift";
        //            currentRow += 2;
        //        }

        //        worksheet.Cell(currentRow, 1).Value = "ACTIVATION SUMMARY BY ACTIVATING BRANCH";
        //        worksheet.Range(currentRow, 1, currentRow, 2).Merge().Style.Font.SetBold().Font.SetFontName("Bahnschrift");
        //        currentRow++;

        //        worksheet.Cell(currentRow, 1).Value = "Activating Branch";
        //        worksheet.Cell(currentRow, 2).Value = "Activations";
        //        worksheet.Row(currentRow).Style.Font.SetBold().Font.SetFontName("Bahnschrift");
        //        currentRow++;

        //        foreach (var branchSummary in activations.GroupBy(a => a.ActivatingBranchName).OrderByDescending(g => g.Count()))
        //        {
        //            worksheet.Cell(currentRow, 1).Value = branchSummary.Key;
        //            worksheet.Cell(currentRow, 2).Value = branchSummary.Count();
        //            worksheet.Range(currentRow, 1, currentRow, 2).Style.Font.SetFontName("Bahnschrift").Font.SetFontSize(10).Border.OutsideBorder = XLBorderStyleValues.Thin;
        //            currentRow++;
        //        }

        //        currentRow += 1;

        //        worksheet.Cell(currentRow, 1).Value = "SUMMARY: ACTIVATIONS PER USER";
        //        worksheet.Range(currentRow, 1, currentRow, 2).Merge().Style.Font.SetBold().Font.SetFontName("Bahnschrift");
        //        currentRow++;

        //        worksheet.Cell(currentRow, 1).Value = "User [Branch]";
        //        worksheet.Cell(currentRow, 2).Value = "Activations Count";
        //        worksheet.Row(currentRow).Style.Font.SetBold().Font.SetFontName("Bahnschrift");
        //        currentRow++;

        //        foreach (var userGroup in activations.GroupBy(a => new { a.ActivatedBy, a.ActivatingBranchName }).OrderByDescending(g => g.Count()))
        //        {
        //            worksheet.Cell(currentRow, 1).Value = $"{userGroup.Key.ActivatedBy} [{userGroup.Key.ActivatingBranchName}]";
        //            worksheet.Cell(currentRow, 2).Value = userGroup.Count();
        //            worksheet.Range(currentRow, 1, currentRow, 2).Style.Font.SetFontName("Bahnschrift").Font.SetFontSize(10).Border.OutsideBorder = XLBorderStyleValues.Thin;
        //            currentRow++;
        //        }

        //        worksheet.Columns().AdjustToContents();

        //        using (var stream = new MemoryStream())
        //        {
        //            workbook.SaveAs(stream);
        //            return new ExportFileResult
        //            {
        //                Content = stream.ToArray(),
        //                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //                FileName = $"C_Money_Activations_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
        //            };
        //        }
        //    }


        //}
    }
}



