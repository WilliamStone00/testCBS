using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.AccountingYear;
using CBS.FrontDesk.Data.Entity.AccountingV2.SharedMonth;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Helper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace CBS.BusinessService.AccountingV2.SharedMonth
{
    public class SharedMonthSimulationService : BaseService
    {

        private readonly ApiCallerHelper _apiCallerHelper;
        private readonly ApiCallerHelper _apiCallerHelperT;
        private  readonly BranchServices _branchServices;



        public SharedMonthSimulationService()
        {

           
            _apiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());
            _apiCallerHelperT = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices = new BranchServices();

        }


        public async Task<bool> ActivateSimulationAsync(InstrestCalculation model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            try
            {
                var apiResponse = await _apiCallerHelperT.PostAsync<ResponseObject<InstrestCalculation>>(
                    APICallHelper.ActivateShareMonthSimulation,
                    model
                );

                // Return true if API response is not null and indicates success
                return apiResponse != null && apiResponse.IsSuccess; // Assuming ApiResponse has IsSuccess property
            }
            catch (Exception ex)
            {
                // Optional: log the error
                return false; // Return false if there was an exception
            }
        }


        public async Task<List<ShareMonthPsiReportLineDto>> GetSimulationData(SharedMonthSimulation model)
        {

            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.ProductId = "0000";
            model.StartPeriodKey = "jan";
            model.EndPeriodKey = "feb";

            var apiResponse =
                await _apiCallerHelper.PostAsync<ResponseObject<List<ShareMonthPsiReportLineDto>>>(
                    APICallHelper.InterestDistributionSimulation1,
                    model
                );

            return apiResponse?.ApiResponseData?.Data ?? new List<ShareMonthPsiReportLineDto>();
        }
        public async Task<JobRequestListResponse> GetRequestDataAsync(SimulationFilterRequest model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.SimulatedByUserId = GetUserID();
            model.Name = "string";

            var apiResponse =
                await _apiCallerHelperT.PostAsync<ResponseObject<JobRequestListResponse>>(
                    APICallHelper.DataTableForRequestSimulation,
                    model
                );

            // 🔥 SAFE deserialization
            if (apiResponse?.ApiResponseData?.Data == null)
                return new JobRequestListResponse
                {
                    Items = new List<JobRequestItem>()
                };

            return apiResponse.ApiResponseData.Data;
        }
         
        public async Task<ApiResponse<ResponseObject<CreateSimulations>>> CreateShareMonthSimulationAsync(CreateSimulations model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Date = DateTime.Now;
            model.SimulatedByUserName = GetUserFullName();
            model.SimulatedByUserId = GetUserID();

            try
            {
                var apiResponse =
                    await _apiCallerHelperT.PostAsync<ResponseObject<CreateSimulations>>(
                        APICallHelper.CreateShareMonthSimulation, // API endpoint
                        model                                      // FULL model posted
                    );

                return apiResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> ValidateSharedMonthExcelUnProcessed(
     HttpPostedFileBase file,
     SharedMonthFileupload model)
        {
            List<string> errs = new List<string>();
            if (file == null || file.ContentLength == 0)
                errs.Add( "❌ No file provided.");

            using (var stream = new MemoryStream())
            {
                file.InputStream.CopyTo(stream);
                stream.Position = 0;

                using (var workbook = new XLWorkbook(stream))
                {
                    var ws = workbook.Worksheet(1);

                    // 🔒 A1 – Bank Name
                    var bankName = ws.Cell("A1").GetString().Trim();
                    var expectedBankName = GetBankName(); // Your method to get current bank name
                    if (!bankName.Equals(expectedBankName, StringComparison.OrdinalIgnoreCase))
                    {
                        errs.Add("❌ Invalid bank name in cell A1.");
                    }

                    // 🔒 A8 – Title
                    var a8 = ws.Cell("A8").GetString().Trim();
                    if (!a8.Equals("Un Processed Shared Month", StringComparison.OrdinalIgnoreCase))
                    {
                        errs.Add("❌ Invalid file format. Cell A8 must be 'Un Processed Shared Month'.");
                    }

                    // 🔒 Branch validation
                    var branch = await _branchServices.GetBranch(model.BranchId);
                    if (branch == null)
                        errs.Add("❌ Branch not found.");

                    // 🔒 Check if branch belongs to bank

                    var branchesUnderBank = await _branchServices.GetBranchesByBankId(branch.BankId); // returns List<SelectListItem>
                    if (!branchesUnderBank.Any(b => b.Value == branch.Id.ToString()))
                    {
                        errs.Add($"❌ Branch '{branch.Name}' does not belong to bank '{expectedBankName}'.");
                    }
                    // 🔒 Validate branch name and code in Excel
                    if (!ws.Cell("B2").GetString().Trim()
                        .Equals(branch.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        errs.Add("❌ Branch name does not match.");
                    }

                    if (!ws.Cell("B3").GetString().Trim()
                        .Equals(branch.BranchCode, StringComparison.OrdinalIgnoreCase))
                    {
                        errs.Add("❌ Branch code does not match.");
                    }

                    // 🔒 Month validation
                    var excelMonth = ws.Cell("B5").GetString().Trim();
                    var modelMonth = model.Month.Trim();

                    if (!excelMonth.Equals(modelMonth, StringComparison.OrdinalIgnoreCase))
                    {
                        errs.Add("❌ Month in Excel does not match selected month.");
                    }

                    // 🔒 Uploaded By
                    var uploadedBy = ws.Cell("B7").GetString().Trim();
                    if (!uploadedBy.Equals(GetUserName(), StringComparison.OrdinalIgnoreCase))
                    {
                        errs.Add("❌ Uploaded By does not match current user.");
                    }

                    // ✅ Headers (Row 9)
                    var headers = new[]
                    {
                "Member Account Number",
                "Member Name",
                "Month",
                "Beginning of Month Balance",
                "Mid month Balance"
            };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        var header = ws.Cell(9, i + 1).GetString().Trim();
                        if (!header.Equals(headers[i], StringComparison.OrdinalIgnoreCase))
                        {
                            errs.Add($"❌ Invalid header in column {(char)('A' + i)}.");
                        }
                    }

                    // 📊 Data rows (Row 10 ↓)
                    var lastRow = ws.LastRowUsed().RowNumber();
                    for (int row = 10; row <= lastRow; row++)
                    {
                        if (!int.TryParse(ws.Cell(row, 1).GetString(), out _))
                            errs.Add($"❌ Row {row}: Member Account Number must be an integer.");

                        if (string.IsNullOrWhiteSpace(ws.Cell(row, 2).GetString()))
                            errs.Add($"❌ Row {row}: Member Name is required.");

                        if (string.IsNullOrWhiteSpace(ws.Cell(row, 3).GetString()))
                            errs.Add($"❌ Row {row}: Month is required.");

                        if (!int.TryParse(ws.Cell(row, 4).GetString(), out _))
                            errs.Add($"❌ Row {row}: Beginning of Month Balance must be an integer.");

                        if (!int.TryParse(ws.Cell(row, 5).GetString(), out _))
                            errs.Add($"❌ Row {row}: Mid month Balance must be an integer.");
                    }
                }
            }

            string err = string.Join(",", errs);
            return err; // ✅ Valid file
        }
        public async Task<string> ValidateSharedMonthExcelProcessed(
    HttpPostedFileBase file,
    SharedMonthFileupload model)
        {
            List<string> errs = new List<string>();

            if (file == null || file.ContentLength == 0)
                return "❌ No file provided.";

            using (var stream = new MemoryStream())
            {
                file.InputStream.CopyTo(stream);
                stream.Position = 0;

                using (var workbook = new XLWorkbook(stream))
                {
                    var ws = workbook.Worksheet(1);

                    /* =====================================================
                     * 🔒 BANK VALIDATION
                     * ===================================================== */
                    var bankName = ws.Cell("A1").GetString().Trim();
                    var expectedBankName = GetBankName();

                    if (!bankName.Equals(expectedBankName, StringComparison.OrdinalIgnoreCase))
                    {
                        errs.Add("❌ Invalid bank name in cell A1.");
                    }

                    /* =====================================================
                     * 🔒 BRANCH VALIDATION
                     * ===================================================== */
                    var branch = await _branchServices.GetBranch(model.BranchId);
                    if (branch == null)
                    {
                        errs.Add("❌ Branch not found.");
                        return string.Join(" | ", errs);
                    }

                    // 🔒 Check if branch belongs to bank
                    var branchesUnderBank =
                        await _branchServices.GetBranchesByBankId(branch.BankId);

                    if (!branchesUnderBank.Any(b => b.Value == branch.Id.ToString()))
                    {
                        errs.Add(
                            $"❌ Branch '{branch.Name}' does not belong to bank '{expectedBankName}'.");
                    }

                    // 🔒 Branch Name (B2)
                    if (!ws.Cell("B2").GetString().Trim()
                        .Equals(branch.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        errs.Add("❌ Branch name does not match.");
                    }

                    // 🔒 Branch Code (B3)
                    if (!ws.Cell("B3").GetString().Trim()
                        .Equals(branch.BranchCode, StringComparison.OrdinalIgnoreCase))
                    {
                        errs.Add("❌ Branch code does not match.");
                    }

                    /* =====================================================
                     * 🔒 HEADER INFO VALIDATION
                     * ===================================================== */

                    // Date (B4)
                    //if (!DateTime.TryParse(ws.Cell("B4").GetString(), out _))
                    //{
                    //    errs.Add("❌ Invalid date format in cell B4.");
                    //}

                    // Month (B5)
                    if (!ws.Cell("B5").GetString().Trim()
                        .Equals(model.Month, StringComparison.OrdinalIgnoreCase))
                    {
                        errs.Add("❌ Month in Excel does not match selected month.");
                    }

                    // Number of Members (B6)
                    if (!int.TryParse(ws.Cell("B6").GetString(), out int memberCount) || memberCount <= 0)
                    {
                        errs.Add("❌ Number Of Members must be a valid number.");
                    }

                    // Uploaded By (B7)
                    if (!ws.Cell("B7").GetString().Trim()
                        .Equals(GetUserName(), StringComparison.OrdinalIgnoreCase))
                    {
                        errs.Add("❌ Uploaded By does not match current user.");
                    }

                    /* =====================================================
                     * 🔒 SECTION TITLE
                     * ===================================================== */
                    if (!ws.Cell("A8").GetString().Trim()
                        .Equals("Processed Shared Month", StringComparison.OrdinalIgnoreCase))
                    {
                        errs.Add("❌ Invalid file format. Missing 'Processed Shared Month' title.");
                    }

                    /* =====================================================
                     * ✅ TABLE HEADERS (ROW 10)
                     * ===================================================== */
                    var headers = new[]
                    {
                "Member Account Number",
                "Member Name",
                "Month",
                "Shared Month",
                "Principal Amount"
            };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        var header = ws.Cell(9, i + 1).GetString().Trim();
                        if (!header.Equals(headers[i], StringComparison.OrdinalIgnoreCase))
                        {
                            errs.Add($"❌ Invalid header in column {(char)('A' + i)}.");
                        }
                    }

                    /* =====================================================
                     * 📊 DATA ROWS (ROW 11 ↓)
                     * ===================================================== */
                    var lastRow = ws.LastRowUsed().RowNumber();
                    int actualRowCount = 0;

                    for (int row = 10; row <= lastRow; row++)
                    {
                        if (ws.Row(row).IsEmpty()) continue;

                        actualRowCount++;

                        if (!int.TryParse(ws.Cell(row, 1).GetString(), out _))
                            errs.Add($"❌ Row {row}: Member Account Number must be numeric.");

                        if (string.IsNullOrWhiteSpace(ws.Cell(row, 2).GetString()))
                            errs.Add($"❌ Row {row}: Member Name is required.");

                        if (!ws.Cell(row, 3).GetString()
                            .Equals(model.Month, StringComparison.OrdinalIgnoreCase))
                            errs.Add($"❌ Row {row}: Month does not match selected month.");

                        if (!decimal.TryParse(ws.Cell(row, 4).GetString(), out _))
                            errs.Add($"❌ Row {row}: Shared Month must be numeric.");

                        if (!decimal.TryParse(ws.Cell(row, 5).GetString(), out _))
                            errs.Add($"❌ Row {row}: Principal Amount must be numeric.");
                    }

                    // 🔒 Member count consistency
                    if (memberCount != actualRowCount)
                    {
                        errs.Add("❌ Number Of Members does not match data rows count.");
                    }
                }
            }

            return string.Join(" | ", errs);
        }
        public async Task<string> ValidateSharedMonthExcelAnnual(
      HttpPostedFileBase file,
      SharedMonthFileupload model)
        {
            List<string> errs = new List<string>();

            if (file == null || file.ContentLength == 0)
                return "❌ No file provided.";

            using (var stream = new MemoryStream())
            {
                file.InputStream.CopyTo(stream);
                stream.Position = 0;

                using (var workbook = new XLWorkbook(stream))
                {
                    var ws = workbook.Worksheet(1);

                    /* =====================================================
                     * 🔒 BANK VALIDATION (A1)
                     * ===================================================== */
                    var bankName = ws.Cell("A1").GetString().Trim();
                    var expectedBankName = GetBankName();
                    if (!bankName.Equals(expectedBankName, StringComparison.OrdinalIgnoreCase))
                    {
                        errs.Add("❌ Invalid bank name in cell A1.");
                    }

                    /* =====================================================
                     * 🔒 BRANCH VALIDATION
                     * ===================================================== */
                    var branch = await _branchServices.GetBranch(model.BranchId);
                    if (branch == null)
                    {
                        errs.Add("❌ Branch not found.");
                        return string.Join(" | ", errs);
                    }

                    var branchesUnderBank = await _branchServices.GetBranchesByBankId(branch.BankId);
                    if (!branchesUnderBank.Any(b => b.Value == branch.Id.ToString()))
                    {
                        errs.Add($"❌ Branch '{branch.Name}' does not belong to bank '{expectedBankName}'.");
                    }

                    var excelBranchName = ws.Cell("B2").GetString().Trim();
                    if (!excelBranchName.Equals(branch.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        errs.Add($"❌ Branch name does not match. Expected: {branch.Name}, Found: {excelBranchName}");
                    }

                    var excelBranchCode = ws.Cell("B3").GetString().Trim();
                    if (!excelBranchCode.Equals(branch.BranchCode, StringComparison.OrdinalIgnoreCase))
                    {
                        errs.Add($"❌ Branch code does not match. Expected: {branch.BranchCode}, Found: {excelBranchCode}");
                    }

                    /* =====================================================
                     * 🔒 HEADER INFO VALIDATION
                     * ===================================================== */

                    if (!int.TryParse(ws.Cell("B6").GetString(), out int memberCount) || memberCount <= 0)
                    {
                        errs.Add("❌ Number Of Members must be a valid positive number.");
                    }

                    var excelUploader = ws.Cell("B7").GetString().Trim();
                    var currentUser = GetUserName();
                    if (!excelUploader.Equals(currentUser, StringComparison.OrdinalIgnoreCase))
                    {
                        errs.Add($"❌ Uploaded By does not match current user. Expected: {currentUser}, Found: {excelUploader}");
                    }

                    /* =====================================================
                     * 🔒 SECTION TITLE (A8)
                     * ===================================================== */
                    if (!ws.Cell("A8").GetString().Trim()
                        .Equals("Anual Shared Month", StringComparison.OrdinalIgnoreCase))
                    {
                        errs.Add("❌ Invalid file format. Missing 'Anual Shared Month' title in cell A8.");
                    }

                    /* =====================================================
                     * ✅ TABLE HEADERS (ROW 9)
                     * ===================================================== */
                    var headers = new[]
                    {
                "Member Account Number",
                "Member Name",
                "Shared Month"
            };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        var header = ws.Cell(9, i + 1).GetString().Trim();
                        if (!header.Equals(headers[i], StringComparison.OrdinalIgnoreCase))
                        {
                            errs.Add($"❌ Invalid header in column {(char)('A' + i)}. Expected: '{headers[i]}', Found: '{header}'");
                        }
                    }

                    /* =====================================================
                     * 📊 DATA ROWS (ROW 10 ↓)
                     * ===================================================== */
                    var lastRow = ws.LastRowUsed().RowNumber();
                    int actualRowCount = 0;

                    for (int row = 10; row <= lastRow; row++)
                    {
                        if (ws.Row(row).IsEmpty()) continue;

                        actualRowCount++;

                        var memberAccount = ws.Cell(row, 1).GetString().Trim();
                        if (string.IsNullOrWhiteSpace(memberAccount))
                            errs.Add($"❌ Row {row}: Member Account Number is required.");
                        else if (!int.TryParse(memberAccount, out _))
                            errs.Add($"❌ Row {row}: Member Account Number must be numeric. Found: '{memberAccount}'");

                        if (string.IsNullOrWhiteSpace(ws.Cell(row, 2).GetString()))
                            errs.Add($"❌ Row {row}: Member Name is required.");

                        var sharedMonthStr = ws.Cell(row, 3).GetString().Trim();
                        if (!decimal.TryParse(sharedMonthStr, out decimal sharedMonth))
                            errs.Add($"❌ Row {row}: Shared Month must be numeric. Found: '{sharedMonthStr}'");
                        else if (sharedMonth < 0)
                            errs.Add($"❌ Row {row}: Shared Month cannot be negative.");
                    }

                    if (memberCount != actualRowCount)
                    {
                        errs.Add($"❌ Number Of Members does not match data rows count. Expected: {memberCount}, Found: {actualRowCount}");
                    }
                }
            }

            return errs.Count == 0
                ? "✅ File validation successful."
                : string.Join(" | ", errs);
        }




        public async Task<ApiResponse<ServiceResponse<SharedMonthFileReponse>>> SharedMonthUpload(SharedMonthFileupload model)
        {
            string validationResult = string.Empty;

            if (model.FileType == "MonthlyUnprocessData")
            {
                validationResult =
                    await ValidateSharedMonthExcelUnProcessed(model.file, model);
            }
            else if (model.FileType == "MonthlyprocessData")
            {
                validationResult =
                    await ValidateSharedMonthExcelProcessed(model.file, model);
            }
            else if (model.FileType == "AnnualprocessData")
            {
                validationResult =
                    await ValidateSharedMonthExcelAnnual(model.file, model);
            }
            else
            {
                throw new Exception("❌ Invalid FileType.");
            }

            // ❌ STOP only when real errors exist
            if (!string.IsNullOrWhiteSpace(validationResult) &&
                validationResult.Contains("❌"))
            {
                throw new Exception(validationResult);
            }

            // 🚀 Upload ONLY when validation passed
            var endpoint = APICallHelper.SharedMonthUpload;

            var result = await _apiCallerHelper.UploadSharedMonthFileAsync<
                ServiceResponse<SharedMonthFileReponse>>(
                model.file,
                model.BranchId,
                model.Month,
                model.FileType,
                endpoint,
                model.ProductId,
                model.InterestRate,
                model.AccountingYearId
            );

            return result;
        }


        public async Task<List<accountingyear>> GetOpenAccountingYearsByBranchAsync(string branchId)
        {
            var url = $"{APICallHelper.GetAllAccoutingYear}?branchId={branchId}";

            var response = await _apiCallerHelper
                .GetAsync<ResponseObject<List<accountingyear>>>(url);

            if (!response.IsSuccess || response.ApiResponseData?.Data == null)
                return new List<accountingyear>();

            return response.ApiResponseData.Data
                .Where(y =>
                    y.BranchId == branchId &&                                  // ✅ FILTER BY BRANCH
                    y.Status != null &&
                    y.Status.Equals("OPEN", StringComparison.OrdinalIgnoreCase)
                )
                .OrderByDescending(y => y.Year)                                // ✅ SORT
                .ToList();
        }

        public async Task<MemberShareMonthUpload> GetFileDetailSharedmonth(string fileUploadId,string type)
        {
            try
            {
                var url = string.Format(APICallHelper.GetSharedMonthDetail, fileUploadId, type);
                var response = await _apiCallerHelper.GetAsync<ResponseObject<MemberShareMonthUpload>>(url);
                
                if (response.IsSuccess)
                {
                    // FileDownloadDto should contain file data and metadata
                    return response.ApiResponseData.Data;
                }
                return new MemberShareMonthUpload { };

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SharedMonthDownloadDto> Download(string fileUploadId)
        {
            var apiResponse = await _apiCallerHelper
                .GetAsync<ResponseObject<SharedMonthDownloadDto>>(
                    string.Format(APICallHelper.DownloadSharedMonth, fileUploadId)
                );

            if (apiResponse.IsSuccess)
                return apiResponse.ApiResponseData.Data;

            return null;
        }

        //public async Task<SharedMonthDownloadDto> Download(string fileId)
        //{
        //    // Simulate async behavior
        //    await Task.Delay(50);

        //    return new SharedMonthDownloadDto
        //    {
        //        Id = "da518d65-792a-4f05-b69c-39350b83ea25",
        //        FileName = "Anual_data_00855",
        //        Extension = ".xlsx",
        //        DownloadPath = "Documents/2026/Head office Bamenda/AccountingManagementV2//uploads/share-month-files/Anual_data_00855_Head office Bamenda_20260115044734.xlsx",
        //        FullPath = "https://identity.bapcculcbs.com/Documents/2026/Head office Bamenda/AccountingManagementV2//uploads/share-month-files/Anual_data_00855_Head office Bamenda_20260115044734.xlsx",
        //        FileType = "Document",
        //        ReportType = "ShareMonth",
        //        BranchName = "Head office Bamenda",
        //        Username = "Paul Formum",
        //        Size = "File not found"
        //    };
        //}


    }
}
