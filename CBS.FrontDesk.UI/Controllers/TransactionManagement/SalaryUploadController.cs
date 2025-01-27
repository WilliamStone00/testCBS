using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

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

        public async Task<ActionResult> Index()
        {
            return View(new SalaryUploadModelCarrier());
        }
        public async Task<ActionResult> Salary(string fileUploadid)
        {
            var salaryUploadModels = await _salaryUploadServices.GetSalaryUploads(fileUploadid);
            var salaryUploadModelSummary = new SalaryUploadModelSummaryDto { FileUploadId=fileUploadid, TotalMembers=salaryUploadModels.Count(), TotalNetSalary=salaryUploadModels.Sum(x => x.NetSalary) };
            return View( new SalaryUploadModelCarrier { SalaryUploadModels=salaryUploadModels.ToList(), SalaryUploadModelSummaryDto=salaryUploadModelSummary,  });
        }
        public async Task<ActionResult> Analysis(string fileUploadid)
        {
            var salaryUploadModels = await _salaryUploadServices.GetSalaryUploads(fileUploadid);
            var salaryUploadModelSummary = new SalaryUploadModelSummaryDto { FileUploadId=fileUploadid, TotalMembers=salaryUploadModels.Count(), TotalNetSalary=salaryUploadModels.Sum(x => x.NetSalary) };
            return View(new SalaryUploadModelCarrier { SalaryUploadModels=salaryUploadModels.ToList(), SalaryUploadModelSummaryDto=salaryUploadModelSummary, });
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
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "list")
            {
                var fileUploads = await _salaryUploadServices.GetFileUploads();
                return PartialView(partialView, new SalaryUploadModelCarrier { FileUploads=fileUploads.ToList()});
            }

        
                return PartialView(partialView, new SalaryUploadModelCarrier());
  
         
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _salaryUploadServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }

}