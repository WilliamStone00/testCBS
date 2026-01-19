using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using CBS.FrontDesk.Data.Entity.AccountingV2.SharedMonth;

namespace CBS.BusinessService.AccountingV2.SharedMonth
{





    public class ShareMonthGenerator
    {
        public static byte[] FillExcelTemplate(MemberShareMonthUpload data, string generatedBy)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Bulk Transfer");

                // Set Bahnschrift SemiCondensed font for entire worksheet
                worksheet.Cells.Style.Font.Name = "Bahnschrift SemiCondensed";

                // -----------------------------
                // SET COLUMN WIDTHS WITH 50% INCREASE
                // -----------------------------
                // Original: A=20, B=20, C=30, D=15
                // With 50% increase: A=30, B=30, C=45, D=22.5 (rounded to 23)
                worksheet.Column(1).Width = 30;  // Column A: Member Reference / Labels (20 + 50% = 30)
                worksheet.Column(2).Width = 30;  // Column B: Account Type / Value Start (20 + 50% = 30)
                worksheet.Column(3).Width = 45;  // Column C: Name / Value Middle (30 + 50% = 45)
                worksheet.Column(4).Width = 23;  // Column D: Amount / Value End (15 + 50% = 22.5 ≈ 23)

                // -----------------------------
                // MERGE A1:D1 WITH ORANGE BACKGROUND
                // -----------------------------
                var titleCell = worksheet.Cells["A1:D1"];
                titleCell.Merge = true;
                titleCell.Value = data.BankName ?? "BAPCCUL";

                // Apply styling to the merged cell
                titleCell.Style.Font.Bold = true;
                titleCell.Style.Font.Size = 16;
                titleCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                titleCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Set orange background
                titleCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                titleCell.Style.Fill.BackgroundColor.SetColor(Color.Orange);
                titleCell.Style.Font.Color.SetColor(Color.Black);

                // Set row height
                worksheet.Row(1).Height = 35;

                // -----------------------------
                // BRANCH NAME (Row 2)
                // -----------------------------
                worksheet.Cells["A2"].Value = "Branch Name:";
                worksheet.Cells["A2"].Style.Font.Bold = true;
                worksheet.Cells["A2"].Style.Fill.PatternType = ExcelFillStyle.None;
                worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Merge B2:D2 for Branch Name value
                var branchNameCell = worksheet.Cells["B2:D2"];
                branchNameCell.Merge = true;
                branchNameCell.Value = data.BranchName;
                branchNameCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                branchNameCell.Style.Fill.PatternType = ExcelFillStyle.None;

                // -----------------------------
                // BRANCH CODE (Row 3)
                // -----------------------------
                worksheet.Cells["A3"].Value = "Branch Code:";
                worksheet.Cells["A3"].Style.Font.Bold = true;
                worksheet.Cells["A3"].Style.Fill.PatternType = ExcelFillStyle.None;
                worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Merge B3:D3 for Branch Code value
                var branchCodeCell = worksheet.Cells["B3:D3"];
                branchCodeCell.Merge = true;
                branchCodeCell.Value = data.BranchCode;
                branchCodeCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                branchCodeCell.Style.Fill.PatternType = ExcelFillStyle.None;

                // -----------------------------
                // DATE (Row 4)
                // -----------------------------
                worksheet.Cells["A4"].Value = "Date:";
                worksheet.Cells["A4"].Style.Font.Bold = true;
                worksheet.Cells["A4"].Style.Fill.PatternType = ExcelFillStyle.None;
                worksheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Merge B4:D4 for Date value
                var dateCell = worksheet.Cells["B4:D4"];
                dateCell.Merge = true;
                dateCell.Value = data.Date?.ToString("dd-MM-yy");
                dateCell.Style.Numberformat.Format = "dd-mm-yy";
                dateCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                dateCell.Style.Fill.PatternType = ExcelFillStyle.None;

                // -----------------------------
                // MONTH (Row 5)
                // -----------------------------
                worksheet.Cells["A5"].Value = "Month:";
                worksheet.Cells["A5"].Style.Font.Bold = true;
                worksheet.Cells["A5"].Style.Fill.PatternType = ExcelFillStyle.None;
                worksheet.Cells["A5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Merge B5:D5 for Month value
                var monthCell = worksheet.Cells["B5:D5"];
                monthCell.Merge = true;
                monthCell.Value = data.Month;
                monthCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                monthCell.Style.Fill.PatternType = ExcelFillStyle.None;

                // Apply THINNER borders (Hairline) to all cells for better visibility
                var thinBorderStyle = ExcelBorderStyle.Hair; // Even thinner than Thin
                for (int row = 2; row <= 5; row++)
                {
                    worksheet.Cells[$"A{row}"].Style.Border.BorderAround(thinBorderStyle);
                    worksheet.Cells[$"B{row}:D{row}"].Style.Border.BorderAround(thinBorderStyle);
                }

                // Set row heights for header rows
                for (int row = 2; row <= 5; row++)
                {
                    worksheet.Row(row).Height = 22; // Slightly shorter for cleaner look
                }

                // -----------------------------
                // TITLE SECTION (Row 6)
                // -----------------------------
                var tableTitleCell = worksheet.Cells["A6:D6"];
                tableTitleCell.Merge = true;
                tableTitleCell.Value = "BULK DEBIT/CREDIT TRANSFER";
                tableTitleCell.Style.Font.Bold = true;
                tableTitleCell.Style.Font.Size = 12; // Slightly smaller for better proportion
                tableTitleCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                tableTitleCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                tableTitleCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                tableTitleCell.Style.Fill.BackgroundColor.SetColor(Color.Orange);
                tableTitleCell.Style.Border.BorderAround(thinBorderStyle);
                worksheet.Row(6).Height = 25;

                // -----------------------------
                // TABLE HEADERS (Row 7)
                // -----------------------------
                int headerRow = 7;

                // Member Reference - LEFT aligned
                worksheet.Cells[headerRow, 1].Value = "Member Reference";
                worksheet.Cells[headerRow, 1].Style.Font.Bold = true;
                worksheet.Cells[headerRow, 1].Style.Font.Name = "Bahnschrift SemiCondensed";
                worksheet.Cells[headerRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[headerRow, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[headerRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 240, 240)); // Lighter gray
                worksheet.Cells[headerRow, 1].Style.Border.BorderAround(thinBorderStyle);

                // Account Type - LEFT aligned
                worksheet.Cells[headerRow, 2].Value = "Account Type";
                worksheet.Cells[headerRow, 2].Style.Font.Bold = true;
                worksheet.Cells[headerRow, 2].Style.Font.Name = "Bahnschrift SemiCondensed";
                worksheet.Cells[headerRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells[headerRow, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[headerRow, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow, 2].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 240, 240)); // Lighter gray
                worksheet.Cells[headerRow, 2].Style.Border.BorderAround(thinBorderStyle);

                // Name - LEFT aligned
                worksheet.Cells[headerRow, 3].Value = "Name";
                worksheet.Cells[headerRow, 3].Style.Font.Bold = true;
                worksheet.Cells[headerRow, 3].Style.Font.Name = "Bahnschrift SemiCondensed";
                worksheet.Cells[headerRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells[headerRow, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[headerRow, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow, 3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 240, 240)); // Lighter gray
                worksheet.Cells[headerRow, 3].Style.Border.BorderAround(thinBorderStyle);

                // Amount - RIGHT aligned
                worksheet.Cells[headerRow, 4].Value = "Amount";
                worksheet.Cells[headerRow, 4].Style.Font.Bold = true;
                worksheet.Cells[headerRow, 4].Style.Font.Name = "Bahnschrift SemiCondensed";
                worksheet.Cells[headerRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[headerRow, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[headerRow, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow, 4].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 240, 240)); // Lighter gray
                worksheet.Cells[headerRow, 4].Style.Border.BorderAround(thinBorderStyle);

                worksheet.Row(headerRow).Height = 22;

                // -----------------------------
                // DATA ROWS (Starting from Row 8)
                // -----------------------------
                int startDataRow = 8;
                int currentRow = startDataRow;

                if (data.Lines != null && data.Lines.Any())
                {
                    foreach (var line in data.Lines)
                    {
                        // Member Reference (Column A) - LEFT aligned
                        worksheet.Cells[currentRow, 1].Value = line.MemberReference;
                        worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        worksheet.Cells[currentRow, 1].Style.Font.Name = "Bahnschrift SemiCondensed";
                        worksheet.Cells[currentRow, 1].Style.Border.BorderAround(thinBorderStyle);

                        // Account Type (Column B) - LEFT aligned
                        worksheet.Cells[currentRow, 2].Value = line.AccountType;
                        worksheet.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        worksheet.Cells[currentRow, 2].Style.Font.Name = "Bahnschrift SemiCondensed";
                        worksheet.Cells[currentRow, 2].Style.Border.BorderAround(thinBorderStyle);

                        // Name (Column C) - LEFT aligned
                        worksheet.Cells[currentRow, 3].Value = line.MemberName;
                        worksheet.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        worksheet.Cells[currentRow, 3].Style.Font.Name = "Bahnschrift SemiCondensed";
                        worksheet.Cells[currentRow, 3].Style.Border.BorderAround(thinBorderStyle);

                        // Amount (Column D) - RIGHT aligned
                        worksheet.Cells[currentRow, 4].Value = line.Amount;
                        worksheet.Cells[currentRow, 4].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[currentRow, 4].Style.Font.Name = "Bahnschrift SemiCondensed";
                        worksheet.Cells[currentRow, 4].Style.Border.BorderAround(thinBorderStyle);

                        currentRow++;
                    }

                    // -----------------------------
                    // TOTALS ROW
                    // -----------------------------
                    if (data.Lines.Any())
                    {
                        // Merge A:C for "TOTAL" label
                        var totalLabelCell = worksheet.Cells[$"A{currentRow}:C{currentRow}"];
                        totalLabelCell.Merge = true;
                        totalLabelCell.Value = "TOTAL";
                        totalLabelCell.Style.Font.Bold = true;
                        totalLabelCell.Style.Font.Name = "Bahnschrift SemiCondensed";
                        totalLabelCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        totalLabelCell.Style.Border.BorderAround(thinBorderStyle);
                        totalLabelCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        totalLabelCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 240, 240)); // Lighter gray

                        // Total amount in column D - RIGHT aligned
                        decimal totalAmount = data.Lines.Sum(x => x.Amount);
                        worksheet.Cells[currentRow, 4].Value = totalAmount;
                        worksheet.Cells[currentRow, 4].Style.Font.Bold = true;
                        worksheet.Cells[currentRow, 4].Style.Font.Name = "Bahnschrift SemiCondensed";
                        worksheet.Cells[currentRow, 4].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[currentRow, 4].Style.Border.BorderAround(thinBorderStyle);
                        worksheet.Cells[currentRow, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[currentRow, 4].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 240, 240)); // Lighter gray

                        currentRow++; // Move to next row for footer
                    }
                }

                // -----------------------------
                // FOOTER SECTION (Generated By and Date)
                // -----------------------------
                // Add one empty row for spacing
                worksheet.Row(currentRow).Height = 10;
                currentRow++;

                // Generated By row
                var generatedByCell = worksheet.Cells[$"A{currentRow}:D{currentRow}"];
                generatedByCell.Merge = true;
                generatedByCell.Value = $"Generated by: {generatedBy}";
                generatedByCell.Style.Font.Name = "Bahnschrift SemiCondensed";
                generatedByCell.Style.Font.Size = 10;
                generatedByCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                generatedByCell.Style.Font.Italic = true;
                worksheet.Row(currentRow).Height = 18;
                currentRow++;

                // Generated Date row
                var generatedDateCell = worksheet.Cells[$"A{currentRow}:D{currentRow}"];
                generatedDateCell.Merge = true;
                generatedDateCell.Value = $"Generated date: {DateTime.Now:dd-MM-yyyy HH:mm}";
                generatedDateCell.Style.Font.Name = "Bahnschrift SemiCondensed";
                generatedDateCell.Style.Font.Size = 10;
                generatedDateCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                generatedDateCell.Style.Font.Italic = true;
                worksheet.Row(currentRow).Height = 18;

                // -----------------------------
                // FINAL FORMATTING ADJUSTMENTS
                // -----------------------------

                // Re-confirm column widths (with 50% increase)
                worksheet.Column(1).Width = 30;  // Column A: Member Reference / Labels
                worksheet.Column(2).Width = 30;  // Column B: Account Type / Value Start  
                worksheet.Column(3).Width = 45;  // Column C: Name / Value Middle
                worksheet.Column(4).Width = 23;  // Column D: Amount / Value End

                // Adjust row heights for better text visibility
                for (int row = 1; row <= currentRow; row++)
                {
                    if (worksheet.Row(row).Height < 18)
                    {
                        worksheet.Row(row).Height = 18;
                    }
                }

                return package.GetAsByteArray();
            }
        }
    }
}
