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

                // Define header end column (5 columns now)
                string headerEndColumn = "E";

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

                // Add data table with only the 5 requested columns
                AddDataTable(worksheet, accounts, startRow);

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
            // Headers exactly as requested: Account, Name En, Name Fr, Nature, Post Code
            string[] headers = {
                "Account", "Name En", "Name Fr", "Nature", "code post"
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

            // Add data - mapping to the 5 columns
            int dataRow = startRow + 1;
            foreach (var account in FlattenAccountTree(accounts))
            {
                // Column 1: Account (using account.Name)
                worksheet.Cells[dataRow, 1].Value = account.Code;

                // Column 2: Name En
                worksheet.Cells[dataRow, 2].Value = account.NameEn;

                // Column 3: Name Fr
                worksheet.Cells[dataRow, 3].Value = account.NameFr;

                // Column 4: Nature (with dropdown values)
                worksheet.Cells[dataRow, 4].Value = GetNatureDisplayValue(account.Nature);

                // Column 5: Post Code
                worksheet.Cells[dataRow, 5].Value = account.PostCode;

                // Add borders (5 columns)
                for (int col = 1; col <= 5; col++)
                {
                    worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Indent based on depth (Account column is column 1)
                if (account.Depth > 0)
                {
                    worksheet.Cells[dataRow, 1].Style.Indent = account.Depth;
                }

                dataRow++;
            }

            // Add dropdown validation for Nature column (now column 4)
            if (dataRow > startRow + 1)
            {
                var natureRange = worksheet.Cells[startRow + 1, 4, dataRow - 1, 4];
                var validation = natureRange.DataValidation.AddListDataValidation();
                validation.Formula.Values.Add("-");
                validation.Formula.Values.Add("Debit");
                validation.Formula.Values.Add("Credit");
                validation.ShowErrorMessage = true;
                validation.ErrorTitle = "Invalid Nature";
                validation.Error = "Please select a value from the dropdown list (-, Debit, Credit)";
            }
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