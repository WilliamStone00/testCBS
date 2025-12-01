using BusinessServices;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using ClosedXML.Excel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.BusinessService.AccountingV2.JournalHead
{
    public class JournalHeadExcelExportGenerator : BaseService
    {

        public void GenerateJournalHeadExcel(WorkflowTicket model,string filePath, string exportedBy)
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
        public void GenerateJournalHeadExcelSheet(CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead model,string filePath,
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
    public class JournalDataTableExcelExportGenerator : BaseService
    {
        private static readonly string[] JournalStages = new[] { "RECEIVED", "VALIDATED", "APPROVED", "POSTED", "RECONCILED" };
        private static readonly string[] JournalStates = new[] { "PENDING", "COMPLETED", "REJECTED", "CANCELLED" };

        public static List<CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead> ConvertToJournalData(List<CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead> tableData)
        {
            var journalList = new List<CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead>();

            if (tableData == null || !tableData.Any())
            {
                Console.WriteLine("No table data provided for conversion");
                return journalList;
            }

            Console.WriteLine($"Converting {tableData.Count} journal records");

            foreach (var item in tableData)
            {
                try
                {
                    var totalDebit = GetSafeDecimal(item, "TotalDebit", "totalDebit") ?? 0m;
                    var totalCredit = GetSafeDecimal(item, "TotalCredit", "totalCredit") ?? 0m;
                    var isBalancedFromSource = GetSafeBool(item, "IsBalanced", "isBalanced") ?? false;

                    // Calculate actual balance status based on debit/credit equality
                    var isActuallyBalanced = totalDebit == totalCredit;

                    var journal = new CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead
                    {
                        Id = GetSafeString(item, "Id", "id") ?? Guid.NewGuid().ToString(),
                        Reference = GetSafeString(item, "Reference", "reference") ?? "N/A",
                        Narrative = GetSafeString(item, "Narrative", "narrative"),
                        AccountingDate = ParseDateTime(GetPropertyValue(item, "AccountingDate", "accountingDate")) ?? DateTime.Now,
                        CreatedAt = ParseDateTime(GetPropertyValue(item, "CreatedAt", "createdAt")) ?? DateTime.Now,
                        IsBalanced = isActuallyBalanced, // Use calculated value instead of source value
                        ExternalOperationType = GetSafeString(item, "ExternalOperationType", "externalOperationType"),
                        PostMode = GetSafeString(item, "PostMode", "postMode"),
                        OperationCode = GetSafeString(item, "OperationCode", "operationCode") ?? "N/A",
                        BranchId = GetSafeString(item, "BranchId", "branchId") ?? "N/A",
                        BranchName = GetSafeString(item, "BranchName", "branchName") ?? "N/A",
                        Memo = GetSafeString(item, "Memo", "memo"),
                        Stage = GetSafeString(item, "Stage", "stage") ?? "RECEIVED",
                        State = GetSafeString(item, "State", "state") ?? "PENDING",
                        RequiresWorkflow = GetSafeBool(item, "RequiresWorkflow", "requiresWorkflow") ?? false,
                        RequiresDestinationApproval = GetSafeBool(item, "RequiresDestinationApproval", "requiresDestinationApproval") ?? false,
                        IsCashOperation = GetSafeBool(item, "IsCashOperation", "isCashOperation") ?? false,
                        MemberReference = GetSafeString(item, "MemberReference", "memberReference"),
                        TillName = GetSafeString(item, "TillName", "tillName"),
                        CashierName = GetSafeString(item, "CashierName", "cashierName"),
                        CashDenomsJson = GetSafeString(item, "CashDenomsJson", "cashDenomsJson"),
                        CashTillId = GetSafeString(item, "CashTillId", "cashTillId"),
                        CashDenomsTotal = GetSafeDecimal(item, "CashDenomsTotal", "cashDenomsTotal") ?? 0m,
                        ClosedAtUtc = ParseDateTime(GetPropertyValue(item, "ClosedAtUtc", "closedAtUtc")),
                        TicketType = GetSafeString(item, "TicketType", "ticketType"),
                        Lines = GetJournalLines(item),
                        Status = GetSafeString(item, "Status", "status") ?? "RECEIVED",
                        TotalDebit = totalDebit,
                        TotalCredit = totalCredit,
                        CorrelationId = GetSafeString(item, "CorrelationId", "correlationId"),
                        CounterpartyBranchId = GetSafeString(item, "CounterpartyBranchId", "counterpartyBranchId"),
                        CounterpartyBranchName = GetSafeString(item, "CounterpartyBranchName", "counterpartyBranchName"),
                        HeadOfficeBranchId = GetSafeString(item, "HeadOfficeBranchId", "headOfficeBranchId"),
                        AuxiliaryRef = GetSafeString(item, "AuxiliaryRef", "auxiliaryRef"),
                        IsInterBranch = GetSafeBool(item, "IsInterBranch", "isInterBranch") ?? false,
                        TicketSource = GetSafeString(item, "TicketSource", "ticketSource"),
                        OpenedAtUtc = ParseDateTime(GetPropertyValue(item, "OpenedAtUtc", "openedAtUtc")) ?? DateTime.UtcNow,
                        ModifiedBy = GetSafeString(item, "ModifiedBy", "modifiedBy"),
                        WorkflowTicketNotes = GetSafeString(item, "WorkflowTicketNotes", "workflowTicketNotes"),
                        CreatedBy = GetSafeString(item, "CreatedBy", "createdBy") ?? "System"
                    };

                    journalList.Add(journal);
                    Console.WriteLine($"Successfully converted journal: {journal.Reference} - {journal.BranchName} - Balanced: {journal.IsBalanced}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting journal data: {ex.Message}");
                    try
                    {
                        string itemJson = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                        Console.WriteLine($"Problematic item: {itemJson}");
                    }
                    catch
                    {
                        Console.WriteLine("Could not serialize problematic item");
                    }
                }
            }

            Console.WriteLine($"Successfully converted {journalList.Count} out of {tableData.Count} journal records");
            Console.WriteLine($"Balance statistics: {journalList.Count(x => x.IsBalanced)} balanced, {journalList.Count(x => !x.IsBalanced)} unbalanced");
            return journalList;
        }

        public void GenerateJournalExcelFromTableData(List<CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead> journalData, string filePath, string exportedBy, ExportOptions exportOptions)
        {
            string bank = GetBankName();
            string branchcode = GetBranchCode();
            string branchid = GetBranchID();
            string branchname = GetBranchName();

            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                // ===== SHEET 1: SUMMARY & OVERVIEW =====
                CreateSummarySheet(package, journalData, bank, branchcode, branchid, branchname, exportedBy, exportOptions);

                // ===== SHEET 2: JOURNAL ENTRIES DETAILS =====
                CreateJournalDetailsSheet(package, journalData, exportedBy, exportOptions);

                // ===== SHEETS FOR EACH BRANCH: Create individual sheets for each branch =====
                var branches = journalData.GroupBy(x => new { x.BranchId, x.BranchName })
                                       .Select(g => new { BranchId = g.Key.BranchId, BranchName = g.Key.BranchName })
                                       .ToList();

                foreach (var branch in branches)
                {
                    var branchData = journalData.Where(x => x.BranchId == branch.BranchId && x.BranchName == branch.BranchName).ToList();
                    CreateBranchSheet(package, branchData, branch.BranchId, branch.BranchName, exportedBy, exportOptions);
                }

                // ===== SHEET FOR CASH OPERATIONS =====
                var cashOperations = journalData.Where(x => x.IsCashOperation).ToList();
                if (cashOperations.Any())
                {
                    CreateCashOperationsSheet(package, cashOperations, exportedBy, exportOptions);
                }

                // Save the file
                package.SaveAs(new FileInfo(filePath));

                Console.WriteLine($"Excel file generated successfully with {journalData.Count} journal records across {branches.Count} branches");
                Console.WriteLine($"File saved to: {filePath}");
            }
        }

        private void CreateSummarySheet(ExcelPackage package, List<CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead> journalData, string bank, string branchcode, string branchid, string branchname, string exportedBy, ExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Summary & Overview");

            // Set font
            var fontName = "Bahnschrift SemiCondensed";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            // Summary sheet uses the widest table (3 columns for summary tables)
            int headerColumns = 3;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, bank, branchcode, branchid, branchname, exportedBy, exportOptions, "JOURNAL HEADER SUMMARY REPORT", headerEndColumn);

            // ===== GENERAL SUMMARY SECTION =====
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "GENERAL SUMMARY";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            currentRow += 2;

            // Calculate summary statistics
            var totalJournals = journalData.Count;
            var totalBranches = journalData.Select(x => new { x.BranchId, x.BranchName }).Distinct().Count();
            var balancedJournals = journalData.Count(x => x.IsBalanced);
            var unbalancedJournals = journalData.Count(x => !x.IsBalanced);
            var totalDebit = journalData.Sum(x => x.TotalDebit);
            var totalCredit = journalData.Sum(x => x.TotalCredit);
            var cashOperations = journalData.Count(x => x.IsCashOperation);
            var interBranchOperations = journalData.Count(x => x.IsInterBranch);
            var workflowRequired = journalData.Count(x => x.RequiresWorkflow);

            // Stage distribution
            var stageSummary = journalData.GroupBy(x => x.Stage)
                                        .Select(g => new { Stage = g.Key, Count = g.Count() })
                                        .OrderByDescending(x => x.Count)
                                        .ToList();

            // State distribution
            var stateSummary = journalData.GroupBy(x => x.State)
                                        .Select(g => new { State = g.Key, Count = g.Count() })
                                        .OrderByDescending(x => x.Count)
                                        .ToList();

            // Summary table headers
            var summaryHeaders = new[] { "Metric", "Value", "Description" };
            for (int i = 0; i < summaryHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, 1 + i].Value = summaryHeaders[i];
                worksheet.Cells[currentRow, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[currentRow, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            // Summary data
            var summaryData = new[]
            {
        new { Metric = "Total Journal Entries", Value = totalJournals.ToString("N0"), Description = "Number of journal entries" },
        new { Metric = "Total Branches", Value = totalBranches.ToString("N0"), Description = "Branches with journal activity" },
        new { Metric = "Balanced Journals", Value = balancedJournals.ToString("N0"), Description = "Journals with debit = credit" },
        new { Metric = "Unbalanced Journals", Value = unbalancedJournals.ToString("N0"), Description = "Journals needing reconciliation" },
        new { Metric = "Total Debit Amount", Value = totalDebit.ToString("N2"), Description = "Sum of all debit entries" },
        new { Metric = "Total Credit Amount", Value = totalCredit.ToString("N2"), Description = "Sum of all credit entries" },
        new { Metric = "Balance Difference", Value = (totalDebit - totalCredit).ToString("N2"), Description = "Difference between debit and credit" },
        new { Metric = "Cash Operations", Value = cashOperations.ToString("N0"), Description = "Cash-based journal entries" },
        new { Metric = "Inter-Branch Operations", Value = interBranchOperations.ToString("N0"), Description = "Operations between branches" },
        new { Metric = "Workflow Required", Value = workflowRequired.ToString("N0"), Description = "Entries requiring approval workflow" }
    };

            foreach (var item in summaryData)
            {
                worksheet.Cells[currentRow, 1].Value = item.Metric;
                worksheet.Cells[currentRow, 2].Value = item.Value;
                worksheet.Cells[currentRow, 3].Value = item.Description;

                for (int col = 1; col <= headerColumns; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            currentRow += 2;

            // ===== STAGE DISTRIBUTION SECTION =====
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "STAGE DISTRIBUTION";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
            currentRow += 2;

            var stageHeaders = new[] { "Stage", "Count", "Percentage" };
            for (int i = 0; i < stageHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, 1 + i].Value = stageHeaders[i];
                worksheet.Cells[currentRow, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[currentRow, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            foreach (var stage in stageSummary)
            {
                decimal percentage = totalJournals > 0 ? (decimal)stage.Count / totalJournals : 0;
                worksheet.Cells[currentRow, 1].Value = stage.Stage;
                worksheet.Cells[currentRow, 2].Value = stage.Count;
                worksheet.Cells[currentRow, 3].Value = percentage;

                for (int col = 1; col <= headerColumns; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            // Format percentages
            worksheet.Cells[$"C{currentRow - stageSummary.Count}:C{currentRow - 1}"].Style.Numberformat.Format = "0.0%";

            currentRow += 2;

            // ===== STATE DISTRIBUTION SECTION =====
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "STATE DISTRIBUTION";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
            currentRow += 2;

            var stateHeaders = new[] { "State", "Count", "Percentage" };
            for (int i = 0; i < stateHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, 1 + i].Value = stateHeaders[i];
                worksheet.Cells[currentRow, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[currentRow, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            foreach (var state in stateSummary)
            {
                decimal percentage = totalJournals > 0 ? (decimal)state.Count / totalJournals : 0;
                worksheet.Cells[currentRow, 1].Value = state.State;
                worksheet.Cells[currentRow, 2].Value = state.Count;
                worksheet.Cells[currentRow, 3].Value = percentage;

                for (int col = 1; col <= headerColumns; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            // Format percentages
            worksheet.Cells[$"C{currentRow - stateSummary.Count}:C{currentRow - 1}"].Style.Numberformat.Format = "0.0%";

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void CreateJournalDetailsSheet(ExcelPackage package, List<CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead> journalData, string exportedBy, ExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Journal Entries");

            // Set font
            var fontName = "Bahnschrift SemiCondensed";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            // Journal Details sheet uses 16 columns (widest table)
            int headerColumns = 16;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, GetBankName(), GetBranchCode(), GetBranchID(), GetBranchName(), exportedBy, exportOptions, "JOURNAL ENTRIES DETAILED REPORT", headerEndColumn);

            // ===== STAGE DISTRIBUTION SECTION (ABOVE DETAILED TABLE) =====
            CreateStageDistributionSection(worksheet, journalData, ref currentRow, headerEndColumn);

            currentRow += 2;

            // ===== DETAILED DATA TABLE =====
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "DETAILED JOURNAL ENTRIES";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
            currentRow += 2;

            // Detailed data headers
            var headers = new[]
            {
        "SN", "Reference", "Narrative", "Accounting Date", "Branch", "Stage", "State",
        "Total Debit", "Total Credit", "Balance Status", "Operation Type", "Post Mode",
        "Cash Operation", "Inter-Branch", "Created By", "Created Date"
    };

            int headerRow = currentRow;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                worksheet.Cells[headerRow, i + 1].Style.Font.Bold = true;
                worksheet.Cells[headerRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[headerRow, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[headerRow, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Data rows
            int dataRow = headerRow + 1;
            int serialNumber = 1;
            decimal totalDebit = 0;
            decimal totalCredit = 0;

            foreach (var journal in journalData.OrderBy(x => x.AccountingDate).ThenBy(x => x.BranchName))
            {
                worksheet.Cells[dataRow, 1].Value = serialNumber++;
                worksheet.Cells[dataRow, 2].Value = journal.Reference;

                // NARRATIVE COLUMN WITHOUT WRAP TEXT
                worksheet.Cells[dataRow, 3].Value = journal.Narrative ?? "N/A";
                // Wrap text removed

                worksheet.Cells[dataRow, 4].Value = journal.AccountingDate.ToString("dd-MM-yyyy");
                worksheet.Cells[dataRow, 5].Value = journal.BranchName;
                worksheet.Cells[dataRow, 6].Value = journal.Stage;
                worksheet.Cells[dataRow, 7].Value = journal.State;
                worksheet.Cells[dataRow, 8].Value = journal.TotalDebit;
                worksheet.Cells[dataRow, 9].Value = journal.TotalCredit;
                worksheet.Cells[dataRow, 10].Value = journal.IsBalanced ? "BALANCED" : "UNBALANCED";
                worksheet.Cells[dataRow, 11].Value = journal.ExternalOperationType ?? "N/A";
                worksheet.Cells[dataRow, 12].Value = journal.PostMode ?? "N/A";
                worksheet.Cells[dataRow, 13].Value = journal.IsCashOperation ? "Yes" : "No";
                worksheet.Cells[dataRow, 14].Value = journal.IsInterBranch ? "Yes" : "No";
                worksheet.Cells[dataRow, 15].Value = journal.CreatedBy ?? "System";
                worksheet.Cells[dataRow, 16].Value = journal.CreatedAt.ToString("dd-MM-yyyy HH:mm:ss");

                // Apply borders
                for (int col = 1; col <= headerColumns; col++)
                {
                    worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                totalDebit += journal.TotalDebit;
                totalCredit += journal.TotalCredit;
                dataRow++;
            }

            // Totals row
            if (journalData.Any())
            {
                worksheet.Cells[dataRow, 1].Value = "TOTALS:";
                worksheet.Cells[dataRow, 1].Style.Font.Bold = true;
                worksheet.Cells[dataRow, 8].Value = totalDebit;
                worksheet.Cells[dataRow, 9].Value = totalCredit;
                worksheet.Cells[dataRow, 10].Value = totalDebit == totalCredit ? "BALANCED" : "UNBALANCED";

                for (int col = 1; col <= headerColumns; col++)
                {
                    worksheet.Cells[dataRow, col].Style.Font.Bold = true;
                    worksheet.Cells[dataRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[dataRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                    worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }

            // Format numbers
            if (journalData.Any())
            {
                worksheet.Cells[$"H{headerRow + 1}:I{dataRow}"].Style.Numberformat.Format = "#,##0.00";
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Freeze panes for easy scrolling
            worksheet.View.FreezePanes(headerRow + 1, 1);
        }

        private void CreateBranchSheet(ExcelPackage package, List<CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead> branchData, string branchId, string branchName, string exportedBy, ExportOptions exportOptions)
        {
            // Clean sheet name
            var cleanSheetName = CleanSheetName($"{branchId} - {branchName}");
            if (cleanSheetName.Length > 31) cleanSheetName = cleanSheetName.Substring(0, 31);

            var worksheet = package.Workbook.Worksheets.Add(cleanSheetName);

            // Set font
            var fontName = "Bahnschrift SemiCondensed";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            // Branch sheets use 12 columns (for detailed tables)
            int headerColumns = 12;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, GetBankName(), branchId, branchId, branchName, exportedBy, exportOptions, $"BRANCH JOURNAL REPORT - {branchName}", headerEndColumn);

            // ===== BRANCH SUMMARY SECTION =====
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"BRANCH SUMMARY - {branchName}";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            currentRow += 2;

            // Calculate branch statistics
            var totalJournals = branchData.Count;
            var balancedJournals = branchData.Count(x => x.IsBalanced);
            var cashOperations = branchData.Count(x => x.IsCashOperation);
            var interBranchOps = branchData.Count(x => x.IsInterBranch);
            var totalDebit = branchData.Sum(x => x.TotalDebit);
            var totalCredit = branchData.Sum(x => x.TotalCredit);

            // Branch summary table
            var branchSummaryHeaders = new[] { "Metric", "Value" };
            for (int i = 0; i < branchSummaryHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, 1 + i].Value = branchSummaryHeaders[i];
                worksheet.Cells[currentRow, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[currentRow, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            var branchSummaryData = new[]
            {
        new { Metric = "Total Journal Entries", Value = totalJournals.ToString("N0") },
        new { Metric = "Balanced Journals", Value = balancedJournals.ToString("N0") },
        new { Metric = "Unbalanced Journals", Value = (totalJournals - balancedJournals).ToString("N0") },
        new { Metric = "Cash Operations", Value = cashOperations.ToString("N0") },
        new { Metric = "Inter-Branch Operations", Value = interBranchOps.ToString("N0") },
        new { Metric = "Total Debit Amount", Value = totalDebit.ToString("N2") },
        new { Metric = "Total Credit Amount", Value = totalCredit.ToString("N2") },
        new { Metric = "Balance Status", Value = totalDebit == totalCredit ? "BALANCED" : "UNBALANCED" }
    };

            foreach (var item in branchSummaryData)
            {
                worksheet.Cells[currentRow, 1].Value = item.Metric;
                worksheet.Cells[currentRow, 2].Value = item.Value;

                for (int col = 1; col <= 2; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            currentRow += 2;

            // ===== STAGE DISTRIBUTION SECTION (ABOVE DETAILED TABLE) =====
            CreateStageDistributionSection(worksheet, branchData, ref currentRow, headerEndColumn);

            currentRow += 2;

            // ===== DETAILED BRANCH DATA =====
            CreateDetailedJournalTable(worksheet, branchData, ref currentRow, $"Detailed Journal Entries - {branchName}", headerEndColumn);

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void CreateCashOperationsSheet(ExcelPackage package, List<CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead> cashData, string exportedBy, ExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Cash Operations");

            // Set font
            var fontName = "Bahnschrift SemiCondensed";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            // Cash Operations sheet uses 12 columns (for detailed tables)
            int headerColumns = 12;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, GetBankName(), GetBranchCode(), GetBranchID(), GetBranchName(), exportedBy, exportOptions, "CASH OPERATIONS REPORT", headerEndColumn);

            // ===== CASH OPERATIONS SUMMARY =====
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "CASH OPERATIONS SUMMARY";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.Gold);
            currentRow += 2;

            // Cash operations statistics
            var totalCashOps = cashData.Count;
            var totalCashAmount = cashData.Sum(x => x.CashDenomsTotal);
            var branchesWithCashOps = cashData.Select(x => x.BranchName).Distinct().Count();

            var cashSummaryHeaders = new[] { "Metric", "Value" };
            for (int i = 0; i < cashSummaryHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, 1 + i].Value = cashSummaryHeaders[i];
                worksheet.Cells[currentRow, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[currentRow, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            var cashSummaryData = new[]
            {
        new { Metric = "Total Cash Operations", Value = totalCashOps.ToString("N0") },
        new { Metric = "Total Cash Amount", Value = totalCashAmount.ToString("N2") },
        new { Metric = "Branches with Cash Ops", Value = branchesWithCashOps.ToString("N0") },
        new { Metric = "Average Cash per Operation", Value = (totalCashAmount / totalCashOps).ToString("N2") }
    };

            foreach (var item in cashSummaryData)
            {
                worksheet.Cells[currentRow, 1].Value = item.Metric;
                worksheet.Cells[currentRow, 2].Value = item.Value;

                for (int col = 1; col <= 2; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            currentRow += 2;

            // ===== STAGE DISTRIBUTION SECTION (ABOVE DETAILED TABLE) =====
            CreateStageDistributionSection(worksheet, cashData, ref currentRow, headerEndColumn);

            currentRow += 2;

            // ===== DETAILED CASH OPERATIONS =====
            CreateDetailedJournalTable(worksheet, cashData, ref currentRow, "Detailed Cash Operations", headerEndColumn, true);

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void CreateDetailedJournalTable(ExcelWorksheet worksheet, List<CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead> data, ref int currentRow, string title, string headerEndColumn, bool isCashSheet = false)
        {
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = title;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
            currentRow += 2;

            // Headers for detailed table
            var headers = isCashSheet ? new[]
            {
        "SN", "Reference", "Accounting Date", "Branch", "Cashier", "Till", "Cash Amount",
        "Total Debit", "Total Credit", "Stage", "State", "Created Date"
    } : new[]
            {
        "SN", "Reference", "Narrative", "Accounting Date", "Branch", "Stage", "State",
        "Total Debit", "Total Credit", "Balance Status", "Created By", "Created Date"
    };

            int headerRow = currentRow;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                worksheet.Cells[headerRow, i + 1].Style.Font.Bold = true;
                worksheet.Cells[headerRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[headerRow, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[headerRow, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Data rows
            int dataRow = headerRow + 1;
            int serialNumber = 1;

            foreach (var journal in data.OrderBy(x => x.AccountingDate).ThenBy(x => x.Reference))
            {
                worksheet.Cells[dataRow, 1].Value = serialNumber++;

                if (isCashSheet)
                {
                    worksheet.Cells[dataRow, 2].Value = journal.Reference;
                    worksheet.Cells[dataRow, 3].Value = journal.AccountingDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[dataRow, 4].Value = journal.BranchName;
                    worksheet.Cells[dataRow, 5].Value = journal.CashierName ?? "N/A";
                    worksheet.Cells[dataRow, 6].Value = journal.TillName ?? "N/A";
                    worksheet.Cells[dataRow, 7].Value = journal.CashDenomsTotal;
                    worksheet.Cells[dataRow, 8].Value = journal.TotalDebit;
                    worksheet.Cells[dataRow, 9].Value = journal.TotalCredit;
                    worksheet.Cells[dataRow, 10].Value = journal.Stage;
                    worksheet.Cells[dataRow, 11].Value = journal.State;
                    worksheet.Cells[dataRow, 12].Value = journal.CreatedAt.ToString("dd-MM-yyyy HH:mm:ss");
                }
                else
                {
                    worksheet.Cells[dataRow, 2].Value = journal.Reference;

                    // NARRATIVE COLUMN WITHOUT WRAP TEXT
                    worksheet.Cells[dataRow, 3].Value = journal.Narrative ?? "N/A";
                    // Wrap text removed

                    worksheet.Cells[dataRow, 4].Value = journal.AccountingDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[dataRow, 5].Value = journal.BranchName;
                    worksheet.Cells[dataRow, 6].Value = journal.Stage;
                    worksheet.Cells[dataRow, 7].Value = journal.State;
                    worksheet.Cells[dataRow, 8].Value = journal.TotalDebit;
                    worksheet.Cells[dataRow, 9].Value = journal.TotalCredit;
                    worksheet.Cells[dataRow, 10].Value = journal.IsBalanced ? "BALANCED" : "UNBALANCED";
                    worksheet.Cells[dataRow, 11].Value = journal.CreatedBy ?? "System";
                    worksheet.Cells[dataRow, 12].Value = journal.CreatedAt.ToString("dd-MM-yyyy HH:mm:ss");
                }

                // Apply borders to match header width
                int columnCount = GetColumnNumber(headerEndColumn);
                for (int col = 1; col <= columnCount; col++)
                {
                    worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                dataRow++;
            }

            // Format numbers
            if (data.Any())
            {
                if (isCashSheet)
                {
                    worksheet.Cells[$"G{headerRow + 1}:I{dataRow}"].Style.Numberformat.Format = "#,##0.00";
                }
                else
                {
                    worksheet.Cells[$"H{headerRow + 1}:I{dataRow}"].Style.Numberformat.Format = "#,##0.00";
                }
            }

            currentRow = dataRow + 2;
        }

        // New method to create Stage Distribution section for all sheets
        private void CreateStageDistributionSection(ExcelWorksheet worksheet, List<CBS.FrontDesk.Data.Entity.AccountingV2.JournalHead> data, ref int currentRow, string headerEndColumn)
        {
            // ===== STAGE DISTRIBUTION SECTION =====
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "STAGE DISTRIBUTION";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
            currentRow += 2;

            var totalJournals = data.Count;
            var stageSummary = data.GroupBy(x => x.Stage)
                                  .Select(g => new { Stage = g.Key, Count = g.Count() })
                                  .OrderByDescending(x => x.Count)
                                  .ToList();

            var stageHeaders = new[] { "Stage", "Count", "Percentage" };
            for (int i = 0; i < stageHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, 1 + i].Value = stageHeaders[i];
                worksheet.Cells[currentRow, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[currentRow, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            foreach (var stage in stageSummary)
            {
                decimal percentage = totalJournals > 0 ? (decimal)stage.Count / totalJournals : 0;
                worksheet.Cells[currentRow, 1].Value = stage.Stage;
                worksheet.Cells[currentRow, 2].Value = stage.Count;
                worksheet.Cells[currentRow, 3].Value = percentage;

                for (int col = 1; col <= 3; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            // Format percentages
            if (stageSummary.Any())
            {
                worksheet.Cells[$"C{currentRow - stageSummary.Count}:C{currentRow - 1}"].Style.Numberformat.Format = "0.0%";
            }
        }

        private int CreateHeaderSection(ExcelWorksheet worksheet, string bank, string branchcode, string branchid, string branchname, string exportedBy, ExportOptions exportOptions, string reportTitle, string headerEndColumn)
        {
            // ===== Bank Information at the TOP =====
            worksheet.Cells[$"A1:{headerEndColumn}1"].Merge = true;
            worksheet.Cells["A1"].Value = bank;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.Font.Size = 18;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(1).Height = 30;
            worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 100, 0));

            // ===== Bank Code and Branch Information =====
            worksheet.Cells[$"A2:{headerEndColumn}2"].Merge = true;
            worksheet.Cells["A2"].Value = $"Branch Code: {branchcode} | Branch: {branchname} | Branch ID: {branchid}";
            worksheet.Cells["A2"].Style.Font.Bold = true;
            worksheet.Cells["A2"].Style.Font.Size = 12;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(2).Height = 22;
            worksheet.Cells["A2"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A2"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(70, 130, 180));

            // ===== Export meta =====
            worksheet.Cells[$"A3:{headerEndColumn}3"].Merge = true;
            worksheet.Cells["A3"].Value = $"Exported By: {exportedBy}";
            worksheet.Cells["A3"].Style.Font.Bold = true;
            worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(3).Height = 18;

            worksheet.Cells[$"A4:{headerEndColumn}4"].Merge = true;
            worksheet.Cells["A4"].Value = $"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            worksheet.Cells["A4"].Style.Font.Bold = true;
            worksheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(4).Height = 18;

            // ===== Report Title =====
            worksheet.Cells[$"A5:{headerEndColumn}5"].Merge = true;
            worksheet.Cells["A5"].Value = reportTitle;
            worksheet.Cells["A5"].Style.Font.Bold = true;
            worksheet.Cells["A5"].Style.Font.Size = 14;
            worksheet.Cells["A5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(5).Height = 25;
            worksheet.Cells["A5"].Style.Font.Color.SetColor(Color.Black);
            worksheet.Cells["A5"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A5"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 215, 0));

            // Optional date range row
            if (exportOptions != null && !string.IsNullOrEmpty(exportOptions.StartDate) && !string.IsNullOrEmpty(exportOptions.EndDate))
            {
                worksheet.Cells[$"A6:{headerEndColumn}6"].Merge = true;
                worksheet.Cells["A6"].Value = $"Date Range: {exportOptions.StartDate} to {exportOptions.EndDate}";
                worksheet.Cells["A6"].Style.Font.Bold = true;
                worksheet.Cells["A6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells["A6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Row(6).Height = 18;
                return 8; // Return the starting row for content
            }

            return 7; // Return the starting row for content
        }

        // ===================================================================
        // HELPER METHODS
        // ===================================================================

        // Helper method to convert column number to letter (1 = A, 2 = B, etc.)
        private string GetColumnLetter(int columnNumber)
        {
            string columnLetter = "";
            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnLetter = Convert.ToChar('A' + modulo) + columnLetter;
                columnNumber = (columnNumber - modulo) / 26;
            }
            return columnLetter;
        }

        // Helper method to convert column letter to number (A = 1, B = 2, etc.)
        private int GetColumnNumber(string columnLetter)
        {
            columnLetter = columnLetter.ToUpper();
            int number = 0;
            for (int i = 0; i < columnLetter.Length; i++)
            {
                number = number * 26 + (columnLetter[i] - 'A' + 1);
            }
            return number;
        }

        private static List<JournalLine> GetJournalLines(dynamic item)
        {
            try
            {
                var lines = GetPropertyValue(item, "Lines", "lines");
                if (lines is List<JournalLine> journalLines)
                    return journalLines;
                return new List<JournalLine>();
            }
            catch
            {
                return new List<JournalLine>();
            }
        }

        private static string CleanSheetName(string name)
        {
            var invalidChars = new char[] { '\\', '/', '*', '?', ':', '[', ']' };
            foreach (var invalidChar in invalidChars)
            {
                name = name.Replace(invalidChar, ' ');
            }
            name = string.Join(" ", name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            return name.Trim();
        }

        // Helper methods for safe property access (same as in your sample)
        private static object GetPropertyValue(dynamic obj, params string[] propertyNames)
        {
            foreach (var propName in propertyNames)
            {
                try
                {
                    if (obj != null)
                    {
                        var property = obj.GetType().GetProperty(propName);
                        if (property != null)
                        {
                            var value = property.GetValue(obj, null);
                            if (value != null)
                                return value;
                        }

                        if (obj is IDictionary<string, object> dict && dict.ContainsKey(propName))
                            return dict[propName];
                    }
                }
                catch { }
            }
            return null;
        }

        private static string GetSafeString(dynamic obj, params string[] propertyNames)
        {
            var value = GetPropertyValue(obj, propertyNames);
            return value?.ToString();
        }

        private static bool? GetSafeBool(dynamic obj, params string[] propertyNames)
        {
            var value = GetPropertyValue(obj, propertyNames);
            if (value == null) return null;

            if (bool.TryParse(value.ToString(), out bool result))
                return result;
            if (value.ToString().ToLower() == "true" || value.ToString() == "1")
                return true;
            if (value.ToString().ToLower() == "false" || value.ToString() == "0")
                return false;

            return null;
        }

        private static decimal? GetSafeDecimal(dynamic obj, params string[] propertyNames)
        {
            var value = GetPropertyValue(obj, propertyNames);
            if (value == null) return null;

            if (decimal.TryParse(value.ToString(), out decimal result))
                return result;
            return null;
        }

        private static DateTime? ParseDateTime(object dateValue)
        {
            if (dateValue == null) return null;

            try
            {
                if (dateValue is DateTime dt) return dt;
                if (dateValue is DateTimeOffset dto) return dto.DateTime;

                string dateString = dateValue.ToString();
                if (DateTime.TryParse(dateString, out DateTime result))
                    return result;

                return null;
            }
            catch
            {
                return null;
            }
        }
    }

}








