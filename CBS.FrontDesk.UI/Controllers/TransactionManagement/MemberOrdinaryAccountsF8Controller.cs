using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.BusinessService;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.BusinessService.Config;

namespace CBS.FrontDesk.UI.Controllers
{
    //[CheckSessionTimeOutAttribute]

    public class MemberOrdinaryAccountsF8Controller : BaseController
    {
        // GET: MemberOrdinaryAccountsF8
        private readonly FileDownloadServices _acountServices;
        private readonly BranchServices _branchServices;

        public MemberOrdinaryAccountsF8Controller(FileDownloadServices acountServices, BranchServices branchServices = null)
        {
            _acountServices = acountServices;
            _branchServices = branchServices;
        }
        public async Task<ActionResult> Index()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> LoadData(string searchCriteria = "All",string branchid= "N/A")
        {
            try
            {
                var dataTable = await _acountServices.GetDataTable(GetDataTableOptions(), searchCriteria, true, branchid);
                return Json(new
                {
                    draw = dataTable.draw,
                    recordsFiltered = dataTable.recordsFiltered,
                    recordsTotal = dataTable.recordsTotal,
                    data = dataTable.data
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Return an error response
                return Json(new { error = ex.Message });
            }
        }
        // Action to handle file download
        public async Task<ActionResult> DownloadFileF8(string fileId = null)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                var downloadInfoLoans = await _acountServices.GetAllFileDownloadInfoPerUser();
                var Branches = await _branchServices.GetBranches();
                ViewBag.Branches = Branches;
                return View(new Loan { FileDownloadInfoLoans = downloadInfoLoans.ToList() });
            }

            try
            {
                // Call the service to download the file
                var response = await _acountServices.DownloadFile(fileId);

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
        public async Task<ActionResult> Delete(string id)
        {
            var data = await _acountServices.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> LoanReportGeneration()
        {
            var downloadInfoLoans = await _acountServices.GetAllFileDownloadInfoPerUser();
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View(new Loan { FileDownloadInfoLoans = downloadInfoLoans.ToList() });
        }
        [HttpPost]
        public async Task<ActionResult> DownloadFileF8(InitiateLoanDownloadCommand initiateLoanDownloadCommand)
        {
            var data = await _acountServices.InitiateBulkDownloadBranch(initiateLoanDownloadCommand);
            ViewBag.Message = Messaging.MessageResult(data);
            ViewBag.Status = data.Result;
            var downloadInfoLoans = await _acountServices.GetAllFileDownloadInfoPerUser();
            var Branches = await _branchServices.GetBranches();

            ViewBag.Branches = Branches;
            return View(new Loan { FileDownloadInfoLoans = downloadInfoLoans.ToList() });
        }
        
    }
}