using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.Migration;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Message;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    public class AccountBalanceInitializationController : Controller
    {
        public AccountMigrationServices AccountMigrationServices { get; set; }
        private BranchServices _branchService { get; set; }
        private SavingProductServices _savingProductServices { get; set; }
        public AccountBalanceInitializationController()
        {
            AccountMigrationServices = new AccountMigrationServices();
            _branchService = new BranchServices();
            _savingProductServices = new SavingProductServices();
        }
        // GET: AccountBalanceInitialization/Index
        public async Task<ActionResult> Index()
        {
            await GetList();
            return View(new AccountBalanInitConfiguration());
        }

        [HttpGet]
        public JsonResult GetAccountList(string BranchID,string ProductId)
        {
            // Normally pulled from DB or service
            var result = AccountMigrationServices.MigrationGLReconciliationAccountList(new GetInfoDto { BranchId = BranchID, ProductId = ProductId });
            if (result == null)
            {
                return Json(new { status = false, message = "No data found." }, JsonRequestBehavior.AllowGet);
            }
           
                return Json(result, JsonRequestBehavior.AllowGet);
        }
        private async Task GetList()
        {
            var BranList = (await _branchService.GetBranches()).ToList();
            ViewBag.Branches = BuildBranch(BranList);

            ViewBag.AccountTypes =await BuildAccountType();
            ViewBag.Scenarios =   BuildScenario();
        }

        private async Task<List<System.Web.WebPages.Html.SelectListItem>> BuildAccountType( )
        {
            var listOfItems = await _savingProductServices.GetSavingProducts();
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();
            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "", Value = $"Select AccountType" });
            foreach (var item in listOfItems)
            {
                
                    selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = $"{item.Name}", Value = item.Id });
                

            }
            return selectListItems;
        }

        private dynamic BuildBranch(List<Branch> listOfItems)
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();
            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "", Value = $"Select BranchCode" });
            foreach (var item in listOfItems)
            {
                if (!item.BranchCode.Equals("000"))
                {
                    selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = $"{item.Name}", Value = item.Id });
                }

            }
            return selectListItems;

        }

        private List<System.Web.WebPages.Html.SelectListItem> BuildScenario()
        {
            // You need to replace this with your actual list source
            var scenarios = new List<(string Id, string Name)>
            {
                ("SC_001", "Scenario 1 [SC_001]"),
                ("SC_002", "Scenario 2 [SC_002]"),
                ("SC_003", "Scenario 3 [SC_003]"),
                ("SC_004", "Scenario 4 [SC_004]"),
                ("SC_005", "Scenario 5 [SC_005]")
            };

            var selectListItems = new List<System.Web.WebPages.Html.SelectListItem>
    {
        new System.Web.WebPages.Html.SelectListItem { Text = "--- Select Scenario ---", Value = "" }
    };

            foreach (var item in scenarios)
            {
                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id
                });
            }

            return selectListItems;
        }


        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            await GetList();
            var partialResult = await GetServiceAction(path, partialView, KEY, serviceOption);

            return partialResult;
        }

        [HttpPost]
        public async Task<ActionResult> Create(AccountBalanInitConfiguration model)
        {
           var data = await AccountMigrationServices.MigrationGLReconciliation(model.InitInfoDto);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }
        private async Task<PartialViewResult> GetServiceAction(string path, string partialView, string key, string serviceOption)
        {

            if (path=="new")
            {
                return PartialView(partialView, new AccountBalanInitConfiguration { });
            }
            else
            {
                return PartialView(partialView, new AccountBalanInitConfiguration { });
            }

                   
            

           
        }


    }
}