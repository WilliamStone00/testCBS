using BusinessServices;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.BusinessService.AccountingV2.JournalHead
{
    public class JournalHeadExcelExportGenerator : BaseService
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









        
            // ================================================================
            // MAIN EXPORT METHOD
            // ================================================================
            public  void GenerateJournalHeadExcelSheet(
                CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead model,
                string filePath,
                string exportedBy)
            {
                using (var workbook = new XLWorkbook())
                {

                workbook.Style.Font.FontName = "Bahnschrift SemiCondensed";

                var worksheet = workbook.Worksheets.Add("Journal Details");
                    int currentRow = 1;

                    // ============================================================
                    // FETCH BANK + BRANCH INFO (CLAIMS)
                    // ============================================================
                    string bank = GetBankName();
                    string branchCode = GetBranchCode();
                    
                    string branchName = GetBranchName();

                    // ============================================================
                    // HEADER (BANK + BRANCH + EXPORT INFO)
                    // ============================================================
                    currentRow = CreateHeaderSection(
                        worksheet,
                        currentRow,
                        bank,
                        branchCode,
                        branchName,
                        exportedBy
                    );

                    // ============================================================
                    // JOURNAL HEADER SECTION
                    // ============================================================
                    currentRow = CreateJournalHeaderSection(worksheet, currentRow, model, exportedBy);

                    // ============================================================
                    // WORKFLOW TICKETS
                    // ============================================================
                    currentRow = CreateWorkflowTicketsSection(worksheet, currentRow, model);

                    // ============================================================
                    // RECONCILED ENTRIES
                    // ============================================================
                    currentRow = CreateReconciledEntriesSection(worksheet, currentRow, model);

                    // ============================================================
                    // FOOTER
                    // ============================================================
                    CreateFooterSection(worksheet, currentRow);

                    // ============================================================
                    // ADJUST COLUMNS
                    // ============================================================
                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.Width < 10) column.Width = 10;
                        else if (column.Width > 50) column.Width = 50;
                    }

                

                workbook.SaveAs(filePath);
                }
            }

            // ================================================================
            // HEADER (BANK + BRANCH + EXPORT)
            // ================================================================
            private static int CreateHeaderSection(
                IXLWorksheet ws,
                int row,
                string bank,
                string branchCode,
               
                string branchName,
                string exportedBy)
            {
               
                ws.Cell(row, 1).Value = bank;
                ws.Range(row, 1, row, 6).Merge().Style
                    .Font.SetBold()
                    .Font.SetFontSize(18)
                    .Font.SetFontColor(XLColor.White)
                        .Fill.SetBackgroundColor(XLColor.FromArgb(0, 100, 0))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                row++;

                // ROW 2 — BRANCH INFORMATION (GREY)
                ws.Cell(row, 1).Value =
                    $"Branch Name: {branchName}   |   Code: {branchCode}   ";
                ws.Range(row, 1, row, 6).Merge().Style
                    .Font.SetBold()
                     .Font.SetFontColor(XLColor.White)
                     .Fill.SetBackgroundColor(XLColor.FromArgb(70, 130, 180))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                row++;

                

            // ROW 3 — EXPORT INFO
                 ws.Cell(row, 1).Value =
                $"Exported On: {DateTime.Now:dd/MM/yyyy HH:mm}   |   Exported By: {exportedBy}";
                        ws.Range(row, 1, row, 8).Merge();

                        // Apply styles separately
                        ws.Cell(row, 1).Style.Font.Bold = true;
                        ws.Cell(row, 1).Style.Font.SetFontColor(XLColor.Gray);


            return row + 2;
            }

            // ================================================================
            // JOURNAL HEADER (MERGED CELLS)
            // ================================================================
            private static int CreateJournalHeaderSection(
                IXLWorksheet worksheet,
                int currentRow,
                CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead model,
                string exportedBy)
            {
                var sectionTitle = worksheet.Cell(currentRow, 1);
                sectionTitle.Value = " JOURNAL ENTRIES";
            worksheet.Range(currentRow, 1, currentRow, 6).Merge();

            ApplySectionTitleStyle(sectionTitle);
                currentRow += 2;


            worksheet.Cell(currentRow, 1).Value = $"Reference: {model.Reference}";

                currentRow++;

                worksheet.Cell(currentRow, 1).Value = $"Status: {model.Status}";

                currentRow++;

                worksheet.Cell(currentRow, 1).Value = $"Operation Code: {model.OperationCode}";

                currentRow++;

            //worksheet.Cell(currentRow, 1).Value = $"Branch ID: {model.BranchId}";
            //worksheet.Range(currentRow, 1, currentRow, 6).Merge();
            //currentRow++;

            //worksheet.Cell(currentRow, 1).Value = $"Ticket Type: {model.TicketType}";
            //worksheet.Range(currentRow, 1, currentRow, 6).Merge();
            //currentRow++;

            worksheet.Cell(currentRow, 1).Value = $"Accounting Date: {model.AccountingDate:dd/MM/yyyy HH:mm}";
            currentRow++;


            var headerData = new[]
                {
            ("Member Reference", model.MemberReference),
            ("Till Name", model.TillName),
            ("Cashier Name", model.CashierName),
            ("Auxiliary Ref", model.AuxiliaryRef),
            ("Memo ", model.Memo),
            //("Created By", model.CreatedBy)
        };

                foreach (var item in headerData)
                {
                    worksheet.Cell(currentRow, 1).Value = $"{item.Item1}: {item.Item2}";
                  
                    currentRow++;
                }

                return currentRow + 1;
            }

            // ================================================================
            // WORKFLOW TICKETS
            // ================================================================
            private static int CreateWorkflowTicketsSection(
                IXLWorksheet worksheet,
                int currentRow,
                CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead model)
            {
                if (model.WorkflowTickets == null || !model.WorkflowTickets.Any())
                    return currentRow;

                var sectionTitle = worksheet.Cell(currentRow, 1);
                sectionTitle.Value = "🧾 WORKFLOW TICKETS";
               worksheet.Range(currentRow, 1, currentRow, 6).Merge();

              ApplySectionTitleStyle(sectionTitle);
                currentRow += 2;

                var headers = new[] { "Reference", "State", "Remarks", "Opened", "Closed" };
                currentRow = CreateTableHeaderRow(worksheet, currentRow, headers);

                foreach (var ticket in model.WorkflowTickets)
                {
                    worksheet.Cell(currentRow, 1).Value = ticket.Reference;
                    worksheet.Cell(currentRow, 2).Value = ticket.State;
                    worksheet.Cell(currentRow, 3).Value = ticket.Remarks;
                    worksheet.Cell(currentRow, 4).Value = ticket.OpenedAtUtc?.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cell(currentRow, 5).Value = ticket.ClosedAtUtc?.ToString("yyyy-MM-dd HH:mm");

                    ApplyTableCellBorders(worksheet, currentRow, 1, 5);
                    currentRow++;
                }

                return currentRow + 1;
            }

        // ================================================================
        // RECONCILED ENTRIES
        // ================================================================
       private static int CreateReconciledEntriesSection(
    IXLWorksheet worksheet,
    int currentRow,
    CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead model)
{
    if (model.ReconciledLedgerLines == null || !model.ReconciledLedgerLines.Any())
        return currentRow;

    var validReconciled = model.ReconciledLedgerLines
        .Where(r => r.DebitAmount != 0 || r.CreditAmount != 0)
        .OrderBy(r => r.BranchName)
        .ThenBy(r => r.CounterpartyBranchName)
        .ThenBy(r => r.Seq)
        .ToList();

    if (!validReconciled.Any())
        return currentRow;

    // ----- Section Title -----
    var title = worksheet.Cell(currentRow, 1);
    title.Value = "🧾 RECONCILED ENTRIES";
    worksheet.Range(currentRow, 1, currentRow, 6).Merge();
    ApplySectionTitleStyle(title);
    currentRow += 2;

    var headers = new[] { "Account Number", "Account Name", "Description", "Auxiliary Ref", "Debit", "Credit" };

    // ----- Group by branch -----
    var groupedByBranch = validReconciled
        .GroupBy(r => new { r.BranchId, r.BranchName })
        .ToList();

    foreach (var branchGroup in groupedByBranch)
    {
        // Branch header
        var branchHeader = worksheet.Cell(currentRow, 1);
        branchHeader.Value = $"Branch: {model.BranchName} ";
        branchHeader.Style.Font.Bold = true;
        currentRow += 2;

        // Lines without counterparty
        var linesWithoutCp = branchGroup.Where(x => string.IsNullOrWhiteSpace(x.CounterpartyBranchId)).ToList();
        if (linesWithoutCp.Any())
        {
            currentRow = CreateTableHeaderRow(worksheet, currentRow, headers);

            decimal totalDebit = 0, totalCredit = 0;
            foreach (var line in linesWithoutCp)
            {
                worksheet.Cell(currentRow, 1).Value = line.AccountNumber;
                worksheet.Cell(currentRow, 2).Value = line.AccountName;
                worksheet.Cell(currentRow, 3).Value = line.Description;
                worksheet.Cell(currentRow, 4).Value = line.AuxiliaryRef;
                worksheet.Cell(currentRow, 5).Value = Math.Abs(line.DebitAmount);
                worksheet.Cell(currentRow, 6).Value = Math.Abs(line.CreditAmount);

                totalDebit += Math.Abs(line.DebitAmount);
                totalCredit += Math.Abs(line.CreditAmount);

                ApplyTableCellBorders(worksheet, currentRow, 1, 6);
                currentRow++;
            }

            currentRow = CreateTotalsRow(worksheet, currentRow, totalDebit, totalCredit, 6);
            currentRow += 2;
        }

        // Lines with counterparty grouped by counterparty
        var cpGroups = branchGroup
            .Where(x => !string.IsNullOrWhiteSpace(x.CounterpartyBranchId))
            .GroupBy(x => new { x.CounterpartyBranchId, x.CounterpartyBranchName })
            .ToList();

        foreach (var cpGroup in cpGroups)
        {
            // Counterparty header
            var cpHeader = worksheet.Cell(currentRow, 1);
                    cpHeader.Value = $"Counterparty Branch: {model.CounterpartyBranchName} ";
                    cpHeader.Style.Font.Bold = true;
            currentRow += 2;

            currentRow = CreateTableHeaderRow(worksheet, currentRow, headers);

            decimal totalDebitCp = 0, totalCreditCp = 0;
            foreach (var line in cpGroup)
            {
                worksheet.Cell(currentRow, 1).Value = line.AccountNumber;
                worksheet.Cell(currentRow, 2).Value = line.AccountName;
                worksheet.Cell(currentRow, 3).Value = line.Description;
                worksheet.Cell(currentRow, 4).Value = line.AuxiliaryRef;
                worksheet.Cell(currentRow, 5).Value = Math.Abs(line.DebitAmount);
                worksheet.Cell(currentRow, 6).Value = Math.Abs(line.CreditAmount);

                totalDebitCp += Math.Abs(line.DebitAmount);
                totalCreditCp += Math.Abs(line.CreditAmount);

                ApplyTableCellBorders(worksheet, currentRow, 1, 6);
                currentRow++;
            }

            currentRow = CreateTotalsRow(worksheet, currentRow, totalDebitCp, totalCreditCp, 6);
            currentRow += 2;
        }
    }

    return currentRow;
}



        // ================================================================
        // FOOTER
        // ================================================================
        private static void CreateFooterSection(IXLWorksheet worksheet, int row)
            {
                var footerCell = worksheet.Cell(row, 1);
                footerCell.Value = $"Generated on {DateTime.Now:yyyy-MM-dd HH:mm}";
                footerCell.Style.Font.Bold = true;
                footerCell.Style.Font.FontColor = XLColor.Gray;
                worksheet.Range(row, 1, row, 8).Merge();
            }

            // ================================================================
            // HELPER STYLES
            // ================================================================
            private static void ApplySectionTitleStyle(IXLCell cell)
            {
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontSize = 13;
            
            cell.Style.Font.FontColor = XLColor.Black;
            cell.Style.Fill.SetBackgroundColor(XLColor.SkyBlue);

            // LightCoral RGB


        }

        private static int CreateTableHeaderRow(IXLWorksheet worksheet, int row, string[] headers)
            {
                for (int i = 0; i < headers.Length; i++)
                {
                    var headerCell = worksheet.Cell(row, i + 1);
                    headerCell.Value = headers[i];
                    headerCell.Style.Font.Bold = true;
                    //headerCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#f1f1f1");
                headerCell.Style.Fill.SetBackgroundColor(XLColor.LightGreen);
                headerCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                return row + 1;
         }

            private static void ApplyTableCellBorders(IXLWorksheet worksheet, int row, int startColumn, int endColumn)
            {
                for (int col = startColumn; col <= endColumn; col++)
                {
                    worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
            }

            private static int CreateTotalsRow(
                IXLWorksheet worksheet,
                int row,
                decimal totalDebit,
                decimal totalCredit,
                int columnsCount)
            {
                var label = worksheet.Cell(row, columnsCount - 2);
                label.Value = "Totals:";
                label.Style.Font.Bold = true;

                worksheet.Cell(row, columnsCount - 1).Value = totalDebit;
                worksheet.Cell(row, columnsCount).Value = totalCredit;

                for (int col = columnsCount - 2; col <= columnsCount; col++)
                {
                    var cell = worksheet.Cell(row, col);
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fa");
                    cell.Style.Font.Bold = true;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                return row + 1;
            }

            // ================================================================
            // CLAIM ACCESS HELPERS
            // ================================================================
           
    }

}








