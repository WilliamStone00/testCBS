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

namespace CBS.FrontDesk.UI.Controllers
{
    public class ManuallyJournalEntryController : BaseController
    {
        private readonly EntryTempDataServices _Service;
 
        private readonly AccountingServices _AccountServices;
       

        public ManuallyJournalEntryController()
        {
            _Service = new EntryTempDataServices();
         
            _AccountServices = new AccountingServices();
         
        }
        // GET: AccountingConfiguration

        public async Task<ActionResult> Index()
        {
            await GetList();
            return View(new ManuallyJournalEntryDataSet());
        }

        private async Task GetList()
        {
          
            var DebitAccounts = await _AccountServices.GetAllAccounting();
            if (DebitAccounts.Count()==0    )
            {
              
                DebitAccounts= AccountDataSample.Accounts;
            }
            var CreditAccounts = BuildMenuViewBag(DebitAccounts);
            ViewBag.Accounts = CreditAccounts;
            ViewBag.BookingDirections= await GetBookingDirections();


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
        public async Task<ActionResult> GetAccountBalance(string Id)
        {


            try
            {
                var AccountData = await _AccountServices.GetAccount(Id);
                if (AccountData == null)
                {
                    AccountData = AccountDataSample.Accounts.Find(i => i.Id == Id);
                }
              var  data = new ManuallyJournalEntryDataSet { Account = AccountData };


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
        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(ManuallyJournalEntryDataSet model)
        {
            Func<Task<ExecutionMessages>> serviceAction = null;

            if (model.ServiceOption == "account")
            {


            }
            else if (model.ServiceOption == "EntryTempData")
            {
                if (model.EntryTempData.BookingDirection == "DEBIT")
                {
                    if (Convert.ToDecimal(model.EntryTempData.AccountBalance) - Convert.ToDecimal(model.EntryTempData.Amount) > 0)
                    {

                    }
                    else
                    {
                        return Json(new { success = false, status = false, message = $"Account Balance is insufficient." });
                    }

                }
                else
                {

                }
                if (model.Action == "insert")
                {
                    var chartOfAccount = await _AccountServices.GetAccount(model.EntryTempData.AccountName);
                    if (chartOfAccount == null)
                    {
                        chartOfAccount = AccountDataSample.Accounts.Find(i => i.Id == model.EntryTempData.AccountName);
                        model.EntryTempData.AccountName = chartOfAccount.AccountHolder;
                        model.EntryTempData.Description = "xxxxxxxxxx";
                    }
                    serviceAction = await GetInsertServiceActionAsync(model.ServiceOption, model);
                }
                else
                {

                    serviceAction = GetUpdateServiceAction(model.ServiceOption, model);
                }
            }
            else if (model.ServiceOption == "EntryDescription")
            {
                if (model.Action == "insert")
                {
                    model.EntryDescription.Reference = model.EntryTempDataResult[0].Reference;
             
                
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
        private async Task<Func<Task<ExecutionMessages>>> GetInsertServiceActionAsync(string serviceOption, ManuallyJournalEntryDataSet model)
        {
              if (serviceOption == "EntryTempData")
            {
                return () => _Service.Create(model.EntryTempData); 
            }
            else if (serviceOption == "EntryDescription")
            {
                return () => _Service.PostAccountingEntry(model.EntryDescription);  
            }

            else 
            {
                return null;
            }
        }
        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(string serviceOption, ManuallyJournalEntryDataSet model)
        {
            if (serviceOption == "EntryTempData")
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
                    var dataset = from res in data
                                  select new EntryTempDataResult
                                  {
                                      Id = res.Id,
                                      AccountName = res.AccountName,
                                      AccountNumber = res.AccountNumber,
                                      Amount = res.Amount,
                                      Reference= res.Reference,
                                      BookingDirection = res.BookingDirection,
                                      SumDebit = data.Where(x => x.BookingDirection == "DEBIT").Sum(x => x.Amount),
                                      SumCredit = data.Where(x => x.BookingDirection == "CREDIT").Sum(x => x.Amount),
                                      Difference=  (data.Where(x => x.BookingDirection == "CREDIT").Sum(x => x.Amount) - data.Where(x => x.BookingDirection == "DEBIT").Sum(x => x.Amount)),
                                  };
                    var sysData = new ManuallyJournalEntryDataSet { EntryTempDataResult = dataset.ToList() , EntryTempDatas = data};

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
                    return PartialView(partialView, new ManuallyJournalEntryDataSet {  });
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
                if (AccountData==null)
                {
                    AccountData = AccountDataSample.Accounts.Find(i => i.Id == key);
                }
                    return PartialView(partialView, new ManuallyJournalEntryDataSet { Account= AccountData });
               

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

    public class AccountDataSample
    {
   

        public static List<Data.Account> Accounts = new List<Data.Account>
{
    new Data.Account
    {
        Id = "1",
        AccountNumber = "123456789",
        AccountHolder = "John Doe",
        AccountTypeId = "AT001",
        ChartOfAccountId = "COA001",
        AccountOwnerId = "OWN001",
        BookingDirection = "Debit",
        CanBeNegative = false,
        IsBalanceSheetAccount = true
    },
    new Data.Account
    {
        Id = "2",
        AccountNumber = "987654321",
        AccountHolder = "Jane Smith",
        AccountTypeId = "AT002",
        ChartOfAccountId = "COA002",
        AccountOwnerId = "OWN002",
        BookingDirection = "Credit",
        CanBeNegative = true,
        IsBalanceSheetAccount = false
    },
    new Data.Account
    {
        Id = "3",
        AccountNumber = "456789012",
        AccountHolder = "Michael Johnson",
        AccountTypeId = "AT003",
        ChartOfAccountId = "COA003",
        AccountOwnerId = "OWN003",
        BookingDirection = "Debit",
        CanBeNegative = false,
        IsBalanceSheetAccount = true
    },
    new Data.Account
    {
        Id = "4",
        AccountNumber = "210987654",
        AccountHolder = "Emily Davis",
        AccountTypeId = "AT004",
        ChartOfAccountId = "COA004",
        AccountOwnerId = "OWN004",
        BookingDirection = "Credit",
        CanBeNegative = true,
        IsBalanceSheetAccount = false
    },
    new Data.Account
    {
        Id = "5",
        AccountNumber = "789012345",
        AccountHolder = "David Wilson",
        AccountTypeId = "AT005",
        ChartOfAccountId = "COA005",
        AccountOwnerId = "OWN005",
        BookingDirection = "Debit",
        CanBeNegative = false,
        IsBalanceSheetAccount = true
    },
    new Data.Account
    {
        Id = "6",
        AccountNumber = "345678901",
        AccountHolder = "Sarah Thompson",
        AccountTypeId = "AT006",
        ChartOfAccountId = "COA006",
        AccountOwnerId = "OWN006",
        BookingDirection = "Credit",
        CanBeNegative = true,
        IsBalanceSheetAccount = false
    },
    new Data.Account
    {
        Id = "7",
        AccountNumber = "678901234",
        AccountHolder = "Robert Anderson",
        AccountTypeId = "AT007",
        ChartOfAccountId = "COA007",
        AccountOwnerId = "OWN007",
        BookingDirection = "Debit",
        CanBeNegative = false,
        IsBalanceSheetAccount = true
    },
    new Data.Account
    {
        Id = "8",
        AccountNumber = "901234567",
        AccountHolder = "Jessica Taylor",
        AccountTypeId = "AT008",
        ChartOfAccountId = "COA008",
        AccountOwnerId = "OWN008",
        BookingDirection = "Credit",
        CanBeNegative = true,
        IsBalanceSheetAccount = false
    },
    new Data.Account
    {
        Id = "9",
        AccountNumber = "234567890",
        AccountHolder = "Daniel Brown",
        AccountTypeId = "AT009",
        ChartOfAccountId = "COA009",
        AccountOwnerId = "OWN009",
        BookingDirection = "Debit",
        CanBeNegative = false,
        IsBalanceSheetAccount = true
    },
    new Data.Account
    {
        Id = "10",
        AccountNumber = "567890123",
        AccountHolder = "Olivia Garcia",
        AccountTypeId = "AT010",
        ChartOfAccountId = "COA010",
        AccountOwnerId = "OWN010",
        BookingDirection = "Credit",
        CanBeNegative = true,
        IsBalanceSheetAccount = false
    }
};
    }
}