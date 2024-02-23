using CBS.BusinessService.Accounting;
using CBS.BusinessService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using CBS.FrontDesk.Data.Entity.Accounting;
using System.Web.Services.Description;
using CBS.FrontDesk.Data;
using System.Reflection;
using System.Web.WebPages.Html;
using Microsoft.Ajax.Utilities;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    public class AccountingConfigurationController : BaseController
    {
        private readonly AccountingEntryRuleService _Service;

        private readonly OperationEventAttributeServices _OperationEventAttributeService;
        private readonly ChartOfAccountServices _chartOfAccountServices;
        private readonly AccountingEntryRuleService _accountingEntryRuleService;
        private readonly OperationEventServices _OperationEventService;
        private readonly AccountingServices _AccountServices;
        private readonly AccountTypeServices _AccountTypeServices;

        private readonly AccountCategoryServices _AccountCategoryServices;

        public AccountingConfigurationController()
        {
            _Service = new AccountingEntryRuleService();
            _OperationEventAttributeService = new OperationEventAttributeServices();
            _chartOfAccountServices = new ChartOfAccountServices();
            _accountingEntryRuleService = new AccountingEntryRuleService();
            _OperationEventService = new OperationEventServices();
            _AccountServices = new AccountingServices();
            _AccountTypeServices = new AccountTypeServices();
            _AccountCategoryServices = new AccountCategoryServices();
        }
        // GET: AccountingConfiguration

        public async Task<ActionResult> Index()
        {
            await GetList();
            return View(new AccountingConfiguration());
        }

        private async Task GetList()
        {
            ViewBag.AccountTypes = await GetAccountTypesAsync();
          
            var DebitAccounts = await _AccountServices.GetAllAccounting();
            var CreditAccounts = BuildMenuViewBag(DebitAccounts);
            ViewBag.Accounts = CreditAccounts;
            var listAccounts = await _chartOfAccountServices.GetAllChartOfAccounts();
            ViewBag.ChartOfAccounts= BuildMenuAccountViewBag(listAccounts.ToList());
            ViewBag.OperationEvent = await _OperationEventService.GetOperationEvents();
            ViewBag.BookingDirections = await this.GetBookingDirections();
            ViewBag.OperationEventAttributes = await _OperationEventAttributeService.GetOperationEventAttributes();
            ViewBag.CreditAccounts = CreditAccounts;
            ViewBag.DebitAccounts = CreditAccounts;
            ViewBag.AccountCartegories = await _AccountCategoryServices.GetAccountCategory();
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

        private async Task<List<System.Web.WebPages.Html.SelectListItem>> GetAccountTypesAsync()
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();
            var models =await _AccountTypeServices.GetAllAccountTypes();
            foreach ( var item in models )
            {
                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value = item.name });
            }
            return selectListItems;
        }

        private Task<List<System.Web.WebPages.Html.SelectListItem>> GetBookingDirections()
        {
            var bookingDirections = new System.Web.WebPages.Html.SelectListItem[] { new System.Web.WebPages.Html.SelectListItem { Text = "DEBIT", Value = "DEBIT" }, new System.Web.WebPages.Html.SelectListItem { Text = "CREDIT", Value = "CREDIT" } }.ToList();
            return Task.FromResult(bookingDirections);
        }
        private dynamic BuildMenuViewBag(IEnumerable<Data.Account> debitAccounts)
        {
            List<System.Web.WebPages.Html.SelectListItem> list = new List<System.Web.WebPages.Html.SelectListItem>();
            foreach (var item in debitAccounts)
            {

                list.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value = item.AccountNumber + "-" + item.AccountHolder });

            }

            return list;
        }

        public async Task<ActionResult> GetOperationEventAttribute(string operationEventId)
        {
            

            try
            {
               var data = await _OperationEventAttributeService.GetOperationEventAttributes();
                 var dataList = data.Where(x=>x.OperationEventId.Equals(operationEventId)).ToList();
                return Json(dataList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(AccountingConfiguration model)
        {
            Func<Task<ExecutionMessages>> serviceAction = null;

            if (model.ServiceOption == "account")
            {
                //return () => _AccountServices.Create(model.Account);
                if (model.ChartOfAccount.IsForUpdate)
                {

                    serviceAction = GetUpdateServiceAction(model.ServiceOption, model);
                }
                else if (model.ChartOfAccount.IsForUpdate == false)
                {
                    serviceAction = await GetInsertServiceActionAsync(model.ServiceOption, model);
                }


            }
            else if (model.ServiceOption == "accountType")
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
            else if (model.ServiceOption == "operationEvent")
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
            else if (model.ServiceOption == "operationEventAttribute")
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
            else if (model.ServiceOption == "accountingRuleEntry")
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
            else if (model.ServiceOption == "chartOfAccount")
            {
                if (model.ChartOfAccount.IsForUpdate)
                {

                    serviceAction = GetUpdateServiceAction(model.ServiceOption, model);
                }
                else if (model.ChartOfAccount.IsForUpdate == false)
                {
                    serviceAction = await GetInsertServiceActionAsync(model.ServiceOption, model);
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
        private async Task<Func<Task<ExecutionMessages>>> GetInsertServiceActionAsync(string serviceOption, AccountingConfiguration model)
        {
            if (serviceOption == "account")
            {
                return () => _AccountServices.Create(model.Account);
            }
            else if (serviceOption == "accountType")
            {
                return () => _AccountTypeServices.Create(model.AccountType);
            }
            else if (serviceOption == "operationEvent")
            {
                return () => _OperationEventService.Create(model.OperationEvent);
            }
            else if (serviceOption == "operationEventAttribute")
            {
                return () => _OperationEventAttributeService.Create(model.OperationEventAttribute);
            }
            else if (serviceOption == "accountingRuleEntry")
            {
                return () => _accountingEntryRuleService.Create(model.AccountingRuleEntry);
            }
            else if (serviceOption == "chartOfAccount")
            {
                //ChartOfAccountDto modelc;
                string accNum = model.ChartOfAccount.AccountNumber.Substring(0, model.ChartOfAccount.AccountNumber.Length - 1);
                var mode = await _chartOfAccountServices.GetChartOfAccountByAccountNumber(accNum);
                if (model.ChartOfAccount.IsForUpdate == false)
                {
                    ChartOfAccountDto modelc = new ChartOfAccountDto
                    {
                        RootParentId = mode.Id,
                        LabelEn = mode.LabelEn,
                        LabelFr = mode.LabelFr,
                        IsBalanceAccount = mode.IsBalanceSheetAccount,
                        AccountNumber = model.ChartOfAccount.AccountNumber,
                      CanBeNegative = model.ChartOfAccount.CanBeNegative,
     IsDebit = model.ChartOfAccount.IsDebit,
                       
 AccountCartegoryId = model.ChartOfAccount.AccountCartegoryId
                         
                    };
                    return () => _chartOfAccountServices.Create(modelc);
                }
                else
                {
                    ChartOfAccountDto modelcomp = new ChartOfAccountDto
                    {
                        RootParentId = mode.Id,
                        LabelEn = mode.LabelEn,
                        LabelFr = mode.LabelFr,
                        IsBalanceAccount = mode.IsBalanceSheetAccount,
                        AccountNumber = mode.AccountNumber
                    };
                    return () => _chartOfAccountServices.Create(modelcomp);
                }



            }
            else
            {
                return null;
            }
        }
        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(string serviceOption, AccountingConfiguration model)
        {
            if (serviceOption == "account")
            {

                return () => _AccountServices.Update(model.Account);
            }
            else if (serviceOption == "operationEvent")
            {
                return () => _OperationEventService.Update(model.OperationEvent);
            }
            else if (serviceOption == "operationEventAttribute")
            {
                return () => _OperationEventAttributeService.Update(model.OperationEventAttribute);
            }
            else if (serviceOption == "accountingRuleEntry")
            {
                return () => _accountingEntryRuleService.Update(model.AccountingRuleEntry);
            }
            else if (serviceOption == "chartOfAccount")
            {

                return () => _chartOfAccountServices.Update(model.ChartOfAccount);
            }
            else if (serviceOption == "accountType")
            {

                return () => _AccountTypeServices.Update(model.AccountType);
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
            if (serviceOption == "account")
            {
                if (path == "list")
                {
                    var dataChart = await _chartOfAccountServices.GetAllChartOfAccountTreeNodes();
                    var data = await _AccountServices.GetAllAccounting();
                    var sysData = new AccountingConfiguration { Accounts = data.ToList(), AccountTreeNodes = dataChart.ToList() };
                    return PartialView(partialView, sysData);

                }

                else if (path == "new")
                {
                    var chartOfAccount = await _AccountServices.GetAccount(key);
                    //Data.Account account = new Data.Account
                    //{
                    //    ChartOfAccountId =  chartOfAccount.Id,
                    //    AccountNumber =  chartOfAccount.AccountNumber,

                    //};
                    return PartialView(partialView, new AccountingConfiguration {  });
                }
                else
                {
                    var data = await _AccountServices.GetAccount(key);
                    return PartialView(partialView, new AccountingConfiguration { Account = data });

                }


            }
            else if (serviceOption == "chartOfAccount")
            {
                if (path == "list")
                {
                    var dataChart = await _chartOfAccountServices.GetAllChartOfAccounts();

                    var sysData = new AccountingConfiguration { ChartOfAccounts = dataChart.ToList() };
                    return PartialView(partialView, sysData);

                }

                else if (path == "new")
                {
                    var chartOfAccount = await _chartOfAccountServices.GetChartOfAccountByAccountNumber(key);
                    if (chartOfAccount==null)
                    {
                        chartOfAccount = new  ChartOfAccount
                        {
                            Id = key,
                            AccountNumber = key
                        };
                    }
                     
                    return PartialView(partialView, new AccountingConfiguration { ChartOfAccount = chartOfAccount });
                }
                else
                {
                    var data = await _chartOfAccountServices.GetChartOfAccountById(key);
                    return PartialView(partialView, new AccountingConfiguration { ChartOfAccount =data });

                }


            }

            else if (serviceOption == "operationEvent")
            {
                if (path == "list")
                {
                    var data = await _OperationEventService.GetAllOperationEvent();
                    var sysData = new AccountingConfiguration { OperationEvents = data.ToList() };
                    return PartialView(partialView, sysData);
                }
                else if (path == "new")
                {
                    return PartialView(partialView, new AccountingConfiguration { OperationEvent = new OperationEvent() });
                }
                else
                {
                    var data = await _OperationEventService.GetOperationEvent(key);
                    return PartialView(partialView, new AccountingConfiguration { OperationEvent = data });


                }


            }
            else if (serviceOption == "operationEventAttribute")
            {
                if (path == "list")
                {

                    var OperationEventList = await _OperationEventService.GetOperationEvents();
                    var OperationEventAttributes = await _OperationEventAttributeService.GetOperationEventAttributes();
                    var sysData = new AccountingConfiguration { OperationEventAttributeDtos = ConvertToOperationEventAttributeDtos(OperationEventAttributes, OperationEventList) };
                    return PartialView(partialView, sysData);

                }
                else if (path == "new")
                {
                    return PartialView(partialView, new AccountingConfiguration { OperationEventAttribute = new OperationEventAttribute() });
                }
                else
                {
                    return PartialView(partialView, new AccountingConfiguration { OperationEventAttribute = await _OperationEventAttributeService.GetOperationEventAttribute(key) });

                }


            }
            else if (serviceOption == "accountingEntryRule")
            {
                if (path == "list")
                {

                    var data = await _accountingEntryRuleService.GetAccountingEntryRules();
                    var OperationEventList = await _OperationEventService.GetOperationEvents();
                    var OperationEventAttributes = await _OperationEventAttributeService.GetOperationEventAttributes();
                    var DebitAccounts = await _AccountServices.GetAllAccounting();
                    var dataList = await _Service.GetAccountingEntryRules();
                    var dataModel = await _Service.GetAccountingEntryRulesDto(dataList, OperationEventList, OperationEventAttributes, DebitAccounts);

                    var sysData = new AccountingConfiguration { AccountingRuleEntries = data.ToList(), AccountingRuleEntriesDTOS = dataModel };
                    return PartialView(partialView, sysData);

                }
                else if (path == "new")
                {
                    return PartialView(partialView, new AccountingConfiguration { });
                }
                else
                {
                    
                    var data = await _accountingEntryRuleService.GetAccountingRuleEntryById(key);
                    return PartialView(partialView, new AccountingConfiguration { AccountingRuleEntry =data});
                }

            }
            else if (serviceOption == "accountType")
            {
                if (path == "list")
                {

                    var data = await _AccountTypeServices.GetAllAccountTypes();
               
                    var sysData = new AccountingConfiguration { AccountTypes = data.ToList()};
                    return PartialView(partialView, sysData);

                }
                else if (path == "new")
                {
                    return PartialView(partialView, new AccountingConfiguration { });
                }
                else
                {
                    var data = await _AccountTypeServices.GetAccountType(key);
                    return PartialView(partialView, new AccountingConfiguration { AccountType = data });
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


            if (serviceOption == "account")
            {
                var data = await _AccountServices.Delete(KEY);
                return Json(new { success = data, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);


            }
            else if (serviceOption == "operationEvent")
            {
                var data = await _OperationEventService.Delete(KEY);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

            }
            else if (serviceOption == "operationEventAttribute")
            {
                var data = await _OperationEventAttributeService.Delete(KEY);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

            }
            else if (serviceOption == "accountingRuleEntry")
            {
                var data = await _accountingEntryRuleService.Delete(KEY);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

            }
            else if (serviceOption == "chartOfAccount")
            {

                var data = await _chartOfAccountServices.Delete(KEY);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

            }
            else if (serviceOption == "accountType")
            {

                var data = await _AccountTypeServices.Delete(KEY);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return null;
            }

        }
    }

}