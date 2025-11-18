using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Base;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

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
                //new SelectListItem { Value = "Trial Balance 4 Columns", Text = "Trial Balance 4 Columns" },
                //new SelectListItem { Value = "Balance Sheet", Text = "Balance Sheet" },
                new SelectListItem { Value = "JE", Text = "General Accounting Journal" },
                new SelectListItem { Value = "Account Statement", Text = "General Ledger" },
                //new SelectListItem { Value = "Income Statement", Text = "Income Statement" },
            };
        }
    }
}
