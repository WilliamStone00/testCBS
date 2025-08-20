using CBS.FrontDesk.Data.Entity.SalaryManagement;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.Helper
{




    public static class SalarySheetExcelExporter
    {
        public static void ExportToExcel(
            List<SalaryAnalysisResultDetailNew> salaryDetails,
            string branchName,
            string filePath,
            string exportedDate,
            string exportedBy,
            string salaryCode)
        {
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Salary Sheet");

            // -------- Color palette (close to screenshot) --------
            var cHeaderGray = XLColor.FromHtml("#E9ECEF");
            var cMainLoan = XLColor.FromHtml("#2F6DB2"); // blue
            var cExceptional = XLColor.FromHtml("#D64545"); // red
            var cElected = XLColor.FromHtml("#3F8E3E"); // green
            var cSpecialOD = XLColor.FromHtml("#6F57B5"); // purple
            var cMicroLoan = XLColor.FromHtml("#2FA3A0"); // teal
            var cSSF = XLColor.FromHtml("#EE8F2A"); // orange

            var cOKbg = XLColor.LightGreen;
            var cOKfg = XLColor.DarkGreen;
            var cBadbg = XLColor.LightPink;
            var cBadfg = XLColor.Red;

            const string font = "Bahnschrift Light";

            // -------- Title block --------
            ws.Cell(1, 1).Value = $"SALARY SHEET FOR {branchName.ToUpper()}";
            ws.Range(1, 1, 1, 28).Merge().Style
                .Font.SetBold().Font.SetFontSize(14).Font.SetFontName(font)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

            ws.Cell(2, 1).Value = $"Exported: {exportedDate} BY {exportedBy}";
            ws.Range(2, 1, 2, 28).Merge().Style.Font.SetFontName(font);

            ws.Cell(3, 1).Value = $"Salary Code: {salaryCode}";
            ws.Range(3, 1, 3, 28).Merge().Style.Font.SetFontName(font);

            ws.Row(4).Height = 6; // spacer

            int groupHeaderRow = 5; // colored/grey band row
            int subHeaderRow = 6; // column labels row

            // -------- Group bars --------
            var groups = new Dictionary<int, (string Title, XLColor Color)>
        {
            {  9, ("MAIN_LOAN",                 cMainLoan)   },
            { 12, ("EXCEPTIONAL_LOAN",          cExceptional)},
            { 15, ("ELECTED_STAFF_OFFICIALS",   cElected)    },
            { 18, ("SPECIAL_LOANS_&_OVERDRAFT", cSpecialOD)  },
            { 21, ("MICRO_LOAN",                cMicroLoan)  },
            { 24, ("SSF",                       cSSF)        }
        };

            // --- DESCRIPTIONS band like the screenshot (C..H) ---
            var desc = ws.Range(groupHeaderRow, 3, groupHeaderRow, 8);
            desc.Merge();
            desc.Value = "DESCRIPTIONS";
            desc.Style.Fill.BackgroundColor = cHeaderGray;
            desc.Style.Font.SetBold().Font.SetFontName(font);
            desc.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            desc.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            desc.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

            // Grey fillers at left/right of that band row
            for (int c = 1; c <= 2; c++)
            {
                var cell = ws.Cell(groupHeaderRow, c);
                cell.Style.Fill.BackgroundColor = cHeaderGray;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            ws.Cell(groupHeaderRow, 27).Style.Fill.BackgroundColor = cHeaderGray;
            ws.Cell(groupHeaderRow, 28).Style.Fill.BackgroundColor = cHeaderGray;

            // Colored 3-col blocks
            foreach (var kv in groups)
            {
                int cs = kv.Key;
                int ce = cs + 2;
                var rg = ws.Range(groupHeaderRow, cs, groupHeaderRow, ce);
                rg.Merge();
                rg.Value = kv.Value.Title;

                rg.Style.Fill.BackgroundColor = kv.Value.Color;
                rg.Style.Font.SetBold().Font.SetFontColor(XLColor.White).Font.SetFontName(font);
                rg.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                rg.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                rg.Style.Alignment.SetWrapText(true);
                rg.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border
                               .SetInsideBorder(XLBorderStyleValues.Thin).Border
                               .SetOutsideBorderColor(XLColor.White).Border
                               .SetInsideBorderColor(XLColor.White);
            }
            ws.Row(groupHeaderRow).Height = 22;

            // -------- Sub-headers (white labels) --------
            string[] headers =
            {
            "ACC. NO","NAME","GROSS AMOUNT","STANDING AMOUNT","SAVINGS","DEPOSIT","NET SALARY","SHARES",
            "LOAN 1","INT","VAT",
            "LOAN 2","INT","VAT",
            "LOAN 3","INT","VAT",
            "LOAN 4","INT","VAT",
            "LOAN 8","INT","VAT",
            "SP SAV","INT","VAT",
            "CHARGES","CONTROL"
        };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(subHeaderRow, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.SetBold().Font.SetFontName(font);
                cell.Style.Fill.BackgroundColor = XLColor.White;
                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment
                                    .SetVertical(XLAlignmentVerticalValues.Center).Alignment
                                    .SetWrapText(true);
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            ws.Row(subHeaderRow).Height = 28;

            // Colored underline under each loan group
            foreach (var kv in groups)
            {
                var rg = ws.Range(subHeaderRow, kv.Key, subHeaderRow, kv.Key + 2);
                rg.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                rg.Style.Border.BottomBorderColor = kv.Value.Color;
            }

            // -------- Column letters for formulas --------
            string grossCol = ws.Column(3).ColumnLetter();  // C
            string sumStart = ws.Column(5).ColumnLetter();  // E
            string sumEnd = ws.Column(27).ColumnLetter(); // AA

            // -------- Data rows --------
            int row = subHeaderRow + 1;
            foreach (var it in salaryDetails)
            {
                ws.Cell(row, 1).Value = it.CustomerId;
                ws.Cell(row, 2).Value = it.MemberName;

                ws.Cell(row, 3).Value  = it.NetSalary;
                ws.Cell(row, 4).Value  = it.StandingOrderAmount;
                ws.Cell(row, 5).Value  = it.Savings;
                ws.Cell(row, 6).Value  = it.Deposit;
                ws.Cell(row, 7).Value  = it.Salary;
                ws.Cell(row, 8).Value  = it.Shares;

                ws.Cell(row, 9).Value = it.Loan1_Main_Loan_Capital;
                ws.Cell(row, 10).Value = it.Loan1_Main_Loan_Interest;
                ws.Cell(row, 11).Value = it.Loan1_Main_Loan_VAT;

                ws.Cell(row, 12).Value = it.Loan2_Exceptional_Loan_Capital;
                ws.Cell(row, 13).Value = it.Loan2_Exceptional_Loan_Interest;
                ws.Cell(row, 14).Value = it.Loan2_Exceptional_Loan_VAT;

                ws.Cell(row, 15).Value = it.Loan3_Elected_Staff_Capital;
                ws.Cell(row, 16).Value = it.Loan3_Elected_Staff_Interest;
                ws.Cell(row, 17).Value = it.Loan3_Elected_Staff_VAT;

                ws.Cell(row, 18).Value = it.Loan4_Special_Loan_With_Overdraft_Capital;
                ws.Cell(row, 19).Value = it.Loan4_Special_Loan_With_Overdraft_Interest;
                ws.Cell(row, 20).Value = it.Loan4_Special_Loan_With_Overdraft_VAT;

                ws.Cell(row, 21).Value = it.Loan8_Micro_Loan_Capital;
                ws.Cell(row, 22).Value = it.Loan8_Micro_Loan_Interest;
                ws.Cell(row, 23).Value = it.Loan8_Micro_Loan_VAT;

                ws.Cell(row, 24).Value = it.SpSavingCapital;
                ws.Cell(row, 25).Value = it.SpSavingInterest;
                ws.Cell(row, 26).Value = it.SpSavingVAT;

                ws.Cell(row, 27).Value = it.Charges;

                // CONTROL (✔ / ✘)
                ws.Cell(row, 28).FormulaA1 =
                    $"=IF(ABS({grossCol}{row}-SUM({sumStart}{row}:{sumEnd}{row}))<=0.01,\"✔\",\"✘\")";
                ws.Cell(row, 28).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Grid borders and fonts
                for (int c = 1; c <= 28; c++)
                {
                    var cell = ws.Cell(row, c);
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    cell.Style.Font.FontName = font;
                }
                // Alignment / formats
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                for (int c = 3; c <= 27; c++)
                {
                    var cell = ws.Cell(row, c);
                    cell.Style.NumberFormat.Format = "#,##0";
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }

                row++;
            }

            // -------- TOTAL row --------
            ws.Cell(row, 1).Value = "TOTAL";
            ws.Range(row, 1, row, 2).Merge().Style
                .Font.SetBold().Font.SetFontName(font)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Range(row, 1, row, 28).Style.Border.TopBorder = XLBorderStyleValues.Medium;
            ws.Range(row, 1, row, 2).Style.Fill.BackgroundColor = cHeaderGray;

            for (int col = 3; col <= 27; col++)
            {
                string L = ws.Column(col).ColumnLetter();
                var cell = ws.Cell(row, col);
                cell.FormulaA1 = $"SUM({L}{subHeaderRow + 1}:{L}{row - 1})";
                cell.Style.NumberFormat.Format = "#,##0";
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                cell.Style.Fill.BackgroundColor = cHeaderGray;
                cell.Style.Font.SetBold().Font.SetFontName(font);
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            var totalCtrl = ws.Cell(row, 28);
            totalCtrl.FormulaA1 =
                $"=IF(ABS({grossCol}{row}-SUM({sumStart}{row}:{sumEnd}{row}))<=0.01,\"✔\",\"✘\")";
            totalCtrl.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            totalCtrl.Style.Font.SetBold();

            // -------- Control conditional formats --------
            var controlRange = ws.Range(subHeaderRow + 1, 28, row, 28);
            controlRange.AddConditionalFormat().WhenEquals("✔")
                .Fill.SetBackgroundColor(cOKbg).Font.SetFontColor(cOKfg);
            controlRange.AddConditionalFormat().WhenEquals("✘")
                .Fill.SetBackgroundColor(cBadbg).Font.SetFontColor(cBadfg);

            // -------- Global cosmetics / UX --------
            ws.Columns(1, 28).AdjustToContents();
            ws.Column(1).Width = Math.Max(ws.Column(1).Width, 14); // ACC. NO
            ws.Column(2).Width = Math.Max(ws.Column(2).Width, 28); // NAME

            // Freeze only up to the last data row (never beyond data area)
            int lastDataRow = Math.Max(subHeaderRow, row - 1);          // if no data, equals subHeaderRow
            int freezeRows = Math.Min(subHeaderRow, lastDataRow);      // cap at subHeaderRow
            int freezeCols = 2;

            if (freezeRows > 0)
                ws.SheetView.FreezeRows(freezeRows);
            if (freezeCols > 0)
                ws.SheetView.FreezeColumns(freezeCols);

            wb.SaveAs(filePath);
        }
    }



    //public static class SalarySheetExcelExporter
    //{
    //    public static void ExportToExcel(
    //        List<SalaryAnalysisResultDetailNew> salaryDetails,
    //        string branchName,
    //        string filePath,
    //        string exportedDate,
    //        string exportedBy,
    //        string salaryCode)
    //    {
    //        var workbook = new XLWorkbook();
    //        var ws = workbook.Worksheets.Add("Salary Sheet");

    //        // === HEADER ===
    //        ws.Cell(1, 1).Value = $"SALARY SHEET FOR {branchName.ToUpper()}";
    //        ws.Range(1, 1, 1, 28).Merge().Style
    //            .Font.SetBold()
    //            .Font.SetFontSize(14)
    //            .Font.SetFontName("Bahnschrift Light")
    //            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

    //        ws.Cell(2, 1).Value = $"Exported: {exportedDate} BY {exportedBy}";
    //        ws.Range(2, 1, 2, 28).Merge().Style.Font.SetFontName("Bahnschrift Light");

    //        ws.Cell(3, 1).Value = $"Salary Code: {salaryCode}";
    //        ws.Range(3, 1, 3, 28).Merge().Style.Font.SetFontName("Bahnschrift Light");

    //        int groupHeaderRow = 5;
    //        int subHeaderRow = 6;

    //        // === GROUP HEADERS (each spans 3 columns: LOAN, INT, VAT) ===
    //        var groupHeaders = new Dictionary<int, string>
    //    {
    //        {  9, "MAIN_LOAN" },
    //        { 12, "EXCEPTIONAL_LOAN" },
    //        { 15, "ELECTED_STAFF_OFFICIALS" },
    //        { 18, "SPECIAL_LOANS_&_OVERDRAFT" },
    //        { 21, "MICRO_LOAN" },
    //        { 24, "SSF" }
    //    };

    //        foreach (var kvp in groupHeaders)
    //        {
    //            int colStart = kvp.Key;
    //            int colEnd = colStart + 2; // 3 columns per group
    //            string text = kvp.Value;

    //            var range = ws.Range(groupHeaderRow, colStart, groupHeaderRow, colEnd);
    //            range.Merge();
    //            range.Value = text;

    //            range.Style.Fill.BackgroundColor = XLColor.LightGray;
    //            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    //            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
    //            range.Style.Alignment.WrapText = true;
    //            range.Style.Font.SetBold();
    //            range.Style.Font.FontName = "Bahnschrift Light";
    //            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    //        }

    //        // Fill remaining blank cells on the group header row
    //        for (int i = 1; i <= 8; i++) ws.Cell(groupHeaderRow, i).Value = "";
    //        ws.Cell(groupHeaderRow, 27).Value = ""; // CHARGES
    //        ws.Cell(groupHeaderRow, 28).Value = ""; // CONTROL

    //        ws.Row(groupHeaderRow).Height = 35;

    //        // === SUB HEADERS (exactly as in the sheet) ===
    //        var headers = new[]
    //        {
    //        "ACC. NO", "NAME", "GROSS AMOUNT", "STANDING AMOUNT", "SAVINGS", "DEPOSIT", "NET SALARY", "SHARES",
    //        // MAIN_LOAN
    //        "LOAN 1", "INT", "VAT",
    //        // EXCEPTIONAL_LOAN
    //        "LOAN 2", "INT", "VAT",
    //        // ELECTED_STAFF_OFFICIALS
    //        "LOAN 3", "INT", "VAT",
    //        // SPECIAL_LOANS_&_OVERDRAFT
    //        "LOAN 4", "INT", "VAT",
    //        // MICRO_LOAN
    //        "LOAN 8", "INT", "VAT",
    //        // SSF
    //        "SP SAV", "INT", "VAT",
    //        "CHARGES", "CONTROL"
    //    };

    //        for (int i = 0; i < headers.Length; i++)
    //        {
    //            var cell = ws.Cell(subHeaderRow, i + 1);
    //            cell.Value = headers[i];
    //            cell.Style.Font.SetBold();
    //            cell.Style.Font.FontName = "Bahnschrift Light";
    //            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
    //            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    //            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
    //            cell.Style.Alignment.WrapText = true;
    //            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    //        }

    //        ws.Row(subHeaderRow).Height = 30;

    //        // Letters used in formulas
    //        string grossCol = ws.Column(3).ColumnLetter();   // C
    //        string sumStart = ws.Column(5).ColumnLetter();   // E  (SAVINGS)
    //        string sumEnd = ws.Column(27).ColumnLetter();  // AA (CHARGES)
    //        string controlCol = ws.Column(28).ColumnLetter(); // AB

    //        // === DATA ROWS ===
    //        int row = subHeaderRow + 1;
    //        foreach (var item in salaryDetails)
    //        {
    //            ws.Cell(row, 1).Value = item.CustomerId;
    //            ws.Cell(row, 2).Value = item.MemberName;

    //            // Col 3-8
    //            ws.Cell(row, 3).Value = item.Salary;               // GROSS AMOUNT
    //            ws.Cell(row, 4).Value = item.StandingOrderAmount;  // STANDING AMOUNT
    //            ws.Cell(row, 5).Value = item.Savings;
    //            ws.Cell(row, 6).Value = item.Deposit;
    //            ws.Cell(row, 7).Value = item.NetSalary;
    //            ws.Cell(row, 8).Value = item.Shares;

    //            // MAIN_LOAN
    //            ws.Cell(row, 9).Value = item.Loan1_Main_Loan_Capital;
    //            ws.Cell(row, 10).Value = item.Loan1_Main_Loan_Interest;
    //            ws.Cell(row, 11).Value = item.Loan1_Main_Loan_VAT;

    //            // EXCEPTIONAL_LOAN
    //            ws.Cell(row, 12).Value = item.Loan2_Exceptional_Loan_Capital;
    //            ws.Cell(row, 13).Value = item.Loan2_Exceptional_Loan_Interest;
    //            ws.Cell(row, 14).Value = item.Loan2_Exceptional_Loan_VAT;

    //            // ELECTED_STAFF_OFFICIALS
    //            ws.Cell(row, 15).Value = item.Loan3_Elected_Staff_Capital;
    //            ws.Cell(row, 16).Value = item.Loan3_Elected_Staff_Interest;
    //            ws.Cell(row, 17).Value = item.Loan3_Elected_Staff_VAT;

    //            // SPECIAL_LOANS_&_OVERDRAFT
    //            ws.Cell(row, 18).Value = item.Loan4_Special_Loan_With_Overdraft_Capital;
    //            ws.Cell(row, 19).Value = item.Loan4_Special_Loan_With_Overdraft_Interest;
    //            ws.Cell(row, 20).Value = item.Loan4_Special_Loan_With_Overdraft_VAT;

    //            // MICRO_LOAN
    //            ws.Cell(row, 21).Value = item.Loan8_Micro_Loan_Capital;
    //            ws.Cell(row, 22).Value = item.Loan8_Micro_Loan_Interest;
    //            ws.Cell(row, 23).Value = item.Loan8_Micro_Loan_VAT;

    //            // SSF (SP SAV)
    //            ws.Cell(row, 24).Value = item.SpSavingCapital;
    //            ws.Cell(row, 25).Value = item.SpSavingInterest;
    //            ws.Cell(row, 26).Value = item.SpSavingVAT;

    //            ws.Cell(row, 27).Value = item.Charges;

    //            // CONTROL (✔/✘) — sums SAVINGS..CHARGES (E..AA), compares with GROSS AMOUNT (C)
    //            ws.Cell(row, 28).FormulaA1 = $"=IF(ROUND(SUM({sumStart}{row}:{sumEnd}{row}),0)=ROUND({grossCol}{row},0),\"✔\",\"✘\")";
    //            ws.Cell(row, 28).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

    //            // Number formatting & borders
    //            for (int col = 3; col <= 27; col++)
    //            {
    //                ws.Cell(row, col).Style.NumberFormat.Format = "#,##0";
    //                ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
    //            }
    //            for (int col = 1; col <= 28; col++)
    //            {
    //                ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    //                ws.Cell(row, col).Style.Font.FontName = "Bahnschrift Light";
    //            }

    //            row++;
    //        }

    //        // === TOTAL ROW ===
    //        ws.Cell(row, 1).Value = "TOTAL";
    //        ws.Range(row, 1, row, 2).Merge().Style
    //            .Font.SetBold()
    //            .Font.SetFontName("Bahnschrift Light")
    //            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
    //        ws.Range(row, 1, row, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
    //        ws.Range(row, 1, row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

    //        for (int col = 3; col <= 27; col++)
    //        {
    //            string colLetter = ws.Column(col).ColumnLetter();
    //            var cell = ws.Cell(row, col);
    //            cell.FormulaA1 = $"SUM({colLetter}{subHeaderRow + 1}:{colLetter}{row - 1})";
    //            cell.Style.NumberFormat.Format = "#,##0";
    //            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
    //            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
    //            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    //            cell.Style.Font.SetBold();
    //            cell.Style.Font.FontName = "Bahnschrift Light";
    //        }

    //        // TOTAL control cell (optional visual check on total line)
    //        var totalControl = ws.Cell(row, 28);
    //        totalControl.FormulaA1 = $"=IF(ROUND(SUM({sumStart}{row}:{sumEnd}{row}),0)=ROUND({grossCol}{row},0),\"✔\",\"✘\")";
    //        totalControl.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    //        totalControl.Style.Font.SetBold();

    //        // === CONDITIONAL FORMATTING FOR CONTROL COLUMN ===
    //        var controlRange = ws.Range(subHeaderRow + 1, 28, row, 28);
    //        controlRange.AddConditionalFormat()
    //            .WhenEquals("✔")
    //            .Fill.SetBackgroundColor(XLColor.LightGreen)
    //            .Font.SetFontColor(XLColor.DarkGreen);

    //        controlRange.AddConditionalFormat()
    //            .WhenEquals("✘")
    //            .Fill.SetBackgroundColor(XLColor.LightPink)
    //            .Font.SetFontColor(XLColor.Red);

    //        // Auto-fit
    //        ws.Columns().AdjustToContents();

    //        // Save
    //        workbook.SaveAs(filePath);
    //    }
    //}


    //public static class SalarySheetExcelExporter
    //{
    //    public static void ExportToExcel(
    //        List<SalaryAnalysisResultDetailNew> salaryDetails,
    //        string branchName,
    //        string filePath,
    //        string exportedDate,
    //        string exportedBy,
    //        string salaryCode)
    //    {
    //        var workbook = new XLWorkbook();
    //        var ws = workbook.Worksheets.Add("Salary Sheet");

    //        // === HEADER ===
    //        ws.Cell(1, 1).Value = $"SALARY SHEET FOR {branchName.ToUpper()}";
    //        ws.Range(1, 1, 1, 22).Merge().Style
    //            .Font.SetBold()
    //            .Font.SetFontSize(14)
    //            .Font.SetFontName("Bahnschrift Light")
    //            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

    //        ws.Cell(2, 1).Value = $"Exported: {exportedDate} BY {exportedBy}";
    //        ws.Range(2, 1, 2, 22).Merge().Style.Font.SetFontName("Bahnschrift Light");

    //        ws.Cell(3, 1).Value = $"Salary Code: {salaryCode}";
    //        ws.Range(3, 1, 3, 22).Merge().Style.Font.SetFontName("Bahnschrift Light");

    //        int groupHeaderRow = 5;
    //        int subHeaderRow = 6;

    //        // === GROUP HEADERS ===
    //        var groupHeaders = new Dictionary<int, string>
    //        {
    //            { 9, "MAIN_LOAN" },
    //            { 11, "EXCEPTIONAL_LOAN" },
    //            { 13, "ELECTED_STAFF_OFFICIALS" },
    //            { 15, "SPECIAL_LOANS_&_OVERDRAFT" },
    //            { 17, "MICRO_LOAN" },
    //            { 19, "SSF" }
    //        };

    //        foreach (var kvp in groupHeaders)
    //        {
    //            int colStart = kvp.Key;
    //            int colEnd = colStart + 1;
    //            string text = kvp.Value;

    //            var range = ws.Range(groupHeaderRow, colStart, groupHeaderRow, colEnd);
    //            range.Merge();
    //            range.Value = text;

    //            range.Style.Fill.BackgroundColor = XLColor.LightGray;
    //            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    //            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
    //            range.Style.Alignment.WrapText = true;
    //            range.Style.Font.SetBold();
    //            range.Style.Font.FontName = "Bahnschrift Light";
    //            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    //        }

    //        // Fill remaining blank cells
    //        for (int i = 1; i <= 8; i++) ws.Cell(groupHeaderRow, i).Value = "";
    //        ws.Cell(groupHeaderRow, 21).Value = "";
    //        ws.Cell(groupHeaderRow, 22).Value = "";

    //        ws.Row(groupHeaderRow).Height = 35;

    //        // === SUB HEADERS ===
    //        var headers = new[]
    //        {
    //        "ACC. NO", "NAME", "AMOUNT", "STANDING AMOUNT", "SAVINGS", "DEPOSIT", "NET SALARY", "SHARES",
    //        "LOAN 1", "INT", "LOAN 2", "INT", "LOAN 3", "INT", "LOAN 4", "INT", "LOAN 8", "INT",
    //        "SP SAV", "INT", "CHARGES", "CONTROL"
    //    };

    //        for (int i = 0; i < headers.Length; i++)
    //        {
    //            var cell = ws.Cell(subHeaderRow, i + 1);
    //            cell.Value = headers[i];
    //            cell.Style.Font.SetBold();
    //            cell.Style.Font.FontName = "Bahnschrift Light";
    //            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
    //            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    //            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
    //            cell.Style.Alignment.WrapText = true;
    //            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    //        }

    //        ws.Row(subHeaderRow).Height = 30;

    //        // === DATA ROWS ===
    //        int row = subHeaderRow + 1;
    //        foreach (var item in salaryDetails)
    //        {
    //            ws.Cell(row, 1).Value = item.CustomerId;
    //            ws.Cell(row, 2).Value = item.MemberName;
    //            ws.Cell(row, 3).Value = item.NetSalary;
    //            ws.Cell(row, 4).Value = item.StandingOrderAmount;
    //            ws.Cell(row, 5).Value = item.Savings;
    //            ws.Cell(row, 6).Value = item.Deposit;
    //            ws.Cell(row, 7).Value = item.Salary;
    //            ws.Cell(row, 8).Value = item.Shares;

    //            ws.Cell(row, 9).Value = item.Loan1_Main_Loan_Capital;
    //            ws.Cell(row, 10).Value = item.Loan1_Main_Loan_Interest;

    //            ws.Cell(row, 11).Value = item.Loan2_Exceptional_Loan_Capital;
    //            ws.Cell(row, 12).Value = item.Loan2_Exceptional_Loan_Interest;

    //            ws.Cell(row, 13).Value = item.Loan3_Elected_Staff_Capital;
    //            ws.Cell(row, 14).Value = item.Loan3_Elected_Staff_Interest;

    //            ws.Cell(row, 15).Value = item.Loan4_Special_Loan_With_Overdraft_Capital;
    //            ws.Cell(row, 16).Value = item.Loan4_Special_Loan_With_Overdraft_Interest;

    //            ws.Cell(row, 17).Value = item.Loan8_Micro_Loan_Capital;
    //            ws.Cell(row, 18).Value = item.Loan8_Micro_Loan_Interest;

    //            ws.Cell(row, 19).Value = item.SpSavingCapital;
    //            ws.Cell(row, 20).Value = item.SpSavingInterest;

    //            ws.Cell(row, 21).Value = item.Charges;

    //            ws.Cell(row, 22).FormulaA1 = $"=IF(ROUND(SUM(E{row}:U{row}),0)=ROUND(C{row},0),\"✔\",\"✘\")";
    //            ws.Cell(row, 22).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

    //            for (int col = 3; col <= 21; col++)
    //            {
    //                ws.Cell(row, col).Style.NumberFormat.Format = "#,##0";
    //                ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
    //            }

    //            for (int col = 1; col <= 22; col++)
    //            {
    //                ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    //                ws.Cell(row, col).Style.Font.FontName = "Bahnschrift Light";
    //            }

    //            row++;
    //        }

    //        // === TOTAL ROW ===
    //        ws.Cell(row, 1).Value = "TOTAL";
    //        ws.Range(row, 1, row, 2).Merge().Style.Font.SetBold().Font.FontName = "Bahnschrift Light";

    //        for (int col = 3; col <= 21; col++)
    //        {
    //            string colLetter = ws.Column(col).ColumnLetter();
    //            var cell = ws.Cell(row, col);
    //            cell.FormulaA1 = $"SUM({colLetter}{subHeaderRow + 1}:{colLetter}{row - 1})";
    //            cell.Style.NumberFormat.Format = "#,##0";
    //            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
    //            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
    //            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    //        }

    //        for (int col = 1; col <= 2; col++)
    //        {
    //            ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.LightGray;
    //            ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    //        }

    //        var controlCell = ws.Cell(row, 22);
    //        controlCell.FormulaA1 = $"=IF(ROUND(SUM(E{row}:U{row}),0)=ROUND(C{row},0),\"✔\",\"✘\")";
    //        controlCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    //        controlCell.Style.Font.SetBold();
    //        controlCell.Style.Fill.BackgroundColor = XLColor.White;

    //        // === CONDITIONAL FORMATTING ===
    //        var controlRange = ws.Range(subHeaderRow + 1, 22, row, 22);
    //        controlRange.AddConditionalFormat()
    //            .WhenEquals("✔")
    //            .Fill.SetBackgroundColor(XLColor.LightGreen)
    //            .Font.SetFontColor(XLColor.DarkGreen);

    //        controlRange.AddConditionalFormat()
    //            .WhenEquals("✘")
    //            .Fill.SetBackgroundColor(XLColor.LightPink)
    //            .Font.SetFontColor(XLColor.Red);

    //        // Auto-fit columns
    //        ws.Columns().AdjustToContents();

    //        // Save file
    //        workbook.SaveAs(filePath);
    //    }
    //}

}