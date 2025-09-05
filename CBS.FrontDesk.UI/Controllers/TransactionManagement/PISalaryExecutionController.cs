using CBS.BusinessService.Accounts;
using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    //[CheckSessionTimeOutAttribute]
    public class PISalaryExecutionController : BaseController
    {
        // GET: SalaryExecutionServices
        private readonly SalaryExecutionServices _salaryExecutionServices;
        private readonly IndividualProfileServices _individualProfileServices;
        private readonly SalaryUploadServices _salaryUploadServices;
        public PISalaryExecutionController(SalaryExecutionServices salaryExecutionServices, SalaryUploadServices salaryUploadServices, IndividualProfileServices individualProfileServices)
        {
            _salaryExecutionServices=salaryExecutionServices;
            _salaryUploadServices=salaryUploadServices;
            _individualProfileServices=individualProfileServices;
        }

        public async Task<ActionResult> Index()
        {
            var fileUploads = await _salaryUploadServices.GetValues(
                       new GetAllFileUploadSalaryFileActivatedQuery { Both = false, Status = true },"ps_salary"
                   );
            ViewBag.Files=fileUploads.ToList();
            return View(new ExecuteSalaryCarrier());
        }
      
        public ActionResult UploadedSalaryFiles()
        {
            return View(new ExecuteSalaryCarrier());
        }
        public async Task<ActionResult> Detail(string fileUploadid)
        {
            var salaryExtractDtos = await _salaryExecutionServices.GetAExecutedSalaryFileByFileUploadId(fileUploadid);
            var fileUpload = await _salaryExecutionServices.GetFileUpload(fileUploadid);
            var dashboardViewModel= _salaryExecutionServices.GetDashboardSummary(salaryExtractDtos.ToList());
            var stringValues = await _individualProfileServices.GetMemberByCustomerTypeDropDown()
; return View(new ExecuteSalaryCarrier { SalaryExtractes=salaryExtractDtos.ToList(), FileUpload=fileUpload, DashboardViewModel=dashboardViewModel, StringValues=stringValues.ToList() });
        }

       
        [HttpPost]
        public async Task<ActionResult> UploadAnalysedSalaryFile(ExecuteSalaryCarrier model)
        {
            try
            {
                // Check if the file is null or not uploaded
                if (model?.UploadAnalysedSalaryCommand?.File == null || model.UploadAnalysedSalaryCommand.File.ContentLength == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please upload a valid file."
                    });
                }

                // Process the file upload
                var data = await _salaryExecutionServices.UploadFile(model.UploadAnalysedSalaryCommand);

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
        public async Task<ActionResult> ExecuteAnalysis(ExecuteSalaryCommand command)
        {
            try
            {
                

                // 🚀 Process salary execution
                var data = await _salaryExecutionServices.ExecuteSalary(command);

                return Json(new
                {
                    success = data.Result,
                    status = data.MessageStatus,
                    message = Messaging.MessageResult(data)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
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
                ExecuteSalaryCarrier carrier = new ExecuteSalaryCarrier();

                var fileUploads = await _salaryExecutionServices.GetFileUploads();
                carrier.FileUploads=fileUploads.ToList();
                return PartialView(partialView, carrier);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in InitializeData: {ex.Message}");
                // Return an empty model in case of an error
                return PartialView(partialView, new ExecuteSalaryCarrier());
            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _salaryExecutionServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }

}