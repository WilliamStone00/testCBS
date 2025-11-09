using CBS.FrontDesk.Data.Entity.AccountingV2;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.JournalHead
{
    public class JournalHeadExcelExportGenerator
    {

        public static void GenerateJournalHeadExcel(WorkflowTicket model, string filePath, string exportedBy)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Journal Head");

                // ===== Title =====
                ws.Cell(1, 1).Value = $"JOURNAL ENTRY - {model.Reference}";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 14;
                ws.Range(1, 1, 1, 6).Merge();
                ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                // ===== Meta Info =====
                ws.Cell(2, 1).Value = $"Exported On: {DateTime.Now:dd/MM/yyyy HH:mm}  |  Exported By: {exportedBy}";
                ws.Range(2, 1, 2, 6).Merge();
                ws.Cell(2, 1).Style.Font.Italic = true;

                ws.Cell(3, 1).Value = $"State: {model.State}";
                ws.Range(3, 1, 3, 6).Merge();

                ws.Cell(4, 1).Value = $"Operation Code: {model.OperationCode}";
                ws.Range(4, 1, 4, 6).Merge();

                ws.Cell(5, 1).Value = $"Branch ID: {model.BranchId}";
                ws.Range(5, 1, 5, 6).Merge();

                ws.Cell(6, 1).Value = $"Ticket Type: {model.TicketType}";
                ws.Range(6, 1, 6, 6).Merge();

                ws.Cell(7, 1).Value = $"Accounting Date: {model.AccountingDate:dd/MM/yyyy HH:mm}";
                ws.Range(7, 1, 7, 6).Merge();

                ws.Cell(8, 1).Value = $"Remarks: {model.Remarks}";
                ws.Range(8, 1, 8, 6).Merge();

                int headerRow = 10;

                // ===== Table Header =====
                var headers = new[] { "Account Number", "Account Name", "Description", "Debit (FCFA)", "Credit (FCFA)" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cell(headerRow, i + 1).Value = headers[i];
                    ws.Cell(headerRow, i + 1).Style.Font.Bold = true;
                    ws.Cell(headerRow, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    ws.Cell(headerRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(headerRow, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                // ===== Table Data =====
                int currentRow = headerRow + 1;
                decimal totalDebit = 0;
                decimal totalCredit = 0;

                if (model.JournalLines != null && model.JournalLines.Any())
                {
                    foreach (var line in model.JournalLines)
                    {
                        var debit = line.DrCr?.ToLower() == "debit" ? Math.Abs(line.Amount) : 0;
                        var credit = line.DrCr?.ToLower() == "credit" ? Math.Abs(line.Amount) : 0;

                        ws.Cell(currentRow, 1).Value = line.AccountNumber;
                        ws.Cell(currentRow, 2).Value = line.AccountName;
                        ws.Cell(currentRow, 3).Value = line.Description;
                        ws.Cell(currentRow, 4).Value = debit;
                        ws.Cell(currentRow, 5).Value = credit;

                        for (int c = 1; c <= 5; c++)
                            ws.Cell(currentRow, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        totalDebit += debit;
                        totalCredit += credit;

                        currentRow++;
                    }

                    // ===== Totals Row =====
                    ws.Cell(currentRow, 1).Value = "TOTALS";
                    ws.Range(currentRow, 1, currentRow, 3).Merge();
                    ws.Cell(currentRow, 4).Value = totalDebit;
                    ws.Cell(currentRow, 5).Value = totalCredit;
                    ws.Range(currentRow, 1, currentRow, 5).Style.Font.Bold = true;
                    ws.Range(currentRow, 1, currentRow, 5).Style.Fill.BackgroundColor = XLColor.LightGray;
                }
                else
                {
                    ws.Cell(currentRow, 1).Value = "No journal lines found.";
                    ws.Range(currentRow, 1, currentRow, 5).Merge();
                    ws.Cell(currentRow, 1).Style.Font.Italic = true;
                }

                ws.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
        }
    


      public static void GenerateJournalHeadExcelSheet(CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead model, string filePath, string exportedBy)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Journal Head");

                // ===== Title =====
                ws.Cell(1, 1).Value = $"JOURNAL ENTRY - {model.Reference}";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 14;
                ws.Range(1, 1, 1, 6).Merge();
                ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                // ===== Meta Info =====
                ws.Cell(2, 1).Value = $"Exported On: {DateTime.Now:dd/MM/yyyy HH:mm}  |  Exported By: {exportedBy}";
                ws.Range(2, 1, 2, 6).Merge();
                ws.Cell(2, 1).Style.Font.Italic = true;

                ws.Cell(3, 1).Value = $"Status: {model.Status}";
                ws.Range(3, 1, 3, 6).Merge();

                ws.Cell(4, 1).Value = $"Operation Code: {model.OperationCode}";
                ws.Range(4, 1, 4, 6).Merge();

                ws.Cell(5, 1).Value = $"Branch Name: {model.BranchName}";
                ws.Range(5, 1, 5, 6).Merge();

                ws.Cell(6, 1).Value = $"Created By : {model.CreatedBy}";
                ws.Range(6, 1, 6, 6).Merge();

                ws.Cell(7, 1).Value = $"Accounting Date: {model.AccountingDate:dd/MM/yyyy HH:mm}";
                ws.Range(7, 1, 7, 6).Merge();
                ws.Cell(8, 1).Value = $"Memo: {model.Memo}";
                ws.Range(8, 1, 8, 6).Merge();


                int headerRow = 10;

                // ===== Table Header =====
                var headers = new[] { "Account Number", "Account Name", "Description", "Debit (FCFA)", "Credit (FCFA)" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cell(headerRow, i + 1).Value = headers[i];
                    ws.Cell(headerRow, i + 1).Style.Font.Bold = true;
                    ws.Cell(headerRow, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    ws.Cell(headerRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(headerRow, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                // ===== Table Data =====
                int currentRow = headerRow + 1;
                decimal totalDebit = 0;
                decimal totalCredit = 0;

                if (model.Lines != null && model.Lines.Any())
                {
                    foreach (var line in model.Lines)
                    {
                        var debit = line.DrCr?.ToLower() == "debit" ? Math.Abs(line.Amount) : 0;
                        var credit = line.DrCr?.ToLower() == "credit" ? Math.Abs(line.Amount) : 0;

                        ws.Cell(currentRow, 1).Value = line.AccountNumber;
                        ws.Cell(currentRow, 2).Value = line.AccountName;
                        ws.Cell(currentRow, 3).Value = line.Description;
                        ws.Cell(currentRow, 4).Value = debit;
                        ws.Cell(currentRow, 5).Value = credit;

                        for (int c = 1; c <= 5; c++)
                            ws.Cell(currentRow, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        totalDebit += debit;
                        totalCredit += credit;

                        currentRow++;
                    }

                    // ===== Totals Row =====
                    ws.Cell(currentRow, 1).Value = "TOTALS";
                    ws.Range(currentRow, 1, currentRow, 3).Merge();
                    ws.Cell(currentRow, 4).Value = totalDebit;
                    ws.Cell(currentRow, 5).Value = totalCredit;
                    ws.Range(currentRow, 1, currentRow, 5).Style.Font.Bold = true;
                    ws.Range(currentRow, 1, currentRow, 5).Style.Fill.BackgroundColor = XLColor.LightGray;
                }
                else
                {
                    ws.Cell(currentRow, 1).Value = "No journal lines found.";
                    ws.Range(currentRow, 1, currentRow, 5).Merge();
                    ws.Cell(currentRow, 1).Style.Font.Italic = true;
                }

                ws.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
      }
    }
}
