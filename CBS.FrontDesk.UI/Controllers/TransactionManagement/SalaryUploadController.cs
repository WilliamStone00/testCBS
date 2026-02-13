using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.MemberReconciliation;
using CBS.BusinessService.AccountingV2.SharedMonth;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.AccountingV2.SharedMonth;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Controllers.Accounting_V2.Affiliate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    //[CheckSessionTimeOutAttribute]
    public class SalaryUploadController : BaseController
    {
        // GET: SalaryUpload
        private readonly SalaryUploadServices _salaryUploadServices;
        private readonly BranchServices _branchServices;
        private readonly SalaryAnalysisResultServices _salaryAnalysisResultServices;
        private readonly BranchAccountService chartOfAccountServices;
        private readonly FileUploadServices _fileUploadServices;
        private readonly SharedMonthSimulationService _sharedMonthSimulationService;
        public SalaryUploadController(SalaryUploadServices salaryUploadServices, SalaryAnalysisResultServices salaryAnalysisResultServices, BranchServices branchServices, BranchAccountService chartOfAccountServices, FileUploadServices fileUploadServices, SharedMonthSimulationService sharedMonthSimulationService)
        {
            _salaryUploadServices = salaryUploadServices;
            _salaryAnalysisResultServices = salaryAnalysisResultServices;
            _branchServices = branchServices;
            this.chartOfAccountServices = chartOfAccountServices;
            _fileUploadServices = fileUploadServices;
            _sharedMonthSimulationService = sharedMonthSimulationService;
        }

        public async Task<ActionResult> Index()
        {
            ViewBag.HeadOffice = "No";
            if (_salaryAnalysisResultServices.IsHeadOffice())
            {
                ViewBag.HeadOffice = "Yes";
            }
            await LoadDroupdowns();
            return View(new SalaryUploadModelCarrier());
        }

        public async Task<ActionResult> ReexecuteSalaryFile()
         {
            ViewBag.HeadOffice = "No";
            if (_salaryAnalysisResultServices.IsHeadOffice())
            {
                ViewBag.HeadOffice = "Yes";
            }
           // await LoadDroupdowns();
            return View(new SalaryUploadModelCarrier());
        }

        // In your controller (fields assumed already injected):
        // private readonly IBranchServices _branchServices;
        // private readonly IChartOfAccountServices chartOfAccountServices;

        // Reuse one immutable list for File Types (no per-request allocation)
        private static readonly IReadOnlyList<SelectListItem> FileTypeOptions =
            new List<SelectListItem>
            {
                new SelectListItem { Value = "CivilServants",       Text = "Civil Servant Files" },
                new SelectListItem { Value = "PrivateInstitutions", Text = "Private Institution Files" },
                new SelectListItem { Value = "StandingOrder",       Text = "Standing Order Files" },
                new SelectListItem { Value = "Analysis",            Text = "Analysed Files" },
                new SelectListItem { Value = "SalaryReExecution",            Text = "Salary ReExecution Files" },
                new SelectListItem { Value = "ManualEntryDailyCollection", Text = "Daily Collection Files" },
                new SelectListItem { Value = "Others",              Text = "Other Files" },
                new SelectListItem { Value = "ShareMonth",              Text = "Share Month Files" },
                 new SelectListItem { Value = "ShareMonthTax",              Text = "Share Month Tax Files" }
            }.AsReadOnly();

        // One helper to populate common dropdowns; optionally include Chart of Accounts
        private async Task PopulateDropdownsAsync(bool includeChartOfAccounts)
        {
            var branchesTask = await _branchServices.GetBranches();
            var chartOfAccounts = await chartOfAccountServices.GetAllBranchAccountsFromDataTableAsync(_branchServices.GetBranchID());
            ViewBag.StandingOrderSourceAccountOptions = chartOfAccountServices.DropDownGen(chartOfAccounts.ToList());
            ViewBag.Branches = branchesTask;
            ViewBag.FileTypes = FileTypeOptions;
        }
        private async Task PopulatereexecutionDropdownsAsync(bool includeChartOfAccounts)
        {
            var branchesTask = await _branchServices.GetBranches();
           // var chartOfAccounts = await chartOfAccountServices.GetAllBranchAccountsFromDataTableAsync(_branchServices.GetBranchID());
          //  ViewBag.StandingOrderSourceAccountOptions = chartOfAccountServices.DropDownGen(chartOfAccounts.ToList());
            ViewBag.Branches = branchesTask;
            ViewBag.FileTypes = FileTypeOptions;
        }

        // Actions
        public async Task<ActionResult> UploadedSalaryFiles()
        {
            await PopulateDropdownsAsync(includeChartOfAccounts: false);
            return View(new SalaryUploadModelCarrier());
        }

        // Keep signature/route the same; just delegate to the helper
        public async Task<bool> LoadDroupdowns()
        {
            await PopulateDropdownsAsync(includeChartOfAccounts: true);
            return true;
        }


        [HttpPost]
        public async Task<ActionResult> SetPrivate(SetFileUploadPrivateViewCommand model)
        {
            try
            {
                var result = await _fileUploadServices.SetPrivatePublicStatus(model);
                return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to update privacy." });
            }
        }

        public async Task<ActionResult> Detail(string fileUploadid)
        {
            var fileUpload = await _salaryUploadServices.GetFileUpload(fileUploadid);

            if (fileUpload.FileType == "ShareMonth" || fileUpload.FileType == "Tax")
            {
                var type = fileUpload.FileType;

                var memberShareMonthUpload = await _sharedMonthSimulationService.GetFileDetailSharedmonth(fileUpload.FileUploadId, type);


                return View("SharedMonthDetails", memberShareMonthUpload);

            }
            else
            {
                var salaryUploadModelWithBranchStatistics = await _salaryUploadServices.GetSalaryUploadModelWithBranchStatistics(fileUploadid);
                var salaryUploadModels = salaryUploadModelWithBranchStatistics.SalaryUploadModelDtos.ToList();

                var activateSalaryFileCommand = new ActivateSalaryFileCommand { Id = fileUploadid, Status = fileUpload.IsAvalaibleForExecution };
                var salaryUploadModelSummary = new SalaryUploadModelSummaryDto
                {
                    FileUploadId = fileUploadid,
                    TotalMembers = salaryUploadModels.Count(),
                    TotalNetSalary = salaryUploadModels.Sum(x => x.NetSalary)
                };
                return View(new SalaryUploadModelCarrier { SalaryUploadModels = salaryUploadModels.ToList(), SalaryUploadModelSummaryDto = salaryUploadModelSummary, ActivateSalaryFileCommand = activateSalaryFileCommand, FileUpload = fileUpload, BranchPayrollSummaries = salaryUploadModelWithBranchStatistics.BranchPayrollSummaries });

            }

        }
        public async Task<ActionResult> Analysis(string fileUploadid)
        {
            var salaryUploadModels = await _salaryUploadServices.GetSalaryUploads(fileUploadid);
            var salaryUploadModelSummary = new SalaryUploadModelSummaryDto { FileUploadId = fileUploadid, TotalMembers = salaryUploadModels.Count(), TotalNetSalary = salaryUploadModels.Sum(x => x.NetSalary) };
            return View(new SalaryUploadModelCarrier { SalaryUploadModels = salaryUploadModels.ToList(), SalaryUploadModelSummaryDto = salaryUploadModelSummary });
        }
        [HttpPost]
        public async Task<ActionResult> LoadLoanData(GetFileUploadsDataTableQuery tableQuery)
        {
            try
            {
                var dataTable = await _fileUploadServices.GetDataTableAsync(tableQuery);
                var loanList = JsonConvert.DeserializeObject<List<FileUploadDto>>(JsonConvert.SerializeObject(dataTable.data));
                if (tableQuery.ActionParam == "analyser")
                {
                    loanList = _salaryUploadServices.GetUploadForAnalysis(loanList);
                }
                else if (tableQuery.ActionParam == "executer")
                {
                    loanList = _salaryUploadServices.GetFileUploads(loanList);
                }
                return Json(new
                {
                    draw = dataTable.DataTableOptions.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = loanList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading loan data.");
            }
        }

        [HttpPost]
        public async Task<ActionResult> LoadreexecutionLoanData(GetFileUploadsDataTableQuery tableQuery)
        {
            try
            {
                // 1️⃣ Validate FileType presence
                if (string.IsNullOrWhiteSpace(tableQuery.FileType))
                {
                    return new HttpStatusCodeResult(
                        HttpStatusCode.BadRequest,
                        "Please select file type"
                    );
                }

                // 2️⃣ Allowed file types (must match your system values exactly)
                var allowedFileTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Analysis", "SalaryReExecution" };

                // 3️⃣ Validate FileType value
                if (!allowedFileTypes.Contains(tableQuery.FileType))
                {
                    return new HttpStatusCodeResult(
                        HttpStatusCode.BadRequest,
                        "Invalid file type"
                    );
                }

                // 4️⃣ Load data normally
                var dataTable = await _fileUploadServices.GetDataTableAsync(tableQuery);

                var loanList = JsonConvert.DeserializeObject<List<FileUploadDto>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );

                // 5️⃣ Action mode handling
                if (tableQuery.ActionParam == "analyser")
                {
                    loanList = _salaryUploadServices.GetUploadForAnalysis(loanList);
                }
                else if (tableQuery.ActionParam == "executer")
                {
                    loanList = _salaryUploadServices.GetFileUploads(loanList);
                }

                // 6️⃣ Return DataTables-compatible response
                return Json(new
                {
                    draw = dataTable.DataTableOptions.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = loanList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.InternalServerError,
                    "Error loading loan data."
                );
            }
        }

        public async Task<ActionResult> LoanRepaymentListing(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "ID is required");
            }

            var data = await _fileUploadServices.GetFileDetailsByIdAsync(id);

            if (data == null)
            {
                return HttpNotFound();
            }


            await PopulateDropdownsAsync(includeChartOfAccounts: false);
            return View(data);
        }

        [HttpPost]
        public async Task<JsonResult> LoanRepaymentsExecution(LoanRepaymentsExecutiontDataTableQuery query)
         {
            try
            {
                var data = await _fileUploadServices.LoanRepayments(query);

                var Fileexecution = JsonConvert.DeserializeObject<List<LoanRepaymentsExecutionDto>>(JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = Fileexecution
                });
            }
            catch (Exception ex)
            {
                // return a DataTables-compatible empty result on error
                return Json(new
                {
                    draw = query?.DataTableOptions?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }

        public async Task<ActionResult> LoanRepaymentDetails(string id, string partialView = null)
        {
            var data = await _fileUploadServices.GetLoanRepaymentByIdAsync(id);
            if (string.IsNullOrEmpty(partialView))
            {
                return View("_LoanRepaymentDetails", data);
            }
            return PartialView(partialView, data);
        }

        //[HttpPost] // Keep as POST to match AJAX call
        //public async Task<ActionResult> FileDetails(string id, string partialView = null)
        //{
        //    if (string.IsNullOrEmpty(id))
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "ID is required");
        //    }

        //    var data = await _fileUploadServices.GetFileDetailsByIdAsync(id);

        //    if (data == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    // If partialView is specified, use that
        //    if (!string.IsNullOrEmpty(partialView))
        //    {
        //        return PartialView(partialView, data);
        //    }

        //    // Default: return the statistics partial view
        //    return PartialView("_FileStatistics", data);
        //}

        public async Task<ActionResult> DownloadFile(string fileId = null, string fileType = null)
        {
            try
            {
                // 🔹 CASE 1: ShareMonth or Tax → normal binary download
                if (fileType != "ShareMonth" && fileType != "Tax")
                {
                    var response = await _salaryUploadServices.DownloadFile(fileId);

                    if (response != null)
                    {
                        return File(
                            response.FileData,
                            response.ContentType,
                            response.FileName
                        );
                    }

                    return View(
                        "Error",
                        new HandleErrorInfo(
                            new Exception(response?.ErrorMessage ?? "File download failed"),
                            "SalaryUpload",
                            "DownloadFile"
                        )
                    );
                }

                // 🔹 CASE 2: All other file types → URL-based SharedMonth download
                var fileUpload = await _salaryUploadServices.GetFileUpload(fileId);

                if (fileUpload == null)
                {
                    return View(
                        "Error",
                        new HandleErrorInfo(
                            new Exception("File upload not found"),
                            "SalaryUpload",
                            "DownloadFile"
                        )
                    );
                }

                //var sharedResponse = await _sharedMonthSimulationService.Download(fileUpload.FileUploadId);

                if (fileUpload != null && !string.IsNullOrWhiteSpace(fileUpload.FilePath))
                {
                    return Redirect(fileUpload.FilePath);
                }

                return View(
                    "Error",
                    new HandleErrorInfo(
                        new Exception("Shared file not available for download"),
                        "SalaryUpload",
                        "DownloadFile"
                    )
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file: {ex.Message}");

                return View(
                    "Error",
                    new HandleErrorInfo(ex, "SalaryUpload", "DownloadFile")
                );
            }
        }

        public async Task<ActionResult> DownloadReexecutionFile(string fileId = null, string fileType = null)
        {
            try
            {
                // 🔹 Allow download ONLY for Analysis files
                if (!string.Equals(fileType, "Analysis", StringComparison.OrdinalIgnoreCase))
                {
                    return View(
                        "Error",
                        new HandleErrorInfo(
                            new Exception("Only Analysis files are allowed for download."),
                            "SalaryUpload",
                            "DownloadReexecutionFile"
                        )
                    );
                }

                var response = await _salaryUploadServices.DownloadExecutionFile(fileId);

                if (response != null && response.FileData != null)
                {
                    return File(
                        response.FileData,
                        response.ContentType,
                        response.FileName
                    );
                }

                return View(
                    "Error",
                    new HandleErrorInfo(
                        new Exception(response?.ErrorMessage ?? "File download failed"),
                        "SalaryUpload",
                        "DownloadReexecutionFile"
                    )
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file: {ex.Message}");

                return View(
                    "Error",
                    new HandleErrorInfo(ex, "SalaryUpload", "DownloadReexecutionFile")
                );
            }
        }


        public async Task<ActionResult> SalaryAnalysisResultSummary(string fileUploadid)
        {
            var salaryAnalysisResult = await _salaryAnalysisResultServices.GetSalaryAnalysisResultByFileUploadId(fileUploadid);
            return View(new SalaryUploadModelCarrier { SalaryAnalysisResultSummary = salaryAnalysisResult, SalaryAnalysisResultDetails = salaryAnalysisResult.salaryAnalysisResultDetails.ToList(), FileUpload = salaryAnalysisResult.FileUpload });
        }

        [HttpPost]
        public async Task<ActionResult> SalaryFile(SalaryUploadModelCarrier model)
        {
            try
            {
                // Check if the file is null or not uploaded
                if (model?.AddSalaryUploadModelCommand?.File == null || model.AddSalaryUploadModelCommand.File.ContentLength == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please upload a valid file."
                    });
                }

                // Process the file upload
                var data = await _salaryUploadServices.UploadFile(model.AddSalaryUploadModelCommand);

                return Json(new
                {
                    success = data.Result,
                    status = data.MessageStatus,
                    message = Messaging.MessageResult(data)
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error: {ex.Message}");

                // Handle the error gracefully, possibly return a meaningful error message
                return Json(new
                {
                    success = false,
                    message = "An error occurred while queuing the background job."
                });
            }
        }
        [HttpPost]
        public async Task<ActionResult> ReexecuteSalaryFileUpload(SalaryUploadModelCarrier model)
        {
            try
            {
                // Check if the file is null or not uploaded
                if (model?.AddSalaryUploadModelCommand?.File == null || model.AddSalaryUploadModelCommand.File.ContentLength == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please upload a valid file."
                    });
                }

                // Process the file upload
                var data = await _salaryUploadServices.ReexecuteSalaryUploadFile(model.AddSalaryUploadModelCommand);

                return Json(new
                {
                    success = data.Result,
                    status = data.MessageStatus,
                    message = Messaging.MessageResult(data)
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error: {ex.Message}");

                // Handle the error gracefully, possibly return a meaningful error message
                return Json(new
                {
                    success = false,
                    message = "An error occurred while queuing the background job."
                });
            }
        }
        [HttpPost]
        public async Task<ActionResult> ExecuteAnalysis(SalaryAnalysisCommand model)
        {
            try
            {

                var data = await _salaryAnalysisResultServices.ExecuteSalaryAnalysis(model);

                return Json(new
                {
                    success = data.Result,
                    status = data.MessageStatus,
                    message = Messaging.MessageResult(data)
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error: {ex.Message}");

                // Handle the error gracefully, possibly return a meaningful error message
                return Json(new
                {
                    success = false,
                    message = "An error occurred while queuing the background job."
                });
            }
        }
        [HttpPost]
        public async Task<ActionResult> ChangeFileStaus(ActivateSalaryFileCommand model)
        {
            try
            {

                var data = await _salaryUploadServices.UpdateFileStatus(model);

                return Json(new
                {
                    success = data.Result,
                    status = data.MessageStatus,
                    message = Messaging.MessageResult(data)
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error: {ex.Message}");

                // Handle the error gracefully, possibly return a meaningful error message
                return Json(new
                {
                    success = false,
                    message = "An error occurred while queuing the background job."
                });
            }
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            try
            {
                SalaryUploadModelCarrier carrier = new SalaryUploadModelCarrier();

                if (path == "list")
                {
                    var fileUploads = await _salaryUploadServices.GetUploadDtosAsyncByStatus(
                        new GetAllFileUploadSalaryFileActivatedQuery { Both = true, Status = true }, path
                    );
                    carrier.FileUploads = fileUploads.ToList();
                }
                else if (path == "branches_view")
                {
                    var fileUploads = await _salaryUploadServices.GetUploadDtosAsyncByStatus(
                        new GetAllFileUploadSalaryFileActivatedQuery { Both = false, Status = true }, path
                    );
                    carrier.FileUploads = fileUploads.ToList();
                }

                return PartialView(partialView, carrier);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in InitializeData: {ex.Message}");
                // Return an empty model in case of an error
                return PartialView(partialView, new SalaryUploadModelCarrier());
            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _salaryUploadServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> DeletFile(string KEY)
        {
            var data = await _fileUploadServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ReexecuteSalaries()
        {
             await PopulatereexecutionDropdownsAsync(includeChartOfAccounts: false);          
            return View(new SalaryUploadModelCarrier());
        }

    }

}