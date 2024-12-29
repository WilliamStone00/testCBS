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
using CBS.BusinessService.UserManagement;
using CBS.BusinessService.Config;
using Newtonsoft.Json;
using Microsoft.AspNet.SignalR.Owin;
using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using Azure;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using CBS.FrontDesk.Helper;

namespace CBS.FrontDesk.UI.Controllers
{
    [CheckSessionTimeOutAttribute]
    public class ManuallyJournalEntryController : BaseController
    {
        private readonly EntryTempDataServices _Service;
        private readonly ChartOfAccountManagementPositionService _ChartOfAccountManagementPositionServicesServices;
        private readonly ChartOfAccountServices _chartOfAccountServices;
        private readonly AccountCategoryServices _accountCategoryServices;
        private readonly UserManagementServices _userService;
        private readonly AccountingServices _AccountServices;
        private readonly AccountingEntryRuleService _accountingEntryRuleService;
        private readonly BranchServices _branchService;
        private readonly AccountingRuleService _AccountingRuleServices;

        public ManuallyJournalEntryController()
        {
            _Service = new EntryTempDataServices();
            _ChartOfAccountManagementPositionServicesServices = new ChartOfAccountManagementPositionService();
            _chartOfAccountServices = new ChartOfAccountServices();
            _accountingEntryRuleService = new AccountingEntryRuleService();
       _AccountServices = new AccountingServices();
            _userService = new UserManagementServices();
            _branchService = new BranchServices();
            _AccountingRuleServices = new AccountingRuleService();
            _accountCategoryServices = new AccountCategoryServices();
                }
        // GET:

        public async Task<ActionResult> Index()
        {
            await GetList();
            return View(new ManuallyJournalEntryDataSet { });
        }
        public async Task<ActionResult> PendingAccountingEntries()
        {
            List<PostedEntry> postedCollectionEntries = new List<PostedEntry>();
            await GetList();
            var PostedEntries = await _Service.GetManualEntriesAsync();
            var users = await _userService.GetUsers();
            var branch = await _branchService.GetBranches();
            var results = (from p in PostedEntries
                           join u in users on p.CreatedBy equals u.id.ToString()
                           join po in users on p.CreatedBy equals po.id.ToString()
                           join b in branch on u.BranchID equals b.Id.ToString()
                           select new PostedEntryX
                           {
                               Amount = Convert.ToDecimal(p.Amount.ToString("N")),
                               BranchCode = b.BranchCode,
                               CreatedBy = u.firstName + " " + u.lastName,
                               IssuedBy = u.id.ToString(),
                               Description = p.Description,
                               CreatedDate = p.CreatedDate,
                               ApprovedBy=po.firstName + " " + po.lastName,
                               ApprovedDate =p.ApprovedDate,
                               Status = p.Status,
                               Id = p.Id,
                               EntryDetail = p.EntryDetail


                           }).ToList();
            this.HttpContext.Session["postedEntryDetails" + _AccountServices.GetUserID()]= results;
            foreach (var item in results)
            {
                postedCollectionEntries.Add(item.ConvertToPostedEntry(item));
            }        
            return View(new ManuallyJournalEntryDataSet { PostedEntries = postedCollectionEntries });
        }

        private async Task GetList()
        {
       
           
            var listAccounts = await _chartOfAccountServices.GetAllChartOfAccounts();
  
            var CreditAccounts = BuildMenuViewBag(await GetAllAccountsExcludingOperationsAccountAsync());
            ViewBag.Accounts = CreditAccounts;
            ViewBag.BookingDirections = await GetBookingDirections();
            ViewBag.ChartOfAccountManagementPositions = BuildMenuCOAccountViewBag((await _ChartOfAccountManagementPositionServicesServices.GetChartOfAccountManagementPositions()).ToList(), listAccounts.ToList());
        

        }
        private dynamic BuildMenuCOAccountViewBag(List<ChartofAccountManagementPosition> ChartofAccountManagementPositions, List<ChartOfAccount> ListchartOfAccounts)
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();
            string code = "[BCD]";
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
                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value = $"{item.Description} - {item.AccountNumber}[BCD]{item.PositionNumber}-{item.Id}" });
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

        private async Task<List<Data.Account>> GetAllAccountsExcludingOperationsAccountAsync()
        {
            var accounts = await _AccountServices.GetAllAccounting();
            var accountingRules = (await _accountingEntryRuleService.GetAccountingEntryRules()).ToList();

            return accounts.Where(account =>
                !CheckIfAccountIsOperationsAccount(account, accountingRules).Result)
                .ToList();
        }

        private Task<bool> CheckIfAccountIsOperationsAccount(Data.Account account, List<AccountingRuleEntry> accountingRules)
        {
            const string OPERATIONS_PREFIX_1 = "3";
            const string OPERATIONS_PREFIX_2 = "571";

            var virtualTellerCodes = new[] {
        "Virtual_Teller_MTN",
        "Virtual_Teller_Orange",
        "Virtual_Teller_Momo_cash_Collection"
    };

            var matchingRules = accountingRules
                .FirstOrDefault(x => x.DeterminationAccountId.Equals(account.ChartOfAccountManagementPositionId));

            return Task.FromResult(
                matchingRules != null && virtualTellerCodes.Contains(matchingRules.EventCode) ||
                account.AccountNumber.StartsWith(OPERATIONS_PREFIX_1) ||
                account.AccountNumber.StartsWith(OPERATIONS_PREFIX_2)
            );
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

                list.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value = item.AccountNumberCU + "-" + item.AccountName });

            }

            return list;
        }
        public async Task<Data.Account> EvaluateCurrentBalance(Data.Account account)
        {
            // Store last balance before any changes
            account.LastBalance = account.CurrentBalance;

            // Determine account behavior based on OHADA rules
            bool isDebitNormal = account.AccountNumber.StartsWith("2") || // Fixed Assets
                                account.AccountNumber.StartsWith("3") || // Inventory
                                account.AccountNumber.StartsWith("5") || // Financial
                                account.AccountNumber.StartsWith("6");   // Expenses

            bool isCreditNormal = account.AccountNumber.StartsWith("1") || // Capital
                                account.AccountNumber.StartsWith("7");    // Income

            // Handle class 4 accounts separately
            bool isClass4 = account.AccountNumber.StartsWith("4");
            bool isReceivable = isClass4 && await CheckIfAccountIsReceivableAsync(account);
            // Calculate current balance based on account type
            if (isDebitNormal || (isClass4 && !isReceivable))
            {
                account.CurrentBalance = (Convert.ToDouble( account.DebitBalance )- Convert.ToDouble( account.CreditBalance)).ToString();
            }
            else
            {
                account.CurrentBalance = (Convert.ToDouble(account.CreditBalance) - Convert.ToDouble(account.DebitBalance)).ToString();
 
            }



            return account;
        }
        private async Task<bool> CheckIfAccountIsReceivableAsync(Data.Account account)
        {
            var model = await _accountCategoryServices.GetAccountCategory(account.AccountCategoryId);
            return model.Name.ToLower() == "revenue";
        }

        public async Task<ActionResult> GetAccountBalance(string Id)
        {


            try
            {
                var AccountData = await _AccountServices.GetAccount(Id);

                var data = new ManuallyJournalEntryDataSet { Account = await EvaluateCurrentBalance(AccountData) };


                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<ActionResult> GetSequenceReference()
        {


            try
            {
                //var AccountData = await _AccountServices.GetSequenceReference();

        
                var data = $"{BaseUtilities.GenerateInsuranceUniqueNumber(12, "METRS")}-{_AccountServices.GetBranchCode()}";

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<ActionResult> GetAccountrJournalEntry(string Id)
        {


            try
            {
                var data = await _Service.GetAccountrJournalEntry(Id);


                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<ActionResult> MultipleJournalEntryConfiguration()
        {
           await GetList();

            try
            {
                var results = (List<PostedEntryX>)this.HttpContext.Session["postedEntryDetails" + _AccountServices.GetUserID()];

                var data = results; //<<<await _Service.GetPostedEntryReference(Id);

                await GetList();
                return View(new ManuallyJournalEntryDataSet { });
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddAccountingEntryRule(ManuallyJournalEntryDataSet model)
        {

            AddAccountingRuleCommand modelRequest = AddAccountingRuleCommand.BuildRequest(model);




                try
                {
                    var data = await _AccountingRuleServices.Creating(modelRequest);
                    if (data.Result)
                    {
                        return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                    }
                    else
                    {
                        return Json(new { success = false, status = false, message = data.MessageString });
                    }

                }
                catch (Exception ex)
                {
                    return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
                }
            
            
        }


        private dynamic BuildMenuAccountViewBag(List<ChartofAccountManagementPosition> ChartofAccountManagementPositions, List<ChartOfAccount> ListchartOfAccounts)
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();
            string code = "[BCD]";
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
        public async Task<ActionResult> MultipleJournalEntryClient()
        {
            var model = new ManuallyJournalEntryDataSet();
            model.AccountingRuleDtos = new List<AccountingRuleDtos>();  
            try
            {
               var ResponseList= await _AccountingRuleServices.GetAccountingRules();
                model.AccountingRuleDtos= BuildEntryTable(ResponseList.ToList());
                return View(model);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public List<AccountingRule> GetUniqueRuleNames(List<AccountingRule> accountingRules)
        {
            Dictionary<string, AccountingRule> uniqueRules = new Dictionary<string, AccountingRule>();

            foreach (var rule in accountingRules)
            {
                if (!uniqueRules.ContainsKey(rule.RuleName))
                {
                    uniqueRules[rule.RuleName] = rule;
                }
            }

            return uniqueRules.Values.ToList();
        }
        private List<AccountingRuleDtos> BuildEntryTable(List<AccountingRule> responseList)
        {
            var collection = GetUniqueRuleNames(responseList);
          
            List < AccountingRuleDtos > modelList = new List < AccountingRuleDtos >();
            foreach (var response in collection) 
            { 
                var result = new AccountingRuleDtos();

                result.RuleName= response.RuleName;
                result.System_Id = response.System_Id;

                modelList.Add(result);
            }
            return modelList;
        }

        private List<AccountingRuleDtos> GetTestData()
        {
            AccountingRuleDtos[] serverResponse = new AccountingRuleDtos[]
        {
            new AccountingRuleDtos { SystemDescription = "Revenue system rule", System_Id = "SYS001",  RuleName = "Revenue" },
            new AccountingRuleDtos { SystemDescription = "Expense system rule", System_Id = "SYS002",  RuleName = "Expense" },
            new AccountingRuleDtos { SystemDescription = "Depreciation system rule", System_Id = "SYS003", RuleName = "Depreciation" },
            new AccountingRuleDtos {  SystemDescription = "Accrual system rule", System_Id = "SYS004",  RuleName = "Liability" }
        };
            return serverResponse.ToList();
        }
        private AccountingModelRule GetJournalEntryTestData(string message)
        {
            AccountingModelRule modelRule = new AccountingModelRule();
          AccountingRule[] serverResponse = new AccountingRule[]
        {
            new AccountingRule { Id = "1", RuleName = $"{message}", Description =  $"{message}", SystemDescription =  $"{message}", System_Id = "SYS001", BookingDirection = "Credit", MFI_ChartOfAccountId = "000000", AccountNumber =  $"{message}", Amount = 1500.00, AccountName =  $"{message}" },
            new AccountingRule { Id = "2", RuleName =  $"{message}", Description =  $"{message}", SystemDescription =  $"{message}", System_Id = "SYS002", BookingDirection = "Debit", MFI_ChartOfAccountId = "000000", AccountNumber =  $"{message}", Amount = 800.00, AccountName =  $"{message}" },
            new AccountingRule { Id = "3", RuleName =  $"{message}", Description =  $"{message}", SystemDescription =  $"{message}", System_Id = "SYS003", BookingDirection = "Debit", MFI_ChartOfAccountId = "000000", AccountNumber =  $"{message}", Amount = 500.00, AccountName = $"{message}" },
            new AccountingRule { Id = "4", RuleName =  $"{message}", Description =  $"{message}", SystemDescription =  $"{message}", System_Id = "SYS004", BookingDirection = "Credit", MFI_ChartOfAccountId = "000000", AccountNumber =  $"{message}", Amount = 1200.00, AccountName = $"{message}" }
        };
            modelRule.AccountingRule= serverResponse.ToList();
            modelRule.HasError= true;
            return modelRule;
        }
        public async Task<ActionResult> GetAllEntriesForJournalEntryReference(string Id)
        {
           
            try
            {
                var results = (List<PostedEntryX>)this.HttpContext.Session["postedEntryDetails" + _AccountServices.GetUserID()];

                var data = results.Find(x => x.Id.Equals(Id)); //<<<await _Service.GetPostedEntryReference(Id);
               
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> GetAccountingEntryEventID(string system_Id)
        {
            AccountingModelRule modelRule = new AccountingModelRule();

            try
            {
                var modelList = await _AccountingRuleServices.GetAccountingRules();
                var list = modelList.Where(c=>c.System_Id == system_Id).ToList();   
                  list = await RebuildEntryBookAsync(list);
                modelRule.AccountingRule = list;
                modelRule.HasError = false;
                return Json(modelRule, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
               
                return Json(GetJournalEntryTestData(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        private async Task<List<AccountingRule>> RebuildEntryBookAsync(List<AccountingRule> list)
        {
            List<AccountingRule> accountingRules = new List<AccountingRule>();
            
                foreach (var rule in list)
                {
                    var model = await _ChartOfAccountManagementPositionServicesServices.GetChartOfAccountManagementPositionServiceByIdandBranchIDAsync(rule.MFI_ChartOfAccountId, _AccountServices.GetUserID());
                    rule.AccountNumber = $"{model.AccountNumberCU}-{model.AccountName}";
                    accountingRules.Add(rule);
                }
                return accountingRules;
         
     
           
        }

        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(ManuallyJournalEntryDataSet model)
        {
            Func<Task<ExecutionMessages>> serviceAction = null;

            if (model.ServiceOption == "CreateAccountingEntries")
            {
               
                    

                    serviceAction = await PostAccountingEntryActionAsync(model.ServiceOption, model);
              
            }
           

            if (serviceAction != null)
            {
                try
                {
                    var data = await serviceAction();
                    if (data.Result)
                    {
                        return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                    }
                    else
                    {
                        return Json(new { success = false, status = false, message = data.MessageString });
                    }

                }
                catch (Exception ex)
                {
                    return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
                }
            }

            return Json(new { success = false, status = false, message = "Invalid option selected." });
        }


        [HttpGet]
        public async Task<ActionResult> ApproveEntries(string Id, bool HasApproved)
        {
            var model = new EntryApproval { HasApproved = HasApproved, Id = Id };
            try
            {
                var data = await _Service.ApproveAccountingEntry(model);
                if (data.Result)
                {
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, status = false, message = data.MessageString }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
            }

        }
        [HttpPost]
        public async Task<ActionResult> PostAutoJournalEntries(AutomatedEventEntryCommand data)
        {
            if (string.IsNullOrEmpty(data.Description))
            {
                return Json(  "Kindly fill in the transaction description");
            }
            // Process the received data
            // For example, you can save it to the database or perform any business logic
            if (data.Entries[0].MFI_ChartOfAccountId.Contains("000000"))
            {
                return Json(await _Service.PostAutomatedJournalEntry(data,true));
            }
            else
            {
              return Json( await _Service.PostAutomatedJournalEntry(data));
                
            }
            // Return a success response
          
        }
        private async Task<Func<Task<ExecutionMessages>>> PostAccountingEntryActionAsync(string serviceOption, ManuallyJournalEntryDataSet model)
        {
            if (serviceOption == "CreateAccountingEntries")
            {
                return () => _Service.Create(model.EntryTempDatas);
            }
            
            else
            {
                return null;
            }
        }
        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(string serviceOption, ManuallyJournalEntryDataSet model)
        {
            if (serviceOption == "Create")
            {
                return () => _Service.Update(model.EntryTempData);
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
            if (serviceOption == "EntryTempData")
            {
                if (path == "list")
                {
                    var data = await _Service.GetAllEntriesForJournalEntryReference(key);
                    var dataAccounts = await _AccountServices.GetAllAccounting();
                    dataAccounts = dataAccounts.Where(po => po.AccountOwnerId == _AccountServices.GetBranchID()).ToList();
                    var dataset = from entry in data
                                  join account in dataAccounts on entry.AccountNumber equals account.AccountNumber
                                  select new EntryTempDataResult
                                  {
                                      Id = entry.Reference,
                                      AccountName = entry.AccountName,
                                      AccountNumber = entry.AccountNumber,
                                      Amount =Convert.ToDecimal( entry.Amount),
                                      Reference = entry.Reference,
                                      BookingDirection = entry.BookingDirection,
                                      SumDebit = data.Where(x => x.BookingDirection == "DEBIT").Sum(x => Convert.ToDecimal(x.Amount)),
                                      SumCredit = data.Where(x => x.BookingDirection == "CREDIT").Sum(x => Convert.ToDecimal(x.Amount)),
                                      Difference = (data.Where(x => x.BookingDirection == "CREDIT").Sum(x => Convert.ToDecimal(x.Amount)) - data.Where(x => x.BookingDirection == "DEBIT").Sum(x => Convert.ToDecimal(x.Amount))),
                                  };
                    var sysData = new ManuallyJournalEntryDataSet { EntryTempDataResult = dataset.ToList(), EntryTempDatas = data };

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
                    return PartialView(partialView, new ManuallyJournalEntryDataSet { });
                }
                else
                {
                    var data = await _Service.GetAccountrJournalEntry(key);
                    return PartialView(partialView, new ManuallyJournalEntryDataSet { EntryTempData = data });

                }


            }
            else if (serviceOption == "Account")
            {
                var AccountData = await _AccountServices.GetAccount(key);
                //if (AccountData == null)
                //{
                //    AccountData = AccountDataSample.Accounts.Find(i => i.Id == key);
                //}
                return PartialView(partialView, new ManuallyJournalEntryDataSet { Account = AccountData });


            }
            else if (serviceOption == "EntryDescription")
            {
                var data = await _Service.GetAllEntriesForJournalEntryReference(key);


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


            if (serviceOption == "EntryTempData")
            {
                var data = await _Service.Delete(KEY);
                return Json(new { success = data, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);


            }

            else
            {
                return null;
            }

        }
    }

 
}