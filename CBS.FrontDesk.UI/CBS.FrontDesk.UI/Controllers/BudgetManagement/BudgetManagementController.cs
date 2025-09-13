using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService;
using CBS.BusinessService.BudgetManagement;
using CBS.BusinessService.Config.Localization;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.BudgetManagement;

namespace CBS.FrontDesk.UI.Controllers
{
    public class BudgetManagementController : Controller
    {
        private readonly BudgetServices _budgetServices;
        private readonly BudgetCategoryServices _budgetCategoryServices;
        private readonly ChartOfAccountServices _accountingServices;

        public BudgetManagementController(BudgetServices budgetServices, BudgetCategoryServices budgetCategoryServices, ChartOfAccountServices accountServices)
        {
            _budgetServices = budgetServices;
            _budgetCategoryServices= budgetCategoryServices;
            _accountingServices= accountServices;

        }


        // GET: BudgetManagement
        public async Task<ActionResult> Index()
        {
            await GetList();
            return View( new BudgetConfiguration());
        }

        private async Task GetList()
        {
          

            var listAccounts = await _accountingServices.GetAllChartOfAccounts();
            ViewBag.ChartOfAccounts = BuildMenuAccountViewBag(listAccounts.ToList());
            var listBudgetPeriodss = await _budgetServices.GetAllBudgetPeriods();
            ViewBag.BudgetPeriods= BuildBudgetPeriods(listBudgetPeriodss.ToList());
            var OrganizationalUnits = await _budgetServices.GetAllOrganizationalUnits();
            ViewBag.OrganizationalUnits = BuildOrganizationalUnits(OrganizationalUnits.ToList());
            ViewBag.TransactionKind = await this.GetTransactionKind();
     
        }

        private dynamic BuildOrganizationalUnits(List<OrganizationalUnit> OrganizationalUnits)
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();

            foreach (var item in OrganizationalUnits)
            {
                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value = item.Name });
            }
            return selectListItems;
        }

        private dynamic BuildBudgetPeriods(List<BudgetPeriod> listBudgetPeriodss)
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();

            foreach (var item in listBudgetPeriodss)
            {
                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value =item.Name });
            }
            return selectListItems;
        }

        private dynamic BuildMenuAccountViewBag(List<ChartOfAccount> ListchartOfAccounts)
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();

            foreach (var item in ListchartOfAccounts)
            {
                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value = $"{item.AccountNumber} - {item.LabelEn}" });
            }
            return selectListItems;
        }


        private Task<List<System.Web.WebPages.Html.SelectListItem>> GetTransactionKind()
        {
            var bookingDirections = new System.Web.WebPages.Html.SelectListItem[] { new System.Web.WebPages.Html.SelectListItem { Text = "INCOME", Value = "INCOME" }, new System.Web.WebPages.Html.SelectListItem { Text = "EXPENSE", Value = "EXPENSE" } }.ToList();
            return Task.FromResult(bookingDirections);
        }
        private dynamic BuildMenuViewBag(IEnumerable<ChartOfAccount> debitAccounts)
        {
            List<System.Web.WebPages.Html.SelectListItem> list = new List<System.Web.WebPages.Html.SelectListItem>();
            foreach (var item in debitAccounts)
            {

                list.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value = item.AccountNumber + "-" + item.LabelEn });
            }

            return list;
        }
 
        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(BudgetConfiguration model)
        {
            Func<Task<ExecutionMessages>> serviceAction = null;

            if (model.ServiceOption == "budget")
            {
                //return () => _AccountServices.Create(model.Account);
                if (model.Action == "insert")
                {
                    serviceAction = await GetInsertServiceActionAsync(model.ServiceOption, model);
                }
                else
                {

                    serviceAction = GetUpdateServiceAction(model.ServiceOption, model);
                }


            }
            else if (model.ServiceOption == "budgetCategory")
            {
                if (model.Action == "insert")
                {
                    serviceAction = await GetInsertServiceActionAsync(model.ServiceOption, model);
                }
                else
                {

                    serviceAction = GetUpdateServiceAction(model.ServiceOption, model);
                }
            }
   



            if (serviceAction != null)
            {
                try
                {
                    var data = await serviceAction();
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
                }
            }

            return Json(new { success = false, status = false, message = "Invalid option selected." });
        }
        private async Task<Func<Task<ExecutionMessages>>> GetInsertServiceActionAsync(string serviceOption, BudgetConfiguration model)
        {
            if (serviceOption == "budget")
            {
                return () => _budgetServices.Create(model.Budget);
            }
            else if (serviceOption == "budgetCategory")
            {
                return () => _budgetCategoryServices.Create(model.BudgetCategory);
            }
            
            else
            {
                return null;
            }
        }
        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(string serviceOption, BudgetConfiguration model)
        {
            if (serviceOption == "budget")
            {

                return () => _budgetServices.Update(model.Budget);
            }
            else if (serviceOption == "budgetCategory")
            {
                return () => _budgetCategoryServices.Update(model.BudgetCategory);
            }
          
            else
            {
                return null;
            }
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            await GetList();
            var partialResult = await GetServiceAction(path, partialView, KEY, serviceOption);

            return partialResult;
        }

        private async Task<PartialViewResult> GetServiceAction(string path, string partialView, string key, string serviceOption)
        {
            if (serviceOption == "budget")
            {
                if (path == "list")
                {
                    var data  = await _budgetServices.GetAllBudgets();
                    
                    var sysData = new BudgetConfiguration { Budgets = data.ToList() };
                    return PartialView(partialView, sysData);

                }

                else if (path == "new")
                {
                  
                    //Data.Account account = new Data.Account
                    //{
                    //    ChartOfAccountId =  chartOfAccount.Id,
                    //    AccountNumber =  chartOfAccount.AccountNumber,

                    //};
                    return PartialView(partialView, new BudgetConfiguration { });
                }
                else
                {
                    var data = await _budgetServices.GetBudget(key);
                    return PartialView(partialView, new BudgetConfiguration { Budget = data });

                }


            }
            else if (serviceOption == "budgetCategory")
            {
                if (path == "list")
                {
                    var data = await _budgetCategoryServices.GetBudgetCategorys();

                    var sysData = new BudgetConfiguration { BudgetCategories = data.ToList() };
                    return PartialView(partialView, sysData);

                }

                else if (path == "new")
                {

                    //Data.Account account = new Data.Account
                    //{
                    //    ChartOfAccountId =  chartOfAccount.Id,
                    //    AccountNumber =  chartOfAccount.AccountNumber,

                    //};
                    return PartialView(partialView, new BudgetConfiguration { });
                }
                else
                {
                    var data = await _budgetCategoryServices.GetBudgetCategory(key);
                    return PartialView(partialView, new BudgetConfiguration { BudgetCategory = data });

                }


            }
 
            return null;
        }
        public List<OperationEventAttributeDto> ConvertToOperationEventAttributeDtos(List<OperationEventAttribute> attributes, List<OperationEvent> events)
        {
            return (from a in attributes
                    join e in events on a.OperationEventId equals e.Id
                    select new OperationEventAttributeDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        OperationEventName = e.OperationEventName
                    }).ToList();
        }
        public async Task<ActionResult> Delete(string KEY, string serviceOption)
        {


            if (serviceOption == "budget")
            {
                var data = await _budgetServices.Delete(KEY);
                return Json(new { success = data, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);


            }
            else if (serviceOption == "budgetCategory")
            {
                var data = await _budgetCategoryServices.Delete(KEY);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

            }

            else
            {
                return null;
            }

        }
    }
}