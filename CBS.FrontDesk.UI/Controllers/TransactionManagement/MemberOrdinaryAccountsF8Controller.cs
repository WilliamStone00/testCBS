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
using System.Net;
using CBS.FrontDesk.Data.Entity.GeneralStatisticReport;

namespace CBS.FrontDesk.UI.Controllers
{
    //[CheckSessionTimeOutAttribute]

    //public class MemberOrdinaryAccountsF8Controller : BaseController
    //{
    //    // GET: MemberOrdinaryAccountsF8
    //    private readonly FileDownloadServices _acountServices;
    //    private readonly BranchServices _branchServices;

    //    public MemberOrdinaryAccountsF8Controller(FileDownloadServices acountServices, BranchServices branchServices = null)
    //    {
    //        _acountServices = acountServices;
    //        _branchServices = branchServices;
    //    }
    //    public async Task<ActionResult> Index()
    //    {
    //        var Branches = await _branchServices.GetBranches();
    //        ViewBag.Branches = Branches;
    //        return View();
    //    }

    //    [HttpPost]
    //    public async Task<ActionResult> LoadData(string searchCriteria = "All",string branchid= "N/A")
    //    {
    //        try
    //        {
    //            var dataTable = await _acountServices.GetDataTable(GetDataTableOptions(), searchCriteria, true, branchid);
    //            return Json(new
    //            {
    //                draw = dataTable.draw,
    //                recordsFiltered = dataTable.recordsFiltered,
    //                recordsTotal = dataTable.recordsTotal,
    //                data = dataTable.data
    //            }, JsonRequestBehavior.AllowGet);
    //        }
    //        catch (Exception ex)
    //        {
    //            // Return an error response
    //            return Json(new { error = ex.Message });
    //        }
    //    }
    //    // Action to handle file download
    //public async Task<ActionResult> DownloadFileF8(string fileId = null)
    //{
    //    if (string.IsNullOrEmpty(fileId))
    //    {
    //        var downloadInfoLoans = await _acountServices.GetAllFileDownloadInfoPerUser();
    //        var Branches = await _branchServices.GetBranches();
    //        ViewBag.Branches = Branches;
    //        return View(new Loan { FileDownloadInfos = downloadInfoLoans.ToList() });
    //    }

    //    try
    //    {
    //        // Call the service to download the file
    //        var response = await _acountServices.DownloadFile(fileId);

    //        if (response != null)
    //        {
    //            // If response is successful, return the file
    //            return File(response.FileData, response.ContentType, response.FileName);
    //        }
    //        else
    //        {
    //            // If the response is null or contains errors, return an error view

    //            return View("Error", new HandleErrorInfo(new Exception(response.ErrorMessage), "ControllerName", "ActionName"));
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        // Handle exception and return an error view
    //        Console.WriteLine($"Error downloading file: {ex.Message}");
    //        return View("Error", new HandleErrorInfo(ex, "ControllerName", "ActionName"));
    //    }
    //}
    //    public async Task<ActionResult> Delete(string id)
    //    {
    //        var data = await _acountServices.Delete(id);
    //        return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
    //    }
    //    public async Task<ActionResult> LoanReportGeneration()
    //    {
    //        var downloadInfoLoans = await _acountServices.GetAllFileDownloadInfoPerUser();
    //        var Branches = await _branchServices.GetBranches();
    //        ViewBag.Branches = Branches;
    //        return View(new Loan { FileDownloadInfos = downloadInfoLoans.ToList() });
    //    }
    //    [HttpPost]
    //    public async Task<ActionResult> DownloadFileF8(InitiateLoanDownloadCommand initiateLoanDownloadCommand)
    //    {
    //        var data = await _acountServices.InitiateBulkDownloadBranch(initiateLoanDownloadCommand);
    //        ViewBag.Message = Messaging.MessageResult(data);
    //        ViewBag.Status = data.Result;
    //        var downloadInfoLoans = await _acountServices.GetAllFileDownloadInfoPerUser();
    //        var Branches = await _branchServices.GetBranches();

    //        ViewBag.Branches = Branches;
    //        return View(new Loan { FileDownloadInfos = downloadInfoLoans.ToList() });
    //    }

    //}

    public class MemberOrdinaryAccountsF8Controller : BaseController
    {
        private readonly FileDownloadServices _acountServices;
        private readonly BranchServices _branchServices;
        private readonly SavingProductServices _savingproductServices;

        public MemberOrdinaryAccountsF8Controller(FileDownloadServices acountServices, BranchServices branchServices, SavingProductServices savingproductServices)
        {
            _acountServices = acountServices;
            _branchServices = branchServices;
            _savingproductServices=savingproductServices;
        }

        // 🏁 GET: F8 Balances Landing Page
        public async Task<ActionResult> Index()
        {
            ViewBag.Branches = await _branchServices.GetBranches();
            ViewBag.AccountTypes = await _savingproductServices.GetSavingProductsDropdownAsync(); // Ensure this is populated
            var downloadInfos = await _acountServices.GetAllFileDownloadInfoPerUser();

            return View();
        }

        // ✅ Handle filter AJAX (optional - list content via filter)
        [HttpPost]
        public async Task<ActionResult> GetFilteredMemberBalances(DownloadF8Filter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.BranchId) || filter.AccountTypes == null || !filter.AccountTypes.Any())
                return Json(new { error = "Please select both Branch and at least one Account Type." });

            var dataTable = await _acountServices.GetDataTable(GetDataTableOptions(), filter.BranchId, true, filter.BranchId);
            return PartialView("_FileDownloadData", dataTable.data);
        }

        // ✅ Export file and return updated list (instead of streaming file directly)
        [HttpPost]
        public async Task<ActionResult> ExportMemberAccountBalances(string branchId, List<string> accountTypes, string AccountProfile)
        {
            if (string.IsNullOrWhiteSpace(branchId) || accountTypes == null || !accountTypes.Any())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Missing branch or account type(s).");

            var filter = new DownloadF8Filter
            {
                BranchId = branchId,
                AccountTypes = accountTypes,
                AccountProfile=AccountProfile
            };

            var newExport = await _acountServices.InitiateBulkDownloadBranch(filter);

            if (newExport == null)
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Failed to generate export.");

            // Return the updated list
            var downloads = await _acountServices.GetAllFileDownloadInfoPerUser();
            return PartialView("_FileDownloadData", downloads);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteAllFiles()
        {
            try
            {
                var result = await _acountServices.Delete();
                // Reload the updated list after deletion
                var downloads = await _acountServices.GetAllFileDownloadInfoPerUser();
                return PartialView("_FileDownloadData", downloads);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, $"Error: {ex.Message}");
            }
        }

        // ✅ Download a prepared file by ID
        public async Task<ActionResult> DownloadFileF8(string fileId = null)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                ViewBag.Branches = await _branchServices.GetBranches();
                ViewBag.AccountTypes = Enum.GetNames(typeof(AccountType)).Select(x => new SelectListItem
                {
                    Value = x,
                    Text = x
                }).ToList();
                ViewBag.AccountProfiles = AccountProfileTypes.All.Select(x => new SelectListItem
                {
                    Value = x,
                    Text = x
                }).ToList();
                var downloadInfos = await _acountServices.GetAllFileDownloadInfoPerUser();
                return View(downloadInfos);
            }

            try
            {
                var response = await _acountServices.DownloadFile(fileId);

                if (response?.FileData != null)
                {
                    return File(response.FileData, response.ContentType, response.FileName);
                }

                return View("Error", new HandleErrorInfo(new Exception(response?.ErrorMessage ?? "Download failed."), nameof(MemberOrdinaryAccountsF8Controller), nameof(DownloadFileF8)));
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, nameof(MemberOrdinaryAccountsF8Controller), nameof(DownloadFileF8)));
            }
        }
    }




}