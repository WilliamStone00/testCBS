using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Message;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    [CheckSessionTimeOutAttribute]
    public class SalaryUploadController : BaseController
    {
        // GET: SalaryUpload
        private readonly SalaryUploadServices _salaryUploadServices;
        private readonly SalaryAnalysisResultServices _salaryAnalysisResultServices;
        public SalaryUploadController(SalaryUploadServices salaryUploadServices, SalaryAnalysisResultServices salaryAnalysisResultServices)
        {
            _salaryUploadServices = salaryUploadServices;
            _salaryAnalysisResultServices=salaryAnalysisResultServices;
        }

        public ActionResult Index()
        {
            return View(new SalaryUploadModelCarrier());
        }
        public ActionResult UploadedSalaryFiles()
        {
            return View(new SalaryUploadModelCarrier());
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
                        new GetAllFileUploadSalaryFileActivatedQuery { Both = true, Status = true }
                    );
                    carrier.FileUploads = fileUploads.ToList();
                }
                else if (path == "branches_view")
                {
                    var fileUploads = await _salaryUploadServices.GetUploadDtosAsyncByStatus(
                        new GetAllFileUploadSalaryFileActivatedQuery { Both = false, Status = true }
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
    }

}