using CBS.FrontDesk.Data.Entity.BulkOperation;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.BulkOperations
{
    public class BulkOperationResultDetailExcelGenerator
    {

        public static void GenerateBulkOperationExcel(BulkOperationData bulkOperationData, string branchName, string filePath, string exportDate, string exportedBy)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Bulk Operation");

                // Title
                worksheet.Cell(1, 1).Value = $"BULK OPERATION - {bulkOperationData.SimulationType.ToUpper()} FOR {branchName.ToUpper()}";
                worksheet.Cell(1, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 14;
                worksheet.Range(1, 1, 1, 16).Merge();
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                // Export Details
                worksheet.Cell(2, 1).Value = $"Generated Date: {exportDate} BY {exportedBy}";
                worksheet.Cell(2, 1).Style.Font.Italic = false;
                worksheet.Cell(2, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Cell(2, 1).Style.Font.FontSize = 10;
                worksheet.Range(2, 1, 2, 16).Merge();
                worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                worksheet.Cell(3, 1).Value = $"Operation ID: {bulkOperationData.Id} | Status: {bulkOperationData.ApprovalStatus}";
                worksheet.Cell(3, 1).Style.Font.Italic = false;
                worksheet.Cell(3, 1).Style.Font.FontSize = 10;
                worksheet.Cell(3, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Range(3, 1, 3, 16).Merge();
                worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                // **Empty Row After Title**
                int summaryStartRow = 5;

                // ======================= SUMMARY TABLE =======================
                worksheet.Cell(summaryStartRow, 1).Value = "OPERATION SUMMARY";
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
            "Total Volume", "Total Members", "Bank", "Branch",
            "Created By", "Approval Status", "Approved By", "Approval Date"
        };

                for (int i = 0; i < summaryLabels.Length; i++)
                {
                    int row = summaryStartRow + i + 1;
                    worksheet.Cell(row, 1).Value = summaryLabels[i];
                    worksheet.Cell(row, 1).Style.Font.Bold = false;
                    worksheet.Cell(row, 1).Style.Font.FontName = "Bahnschrift Light";
                    worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    // Set summary values
                    switch (i)
                    {
                        case 0:
                            worksheet.Cell(row, 2).Value = bulkOperationData.TotalVolume;
                            break;
                        case 1:
                            worksheet.Cell(row, 2).Value = bulkOperationData.TotalMembers;
                            break;
                        case 2:
                            worksheet.Cell(row, 2).Value = $"{bulkOperationData.BankName} ({bulkOperationData.BankCode})";
                            break;
                        case 3:
                            worksheet.Cell(row, 2).Value = $"{bulkOperationData.InitiatorBranchName} ({bulkOperationData.InitiatorBranchName})";
                            break;
                        case 4:
                            worksheet.Cell(row, 2).Value = bulkOperationData.CreatedBy;
                            break;
                        case 5:
                            worksheet.Cell(row, 2).Value = bulkOperationData.ApprovalStatus;
                            break;
                        case 6:
                            worksheet.Cell(row, 2).Value = bulkOperationData.ApprovalBy ?? "N/A";
                            break;
                        case 7:
                            worksheet.Cell(row, 2).Value = bulkOperationData.ApprovalValidationDate.ToString("yyyy-MM-dd HH:mm");
                            break;
                    }

                    if (i < 2) // Format numbers
                    {
                        worksheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
                    }
                    worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Cell(row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                // **Empty Row after Summary Table**
                int headersRow = summaryStartRow + summaryLabels.Length + 2;

                // ======================= HEADERS =======================
                var headers = new[]
                {
            "Member Ref", "Member Name", "Source Account", "Source Type", "Source Balance",
            "Destination Account", "Destination Type", "Destination Balance", "Amount",
            "Net Balance", "Status", "Error Message", "Transfer Date", "Approval Status",
            "Approval Date", "Branch"
        };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(headersRow, i + 1).Value = headers[i].ToUpper();
                    worksheet.Cell(headersRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(headersRow, i + 1).Style.Font.FontName = "Bahnschrift Light";
                    worksheet.Cell(headersRow, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Cell(headersRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(headersRow, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                // ======================= DATA =======================
                int currentRow = headersRow + 1;
                foreach (var detail in bulkOperationData.BulkOperationSimulationDetails)
                {
                    worksheet.Cell(currentRow, 1).Value = detail.MemberReference;
                    worksheet.Cell(currentRow, 2).Value = detail.MemberName;
                    worksheet.Cell(currentRow, 3).Value = detail.SourceAccountNumber;
                    worksheet.Cell(currentRow, 4).Value = detail.SourceAccountType;
                    worksheet.Cell(currentRow, 5).Value = detail.SourceAccountBalance;
                    worksheet.Cell(currentRow, 6).Value = detail.DestinationAccountNumber;
                    worksheet.Cell(currentRow, 7).Value = detail.DestinationAccountType;
                    worksheet.Cell(currentRow, 8).Value = detail.DestinationBalance;
                    worksheet.Cell(currentRow, 9).Value = detail.AmountToDebit;
                    worksheet.Cell(currentRow, 10).Value = detail.NetBalance;
                    worksheet.Cell(currentRow, 11).Value = detail.TransferStatus;
                    worksheet.Cell(currentRow, 12).Value = detail.TransferMessage;
                    worksheet.Cell(currentRow, 13).Value = detail.TransferDate.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cell(currentRow, 14).Value = detail.ApprovalStatus;
                    worksheet.Cell(currentRow, 15).Value = detail.ApprovalDate.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cell(currentRow, 16).Value = detail.BranchName;

                    // Apply border and font styles
                    for (int col = 1; col <= 16; col++)
                    {
                        worksheet.Cell(currentRow, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(currentRow, col).Style.Font.FontName = "Bahnschrift Light";
                    }

                    // Format numeric values
                    for (int col = 5; col <= 10; col++)
                    {
                        worksheet.Cell(currentRow, col).Style.NumberFormat.Format = "#,##0.00";
                    }

                    // Highlight row in red if transfer failed
                    if (detail.TransferStatus == "Failed")
                    {
                        worksheet.Row(currentRow).Style.Fill.BackgroundColor = XLColor.Red;
                    }

                    currentRow++;
                }

                // ======================= FOOTER =======================
                worksheet.Cell(currentRow, 1).Value = "TOTAL";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(currentRow, 1, currentRow, 4).Merge();
                worksheet.Range(currentRow, 1, currentRow, 4).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Range(currentRow, 1, currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // Sum relevant columns
                int[] sumColumns = { 5, 8, 9, 10 };
                foreach (var col in sumColumns)
                {
                    worksheet.Cell(currentRow, col).FormulaA1 = $"SUM({worksheet.Cell(headersRow + 1, col).Address}:{worksheet.Cell(currentRow - 1, col).Address})";
                    worksheet.Cell(currentRow, col).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, col).Style.Font.FontName = "Bahnschrift Light";
                    worksheet.Cell(currentRow, col).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell(currentRow, col).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Cell(currentRow, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                currentRow++;

                // Footer Signature
                worksheet.Cell(currentRow, 1).Value = "@Trust Soft Credit.";
                worksheet.Cell(currentRow, 1).Style.Font.Italic = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 10;
                worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(currentRow, 1, currentRow, 16).Merge();

                // Adjust column widths
                worksheet.Columns().AdjustToContents();

                // Save the file
                workbook.SaveAs(filePath);
            }
        }
    }
}
