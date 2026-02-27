using CBS.FrontDesk.Data.Entity.Accounting_V2.HoPcmfAccount;
using BusinessServices;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace CBS.BusinessService.Accounting_V2.ChatoFAccount
{

    public class PcmfExcelGenerator : BaseService
    {
        public byte[] ExportPcmfAccountTree(
            List<HoPcmfAccountTreeDto> accounts,
            string exportedBy,
            string fileTitle,
            GetHoPcmfCoaQuery tableQuery)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("PCMF Chart of Accounts");

                // Set font for the entire worksheet
                var fontName = "Bahnschrift SemiCondensed";
                worksheet.Cells.Style.Font.Name = fontName;

                // Get bank and branch information from BaseService methods
                string bank = GetBankName();
                string branchCode = GetBranchCode();
                string branchId = GetBranchID();
                string branchName = GetBranchName();

                // Define header end column (8 columns now)
                string headerEndColumn = "H";

                // Create header section and get starting row for data
                int startRow = CreateHeaderSection(
                    worksheet,
                    bank,
                    branchCode,
                    branchId,
                    branchName,
                    exportedBy,
                    fileTitle,
                    tableQuery,
                    headerEndColumn
                );

                // Add data table
                AddDataTable(worksheet, accounts, startRow);

                // Add footer section
                int lastDataRow = startRow + GetTotalDataRows(accounts) + 1;
                AddFooterSection(worksheet, lastDataRow, exportedBy, bank, branchCode, branchName);

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                return package.GetAsByteArray();
            }
        }

        private static int CreateHeaderSection(
            ExcelWorksheet worksheet,
            string bank,
            string branchCode,
            string branchId,
            string branchName,
            string exportedBy,
            string fileTitle,
            GetHoPcmfCoaQuery tableQuery,
            string headerEndColumn)
        {
            int currentRow = 1;

            // Row 1: Bank Name
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = bank;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 18;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(currentRow).Height = 30;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 100, 0));
            currentRow++;

            // Row 2: Branch Information
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"Branch Code: {branchCode} | Branch: {branchName} | Branch ID: {branchId}";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 12;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(currentRow).Height = 22;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(70, 130, 180));
            currentRow++;

            // Row 3: Exported By
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"Exported By: {exportedBy}";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(currentRow).Height = 18;
            currentRow++;

            // Row 4: Export Date
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"Export Date: {DateTime.Now:dd-MM-yyyy HH:mm:ss}";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(currentRow).Height = 18;
            currentRow++;

            // Row 5: Report Title
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"{fileTitle.ToUpper()} - PCMF CHART OF ACCOUNTS";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(currentRow).Height = 25;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Color.SetColor(Color.Black);
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 215, 0));
            currentRow++;

            return currentRow;
        }

        private static void AddDataTable(ExcelWorksheet worksheet, List<HoPcmfAccountTreeDto> accounts, int startRow)
        {
            // Updated headers - removed Depth and Path, moved Post Code between ID and Code
            string[] headers = {
            "ID", "Post Code", "Code", "Name", "Class", "Parent ID",
            "Nature", "Created Date"
        };

            // Add headers
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cells[startRow, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Add data - updated mapping with Post Code moved to column 2
            int dataRow = startRow + 1;
            foreach (var account in FlattenAccountTree(accounts))
            {
                worksheet.Cells[dataRow, 1].Value = account.Id;
                worksheet.Cells[dataRow, 2].Value = account.PostCode;      // Post Code now column 2
                worksheet.Cells[dataRow, 3].Value = account.Code;          // Code now column 3
                worksheet.Cells[dataRow, 4].Value = account.Name;
                worksheet.Cells[dataRow, 5].Value = account.Class;
                worksheet.Cells[dataRow, 6].Value = account.ParentId;
                worksheet.Cells[dataRow, 7].Value = GetNatureDisplayValue(account.Nature);
                worksheet.Cells[dataRow, 8].Value = account.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss");

                // Add borders (now only 8 columns)
                for (int col = 1; col <= 8; col++)
                {
                    worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Indent based on depth (Name column is now column 4)
                if (account.Depth > 0)
                {
                    worksheet.Cells[dataRow, 4].Style.Indent = account.Depth;
                }

                dataRow++;
            }

            // Add dropdown validation for Nature column (now column 7)
            if (dataRow > startRow + 1)
            {
                var natureRange = worksheet.Cells[startRow + 1, 7, dataRow - 1, 7];
                var validation = natureRange.DataValidation.AddListDataValidation();
                validation.Formula.Values.Add("-");
                validation.Formula.Values.Add("Debit");
                validation.Formula.Values.Add("Credit");
                validation.ShowErrorMessage = true;
                validation.ErrorTitle = "Invalid Nature";
                validation.Error = "Please select a value from the dropdown list (-, Debit, Credit)";
            }
        }

        private static void AddFooterSection(ExcelWorksheet worksheet, int startRow, string exportedBy,
            string bank, string branchCode, string branchName)
        {
            int row = startRow + 2;
            string footerEndColumn = "H"; // Updated to H (8 columns)

            // Add separator
            worksheet.Cells[row - 1, 1, row - 1, 8].Style.Border.Top.Style = ExcelBorderStyle.Thick;

            // Summary section
            worksheet.Cells[$"A{row}:{footerEndColumn}{row}"].Merge = true;
            worksheet.Cells[$"A{row}"].Value = "SUMMARY";
            worksheet.Cells[$"A{row}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{row}"].Style.Font.Size = 12;
            worksheet.Cells[$"A{row}"].Style.Font.UnderLine = true;
            row++;

            worksheet.Cells[$"A{row}"].Value = "Total Accounts:";
            worksheet.Cells[$"A{row}"].Style.Font.Bold = true;
            worksheet.Cells[$"B{row}"].Value = (startRow - 9).ToString();
            row += 2;

            // Footer information
            worksheet.Cells[$"A{row}:{footerEndColumn}{row}"].Merge = true;
            worksheet.Cells[$"A{row}"].Value = "EXPORT DETAILS";
            worksheet.Cells[$"A{row}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{row}"].Style.Font.Size = 12;
            worksheet.Cells[$"A{row}"].Style.Font.UnderLine = true;
            row++;

            worksheet.Cells[$"A{row}"].Value = "Export Timestamp:";
            worksheet.Cells[$"A{row}"].Style.Font.Bold = true;
            worksheet.Cells[$"B{row}"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            row++;

            worksheet.Cells[$"A{row}"].Value = "Exported By:";
            worksheet.Cells[$"A{row}"].Style.Font.Bold = true;
            worksheet.Cells[$"B{row}"].Value = exportedBy;
            row++;

            worksheet.Cells[$"A{row}"].Value = "Bank:";
            worksheet.Cells[$"A{row}"].Style.Font.Bold = true;
            worksheet.Cells[$"B{row}"].Value = bank;
            row++;

            worksheet.Cells[$"A{row}"].Value = "Branch Code:";
            worksheet.Cells[$"A{row}"].Style.Font.Bold = true;
            worksheet.Cells[$"B{row}"].Value = branchCode;
            row++;

            worksheet.Cells[$"A{row}"].Value = "Branch Name:";
            worksheet.Cells[$"A{row}"].Style.Font.Bold = true;
            worksheet.Cells[$"B{row}"].Value = branchName;
            row += 2;

            // Generation note
            worksheet.Cells[$"A{row}:{footerEndColumn}{row}"].Merge = true;
            worksheet.Cells[$"A{row}"].Value = "This report was generated automatically by the system.";
            worksheet.Cells[$"A{row}"].Style.Font.Italic = true;
            worksheet.Cells[$"A{row}"].Style.Font.Color.SetColor(Color.Gray);
        }

        private static List<HoPcmfAccountTreeDto> FlattenAccountTree(List<HoPcmfAccountTreeDto> accounts)
        {
            var flattened = new List<HoPcmfAccountTreeDto>();

            foreach (var account in accounts)
            {
                flattened.Add(account);
                if (account.Children != null && account.Children.Any())
                {
                    flattened.AddRange(FlattenAccountTree(account.Children));
                }
            }

            return flattened;
        }

        private static int GetTotalDataRows(List<HoPcmfAccountTreeDto> accounts)
        {
            return FlattenAccountTree(accounts).Count;
        }

        private static string GetNatureDisplayValue(string nature)
        {
            if (string.IsNullOrEmpty(nature))
                return "-";

            string natureLower = nature.ToLower();
            if (natureLower == "debit")
                return "Debit";
            else if (natureLower == "credit")
                return "Credit";
            else
                return "-";
        }
    }
}



