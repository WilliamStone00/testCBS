using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Base;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ZXing;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2
{
    public class CustomeReportsController : BaseController
    {
        private readonly BranchAccountService _branchAccountService;
        private readonly BranchServices _branchServices;

        public CustomeReportsController(BranchAccountService branchAccountService, BranchServices branchServices)
        {
            _branchAccountService = branchAccountService;
            _branchServices = branchServices;
        }

        public async Task<ActionResult> Index()
        {
            ReportTypes();
            await LoadBranchesAsync();
            await LoadAccountsAsync();
            return View();
        }

        private async Task LoadBranchesAsync()
        {
            var branches = await _branchServices.GetBranches();
             ViewBag.Branches = branches;
            //ViewBag.Branches = branches.Select(b => new SelectListItem
            //{
            //    Value = b.Id.ToString(),
            //    Text = $"{b.Name} ({b.BranchCode})"
            //}).ToList();
        }


        [HttpPost]
        public async Task<ActionResult> loadAccountById(string branchId)
        {
          var results  =   await  _branchAccountService.GetAllBranchAccountsFromDataTableAsync(branchId);
            return  Json(new { success = true, results });
        }
       

        public async Task<ActionResult> ClearGeneratedReports()
        {
            var username = _branchAccountService.GetUserFullName();


            string basePath = Server.MapPath("~/TempReportFiles");

            if (!Directory.Exists(basePath))
                return Json(new { success = false, message = "Base folder not found" });

            // Loop through TrialBalance, TrialBalance6, etc.
            var trialBalanceFolders = Directory.GetDirectories(basePath, "TrialBalance*");

            foreach (var trialFolder in trialBalanceFolders)
            {
                // SAFE even with spaces
                string userFolderPath = Path.Combine(trialFolder, username);

                if (!Directory.Exists(userFolderPath))
                    continue;

                // Option A: delete the whole user folder
                Directory.Delete(userFolderPath, true);

                
            }

            return Json(new
            {
                success = true,
                message = $"Reports cleared for user '{username}'"
            });


        }

        private async Task LoadAccountsAsync()
        {
            var branchAccounts = await _branchAccountService.GetAsync();

            ViewBag.AccountNumbers = branchAccounts.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.Code} - {a.NameEn}"
            }).ToList();
        }

        private void ReportTypes()
        {
            ViewBag.ReportTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Trial Balance 6 Columns", Text = "Trial Balance 6 Columns" },
                new SelectListItem { Value = "Trial Balance 4 Columns", Text = "Trial Balance 4 Columns" },
                //new SelectListItem { Value = "BS", Text = "Balance Sheet" },
                new SelectListItem { Value = "JE", Text = "General Accounting Journal" },
                new SelectListItem { Value = "Account Statement", Text = "General Ledger" },
                //new SelectListItem { Value = "IN", Text = "Income Statement" },
            };
        }
    }


    //public class CustomReportViewModel
    //{
    //    public IEnumerable<SelectListItem> Branches { get; set; }
    //    public IEnumerable<SelectListItem> Accounts { get; set; }
    //    public IEnumerable<SelectListItem> ReportTypes { get; set; }
    //}
}
