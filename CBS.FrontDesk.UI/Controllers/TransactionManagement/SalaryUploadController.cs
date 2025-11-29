using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    [CheckSessionTimeOutAttribute]
    public class SalaryUploadController : BaseController
    {
        // GET: SalaryUpload
        private readonly SalaryUploadServices _salaryUploadServices;
        private readonly BranchServices _branchServices;
        private readonly SalaryAnalysisResultServices _salaryAnalysisResultServices;
        private readonly BranchAccountService chartOfAccountServices;
        private readonly FileUploadServices _fileUploadServices;
        public SalaryUploadController(SalaryUploadServices salaryUploadServices, SalaryAnalysisResultServices salaryAnalysisResultServices, BranchServices branchServices, BranchAccountService chartOfAccountServices, FileUploadServices fileUploadServices)
        {
            _salaryUploadServices = salaryUploadServices;
            _salaryAnalysisResultServices=salaryAnalysisResultServices;
            _branchServices=branchServices;
            this.chartOfAccountServices=chartOfAccountServices;
            _fileUploadServices=fileUploadServices;
        }

        public async Task<ActionResult> Index()
        {
            ViewBag.HeadOffice="No";
            if (_salaryAnalysisResultServices.IsHeadOffice())
            {
                ViewBag.HeadOffice="Yes";
            }
            await LoadDroupdowns();
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
                new SelectListItem { Value = "ManualEntryDailyCollection", Text = "Daily Collection Files" },
                new SelectListItem { Value = "Others",              Text = "Other Files" }
            }.AsReadOnly();

        // One helper to populate common dropdowns; optionally include Chart of Accounts
        private async Task PopulateDropdownsAsync(bool includeChartOfAccounts)
        {
            var branchesTask = await _branchServices.GetBranches();
            if (includeChartOfAccounts)
            {
                var chartOfAccounts = await chartOfAccountServices.GetAllBranchAccountsFromDataTableAsync(null);
                ViewBag.StandingOrderSourceAccountOptions = chartOfAccountServices.DropDownGen(chartOfAccounts.ToList());
            }
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
            var salaryUploadModelWithBranchStatistics = await _salaryUploadServices.GetSalaryUploadModelWithBranchStatistics(fileUploadid);
            var salaryUploadModels = salaryUploadModelWithBranchStatistics.SalaryUploadModelDtos.ToList();
            var fileUpload = await _salaryUploadServices.GetFileUpload(fileUploadid);
            var activateSalaryFileCommand = new ActivateSalaryFileCommand { Id=fileUploadid, Status=fileUpload.IsAvalaibleForExecution};
            var salaryUploadModelSummary = new SalaryUploadModelSummaryDto { FileUploadId=fileUploadid, 
TotalMembers=salaryUploadModels.Count(), TotalNetSalary=salaryUploadModels.Sum(x => x.NetSalary) };
            return View( new SalaryUploadModelCarrier { SalaryUploadModels=salaryUploadModels.ToList(), SalaryUploadModelSummaryDto=salaryUploadModelSummary, ActivateSalaryFileCommand=activateSalaryFileCommand,FileUpload=fileUpload, BranchPayrollSummaries=salaryUploadModelWithBranchStatistics.BranchPayrollSummaries});
        }
        public async Task<ActionResult> Analysis(string fileUploadid)
        {
            var salaryUploadModels = await _salaryUploadServices.GetSalaryUploads(fileUploadid);
            var salaryUploadModelSummary = new SalaryUploadModelSummaryDto { FileUploadId=fileUploadid, TotalMembers=salaryUploadModels.Count(), TotalNetSalary=salaryUploadModels.Sum(x => x.NetSalary) };
            return View(new SalaryUploadModelCarrier { SalaryUploadModels=salaryUploadModels.ToList(), SalaryUploadModelSummaryDto=salaryUploadModelSummary });
        }
        [HttpPost]
        public async Task<ActionResult> LoadLoanData(GetFileUploadsDataTableQuery tableQuery)
        {
            try
            {
                var dataTable = await _fileUploadServices.GetDataTableAsync(tableQuery);
                var loanList = JsonConvert.DeserializeObject<List<FileUploadDto>>(JsonConvert.SerializeObject(dataTable.data));
                if (tableQuery.ActionParam=="analyser")
                {
                    loanList = _salaryUploadServices.GetUploadForAnalysis(loanList);
                }
                else if (tableQuery.ActionParam=="executer")
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
        public async Task<ActionResult> DownloadFile(string fileId = null)
        {
            

            try
            {
                // Call the service to download the file
                var response = await _salaryUploadServices.DownloadFile(fileId);

                if (response != null)
                {
                    // If response is successful, return the file
                    return File(response.FileData, response.ContentType, response.FileName);
                }
                else
                {
                    // If the response is null or contains errors, return an error view

                    return View("Error", new HandleErrorInfo(new Exception(response.ErrorMessage), "ControllerName", "ActionName"));
                }
            }
            catch (Exception ex)
            {
                // Handle exception and return an error view
                Console.WriteLine($"Error downloading file: {ex.Message}");
                return View("Error", new HandleErrorInfo(ex, "ControllerName", "ActionName"));
            }
        }


        public async Task<ActionResult> SalaryAnalysisResultSummary(string fileUploadid)
        {
            var salaryAnalysisResult = await _salaryAnalysisResultServices.GetSalaryAnalysisResultByFileUploadId(fileUploadid);
            return View(new SalaryUploadModelCarrier { SalaryAnalysisResultSummary=salaryAnalysisResult, SalaryAnalysisResultDetails=salaryAnalysisResult.salaryAnalysisResultDetails.ToList(), FileUpload=salaryAnalysisResult.FileUpload});
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
      
    }

}