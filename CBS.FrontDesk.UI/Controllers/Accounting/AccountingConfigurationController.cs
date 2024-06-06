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
using CBS.FrontDesk.Data.Entity;
using DocumentFormat.OpenXml.Office2010.Word;
using System.Data;
using System.IO;
using CBS.FrontDesk.UI.Models;
using System.Web.UI.WebControls;
using OfficeOpenXml;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using CBS.BusinessService.Config;
using System.Xml.Linq;
using CBS.FrontDesk.Data.Entity.Config;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    [CheckSessionTimeOutAttribute]
    public class AccountingConfigurationController : BaseController
    {
        private readonly AccountingEntryRuleService _Service;

        private readonly OperationEventAttributeServices _OperationEventAttributeService;
        private readonly ChartOfAccountServices _chartOfAccountServices;
        private readonly AccountingEntryRuleService _accountingEntryRuleService;
        private readonly BranchServices _branchService;
        private readonly OperationEventServices _OperationEventService;
        private readonly AccountingServices _AccountServices;
        private readonly AccountClassServices _AccountClassServices;
        private readonly AccountingRuleService _AccountingRuleServices;
        private readonly AccountTypeServices _AccountTypeServices;
        private readonly ChartOfAccountManagementPositionService _ChartOfAccountManagementPositionServicesServices;
        private readonly AccountCategoryServices _AccountCategoryServices;
        private readonly StatementModelServices _statementModelServices;
        private readonly TrialBalanceReferenceServices _trialBalanceReferenceServices;
        public AccountingConfigurationController()
        {
            _AccountingRuleServices = new AccountingRuleService();
            _Service = new AccountingEntryRuleService();
            _OperationEventAttributeService = new OperationEventAttributeServices();
            _chartOfAccountServices = new ChartOfAccountServices();
            _accountingEntryRuleService = new AccountingEntryRuleService();
            _branchService = new BranchServices();
            _OperationEventService = new OperationEventServices();
            _AccountServices = new AccountingServices();
            _AccountTypeServices = new AccountTypeServices();
            _AccountCategoryServices = new AccountCategoryServices();
            _trialBalanceReferenceServices = new TrialBalanceReferenceServices();
            _statementModelServices = new StatementModelServices();
            _ChartOfAccountManagementPositionServicesServices = new ChartOfAccountManagementPositionService();
            _AccountClassServices = new AccountClassServices();
        }
        // GET: AccountingConfiguration

        public async Task<ActionResult> Index()
        {
            await GetList();
            return View(new AccountingConfiguration());
        }
    
        public async Task<ActionResult> AccountUpload()
        {
            ViewBag.Branches = BuildMenuISViewBag((await _branchService.GetBranches()).ToList());
            ViewBag.BankName= _branchService.GetBankName();
            return View(new AccountingConfiguration { BranchId = _branchService.GetBranchID() });
        }


        private async Task GetList()
        {
            ViewBag.AccountTypes = await GetAccountTypesAsync();

            var DebitAccounts = await _AccountServices.GetAllAccounting();
            var CreditAccounts = BuildMenuViewBag(DebitAccounts);
            ViewBag.Accounts = CreditAccounts;
            var listAccounts = await _chartOfAccountServices.GetAllChartOfAccounts();
            ViewBag.OperationEvent = await _OperationEventService.GetOperationEvents();
            ViewBag.ChartOfAccountManagementPositions = BuildMenuAccountViewBag((await _ChartOfAccountManagementPositionServicesServices.GetChartOfAccountManagementPositions()).ToList(), listAccounts.ToList());

            ViewBag.ChartOfAccounts = BuildMenuAccountViewBag(listAccounts.ToList());
            ViewBag.AccountingRuleEntries = BuildAccountingRuleEntryViewBag((await _accountingEntryRuleService.GetAccountingRuleEntries()).ToList());
            ViewBag.BookingDirections = await this.GetBookingDirections();
            ViewBag.OperationEventAttributes = await _OperationEventAttributeService.GetOperationEventAttributes();
            ViewBag.CreditAccounts = ViewBag.ChartOfAccounts;
            ViewBag.DebitAccounts = ViewBag.ChartOfAccounts;
            ViewBag.AccountCartegories = await _AccountCategoryServices.GetAccountCategory();
            ViewBag.Document_type = BuildMenuViewBag();
            ViewBag.Document_Sub_type = BuildMenuISViewBag();
            ViewBag.OperationSide = BuildMenuViewBagopside();
            ViewBag.ChartOfAccountReport = BuildMenuAccountViewBag(listAccounts.ToList());
        }

        private dynamic BuildMenuViewBag(string DocumentId)
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();
            if ((DocumentId == "Income Statement") || (DocumentId == "Expense Statement"))
            {
                selectListItems = BuildMenuISViewBag();
            }
            else
            {
                selectListItems = BuildMenuBSViewBag();
            }
            return Json(selectListItems, JsonRequestBehavior.AllowGet);
        }
        private dynamic BuildMenuISViewBag()
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>
            {
            new System.Web.WebPages.Html.SelectListItem { Value = "Income Statement", Text = "Income Statement" },
            new System.Web.WebPages.Html.SelectListItem { Value = "Expense Statement", Text = "Expense Statement" }

            };
            return selectListItems;
        }
        private dynamic BuildMenuISViewBag(List<Branch> listOfItems)
        {
           List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();
            foreach (var item in listOfItems)
            {
                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value = $"{item.BranchCode} - {item.Name}" });
            }
            return selectListItems;
 
        }
        private dynamic BuildMenuViewBag()
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>
            {
            new System.Web.WebPages.Html.SelectListItem { Value = "PROFIT AND LOSS", Text = "PANDL" },
            new System.Web.WebPages.Html.SelectListItem { Value = "BALANCE SHEET", Text = "BS" }

            };
            return selectListItems;
        }
        private dynamic BuildMenuBSViewBag()
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>
            {

            new System.Web.WebPages.Html.SelectListItem { Value = "Balance SheetAsset", Text = "Balance SheetAsset" },
            new System.Web.WebPages.Html.SelectListItem { Value = "BalanceSheet Liabilities", Text = "BalanceSheet Liabilities" }
            };
            return selectListItems;
        }

        private dynamic BuildMenuViewBagopside()
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>
            {
            new System.Web.WebPages.Html.SelectListItem { Value = "Debit", Text = "Debit" },
            new System.Web.WebPages.Html.SelectListItem { Value = "Credit", Text = "Credit" },
             new System.Web.WebPages.Html.SelectListItem { Value = "NONE", Text = "NONE" }

            };
            return selectListItems;
        }
        private dynamic BuildMenuAccountViewBag(List<ChartofAccountManagementPosition> ChartofAccountManagementPositions, List<ChartOfAccount> ListchartOfAccounts)
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();
            string code = "[BranchCode]";
            var listOfItems = (from item in ChartofAccountManagementPositions
                               join element in ListchartOfAccounts on item.ChartOfAccountId equals element.Id
                               select new ManagementSelectionOption
                               {
                                   Id = item.Id,
                                   AccountNumber = element.AccountNumber.PadRight(6, '0'),
                                   PositionNumber = item.PositionNumber.PadRight(3, '0'),
                                   Description = item.Description,
                                   GeneralRepresentation = element.AccountNumber.PadRight(6, '0') + code + item.PositionNumber.PadRight(3, '0')

                               }).ToList();
            foreach (var item in listOfItems)
            {
                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value = $"{item.Description} - {item.AccountNumber}[BCD]{item.PositionNumber}" });
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
        private dynamic BuildAccountingRuleEntryViewBag(List<Data.Entity.Accounting.AccountingRuleEntryx> ListchartOfAccounts)
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();

            foreach (var item in ListchartOfAccounts)
            {
                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value = $"{item.AccountingRuleEntryName}" });
            }
            return selectListItems;
        }
        private List<StringValues> BuildStringValuesViewBag(List<ChartOfAccount> ListchartOfAccounts)
        {
            List<StringValues> selectListItems = new List<StringValues>();

            foreach (var item in ListchartOfAccounts)
            {
                selectListItems.Add(new StringValues { Text = item.Id, Value = $"{item.AccountNumber} - {item.LabelEn}" });
            }
            return selectListItems;
        }
        private async Task<List<System.Web.WebPages.Html.SelectListItem>> GetAccountTypesAsync()
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();
            var models = await _AccountTypeServices.GetAllAccountTypes();
            foreach (var item in models)
            {
                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value = item.name });
            }
            return selectListItems;
        }

        private Task<List<System.Web.WebPages.Html.SelectListItem>> GetBookingDirections()
        {
            var bookingDirections = new System.Web.WebPages.Html.SelectListItem[] { new System.Web.WebPages.Html.SelectListItem { Text = "DEBIT", Value = "DEBIT" }, new System.Web.WebPages.Html.SelectListItem { Text = "CREDIT", Value = "CREDIT" }, new System.Web.WebPages.Html.SelectListItem { Text = "NOT DEFINE", Value = "NOT DEFINE" } }.ToList();
            return Task.FromResult(bookingDirections);
        }
        private dynamic BuildMenuViewBag(IEnumerable<Data.Account> debitAccounts)
        {
            List<System.Web.WebPages.Html.SelectListItem> list = new List<System.Web.WebPages.Html.SelectListItem>();
            if (debitAccounts != null)

                foreach (var item in debitAccounts)
                {

                    list.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value = item.AccountNumber + "-" + item.AccountName });

                }

            return list;
        }

        public async Task<ActionResult> GetAccountNUMBERByDOCUMENTYPE(string DocumentId)
        {

            List<ChartOfAccount> chartOfAccounts = new List<ChartOfAccount>();

            try
            {
                //var number = chartOfAccountNumber.Length==1? chartOfAccountNumber: chartOfAccountNumber.Substring(0, 1);
                List<ChartOfAccount> data = (await _chartOfAccountServices.GetAllChartOfAccounts()).ToList();
                if (DocumentId == "liability")
                {
                    var filteredAccounts = data.Where(account => account.AccountNumber.StartsWith("4") || account.AccountNumber.StartsWith("3")).ToList();

                    data = filteredAccounts;
                }
                else if (DocumentId == "asset")
                {
                    var filteredAccounts = data.Where(account => account.AccountNumber.StartsWith("1") || account.AccountNumber.StartsWith("2") || account.AccountNumber.StartsWith("5")).ToList();

                    data = filteredAccounts;
                }
                else if (DocumentId == "expenseStatement")
                {
                    var filteredAccounts = data.Where(account => account.AccountNumber.StartsWith("6")).ToList();

                    data = filteredAccounts;
                }
                else if (DocumentId == "incomeStatement")
                {
                    var filteredAccounts = data.Where(account => account.AccountNumber.StartsWith("7")).ToList();

                    data = filteredAccounts;
                }
                var dataList = new List<StringValues>();
                dataList.AddRange(BuildStringValuesViewBag(data.ToList()));
                return Json(dataList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<ActionResult> GetAccountCartegoryById(string Id)
        {


            try
            {
                //var number = chartOfAccountNumber.Length==1? chartOfAccountNumber: chartOfAccountNumber.Substring(0, 1);
                var data = await _chartOfAccountServices.GetChartOfAccountById(Id);
                var dataList = new List<StringValues>();
                if (data.LabelEn== "BALANCING_ACCOUNT")
                {
                    var dataModel = (await _AccountCategoryServices.GetAccountCategory()).FirstOrDefault();
                    dataList.Add(new StringValues { Text = dataModel.Name, Value = dataModel.Id });

                }
                else
                {
                    if (data.AccountNumber.StartsWith("4") || data.AccountNumber.StartsWith("3"))
                    {
                        var datas = (await _AccountCategoryServices.GetAccountCategory()).Where(x => x.Name.ToLower() == "asset"|| x.Name.ToLower() == "liability").ToList();
                        foreach (var item in datas)
                        {

                            dataList.Add(new StringValues { Text = item.Name, Value = item.Id });
                        }
                    }
                    else
                    {
                        dataList.Add(new StringValues { Text = (await _AccountCategoryServices.GetAccountCategory(data.AccountCartegoryId)).Name, Value = data.AccountCartegoryId });

                    }

                }
                return Json(dataList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<ActionResult> GetOperationEventAttribute(string operationEventId)
        {


            try
            {
                var data = await _OperationEventAttributeService.GetOperationEventAttributes();
                var dataList = data.Where(x => x.OperationEventId.Equals(operationEventId)).ToList();
                return Json(dataList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddOrUpdateRole(AccountingConfiguration model)
        {
            ExecutionMessages data = new ExecutionMessages();
            List<AccountingRule> list = new List<AccountingRule>();
            try
            {
                if (this.HttpContext.Session["items" + this.HttpContext.Session.SessionID] != null)
                {
                    list = (List<AccountingRule>)this.HttpContext.Session["items" + this.HttpContext.Session.SessionID];
                    model.AccountingRule.Id = list.Count().ToString();
                    list.Add(model.AccountingRule);
                    this.HttpContext.Session["items" + this.HttpContext.Session.SessionID] = list;
                }
                else
                {
                    model.AccountingRule.Id = "0";
                    list.Add(model.AccountingRule);
                    this.HttpContext.Session["items" + this.HttpContext.Session.SessionID] = list;
                }


                return Json(new { success = true, status = "true", message = "Creation was successfull" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpPost]
        public async Task<ActionResult> AddAccountingRole(AccountingConfiguration model)
        {
            ExecutionMessages data = new ExecutionMessages();
            List<AccountingRule> list = new List<AccountingRule>();
            try
            {
                if (this.HttpContext.Session["items" + this.HttpContext.Session.SessionID] != null)
                {
                    list = (List<AccountingRule>)this.HttpContext.Session["items" + this.HttpContext.Session.SessionID];
            
                    var modelAcc= new AccountingRuleXRoot();
                    foreach (var item in list) 
                    {
                        modelAcc.accountingRules.Add(new AccountingRuleX
                        {
                            accountingEntryRuleId = item.AccountingEntryRuleId,
                            bookingDirection = item.BookingDirection,
                            ruleName = item.RuleName
                        });
                    }
                    _AccountingRuleServices.Create(modelAcc);
                }
                else
                {
                    model.AccountingRule.Id = "0";
                    list.Add(model.AccountingRule);
                    this.HttpContext.Session["items" + this.HttpContext.Session.SessionID] = list;
                }


                return Json(new { success = true, status = "true", message = "Creation was successfull" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<ActionResult> DeleteAccountingRole(string  Id)
        {
            ExecutionMessages data = new ExecutionMessages();
            List<AccountingRule> list = new List<AccountingRule>();
            List<AccountingRuleDtos> AccountingRuleDtos = new List<AccountingRuleDtos>();
            try
            {
                if (this.HttpContext.Session["items" + this.HttpContext.Session.SessionID] != null)
                {
                    list = (List<AccountingRule>)this.HttpContext.Session["items" + this.HttpContext.Session.SessionID];
                    var objectmodel = list.Find(c=>c.Id== Id);
                    list.Remove(objectmodel);
                    this.HttpContext.Session["items" + this.HttpContext.Session.SessionID] = list;
                    var dataList = await _Service.GetAccountingEntryRules();
                    var ChartofAccountManagementPositions = (await _ChartOfAccountManagementPositionServicesServices.GetChartOfAccountManagementPositions()).ToList();

                    var listOfItems = (from d in list
                                       join e in dataList on d.AccountingEntryRuleId equals e.Id
                                       join f in ChartofAccountManagementPositions on e.DeterminationAccountId equals f.Id
                                       join h in ChartofAccountManagementPositions on e.BalancingAccountId equals h.Id

                                       select new AccountingRuleDtos
                                       {
                                           Id = d.Id,
                                           RuleName = d.RuleName,
                                           BookingDirection = d.BookingDirection,
                                           DeterminantAccount = f.Id + "@" + f.ChartOfAccountId,
                                           BalancingAccount = h.Id + "@" + h.ChartOfAccountId,
                                       }).ToList();

                    foreach (var item in listOfItems)
                    {
                        item.DeterminantAccount = await GetAccountNumberWithMangementPositon(item.DeterminantAccount.Split('@')[0], item.DeterminantAccount.Split('@')[1]);
                        item.BalancingAccount = await GetAccountNumberWithMangementPositon(item.BalancingAccount.Split('@')[0], item.BalancingAccount.Split('@')[1]);
                        AccountingRuleDtos.Add(item);
                    }

                    return Json(AccountingRuleDtos, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new AccountingRole(), JsonRequestBehavior.AllowGet);
                }


          
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
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
            else if (model.ServiceOption == "accountingRule")
            {

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
            else if (model.ServiceOption == "trialbalancereference")
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
            else if (model.ServiceOption == "statementModel")
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
            else if (model.ServiceOption == "chartOfAccountManagementPosition")
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
        private async Task<Func<Task<ExecutionMessages>>> GetInsertServiceActionAsync(string serviceOption, AccountingConfiguration model)
        {
            //
            if (serviceOption == "account")
            {
                var chartOfAccount = await _chartOfAccountServices.GetChartOfAccountByAccountNumber(model.Account.AccountNumber.Substring(0, model.Account.AccountNumber.Length - 1));
                if (chartOfAccount == null)
                {

                }
                else
                {
                    model.Account.AccountCategoryId = chartOfAccount.AccountCartegoryId;
                }
                return () => _AccountServices.Create(model.Account);
            }
            else if (serviceOption == "chartOfAccountManagementPosition")
            {
                return () => _ChartOfAccountManagementPositionServicesServices.Create(model.ChartofAccountManagementPosition);
            }
            else if (serviceOption == "accountingRule")
            {
                return () => _AccountingRuleServices.Create(model.AccountingRule);
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
            else if (serviceOption == "trialbalancereference")
            {
                return () => _trialBalanceReferenceServices.Create(model.TrialBalanceReference);
            }
            else if (serviceOption == "statementModel")
            {
                return () => _statementModelServices.Create(model.IncomeStatement);
            }
            else if (serviceOption == "chartOfAccount")
            {
                ChartOfAccount mode = null;
                int numberLength = 1;
                do
                {
                    string accNum = model.ChartOfAccount.AccountNumber.Substring(0, model.ChartOfAccount.AccountNumber.Length - numberLength);
                    mode = await _chartOfAccountServices.GetChartOfAccountByAccountNumber(accNum);
                    numberLength++;
                } while (mode == null);

                if (model.ChartOfAccount.IsForUpdate == false)
                {
                    ChartOfAccountDto modelc = new ChartOfAccountDto
                    {
                        RootParentId = mode.Id,
                        LabelEn = model.ChartOfAccount.LabelEn,
                        LabelFr = model.ChartOfAccount.LabelFr,
                        IsBalanceAccount = model.ChartOfAccount.IsBalanceSheetAccount,
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
            else if (serviceOption == "chartOfAccountManagementPosition")
            {
                return () => _ChartOfAccountManagementPositionServicesServices.Update(model.ChartofAccountManagementPosition);
            }
            else if (serviceOption == "chartOfAccount")
            {

                return () => _chartOfAccountServices.Update(model.ChartOfAccount);
            }
            else if (serviceOption == "accountType")
            {

                return () => _AccountTypeServices.Update(model.AccountType);
            }
            else if (serviceOption == "statementModel")
            {

                return () => _statementModelServices.Update(model.IncomeStatement);
            }
            else if (serviceOption == "trialbalancereference")
            {

                return () => _trialBalanceReferenceServices.Update(model.TrialBalanceReference);
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

                    return PartialView(partialView, new AccountingConfiguration { Account = new Data.Account() });
                }
                else if (path == "details")
                {
                    var data = await _AccountServices.GetAccount(key);
                    var chartOfAccount = await _chartOfAccountServices.GetChartOfAccountByAccountNumber(data.AccountNumber);

                    if (chartOfAccount == null)
                    {
                        chartOfAccount = new ChartOfAccount
                        {
                            Id = key,
                            AccountNumber = key
                        };
                    }
                    var chartOfAccountf = await _chartOfAccountServices.GetChartOfAccountByAccountNumber(data.AccountNumber.Substring(0, data.AccountNumber.Length - 1));

                    if (chartOfAccountf == null)
                    {
                        chartOfAccountf = new ChartOfAccount
                        {
                            Id = key,
                            AccountNumber = key
                        };
                    }
                    return PartialView(partialView, new AccountingConfiguration { Account = data });

                }
                else if (path == "upload")
                {

                    return PartialView(partialView, new AccountingConfiguration { Accounts = new List<Data.Account>() });

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
                    if (chartOfAccount == null)
                    {
                        chartOfAccount = new ChartOfAccount
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
                    return PartialView(partialView, new AccountingConfiguration { ChartOfAccount = data });

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
                    var DebitAccounts = await _ChartOfAccountManagementPositionServicesServices.GetChartOfAccountManagementPositions();
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
                    return PartialView(partialView, new AccountingConfiguration { AccountingRuleEntry = data });
                }

            }
            else if (serviceOption == "accountType")
            {
                if (path == "list")
                {

                    var data = await _AccountTypeServices.GetAllAccountTypes();

                    var sysData = new AccountingConfiguration { AccountTypes = data.ToList() };
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
            else if (serviceOption == "trialbalancereference")
            {
                if (path == "list")
                {

                    var data = await _trialBalanceReferenceServices.GetAllTrialBalanceReference();

                    var sysData = new AccountingConfiguration { TrialBalanceReferences = data.ToList() };
                    return PartialView(partialView, sysData);

                }
                else if (path == "new")
                {
                    var data = await _statementModelServices.GetStatementModel(key);
                    return PartialView(partialView, new AccountingConfiguration { ServiceOption = "trialbalancereference", IncomeStatement = data, TrialBalanceReference = new TrialBalanceReference { StatementModelId = data.Id } });
                }
                else
                {
                    var data = await _trialBalanceReferenceServices.GetTrialBalanceReference(key);
                    var datast = await _statementModelServices.GetStatementModel(data.StatementModelId);
                    return PartialView(partialView, new AccountingConfiguration { ServiceOption = "trialbalancereference", TrialBalanceReference = data, IncomeStatement = datast });
                }

            }
            else if (serviceOption == "statementmodel")
            {
                if (path == "list")
                {

                    var data = await _statementModelServices.GetAllStatementModel();

                    var sysData = new AccountingConfiguration { IncomeStatements = data.ToList() };
                    return PartialView(partialView, sysData);

                }
                else if (path == "new")
                {
                    return PartialView(partialView, new AccountingConfiguration { });
                }
                else if (path == "details")
                {

                    var data = await _statementModelServices.GetStatementModel(key);
                    var datas = (await _trialBalanceReferenceServices.GetAllTrialBalanceReference()).Where(c => c.StatementModelId == data.Id).ToList();
                    var chartOfAccounts = (await _chartOfAccountServices.GetAllChartOfAccounts());
                    var result = from tr in datas
                                 join acc in chartOfAccounts
                                 on tr.ChartOfAccountId equals acc.Id

                                 select new TrialBalanceReference
                                 {
                                     Id = tr.Id,
                                     ChartOfAccountId = tr.ChartOfAccountId,
                                     AccountInfo = acc.AccountNumber.PadRight(6,'0' ) + "-" + acc.LabelEn,
                                     //OperationSide = tr.OperationSide,
                                     StatementModelId = data.Id
                                 };

                    return PartialView(partialView, new AccountingConfiguration { IncomeStatement = data, TrialBalanceReferences = result.ToList() });
                }
                else
                {
                    var data = await _statementModelServices.GetStatementModel(key);
                    var AccountIds = (from d in (await _trialBalanceReferenceServices.GetAllTrialBalanceReference())
                                      select d.ChartOfAccountId).ToList();
                    data.AccountIds = AccountIds;
                    //data.OperationSide= 

                    return PartialView(partialView, new AccountingConfiguration { IncomeStatement = data });
                }

            }
            else if (serviceOption == "chartOfAccountManagementPosition")
            {
                if (path == "list")
                {
                    string code = "[BranchCode]";
                    var dataChart = await _chartOfAccountServices.GetAllChartOfAccounts();
                    var ChartofAccountManagementPositions = await _ChartOfAccountManagementPositionServicesServices.GetChartOfAccountManagementPositions();
                    var listOfItems = (from item in ChartofAccountManagementPositions
                                       join element in dataChart on item.ChartOfAccountId equals element.Id
                                       select new ManagementSelectionOption
                                       {
                                           Id = item.Id,
                                           AccountNumber = element.AccountNumber.PadRight(6, '0'),
                                           PositionNumber = item.PositionNumber.PadRight(3, '0'),
                                           Description = item.Description,
                                           GeneralRepresentation = element.AccountNumber.PadRight(6, '0') + code + item.PositionNumber.PadRight(3, '0')

                                       }).ToList();
                    var sysData = new AccountingConfiguration { ChartofAccountManagementPositionDtos = listOfItems.ToList() };
                    return PartialView(partialView, sysData);

                }
                else if (path == "new")
                {
                    return PartialView(partialView, new AccountingConfiguration { });
                }

                else
                {
                    var data = await _ChartOfAccountManagementPositionServicesServices.GetChartOfAccountManagementPosition(key);

                    //data.OperationSide= 

                    return PartialView(partialView, new AccountingConfiguration { ChartofAccountManagementPosition = data });
                }

            }
            else if (serviceOption == "accountingRule")
            {
                if (path == "list")
                {
                    List<AccountingRule> list = new List<AccountingRule>();
                    List<AccountingRuleDtos> AccountingRuleDtos = new List<AccountingRuleDtos>();
                    if (this.HttpContext.Session["items" + this.HttpContext.Session.SessionID] != null)
                    {
                        list = (List<AccountingRule>)this.HttpContext.Session["items" + this.HttpContext.Session.SessionID];

                    }
                    var dataList = await _Service.GetAccountingEntryRules();
                    var ChartofAccountManagementPositions = (await _ChartOfAccountManagementPositionServicesServices.GetChartOfAccountManagementPositions()).ToList();

                    var listOfItems = (from d in list
                                       join e in dataList on d.AccountingEntryRuleId equals e.Id
                                       join f in ChartofAccountManagementPositions on e.DeterminationAccountId equals f.Id
                                       join h in ChartofAccountManagementPositions on e.BalancingAccountId equals h.Id

                                       select new AccountingRuleDtos
                                       {
                                           Id = d.Id,
                                           RuleName= d.RuleName,
                                           BookingDirection = d.BookingDirection,
                                           DeterminantAccount =   f.Id+"@"+f.ChartOfAccountId,
                                           BalancingAccount = h.Id + "@" + h.ChartOfAccountId,
                                       }).ToList();

                    foreach (var item in listOfItems)
                    {
                        item.DeterminantAccount = await GetAccountNumberWithMangementPositon(item.DeterminantAccount.Split('@')[0], item.DeterminantAccount.Split('@')[1]);
                        item.BalancingAccount = await GetAccountNumberWithMangementPositon(item.BalancingAccount.Split('@')[0], item.BalancingAccount.Split('@')[1]);
                        AccountingRuleDtos.Add(item);
                    }

                    var sysData = new AccountingConfiguration { AccountingRules = AccountingRuleDtos };
                    return PartialView(partialView, sysData);

                }
                else if (path == "new")
                {
                    return PartialView(partialView, new AccountingConfiguration { });
                }
                else
                {
                  


                    return PartialView(partialView, new AccountingConfiguration { ChartofAccountManagementPosition = new ChartofAccountManagementPosition() });
                }

            }
            return null;
        }

        private async Task<string > GetAccountNumberWithMangementPositon(string ChartOfAccountId, string chartOfAccountPositionId)
        {
            string accountNumber = string.Empty;
            string branchCode = "[BC]";
            var f = await _ChartOfAccountManagementPositionServicesServices.GetChartOfAccountManagementPosition(ChartOfAccountId);
            if (f == null)
            {
                return "Empty";
            }
            var chartofAccount = await  _chartOfAccountServices.GetChartOfAccountById(chartOfAccountPositionId);
            accountNumber = $"{chartofAccount.AccountNumber.PadRight(6, '0')}[BC]{f.PositionNumber.PadRight(3, '0')}";
            return accountNumber;
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
            else if (serviceOption == "statementmodel")
            {

                var data = await _statementModelServices.Delete(KEY);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

            }
            else if (serviceOption == "trialbalancereference")
            {

                var data = await _trialBalanceReferenceServices.Delete(KEY);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return null;
            }

        }


        public async Task<ActionResult> UploadAccountModel(AccountUploadModel model)
        {
            List<AccountModel> models = new List<AccountModel>();

            if (ModelState.IsValid)
            {
                if (model.ExcelFile.ContentType == "application/vnd.ms-excel" || model.ExcelFile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    string filePath = Path.GetTempFileName();

                    // Save the uploaded file to the file system
                    model.ExcelFile.SaveAs(filePath);

                    // Read data from the Excel file
                    DataTable dt = ReadExcelFile(filePath);

                    // Process the data from the DataTable
                    models = await ProcessExcelDataAsync(dt);

                    // Delete the temporary file
                    System.IO.File.Delete(filePath);
                }
                else
                {
                    ModelState.AddModelError("ExcelFile", "Please upload an Excel file.");
                }
            }
            else
            {
                return Json("Please upload an Excel file.", JsonRequestBehavior.AllowGet);
            }
            // Check if the uploaded file is an Excel file

            return Json(models, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public async Task<ActionResult> UploadAccountsAsync(string branchId)
        {
            UploadAccountCommand model = new UploadAccountCommand();
           
            if (this.HttpContext.Session["account" + this.HttpContext.Session.SessionID] != null)
            {
                 model= (UploadAccountCommand)this.HttpContext.Session["account" + this.HttpContext.Session.SessionID];
                model.BranchId = branchId;
                model.BranchCode = (await _branchService.GetBranchByBankID(branchId)).BranchCode;
                var node= _AccountServices.Create(model);
            }

            return Json(true, JsonRequestBehavior.AllowGet);

        }


        private DataTable ReadExcelFile(string filePath)
        {
            DataTable dt = new DataTable();
            // Set the license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new OfficeOpenXml.ExcelPackage(filePath))
            {
                // Get the first worksheet
                var worksheet = package.Workbook.Worksheets.First();

                // Add columns to the DataTable
                try
                {
                    for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                    {
                        dt.Columns.Add(new DataColumn(worksheet.Cells[1, col].Value.ToString()));
                    }
                }
                catch (Exception ex)
                {

                    throw(ex);
                }

                // Populate the DataTable with data from the worksheet
                for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                {
                    DataRow dr = dt.NewRow();
                    for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                    {
                        dr[col - 1] = worksheet.Cells[row, col].Value;
                    }
                    dt.Rows.Add(dr);
                }
            }

            return dt;
        }

        private async Task<List<AccountModel>> ProcessExcelDataAsync(DataTable dt)
        {
            List<AccountModel> models = new List<AccountModel>();
            var model = new UploadAccountCommand();
            foreach (DataRow row in dt.Rows)
            {
                //Account Number	Account name	BEG.DEBIT	BEG.CREDIT	MOV.DEBIT	MOV.CREDIT	End.DEBIT	End.CREDIT	BranchCode
                string accountNumber = row["Account Number"].ToString();
                string accountName = row["Account name"].ToString();
                string beginningDebit = row["BEG.DEBIT"].ToString();
                string beginningCredit = row["BEG.CREDIT"].ToString();
                string movementDebit = row["MOV.DEBIT"].ToString();
                string movementCredit = row["MOV.CREDIT"].ToString();
                string endDebit = row["End.DEBIT"].ToString();
                string endCredit = row["End.CREDIT"].ToString();
                string branchCode = row["BranchCode"].ToString();
              
                models.Add(new AccountModel(accountNumber, accountName, accountNumber.Substring(0,6), DateTime.Today.ToString("yyyy-MM-dd"), (Convert.ToDecimal(beginningCredit)- Convert.ToDecimal(beginningDebit)),( Convert.ToDecimal(endCredit)- Convert.ToDecimal(endDebit)), branchCode));
            }
            model.AccountModelList = models;
            //this.HttpContext.Session["account" + this.HttpContext.Session.SessionID] = model;
         var listOfBranches=   (await _branchService.GetBranches()).ToList();
            var modelBR = listOfBranches.Find(x => x.BranchCode ==  models[0].BranchCode);
            model.BranchId = modelBR.Id;
            model.BranchCode = modelBR.BranchCode;
            var node = _AccountServices.Create(model);
            return models;
        }

    }

}