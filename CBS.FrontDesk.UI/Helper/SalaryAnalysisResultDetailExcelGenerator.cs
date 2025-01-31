using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using CBS.FrontDesk.Data.Entity.SalaryManagement;

namespace CBS.FrontDesk.UI.Helper
{


    public static class SalaryAnalysisResultDetailExcelGenerator
    {
        public static void GenerateSalaryAnalysisExcel(List<SalaryAnalysisResultDetail> salaryDetails, string branchName, string filePath, string exportDate, string exportedBy, string fileCode)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Salary Analysis");

                // Title
                worksheet.Cell(1, 1).Value = $"SALARY ANALYSIS FOR {branchName.ToUpper()}";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 14;
                worksheet.Range(1, 1, 1, 15).Merge();
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Export Details
                worksheet.Cell(2, 1).Value = $"Export Date: {exportDate} BY {exportedBy}";
                worksheet.Cell(2, 1).Style.Font.Italic = true;
                worksheet.Cell(2, 1).Style.Font.FontSize = 10;
                worksheet.Range(2, 1, 2, 15).Merge();
                worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(3, 1).Value = $"Salary File Code: {fileCode}";
                worksheet.Cell(3, 1).Style.Font.Italic = true;
                worksheet.Cell(3, 1).Style.Font.FontSize = 10;
                worksheet.Range(3, 1, 3, 15).Merge();
                worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Headers
                var headers = new[]
                {
                "Matricule", "Member Reference", "Member Name", "Net Salary", "Savings", "Deposit",
                "Ordinary Shares", "Preference Shares", "Loan Repayment", "Loan Capital", "Loan Interest",
                "VAT", "Charges", "Salary Balance", "Status"
            };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(5, i + 1).Value = headers[i];
                    worksheet.Cell(5, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(5, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Cell(5, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(5, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(5, i + 1).Style.Border.OutsideBorderColor = XLColor.Black;
                }

                // Data
                int currentRow = 6;
                foreach (var detail in salaryDetails)
                {
                    worksheet.Cell(currentRow, 1).Value = detail.Matricule;
                    worksheet.Cell(currentRow, 2).Value = detail.CustomerId;
                    worksheet.Cell(currentRow, 3).Value = detail.MemberName;
                    worksheet.Cell(currentRow, 4).Value = detail.NetSalary;
                    worksheet.Cell(currentRow, 5).Value = detail.Savings;
                    worksheet.Cell(currentRow, 6).Value = detail.Deposit;
                    worksheet.Cell(currentRow, 7).Value = detail.Shares;
                    worksheet.Cell(currentRow, 8).Value = detail.PreferenceShares;
                    worksheet.Cell(currentRow, 9).Value = detail.TotalLoanRepayment;
                    worksheet.Cell(currentRow, 10).Value = detail.LoanCapital;
                    worksheet.Cell(currentRow, 11).Value = detail.LoanInterest;
                    worksheet.Cell(currentRow, 12).Value = detail.VAT;
                    worksheet.Cell(currentRow, 13).Value = detail.Charges;
                    worksheet.Cell(currentRow, 14).Value = detail.RemainingSalary;
                    worksheet.Cell(currentRow, 15).Value = detail.Status;

                    for (int col = 1; col <= 15; col++)
                    {
                        worksheet.Cell(currentRow, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(currentRow, col).Style.Border.OutsideBorderColor = XLColor.Black;
                    }

                    // Format currency values
                    for (int col = 4; col <= 14; col++)
                    {
                        worksheet.Cell(currentRow, col).Style.NumberFormat.Format = "#,##0.0";
                    }

                    currentRow++;
                }

                // Footer
                worksheet.Cell(currentRow, 1).Value = "TOTAL";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(currentRow, 1, currentRow, 3).Merge();
                worksheet.Range(currentRow, 1, currentRow, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Range(currentRow, 1, currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(currentRow, 1, currentRow, 3).Style.Border.OutsideBorderColor = XLColor.Black;

                for (int i = 4; i <= 14; i++)
                {
                    worksheet.Cell(currentRow, i).FormulaA1 = $"SUM({worksheet.Cell(6, i).Address}:{worksheet.Cell(currentRow - 1, i).Address})";
                    worksheet.Cell(currentRow, i).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, i).Style.NumberFormat.Format = "#,##0.0";
                    worksheet.Cell(currentRow, i).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Cell(currentRow, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(currentRow, i).Style.Border.OutsideBorderColor = XLColor.Black;
                }

                currentRow++;

                // Footer Signature
                worksheet.Cell(currentRow, 1).Value = "@Trust Soft Credit.";
                worksheet.Cell(currentRow, 1).Style.Font.Italic = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 10;
                worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(currentRow, 1, currentRow, 15).Merge();

                // Adjust column widths
                worksheet.Columns().AdjustToContents();

                // Save the file
                workbook.SaveAs(filePath);
            }
        }
    }
}