using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Helper;
using Hangfire;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    [CheckSessionTimeOutAttribute]
    public class PISalaryAnalyzerController : BaseController
    {
        // GET: SalaryAnalyzer
        private readonly SalaryUploadServices _salaryUploadServices;
        private readonly SalaryAnalysisResultServices _salaryAnalysisResultServices;
        public PISalaryAnalyzerController(SalaryUploadServices salaryUploadServices, SalaryAnalysisResultServices salaryAnalysisResultServices)
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
            var salaryUploadModels = await _salaryUploadServices.GetSalaryModelByBranchUsingFileId(fileUploadid);
            var fileUpload = await _salaryUploadServices.GetFileUpload(fileUploadid);
            var activateSalaryFileCommand = new ActivateSalaryFileCommand { Id=fileUploadid, Status=fileUpload.IsAvalaibleForExecution };
            var salaryUploadModelSummary = new SalaryUploadModelSummaryDto
            {
                FileUploadId=fileUploadid,
                TotalMembers=salaryUploadModels.Count(),
                TotalNetSalary=salaryUploadModels.Sum(x => x.NetSalary)
            };
            return View(new SalaryUploadModelCarrier { SalaryUploadModels=salaryUploadModels.ToList(), SalaryUploadModelSummaryDto=salaryUploadModelSummary, ActivateSalaryFileCommand=activateSalaryFileCommand, FileUpload=fileUpload });
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
            return View(new SalaryUploadModelCarrier { SalaryAnalysisResultSummary=salaryAnalysisResult, SalaryAnalysisResultDetails=salaryAnalysisResult.salaryAnalysisResultDetails.ToList(), FileUpload=salaryAnalysisResult.FileUpload });
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
                var fileUploads = await _salaryUploadServices.GetUploadDtosAsyncByStatus(new GetAllFileUploadSalaryFileActivatedQuery { Both = false, Status = true }, "pi_salary");
                carrier.FileUploads = fileUploads.ToList();
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

        public async Task<ActionResult> DownloadSalaryAnalysisExcel(string fileUploadId)
        {
            try
            {
                // Fetch salary analysis details and file upload data
                var salaryAnalysis = await _salaryAnalysisResultServices.GetSalaryAnalysisResultByFileUploadId(fileUploadId);
                
                if (salaryAnalysis.Id == null)
                {
                    return Json(new { success = false, message = "No salary analysis details found for the specified file upload ID." }, JsonRequestBehavior.AllowGet);
                }
                var salaryDetails = salaryAnalysis.salaryAnalysisResultDetails.ToList();
                var fileUpload = await _salaryUploadServices.GetFileUpload(fileUploadId);



                // Define file path and branch name
                string branchName = Session["BranchName"].ToString();
                string fileName = $"SalaryAnalysis_{branchName}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string directoryPath = Server.MapPath("~/TempFiles");

                // Ensure the directory exists
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, fileName);
                string exportedDate = DateTime.Now.ToString();
                string exportedBy = Session["FullName"].ToString();
                string salaryCode = fileUpload.FileCode;
                // Generate Excel file
                //SalaryAnalysisResultDetailExcelGenerator.GenerateSalaryAnalysisExcel(salaryDetails, branchName, filePath, exportedDate, exportedBy, salaryCode);
                SalarySheetExcelExporter.ExportToExcel(salaryDetails, branchName, filePath, exportedDate, exportedBy, salaryCode);
                
                // Return the file for download
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                System.IO.File.Delete(filePath); // Clean up temporary file

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                // Log the exception and return an error response
                Console.WriteLine($"Error generating Excel file: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while generating the Excel file." }, JsonRequestBehavior.AllowGet);
            }
        }

    }

}