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
                worksheet.Cell(1, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 14;
                worksheet.Range(1, 1, 1, 22).Merge();
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
               
                // Export Details
                worksheet.Cell(2, 1).Value = $"Analyzed Date: {exportDate} BY {exportedBy}";
                worksheet.Cell(2, 1).Style.Font.Italic = false;
                worksheet.Cell(2, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Cell(2, 1).Style.Font.FontSize = 10;
                worksheet.Range(2, 1, 2, 22).Merge();
                worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                worksheet.Cell(3, 1).Value = $"Salary File Code: {fileCode}";
                worksheet.Cell(3, 1).Style.Font.Italic = false;
                worksheet.Cell(3, 1).Style.Font.FontSize = 10;
                worksheet.Cell(3, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Range(3, 1, 3, 22).Merge();
                worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                // **Empty Row After Title**
                int summaryStartRow = 5;

                // ======================= SUMMARY TABLE =======================
                worksheet.Cell(summaryStartRow, 1).Value = "ANALYSIS SUMMARY";
                worksheet.Cell(summaryStartRow, 1).Style.Font.Bold = true;
                worksheet.Cell(summaryStartRow, 1).Style.Font.FontSize = 12;
                worksheet.Cell(summaryStartRow, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Cell(summaryStartRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(summaryStartRow, 1, summaryStartRow, 2).Merge();
                worksheet.Range(summaryStartRow, 1, summaryStartRow, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Range(summaryStartRow, 1, summaryStartRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // Summary Data
                var summaryLabels = new[]
                {
                    "Total Members", "Total Net Salary", "Total Standing Order", "Total Savings", "Total Deposit",
                    "Total Ordinary Shares", "Total Preference Shares", "Total Loan Repayment", "Total Loan Capital",
                    "Total Loan Interest", "Total VAT", "Total Charges", "Total Salary Balance",
                    "Total Loans to be Treated", "Number of Migrated Loans"
                };

                // Compute the row where data starts
                int dataStartRow = summaryStartRow + summaryLabels.Length + 3;

                for (int i = 0; i < summaryLabels.Length; i++)
                {
                    int row = summaryStartRow + i + 1;
                    worksheet.Cell(row, 1).Value = summaryLabels[i];
                    worksheet.Cell(row, 1).Style.Font.Bold = false;
                    worksheet.Cell(row, 1).Style.Font.FontName = "Bahnschrift Light";
                    worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    // Compute summary values
                    switch (i)
                    {
                        case 0:
                            worksheet.Cell(row, 2).Value = salaryDetails.Count;
                            break;
                        case 1:
                            worksheet.Cell(row, 2).FormulaA1 = $"SUM(D{dataStartRow}:D{dataStartRow + salaryDetails.Count - 1})"; // Net Salary
                            break;
                        case 2:
                            worksheet.Cell(row, 2).FormulaA1 = $"SUM(E{dataStartRow}:E{dataStartRow + salaryDetails.Count - 1})"; // Standing Order
                            break;
                        case 3:
                            worksheet.Cell(row, 2).FormulaA1 = $"SUM(F{dataStartRow}:F{dataStartRow + salaryDetails.Count - 1})"; // Savings
                            break;
                        case 4:
                            worksheet.Cell(row, 2).FormulaA1 = $"SUM(G{dataStartRow}:G{dataStartRow + salaryDetails.Count - 1})"; // Deposit
                            break;
                        case 5:
                            worksheet.Cell(row, 2).FormulaA1 = $"SUM(H{dataStartRow}:H{dataStartRow + salaryDetails.Count - 1})"; // Ordinary Shares
                            break;
                        case 6:
                            worksheet.Cell(row, 2).FormulaA1 = $"SUM(I{dataStartRow}:I{dataStartRow + salaryDetails.Count - 1})"; // Preference Shares
                            break;
                        case 7:
                            worksheet.Cell(row, 2).FormulaA1 = $"SUM(J{dataStartRow}:J{dataStartRow + salaryDetails.Count - 1})"; // Loan Repayment
                            break;
                        case 8:
                            worksheet.Cell(row, 2).FormulaA1 = $"SUM(K{dataStartRow}:K{dataStartRow + salaryDetails.Count - 1})"; // Loan Capital
                            break;
                        case 9:
                            worksheet.Cell(row, 2).FormulaA1 = $"SUM(L{dataStartRow}:L{dataStartRow + salaryDetails.Count - 1})"; // Loan Interest
                            break;
                        case 10:
                            worksheet.Cell(row, 2).FormulaA1 = $"SUM(M{dataStartRow}:M{dataStartRow + salaryDetails.Count - 1})"; // VAT
                            break;
                        case 11:
                            worksheet.Cell(row, 2).FormulaA1 = $"SUM(N{dataStartRow}:N{dataStartRow + salaryDetails.Count - 1})"; // Charges
                            break;
                        case 12:
                            worksheet.Cell(row, 2).FormulaA1 = $"SUM(O{dataStartRow}:O{dataStartRow + salaryDetails.Count - 1})"; // Salary Balance
                            break;
                        case 13:
                            worksheet.Cell(row, 2).Value = salaryDetails.Count(x => x.LoanId != "n/a");
                            break;
                        case 14:
                            worksheet.Cell(row, 2).Value = salaryDetails.Count(x => x.IsOnldLoan);
                            break;
                    }

                    worksheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0.0";
                    worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Cell(row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }


                // **Empty Row after Summary Table**
                int headersRow = summaryStartRow + summaryLabels.Length + 2;

               
                // ======================= HEADERS =======================
                var headers = new[]
                {
                    "Matricule", "Member Reference", "Member Name", "Gross Salary", "Standing Order", "Savings", "Deposit",
                    "Ordinary Shares", "Preference Shares", "Loan Repayment", "Loan Capital", "Loan Interest",
                    "VAT", "Charges", "Net Salary", "Loan Id", "Loan Type", "Standing Order Statement", "Status",
                    "Is Migrated Loan?", "Loan Product Name", "Loan Product Id"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(headersRow, i + 1).Value = headers[i].ToUpper();
                    worksheet.Cell(headersRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(headersRow, i + 1).Style.Font.FontName = "Bahnschrift Light";
                    worksheet.Cell(headersRow, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Cell(headersRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(headersRow, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    // Lock all cells except VAT (column 13), Interest (column 12), and Capital (column 11)
                    if (i != 10 && i != 11 && i != 12)
                    {
                        worksheet.Column(i + 1).Style.Protection.Locked = true;
                    }
                    else
                    {
                        worksheet.Column(i + 1).Style.Protection.Locked = false;
                    }
                }

                // ======================= DATA =======================
                int currentRow = headersRow + 1;
                foreach (var detail in salaryDetails)
                {
                    worksheet.Cell(currentRow, 1).Value = detail.Matricule;
                    worksheet.Cell(currentRow, 2).Value = detail.CustomerId;
                    worksheet.Cell(currentRow, 3).Value = detail.MemberName;
                    worksheet.Cell(currentRow, 4).Value = detail.NetSalary;
                    worksheet.Cell(currentRow, 5).Value = detail.StandingOrderAmount;
                    worksheet.Cell(currentRow, 6).Value = detail.Savings;
                    worksheet.Cell(currentRow, 7).Value = detail.Deposit;
                    worksheet.Cell(currentRow, 8).Value = detail.Shares;
                    worksheet.Cell(currentRow, 9).Value = detail.PreferenceShares;
                    worksheet.Cell(currentRow, 10).Value = detail.TotalLoanRepayment;
                    worksheet.Cell(currentRow, 11).Value = detail.LoanCapital;  // Editable
                    worksheet.Cell(currentRow, 12).Value = detail.LoanInterest; // Editable
                    worksheet.Cell(currentRow, 13).Value = detail.VAT;          // Editable
                    worksheet.Cell(currentRow, 14).Value = detail.Charges;
                    worksheet.Cell(currentRow, 15).Value = detail.RemainingSalary;
                    worksheet.Cell(currentRow, 16).Value = detail.LoanId;
                    worksheet.Cell(currentRow, 17).Value = detail.LoanType;
                    worksheet.Cell(currentRow, 18).Value = detail.StandingOrderStatement;
                    worksheet.Cell(currentRow, 19).Value = detail.Status;
                    worksheet.Cell(currentRow, 20).Value = detail.IsOnldLoan ? "Yes" : "No"; // Boolean to Yes/No
                    worksheet.Cell(currentRow, 21).Value = detail.LoanProductName;
                    worksheet.Cell(currentRow, 22).Value = detail.LoanProductId;

                    // Apply border and font styles
                    for (int col = 1; col <= 22; col++)
                    {
                        worksheet.Cell(currentRow, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(currentRow, col).Style.Font.FontName = "Bahnschrift Light";
                    }

                    // Format currency values
                    for (int col = 4; col <= 15; col++)
                    {
                        worksheet.Cell(currentRow, col).Style.NumberFormat.Format = "#,##0.0";
                    }

                    // Highlight row in red if any value is negative
                    for (int col = 4; col <= 15; col++)
                    {
                        if (worksheet.Cell(currentRow, col).GetDouble() < 0)
                        {
                            worksheet.Row(currentRow).Style.Fill.BackgroundColor = XLColor.Red;
                            break;
                        }
                    }

                    currentRow++;
                }

                // ======================= FOOTER =======================
                worksheet.Cell(currentRow, 1).Value = "TOTAL";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(currentRow, 1, currentRow, 3).Merge();
                worksheet.Range(currentRow, 1, currentRow, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Range(currentRow, 1, currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                for (int i = 4; i <= 22; i++)
                {
                    worksheet.Cell(currentRow, i).FormulaA1 = $"SUM({worksheet.Cell(7, i).Address}:{worksheet.Cell(currentRow - 1, i).Address})";
                    worksheet.Cell(currentRow, i).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, i).Style.Font.FontName = "Bahnschrift Light";
                    worksheet.Cell(currentRow, i).Style.NumberFormat.Format = "#,##0.0";
                    worksheet.Cell(currentRow, i).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Cell(currentRow, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                currentRow++;

                // Footer Signature
                worksheet.Cell(currentRow, 1).Value = "@Trust Soft Credit.";
                worksheet.Cell(currentRow, 1).Style.Font.Italic = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 10;
                worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(currentRow, 1, currentRow, 22).Merge();

                // Adjust column widths
                worksheet.Columns().AdjustToContents();

                // Save the file
                workbook.SaveAs(filePath);
            }
        }

        
    }


}