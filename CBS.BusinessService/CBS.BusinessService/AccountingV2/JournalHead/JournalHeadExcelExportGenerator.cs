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

        public void GenerateJournalHeadExcel(
    WorkflowTicket model,
    string filePath,
    string exportedBy)
        {
            using (var workbook = new XLWorkbook())
            {
                workbook.Style.Font.FontName = "Bahnschrift SemiCondensed";

                var ws = workbook.Worksheets.Add("Journal Head");
                int row = 1;

                // ============================================================
                // FETCH BANK + BRANCH (SAME AS MAIN METHOD)
                // ============================================================
                string bank = GetBankName();
                string branchCode = GetBranchCode();
                string branchName = GetBranchName();

                // ============================================================
                // HEADER SECTION (Same style as first export)
                // ============================================================
                ws.Cell(row, 1).Value = bank;
                ws.Range(row, 1, row, 5).Merge().Style
                    .Font.SetBold()
                    .Font.SetFontSize(18)
                    .Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(XLColor.FromArgb(0, 100, 0))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                row++;

                ws.Cell(row, 1).Value =
                    $"Branch Name: {branchName}   |   Code: {branchCode}";
                ws.Range(row, 1, row, 5).Merge().Style
                    .Font.SetBold()
                    .Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(XLColor.FromArgb(70, 130, 180))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                row++;

                ws.Cell(row, 1).Value =
                    $"Exported On: {DateTime.Now:dd/MM/yyyy HH:mm}   |   Exported By: {exportedBy}";
                ws.Range(row, 1, row, 5).Merge().Style
                    .Font.SetBold()
                    .Font.SetFontColor(XLColor.Gray);
                row += 2;

                // ============================================================
                // JOURNAL HEADER SECTION
                // ============================================================
                var sectionTitle = ws.Cell(row, 1);
                sectionTitle.Value = "🧾 JOURNAL HEADER";
                ws.Range(row, 1, row, 5).Merge();
                ApplySectionTitleStyle(sectionTitle);
                row += 2;

                var headerData = new[]
                {
            ("Reference", model.Reference),
            ("State", model.State),
            ("Operation Code", model.OperationCode),
            ("Branch ID", model.BranchId),
            ("Ticket Type", model.TicketType),
            ("Accounting Date", model.AccountingDate.ToString("dd/MM/yyyy HH:mm")),
            ("Remarks", model.Remarks)
        };

                foreach (var item in headerData)
                {
                    ws.Cell(row, 1).Value = $"{item.Item1}: {item.Item2}";
                    row++;
                }

                row += 1;

                // ============================================================
                // JOURNAL LINES SECTION
                // ============================================================
                var title2 = ws.Cell(row, 1);
                title2.Value = "🧾 JOURNAL LINES";
                ws.Range(row, 1, row, 5).Merge();
                ApplySectionTitleStyle(title2);
                row += 2;

                var headers = new[] { "Account Number", "Account Name", "Description", "Debit", "Credit" };
                row = CreateTableHeaderRow(ws, row, headers);

                decimal totalDebit = 0;
                decimal totalCredit = 0;

                if (model.JournalLines != null && model.JournalLines.Any())
                {
                    foreach (var line in model.JournalLines)
                    {
                        var debit = line.DrCr?.ToLower() == "debit" ? Math.Abs(line.Amount) : 0;
                        var credit = line.DrCr?.ToLower() == "credit" ? Math.Abs(line.Amount) : 0;

                        ws.Cell(row, 1).Value = line.AccountNumber;
                        ws.Cell(row, 2).Value = line.AccountName;
                        ws.Cell(row, 3).Value = line.Description;

                        ws.Cell(row, 4).Value = debit;
                        ws.Cell(row, 5).Value = credit;

                        // 📌 FORMAT AS 1,000,000
                        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                        ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0";

                        ApplyTableCellBorders(ws, row, 1, 5);

                        totalDebit += debit;
                        totalCredit += credit;

                        row++;
                    }


                    row = CreateTotalsRow(ws, row, totalDebit, totalCredit, 5);
                    row++;
                }
                else
                {
                    ws.Cell(row, 1).Value = "No journal lines found.";
                    ws.Range(row, 1, row, 5).Merge().Style.Font.Italic = true;
                    row++;
                }

                // ============================================================
                // FOOTER
                // ============================================================
                var footer = ws.Cell(row, 1);
                footer.Value = $"Generated on {DateTime.Now:yyyy-MM-dd HH:mm}";
                footer.Style.Font.Bold = true;
                footer.Style.Font.FontColor = XLColor.Gray;
                ws.Range(row, 1, row, 6).Merge();

                // ============================================================
                // AUTO SIZING
                // ============================================================
                ws.Columns().AdjustToContents();
                foreach (var column in ws.ColumnsUsed())
                {
                    if (column.Width < 10) column.Width = 10;
                    else if (column.Width > 50) column.Width = 50;
                }

                workbook.SaveAs(filePath);
            }
        }










        // ================================================================
        // MAIN EXPORT METHOD
        // ================================================================
        // ======================================================================
        // MAIN EXPORT METHOD (WITH OPTION C)
        // ======================================================================
        public void GenerateJournalHeadExcelSheet(
                CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead model,
                string filePath,
                string exportedBy)
        {
            using (var workbook = new XLWorkbook())
            {
                workbook.Style.Font.FontName = "Bahnschrift SemiCondensed";

                var worksheet = workbook.Worksheets.Add("Journal Details");
                int currentRow = 1;

                // ----------------------------------------------------------------
                // DYNAMIC COLUMN COUNT (OPTION C)
                // ----------------------------------------------------------------
                int maxColumns = 0;

                if (model.WorkflowTickets?.Any() == true)
                    maxColumns = Math.Max(maxColumns, 5);

                if (model.Lines?.Any() == true)
                    maxColumns = Math.Max(maxColumns, 5);

                if (model.ReconciledLedgerLines?.Any() == true)
                    maxColumns = Math.Max(maxColumns, 6);

                if (maxColumns == 0)
                    maxColumns = 5; // default

                // ----------------------------------------------------------------
                // FETCH BRANCH DETAILS
                // ----------------------------------------------------------------
                string bank = GetBankName();
                string branchCode = GetBranchCode();
                string branchName = GetBranchName();

                // ----------------------------------------------------------------
                // HEADER (BANK + BRANCH + EXPORT INFO)
                // ----------------------------------------------------------------
                currentRow = CreateHeaderSection(
                    worksheet,
                    currentRow,
                    bank,
                    branchCode,
                    branchName,
                    exportedBy,
                    maxColumns
                );

                // ----------------------------------------------------------------
                // JOURNAL HEADER SECTION
                // ----------------------------------------------------------------
                currentRow = CreateJournalHeaderSection(worksheet, currentRow, model, exportedBy, maxColumns);

                // ----------------------------------------------------------------
                // WORKFLOW TICKETS
                // ----------------------------------------------------------------
                currentRow = CreateWorkflowTicketsSection(worksheet, currentRow, model, maxColumns);

                // ----------------------------------------------------------------
                // RECONCILED ENTRIES
                // ----------------------------------------------------------------
                currentRow = CreateReconciledEntriesSection(worksheet, currentRow, model, maxColumns);

                // ----------------------------------------------------------------
                // JOURNAL LINES (ONLY IF NO RECONCILED ENTRIES)
                // ----------------------------------------------------------------
                if (model.ReconciledLedgerLines == null || !model.ReconciledLedgerLines.Any())
                {
                    currentRow = CreateJournalLinesSection(worksheet, currentRow, model, maxColumns);
                }

                // ----------------------------------------------------------------
                // AUTO-FIT COLUMNS
                // ----------------------------------------------------------------
                worksheet.Columns().AdjustToContents();
                foreach (var column in worksheet.ColumnsUsed())
                {
                    if (column.Width < 10) column.Width = 10;
                    else if (column.Width > 50) column.Width = 50;
                }

                workbook.SaveAs(filePath);
            }
        }



        // ======================================================================
        // HEADER (Option C - Dynamic maxColumns)
        // ======================================================================
        private static int CreateHeaderSection(
            IXLWorksheet ws,
            int row,
            string bank,
            string branchCode,
            string branchName,
            string exportedBy,
            int maxColumns)
        {
            // --------------------------- ROW 1 — BANK
            ws.Cell(row, 1).Value = bank;
            ws.Range(row, 1, row, maxColumns).Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(18)
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.FromArgb(0, 100, 0))
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            row++;

            // --------------------------- ROW 2 — BRANCH
            ws.Cell(row, 1).Value =
                $"Branch Name: {branchName}   |   Code: {branchCode}";
            ws.Range(row, 1, row, maxColumns).Merge().Style
                .Font.SetBold()
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.FromArgb(70, 130, 180))
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            row++;

            // --------------------------- ROW 3 — EXPORT INFO
            ws.Cell(row, 1).Value =
                $"Exported On: {DateTime.Now:dd/MM/yyyy HH:mm}   |   Exported By: {exportedBy}";
            ws.Range(row, 1, row, maxColumns).Merge();

            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.SetFontColor(XLColor.Gray);

            return row + 2;
        }



        // ======================================================================
        // JOURNAL HEADER SECTION
        // ======================================================================
        private static int CreateJournalHeaderSection(
            IXLWorksheet worksheet,
            int currentRow,
            CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead model,
            string exportedBy,
            int maxColumns)
        {
            var sectionTitle = worksheet.Cell(currentRow, 1);
            sectionTitle.Value = "🧾  JOURNAL ENTRIES REPORT";
            worksheet.Range(currentRow, 1, currentRow, maxColumns).Merge();
            ApplySectionTitleStyle(sectionTitle);

            currentRow += 2;

            // BASIC INFO
            worksheet.Cell(currentRow++, 1).Value = $"Reference: {model.Reference}";
            worksheet.Cell(currentRow++, 1).Value = $"Status: {model.Status}";
            worksheet.Cell(currentRow++, 1).Value = $"Branch: {model.BranchName}";
            worksheet.Cell(currentRow++, 1).Value = $"Operation Code: {model.OperationCode}";
            worksheet.Cell(currentRow++, 1).Value = $"Accounting Date: {model.AccountingDate:dd/MM/yyyy HH:mm}";

            var headerData = new[]
            {
        ("Member Reference", model.MemberReference),
        ("Till Name", model.TillName),
        ("Cashier Name", model.CashierName),
        ("Auxiliary Ref", model.AuxiliaryRef),
        ("Memo", model.Memo)
    };

            foreach (var item in headerData)
            {
                worksheet.Cell(currentRow, 1).Value = $"{item.Item1}: {item.Item2}";
                currentRow++;
            }

            return currentRow + 1;
        }



        // ======================================================================
        // WORKFLOW TICKETS (Dynamic Columns Applied)
        // ======================================================================
        private static int CreateWorkflowTicketsSection(
            IXLWorksheet worksheet,
            int currentRow,
            CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead model,
            int maxColumns)
        {
            if (model.WorkflowTickets == null || !model.WorkflowTickets.Any())
                return currentRow;

            var title = worksheet.Cell(currentRow, 1);
            title.Value = "🧾 WORKFLOW TICKETS";
            worksheet.Range(currentRow, 1, currentRow, maxColumns).Merge();
            ApplySectionTitleStyle(title);

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



        // ======================================================================
        // RECONCILED ENTRIES (Dynamic maxColumns)
        // ======================================================================
        private static int CreateReconciledEntriesSection(
            IXLWorksheet worksheet,
            int currentRow,
            CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead model,
            int maxColumns)
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

            var title = worksheet.Cell(currentRow, 1);
            title.Value = "🧾 RECONCILED JOURNAL ENTRIES";
            worksheet.Range(currentRow, 1, currentRow, maxColumns).Merge();
            ApplySectionTitleStyle(title);

            currentRow += 1;

            var headers = new[] { "Account Number", "Account Name", "Description", "Auxiliary Ref", "Debit", "Credit" };

            // GROUP BY BRANCH
            var groupedByBranch = validReconciled
                .GroupBy(r => new { r.BranchId, r.BranchName })
                .ToList();

            foreach (var branchGroup in groupedByBranch)
            {
                worksheet.Cell(currentRow++, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow++, 1).Value = $"Branch: {branchGroup.Key.BranchName}";

                // LINES WITHOUT COUNTERPARTY
                var noCp = branchGroup.Where(x => string.IsNullOrWhiteSpace(x.CounterpartyBranchId)).ToList();
                if (noCp.Any())
                {
                    currentRow = CreateTableHeaderRow(worksheet, currentRow, headers);

                    decimal totalDr = 0, totalCr = 0;

                    foreach (var line in noCp)
                    {
                        worksheet.Cell(currentRow, 1).Value = line.AccountNumber;
                        worksheet.Cell(currentRow, 2).Value = line.AccountName;
                        worksheet.Cell(currentRow, 3).Value = line.Description;
                        worksheet.Cell(currentRow, 4).Value = line.AuxiliaryRef;

                        worksheet.Cell(currentRow, 5).Value = Math.Abs(line.DebitAmount);
                        worksheet.Cell(currentRow, 6).Value = Math.Abs(line.CreditAmount);

                        worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0";

                        totalDr += Math.Abs(line.DebitAmount);
                        totalCr += Math.Abs(line.CreditAmount);

                        ApplyTableCellBorders(worksheet, currentRow, 1, 6);
                        currentRow++;
                    }

                    currentRow = CreateTotalsRow(worksheet, currentRow, totalDr, totalCr, 6);
                    currentRow += 2;
                }

                // LINES WITH COUNTERPARTY
                var cpGroups = branchGroup
                    .Where(x => !string.IsNullOrWhiteSpace(x.CounterpartyBranchId))
                    .GroupBy(x => new { x.CounterpartyBranchId, x.CounterpartyBranchName })
                    .ToList();

                foreach (var cpGroup in cpGroups)
                {
                    worksheet.Cell(currentRow++, 1).Style.Font.Bold = true;
                    worksheet.Cell(currentRow++, 1).Value = $"Counterparty Branch: {cpGroup.Key.CounterpartyBranchName}";

                    currentRow = CreateTableHeaderRow(worksheet, currentRow, headers);

                    decimal totalDr2 = 0, totalCr2 = 0;

                    foreach (var line in cpGroup)
                    {
                        worksheet.Cell(currentRow, 1).Value = line.AccountNumber;
                        worksheet.Cell(currentRow, 2).Value = line.AccountName;
                        worksheet.Cell(currentRow, 3).Value = line.Description;
                        worksheet.Cell(currentRow, 4).Value = line.AuxiliaryRef;

                        worksheet.Cell(currentRow, 5).Value = Math.Abs(line.DebitAmount);
                        worksheet.Cell(currentRow, 6).Value = Math.Abs(line.CreditAmount);

                        worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0";

                        totalDr2 += Math.Abs(line.DebitAmount);
                        totalCr2 += Math.Abs(line.CreditAmount);

                        ApplyTableCellBorders(worksheet, currentRow, 1, 6);
                        currentRow++;
                    }

                    currentRow = CreateTotalsRow(worksheet, currentRow, totalDr2, totalCr2, 6);
                    currentRow += 2;
                }
            }

            return currentRow;
        }



        // ======================================================================
        // JOURNAL LINES (Dynamic Columns)
        // ======================================================================
        private static int CreateJournalLinesSection(
            IXLWorksheet worksheet,
            int currentRow,
            CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead model,
            int maxColumns)
        {
            if (model.Lines == null || !model.Lines.Any())
                return currentRow;

            var validLines = model.Lines
                .Where(l => !string.IsNullOrWhiteSpace(l.AccountName) || l.Amount != 0)
                .OrderBy(l => l.BranchName)
                .ThenBy(l => l.CounterpartyBranchName)
                .ToList();

            if (!validLines.Any())
                return currentRow;

            var title = worksheet.Cell(currentRow, 1);
            title.Value = "🧾 JOURNAL LINE ENTRIES (UNRECONCILED)";
            worksheet.Range(currentRow, 1, currentRow, maxColumns).Merge();
            ApplySectionTitleStyle(title);

            currentRow += 2;

            var headers = new[] { "Account Number", "Account Name", "Description", "Debit", "Credit" };

            // GROUP BY BRANCH
            var groupedByBranch = validLines
                .GroupBy(l => new { l.BranchId, l.BranchName })
                .ToList();

            foreach (var branchGroup in groupedByBranch)
            {
                worksheet.Cell(currentRow++, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow++, 1).Value = $"Branch: {branchGroup.Key.BranchName}";

                // ------------------- NO COUNTERPARTY
                var noCp = branchGroup.Where(x => string.IsNullOrWhiteSpace(x.CounterpartyBranchId)).ToList();

                if (noCp.Any())
                {
                    currentRow = CreateTableHeaderRow(worksheet, currentRow, headers);

                    decimal dr = 0, cr = 0;

                    foreach (var line in noCp)
                    {
                        var debit = line.DrCr?.ToLower() == "debit" ? Math.Abs(line.Amount) : 0;
                        var credit = line.DrCr?.ToLower() == "credit" ? Math.Abs(line.Amount) : 0;

                        worksheet.Cell(currentRow, 1).Value = line.AccountNumber;
                        worksheet.Cell(currentRow, 2).Value = line.AccountName;
                        worksheet.Cell(currentRow, 3).Value = line.Description;
                        worksheet.Cell(currentRow, 4).Value = debit;
                        worksheet.Cell(currentRow, 5).Value = credit;

                        worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";

                        dr += debit;
                        cr += credit;

                        ApplyTableCellBorders(worksheet, currentRow, 1, 5);
                        currentRow++;
                    }

                    currentRow = CreateTotalsRow(worksheet, currentRow, dr, cr, 5);
                    currentRow += 2;
                }

                // ------------------- COUNTERPARTY
                var cpGroups = branchGroup
                    .Where(x => !string.IsNullOrWhiteSpace(x.CounterpartyBranchId))
                    .GroupBy(x => new { x.CounterpartyBranchId, x.CounterpartyBranchName })
                    .ToList();

                foreach (var cpGroup in cpGroups)
                {
                    worksheet.Cell(currentRow++, 1).Style.Font.Bold = true;
                    worksheet.Cell(currentRow++, 1).Value = $"Counterparty Branch: {cpGroup.Key.CounterpartyBranchName}";

                    currentRow = CreateTableHeaderRow(worksheet, currentRow, headers);

                    decimal dr2 = 0, cr2 = 0;

                    foreach (var line in cpGroup)
                    {
                        var debit = line.DrCr?.ToLower() == "debit" ? Math.Abs(line.Amount) : 0;
                        var credit = line.DrCr?.ToLower() == "credit" ? Math.Abs(line.Amount) : 0;

                        worksheet.Cell(currentRow, 1).Value = line.AccountNumber;
                        worksheet.Cell(currentRow, 2).Value = line.AccountName;
                        worksheet.Cell(currentRow, 3).Value = line.Description;
                        worksheet.Cell(currentRow, 4).Value = debit;
                        worksheet.Cell(currentRow, 5).Value = credit;

                        worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";

                        dr2 += debit;
                        cr2 += credit;

                        ApplyTableCellBorders(worksheet, currentRow, 1, 5);
                        currentRow++;
                    }

                    currentRow = CreateTotalsRow(worksheet, currentRow, dr2, cr2, 5);
                    currentRow += 2;
                }
            }

            return currentRow;
        }



        // ======================================================================
        // HELPER STYLES + TABLES
        // ======================================================================
        private static void ApplySectionTitleStyle(IXLCell cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontSize = 13;
            cell.Style.Font.FontColor = XLColor.Black;
            cell.Style.Fill.SetBackgroundColor(XLColor.SkyBlue);
        }

        private static int CreateTableHeaderRow(IXLWorksheet worksheet, int row, string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                var headerCell = worksheet.Cell(row, i + 1);
                headerCell.Value = headers[i];
                headerCell.Style.Font.Bold = true;
                headerCell.Style.Fill.SetBackgroundColor(XLColor.LightGreen);
                headerCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            return row + 1;
        }

        private static void ApplyTableCellBorders(IXLWorksheet worksheet, int row, int startColumn, int endColumn)
        {
            for (int col = startColumn; col <= endColumn; col++)
                worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
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

            var debitCell = worksheet.Cell(row, columnsCount - 1);
            var creditCell = worksheet.Cell(row, columnsCount);

            debitCell.Value = totalDebit;
            creditCell.Value = totalCredit;

            debitCell.Style.NumberFormat.Format = "#,##0";
            creditCell.Style.NumberFormat.Format = "#,##0";

            for (int col = columnsCount - 2; col <= columnsCount; col++)
            {
                var cell = worksheet.Cell(row, col);
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fa");
                cell.Style.Font.Bold = true;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            return row + 1;
        }


    }
}








