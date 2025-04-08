using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.UI.AppFiles.Reporting.Accounting;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Wordprocessing;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    [CheckSessionTimeOutAttribute] 
    public class AccountingStatementsController : BaseController
    {
        private readonly AccountingStatementService _acountServices;
        private readonly BranchServices _branchServices;
        private readonly AccountingServices _accountingServices;
        private readonly AccountingEntryServices _accountingEntryServices;
        private const string UniversalId = "XXXXXX";
        public AccountingStatementsController()
        {
            _acountServices = new AccountingStatementService();
            _branchServices = new BranchServices();
            _accountingServices = new AccountingServices();
            _accountingEntryServices = new AccountingEntryServices();
        }
        // GET: AccountingStatements
        public async Task<ActionResult> Index()
        {
            ViewBag.FileTypes = BuildDropDown(GenerateFIleType());
            ViewBag.ReportTypes = BuildDropDown(GenerateReportType());
            //var listAccount = (await _accountingServices.GetAllAccounting());
            //listAccount.Add(new Account { Id = "XXXXXX", AccountNumber = "000000", AccountName = "ALL" });
            //ViewBag.Accounts = BuildDropDown(GenerateAccountsListView(listAccount));
            var listBranches = (await _branchServices.GetBranches()).ToList();
            listBranches.Add(new Branch { Id = "XXXXXX", Name = "[x]ALL" });
            ViewBag.Branches = BuildDropDown(GenerateBranchListView(listBranches));
            var reportData = await _accountingServices.GetAllFileDownloadInfoPerUser();
            return View(new AccountingEntryQuery { ReportDownloadInfo = reportData.OrderByDescending(x=>x.CreatedDate).ToList() });
        }
        public async Task<ActionResult> DownloadById(string fileId)
        {
            try
            {
                var couApiResponse = await _accountingServices.GetFileDownloadById(fileId);
                if (string.IsNullOrEmpty(fileId))
                {
                    var reportData = await _accountingServices.GetAllFileDownloadInfoPerUser();
                    var Branches = await _branchServices.GetBranches();
                    ViewBag.Branches = Branches;
                    return View(new AccountingEntryQuery { ReportDownloadInfo = reportData.OrderByDescending(x => x.CreatedDate).ToList() });
                }

                try
                {
                    // Call the service to download the file
                    var response = await _accountingServices.GetFileDownloadedByFileId(fileId);

                    if (response.FileData != null)
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
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }


        public async Task<ActionResult> Delete(string id)
        {
            var data = await _accountingServices.DeleteReportDto(id);
            return Json(new { success = "Data deleted successfully", status = "Success", message = "Data deleted successfully" }, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<StringValues> GenerateAccountsListView(List<Account> accounts)
        {
            List<StringValues> stringValues = new List<StringValues>();
            foreach (var branch in accounts)
            {
                string accountMessage = $"{branch.AccountName}-{branch.AccountName}";
                stringValues.Add(new StringValues(branch.Id, accountMessage));
            }
            return stringValues;
        }

        private IEnumerable<StringValues> GenerateBranchListView(List<Branch> branches)
        {
            List<StringValues> stringValues = new List<StringValues>();
            foreach (var branch in branches)
            {

                stringValues.Add(new StringValues(branch.Id, branch.Name));
            }
            return stringValues;
        }

        private IEnumerable<StringValues> GenerateFIleType()
        {
            List<StringValues> stringValues = new List<StringValues>();
            stringValues.Add(new StringValues("EXCEL", "EXCEL"));
            stringValues.Add(new StringValues("PDF", "PDF"));

            return stringValues;
        }
        private IEnumerable<StringValues> GenerateReportType()
        {
            List<StringValues> stringValues = new List<StringValues>();
            stringValues.Add(new StringValues("GLD", "General Ledger"));
            stringValues.Add(new StringValues("GL", "Detail General Ledger"));
            stringValues.Add(new StringValues("JE", "Journal Entries"));
            stringValues.Add(new StringValues("LL", "Liaison Ledger"));
            stringValues.Add(new StringValues("LLA", "Liaison Ledger An Account"));
            stringValues.Add(new StringValues("TB6", "Trial Balance 6C"));
            stringValues.Add(new StringValues("TB4", "Trial Balance 4C"));
            stringValues.Add(new StringValues("BS", "Balance Sheet"));
            stringValues.Add(new StringValues("PANDL", "Profit and loss"));
            return stringValues;
        }
        [HttpGet]
        public async Task<ActionResult> GetAllLiasionAccount(string branchId)
        {
            if (!string.IsNullOrEmpty(branchId))
            {
                var listOfAccounts = await _accountingServices.GetAllLiasionAccount(branchId);

                var data = BuildDropDown(GenerateAccountsListView(listOfAccounts));
         
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = false, status = false, message = "Fill the required fields." });
            }

      
        }


        [HttpGet]
        public async Task<ActionResult> GeneralLedger(string branchId=null)
        {
            if (_accountingServices.IsHeadOffice())
            {
                if (branchId==null)
                {
                    var listOfBranch = await _branchServices.GetBranches();

                    return View(new AccountingStatementDto { Branches = listOfBranch.ToList() });
                }
                else
                {
                    var listOfAccounts = (await _accountingServices.GetAllAccounting()).Where(x=>x.AccountOwnerId==branchId);

                    return View(new AccountingStatementDto { Accounts = listOfAccounts.ToList() });
                }
               

            }
            else
            {
                var listOfAccounts = (await _accountingServices.GetAllAccounting()).Where(x => x.AccountOwnerId == _accountingServices.GetBranchID());

                return View(new AccountingStatementDto { Accounts = listOfAccounts.ToList() });
            }


        }



        [HttpGet]
        public async Task<ActionResult> JournalEntries(string branchId = null)
        {
            if (_accountingServices.IsHeadOffice())
            {
                if (branchId == null)
                {
                    var listOfBranch = await _branchServices.GetBranches();

                    return View(new AccountingStatementDto { Branches = listOfBranch.ToList() });
                }
                else
                {
                    SystemQuery query = new SystemQuery();
                    query.BranchId = _branchServices.GetBranchID();

                    var models = await _acountServices.GenerateAccountingLedgerForAnumber(query);
                    return View(new AccountingStatementDto { AccountingGeneralLedger = models });
                }


            }
            else
            {
                SystemQuery query = new SystemQuery();
                query.BranchId = _branchServices.GetBranchID();

                var models = await _acountServices.GenerateAccountingLedgerForAnumber(query);
                return View(new AccountingStatementDto { AccountingGeneralLedger = models });
            }


        }


        [HttpGet]
        public async Task<ActionResult> LiaisonLedger(string branchId = null)
        {
            if (_accountingServices.IsHeadOffice())
            {
                if (branchId == null)
                {
                    var listOfBranch = await _branchServices.GetBranches();

                    return View(new AccountingStatementDto { Branches = listOfBranch.ToList() });
                }
                else
                {
                    SystemQuery query = new SystemQuery();
                    query.BranchId = _branchServices.GetBranchID();

                    var models = await _acountServices.GenerateLiaisonLedgerForBranch(query.BranchId);
                    return View(new AccountingStatementDto { AccountDtos = models });
                }


            }
            else
            {
                SystemQuery query = new SystemQuery();
                query.BranchId = _branchServices.GetBranchID();

                var models = await _acountServices.GenerateLiaisonLedgerForBranch(query.BranchId);
                return View(new AccountingStatementDto { AccountDtos = models });
            }


        }
        public async Task<ActionResult> JournalEntriesPerBranch(SystemQuery query)
        {
            try
            {
                //SystemQuery query = new SystemQuery();
                //query.BranchId = branchId;

                var models = await _acountServices.GenerateAccountingLedgerForAnumber(query);
              

                return Json(models, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> LiaisonLedgerPerBranch(string branchId)
        {
            try
            {
                //SystemQuery query = new SystemQuery();
                //query.BranchId = branchId;

                var models = await _acountServices.GenerateLiaisonLedgerForBranch(branchId);


                return Json(models, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> JournalEntriesPerBranchPerPeriod(JEQuery query)
        {


            try
            {
                var models = await _acountServices.GenerateJournalEntry(query);
                return Json(models, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
  

        public async Task<ActionResult> GetAccountForABranch(string BranchId)
        {


            try
            {
                var models = await _accountingServices.GetAllAccountForABranch(BranchId);
                return Json(BuildMenuViewBag(models), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        private dynamic BuildMenuViewBag(IEnumerable<Data.Account> debitAccounts)
        {
            List<System.Web.WebPages.Html.SelectListItem> list = new List<System.Web.WebPages.Html.SelectListItem>();
            if (debitAccounts != null)

                foreach (var item in debitAccounts)
                {

                    list.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value = item.TempData + "-" + item.AccountName });

                }

            return list;
        }

        public async Task<ActionResult> GetBranchAccounts(string branchId)
        {


            try
            {
                var listOfAccounts = (await _accountingServices.GetAllAccountForABranch(branchId));
                 var data = (from account in listOfAccounts
                            select new Account
                            {
                                AccountNumber= account.TempData,
                            AccountName = account.AccountName,
                            Id = account.Id
                            }).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<ActionResult> GetBranchAccountJournalEntriesForAnAccount(BranchAccountJournalEntriesQueryObject model)
        {


            try
            {
                var listOfAccounts =( await _accountingEntryServices.GetAllAccountingEntriesForAnAccountPerBranch(model.BranchId,model.AccountId)).ToList().OrderBy(x =>x.ReferenceID);
                var models = from m in listOfAccounts
                             select new AccountEntry { Reference=m.ReferenceID, AccountNumber=m.AccountNumber, Description= m.Description,Debit=m.DrAmount,Credit=m.CrAmount };


                return Json(models.ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        private dynamic BuildDropDown(IEnumerable<StringValues> stringValues)
        {
            List<System.Web.WebPages.Html.SelectListItem> list = new List<System.Web.WebPages.Html.SelectListItem>();
            foreach (var item in stringValues)
            {

                list.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Text, Value = item.Value });

            }

            return list;
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            try
            {
               
                    var reportData = await _accountingServices.GetAllFileDownloadInfoPerUser();
              

                return PartialView(partialView, new AccountingEntryQuery { ReportDownloadInfo = reportData.OrderByDescending(x => x.CreatedDate).ToList() });
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = ex.Message; // Store error message
                return RedirectToAction("Index", "Error"); // Redirect to error page
            }
        }
        [HttpPost]
        public async Task<ActionResult> PostSearch(AccountingEntryQuery model)
        {
            try
            {
                if (Request.IsAjaxRequest())
                {
                    model.SystemQuery.BranchId = _accountingServices.IsHeadOffice() ? model.SystemQuery.BranchId : _accountingServices.GetBranchID();

                    // Process the data
                    // Generate the report or prepare the data
                    switch (model.SystemQuery.ReportType)
                    {
                        case "JE":
                            {
                                if (model.SystemQuery.FileType.ToLower()=="pdf")
                                {
                                    string fileTitle = $"JournalEntries_{DateTime.UtcNow.ToString("yyyyMMddhhmmss")}";
                                    var account = await _accountingServices.PostJournalEntries(new JEQuery { FromDate = model.SystemQuery.FromDate, ToDate = model.SystemQuery.ToDate, FileType = model.SystemQuery.FileType, BranchId = model.SystemQuery.BranchId }, APICallHelper.JournalEntryUrl);

                                    this.HttpContext.Session["rptSource"] = account;
                                    string ReportName = $"JournalEntries.rpt";



                                    if (account.AccountingEntries == null)
                                    {
                                        this.HttpContext.Session["rptSource"] = "empty";
                                    }
                                    this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                    this.HttpContext.Session["fileType"] = $"JE";
                                    this.HttpContext.Session["ReportName"] = $"{ReportName}";
                                    this.HttpContext.Session["rptType"] = $"{model.SystemQuery.ReportType}";
                                    this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Accounting/{ReportName}";
                                }
                                else
                                {
                                    var account = await _accountingServices.PostJE(new JEQuery { FromDate = model.SystemQuery.FromDate, ToDate = model.SystemQuery.ToDate, FileType = model.SystemQuery.FileType, BranchId = model.SystemQuery.BranchId }, APICallHelper.JournalEntryUrl);

                                }
                            }
                            break;
                        case "GL":
                            {
                                if (model.SystemQuery.FileType.ToLower() == "pdf")
                                {
                                    string fileTitle = $"GeneralLedger_{DateTime.UtcNow.ToString("yyyyMMddhhmmss")}";


                                    string ReportName = $"GeneralLedger.rpt";
                                    var account = await _acountServices.GenerateAccountingLedgerForAnumber(model.SystemQuery);//new SystemQuery { BranchId = model.SystemQuery.BranchId, FileType = model.SystemQuery.FileType, AccountIds = model.SystemQuery.AccountIds, FromDate = model.SystemQuery.FromDate, ToDate = model.SystemQuery.ToDate });

                                    if (model.SystemQuery.FileType == "PDF")
                                    {
                                        this.HttpContext.Session["rptSource"] = account;
                                    }
                                    else
                                    {
                                        this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                        this.HttpContext.Session["rptType"] = $"{model.SystemQuery.FileType}";
                                        this.HttpContext.Session["rptSource"] = (account != null) ? account : new AccountingGeneralLedgerDetails();
                                    }

                                    if (account == null)
                                    {
                                        this.HttpContext.Session["rptSource"] = "empty";
                                    }
                                    this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                    this.HttpContext.Session["rptType"] = $"{model.SystemQuery.FileType}";
                                    this.HttpContext.Session["ReportName"] = $"{ReportName}";
                                    this.HttpContext.Session["fileType"] = $"GL";
                                    this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Accounting/{ReportName}";

                                }
                                else
                                {
                                    var account = await _accountingServices.PostJE(new JEQuery { FromDate = model.SystemQuery.FromDate, ToDate = model.SystemQuery.ToDate, FileType = model.SystemQuery.FileType, BranchId = model.SystemQuery.BranchId }, APICallHelper.JournalEntryUrl);

                                }


                            }
                            break;
                        case "LL":
                            {
                                if (model.SystemQuery.FileType.ToLower() == "pdf")
                                {
                                    if (model.SystemQuery.Equals(UniversalId))
                                    {
                                        string fileTitle = $"Liaison_{DateTime.UtcNow.ToString("yyyyMMddhhmmss")}";
                                        var account = await _acountServices.GenerateLiasonAccountLiaisonledger(model.SystemQuery);
                                        this.HttpContext.Session["rptSource"] = account;
                                        if (!account.Any())
                                        {
                                            this.HttpContext.Session["rptSource"] = "empty";
                                        }
                                        this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                    }
                                    else
                                    {
                                        string fileTitle = $"BranchLiaison_{model.SystemQuery.FromDate}_{model.SystemQuery.ToDate}";
                                        var account = await _acountServices.GenerateLiasonAccountBranchLiaison(model.SystemQuery);
                                        this.HttpContext.Session["rptSource"] = account;
                                        if (!account.Any())
                                        {
                                            this.HttpContext.Session["rptSource"] = "empty";
                                        }
                                        this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                    }
                                }
                                else
                                {
                                    var account = await _accountingServices.PostJE(new JEQuery { FromDate = model.SystemQuery.FromDate, ToDate = model.SystemQuery.ToDate, FileType = model.SystemQuery.FileType, BranchId = model.SystemQuery.BranchId }, APICallHelper.JournalEntryUrl);

                                }
                            }
                            break;
                        case "TB4":
                            {
                                if (model.SystemQuery.FileType.ToLower() == "pdf")
                                {
                                    string fileTitle = $"TB4C{model.SystemQuery.FromDate.Date.ToString("yyyyMMddhhmmss")}";
                                    var account = await _acountServices.GenerateTrialBalance_4column(model.SystemQuery);
                                   
                                    string ReportName = $"TrialBalance6Column.rpt";
                                    if (!account.Any())
                                    {
                                        this.HttpContext.Session["rptSource"] = "empty";
                                    }
                             
                                    this.HttpContext.Session["rptSource"] = account;

                                    this.HttpContext.Session["fileType"] = $"TB4";
                                    if (!account.Any())
                                    {
                                        this.HttpContext.Session["rptSource"] = "empty";
                                    }
                                    this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                    this.HttpContext.Session["rptType"] = $"{model.SystemQuery.FileType}";
                                    this.HttpContext.Session["ReportName"] = $"{ReportName}";
                                    this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Accounting/{ReportName}";

                                }
                                else
                                {
                                    var account = await _accountingServices.PostJE(new JEQuery { FromDate = model.SystemQuery.FromDate, ToDate = model.SystemQuery.ToDate, FileType = model.SystemQuery.FileType, BranchId = model.SystemQuery.BranchId }, APICallHelper.JournalEntryUrl);

                                }

                            }
                            break;
                        case "TB6":
                            {
                                if (model.SystemQuery.FileType.ToLower() == "pdf")
                                {
                                    string fileTitle = $"TB6C{model.SystemQuery.FromDate.Date.ToString("yyyyMMddhhmmss")}";
                                    string ReportName = $"TrialBalance8Column.rpt";
                                    var account = await _acountServices.GenerateTrialBalance_6column(model.SystemQuery);
                                 
                                        this.HttpContext.Session["rptSource"] = account;

                                    this.HttpContext.Session["fileType"] = $"TB6";
                                    if (!account.Any())
                                    {
                                        this.HttpContext.Session["rptSource"] = "empty";
                                    }
                                    this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                    this.HttpContext.Session["rptType"] = $"{model.SystemQuery.FileType}";
                                    this.HttpContext.Session["ReportName"] = $"{ReportName}";
                                    this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Accounting/{ReportName}";


                                }
                                else
                                {
                                    var account = await _accountingServices.PostJE(new JEQuery { FromDate = model.SystemQuery.FromDate, ToDate = model.SystemQuery.ToDate, FileType = model.SystemQuery.FileType, BranchId = model.SystemQuery.BranchId }, APICallHelper.JournalEntryUrl);

                                }
                            }
                            break;
                        case "BS":
                            {
                                if (model.SystemQuery.FileType.ToLower() == "pdf")
                                {
                                    string fileTitle = $"BalanceSheet_{model.SystemQuery.FromDate.ToString("ddMMyyyy")}_{model.SystemQuery.ToDate.ToString("ddMMyyyy")}";
                                    var DocModel = (await _accountingEntryServices.GetAllFSDocument()).Where(x => x.name.ToUpper() == "BALANCESHEET").First();
                                    var modelx = new BSQuery { BranchId = model.SystemQuery.BranchId, Date = model.SystemQuery.ToDate, DocumentId = DocModel.id, FileType="pdf" };
                                    var account = await _acountServices.GenerateBalanceSheet(modelx);
                                    this.HttpContext.Session["rptSource"] = account;
                                    string ReportName = $"BalanceSheet.rpt";
                                    if (account == null)
                                    {
                                        this.HttpContext.Session["rptSource"] = "empty";

                                    }
                                    this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                    this.HttpContext.Session["dtoPasser"] = modelx;
                                    this.HttpContext.Session["rptType"] = model.SystemQuery.FileType;
                                    this.HttpContext.Session["fileType"] = $"BS";
                                    this.HttpContext.Session["ReportName"] = $"{ReportName}";
                                    this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Accounting/{ReportName}";
                                }
                                else
                                {
                                    var account = await _accountingServices.PostJE(new JEQuery { FromDate = model.SystemQuery.FromDate, ToDate = model.SystemQuery.ToDate, FileType = model.SystemQuery.FileType, BranchId = model.SystemQuery.BranchId }, APICallHelper.JournalEntryUrl);

                                }

                            }
                            break;
                        case "PANDL":
                            {
                                if (model.SystemQuery.FileType.ToLower() == "pdf")
                                {
                                    string fileTitle = $"InComeStatement{model.SystemQuery.FromDate}_{model.SystemQuery.ToDate}";
                                    var account = await _acountServices.GenerateIncomeStatement(model.SystemQuery);
                                    this.HttpContext.Session["rptSource"] = account;
                                    if (!account.Any())
                                    {
                                        this.HttpContext.Session["rptSource"] = "empty";
                                    }
                                    this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                }
                                else
                                {
                                    var account = await _accountingServices.PostJE(new JEQuery { FromDate = model.SystemQuery.FromDate, ToDate = model.SystemQuery.ToDate, FileType = model.SystemQuery.FileType, BranchId = model.SystemQuery.BranchId }, APICallHelper.JournalEntryUrl);

                                }
                            }
                            break;
                    }

                    return Json(new { success = true, message = "Report generated successfully" }, JsonRequestBehavior.AllowGet);
                    // Return a JSON result
                  
                }
                else
                {
                    // Handle non-AJAX requests if needed
                    // You might want to redirect here or return a view
                    return RedirectToAction("Index");
                }
          

            }
            catch (Exception ex)
            {

                throw(ex);
            }
       
        }
        public async Task<ActionResult> DownloadFiles(string fileId = null)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                var downloadInfo = await _accountingServices.GetAllFileDownloadInfoPerUser();
                var Branches = await _branchServices.GetBranches();
                ViewBag.Branches = Branches;
                return View(new AccountingEntryQuery { ReportDownloadInfo = downloadInfo.ToList() });
            }

            try
            {
                // Call the service to download the file
                var response = await _accountingServices.GetFileDownloadedByFileId(fileId);

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

        [HttpPost]
        public async Task<ActionResult> PostSearchJquery(AccountingEntryQuery model)
        {
            try
            {
                if (Request.IsAjaxRequest())
                {
                    model.SystemQuery.BranchId = _accountingServices.IsHeadOffice() ? model.SystemQuery.BranchId : _accountingServices.GetBranchID();

                    // Process the data
                    // Generate the report or prepare the data

                    if (model.SystemQuery.FileType.Equals("PDF"))
                    {
                     
                            // Process the data
                            // Generate the report or prepare the data
                            switch (model.SystemQuery.ReportType)
                            {
                                case "JE":
                                    {
                                        string fileTitle = $"JournalEntries_{DateTime.UtcNow.ToString("yyyyMMddhhmmss")}";
                                        var account = await _acountServices.GenerateJournalEntry(new JEQuery { FromDate = model.SystemQuery.FromDate, ToDate = model.SystemQuery.ToDate, FileType = model.SystemQuery.FileType, BranchId = model.SystemQuery.BranchId });
                                        this.HttpContext.Session["rptSource"] = account;
                                        string ReportName = $"JournalEntries.rpt";
                                     
                                        this.HttpContext.Session["rptSource"] = account;
                                      

                                        if (account == null)
                                        {
                                            this.HttpContext.Session["rptSource"] = "empty";
                                        }
                                        this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                        this.HttpContext.Session["rptType"] = $"{model.SystemQuery.FileType}";
                                        this.HttpContext.Session["ReportName"] = $"{ReportName}";
                                        this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Accounting/{ReportName}";
                                    }
                                    break;
                                case "GL":
                                    {
                                        string fileTitle = $"GeneralLedger_{DateTime.UtcNow.ToString("yyyyMMddhhmmss")}";

                          
                                        string ReportName = $"GeneralLedger.rpt";
                                        var account = await _acountServices.GenerateAccountingLedgerForAnumber(model.SystemQuery);//new SystemQuery { BranchId = model.SystemQuery.BranchId, FileType = model.SystemQuery.FileType, AccountIds = model.SystemQuery.AccountIds, FromDate = model.SystemQuery.FromDate, ToDate = model.SystemQuery.ToDate });

                                        if (model.SystemQuery.FileType == "PDF")
                                        {
                                            this.HttpContext.Session["rptSource"] = account;
                                        }
                                        else
                                        {
                                            this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                            this.HttpContext.Session["rptType"] = $"{model.SystemQuery.FileType}";
                                            this.HttpContext.Session["rptSource"] = (account != null) ? account : new AccountingGeneralLedgerDetails();
                                        }

                                        if (account == null)
                                        {
                                            this.HttpContext.Session["rptSource"] = "empty";
                                        }
                                        this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                        this.HttpContext.Session["rptType"] = $"{model.SystemQuery.FileType}";
                                        this.HttpContext.Session["ReportName"] = $"{ReportName}";
                                        this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Accounting/{ReportName}";



                                    }
                                    break;
                                case "LL":
                                    {
                                        if (model.SystemQuery.Equals(UniversalId))
                                        {
                                            string fileTitle = $"Liaison_{DateTime.UtcNow.ToString("yyyyMMddhhmmss")}";
                                            var account = await _acountServices.GenerateLiasonAccountLiaisonledger(model.SystemQuery);
                                            this.HttpContext.Session["rptSource"] = account;
                                            if (!account.Any())
                                            {
                                                this.HttpContext.Session["rptSource"] = "empty";
                                            }
                                            this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                        }
                                        else
                                        {
                                            string fileTitle = $"BranchLiaison_{model.SystemQuery.FromDate}_{model.SystemQuery.ToDate}";
                                            var account = await _acountServices.GenerateLiasonAccountBranchLiaison(model.SystemQuery);
                                            this.HttpContext.Session["rptSource"] = account;
                                            if (!account.Any())
                                            {
                                                this.HttpContext.Session["rptSource"] = "empty";
                                            }
                                            this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                        }
                                    }
                                    break;
                                case "TB4":
                                    {
                                        string fileTitle = $"TB4C{model.SystemQuery.FromDate.Date.ToString("yyyyMMddhhmmss")}";
                                        var account = await _acountServices.GenerateTrialBalance_4column(model.SystemQuery);
                                        this.HttpContext.Session["rptSource"] = account;
                                        string ReportName = $"TrialBalance6Column.rpt";
                                        if (!account.Any())
                                        {
                                            this.HttpContext.Session["rptSource"] = "empty";
                                        }

                                        this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                        this.HttpContext.Session["rptType"] = $"{model.SystemQuery.FileType}";
                                        this.HttpContext.Session["ReportName"] = $"{ReportName}";
                                        this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Accounting/{ReportName}";

                                    }
                                    break;
                                case "TB6":
                                    {
                                        string fileTitle = $"TB6C{model.SystemQuery.FromDate.Date.ToString("yyyyMMddhhmmss")}";
                                        string ReportName = $"TrialBalance8Column.rpt";
                                        var account = await _acountServices.GenerateTrialBalance_6column(model.SystemQuery);
                                        if (model.SystemQuery.FileType == "PDF")
                                        {
                                            this.HttpContext.Session["rptSource"] = account;
                                        }
                                        else
                                        {
                                            this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                            this.HttpContext.Session["rptType"] = $"{model.SystemQuery.FileType}";
                                            this.HttpContext.Session["rptSource"] = (account.Count() > 0) ? account : new List<TrialBalance6ColumnDto>();
                                        }

                                        if (!account.Any())
                                        {
                                            this.HttpContext.Session["rptSource"] = "empty";
                                        }
                                        this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                        this.HttpContext.Session["rptType"] = $"{model.SystemQuery.FileType}";
                                        this.HttpContext.Session["ReportName"] = $"{ReportName}";
                                        this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Accounting/{ReportName}";


                                    }
                                    break;
                                case "BS":
                                    {
                                        string fileTitle = $"BalanceSheet_{model.SystemQuery.FromDate.ToString("ddMMyyyy")}_{model.SystemQuery.ToDate.ToString("ddMMyyyy")}";
                                        var DocModel = (await _accountingEntryServices.GetAllFSDocument()).Where(x => x.name.ToUpper() == "BALANCESHEET").First();
                                        var modelx = new BSQuery { BranchId = model.SystemQuery.BranchId, Date = model.SystemQuery.ToDate, DocumentId = DocModel.id };
                                        var account = await _acountServices.GenerateBalanceSheet(modelx);
                                        this.HttpContext.Session["rptSource"] = account;
                                        string ReportName = $"BalanceSheet.rpt";
                                        if (account == null)
                                        {
                                            this.HttpContext.Session["rptSource"] = "empty";

                                        }
                                        this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                        this.HttpContext.Session["dtoPasser"] = modelx;
                                        this.HttpContext.Session["rptType"] = model.SystemQuery.FileType;
                                        this.HttpContext.Session["ReportName"] = $"{ReportName}";
                                        this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Accounting/{ReportName}";

                                    }
                                    break;
                                case "PANDL":
                                    {
                                        string fileTitle = $"InComeStatement{model.SystemQuery.FromDate}_{model.SystemQuery.ToDate}";
                                        var account = await _acountServices.GenerateIncomeStatement(model.SystemQuery);
                                        this.HttpContext.Session["rptSource"] = account;
                                        if (!account.Any())
                                        {
                                            this.HttpContext.Session["rptSource"] = "empty";
                                        }
                                        this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                    }
                                    break;
                            }

                            return Json(new { success = true, message = "Report generated successfully" }, JsonRequestBehavior.AllowGet);
                        
                    }
                    else
                    {
                        switch (model.SystemQuery.ReportType)
                        {
                            case "JE":
                                {
                                    var account = await _accountingServices.PostJE(new JEQuery { FromDate = model.SystemQuery.FromDate, ToDate = model.SystemQuery.ToDate, FileType = model.SystemQuery.FileType, BranchId = model.SystemQuery.BranchId }, APICallHelper.JournalEntryUrl);

                                }
                                break;
                            case "GL":
                                {
                                    var account = await _accountingServices.PostJE(model.SystemQuery, APICallHelper.GeneralLedgerStatementDetailUrl);//new SystemQuery { BranchId = model.SystemQuery.BranchId, FileType = model.SystemQuery.FileType, AccountIds = model.SystemQuery.AccountIds, FromDate = model.SystemQuery.FromDate, ToDate = model.SystemQuery.ToDate });

                                }
                                break;
                            case "GLD":
                                {
                                    var account = await _accountingServices.PostJE(model.SystemQuery, APICallHelper.GeneralLedgerStatementUrl);//new SystemQuery { BranchId = model.SystemQuery.BranchId, FileType = model.SystemQuery.FileType, AccountIds = model.SystemQuery.AccountIds, FromDate = model.SystemQuery.FromDate, ToDate = model.SystemQuery.ToDate });

                                }
                                break;
                            case "LL":
                                {

                                }
                                break;
                            case "TB4":
                                {
                                    var account = await _accountingServices.PostJE(model.SystemQuery, APICallHelper.AccountingEntry_Generate4ColumnTrialBalance);//new SystemQuery { BranchId = model.SystemQuery.BranchId, FileType = model.SystemQuery.FileType, AccountIds = model.SystemQuery.AccountIds, FromDate = model.SystemQuery.FromDate, ToDate = model.SystemQuery.ToDate });


                                }
                                break;
                            case "TB6":
                                {
                                    var account = await _accountingServices.PostJE(model.SystemQuery, APICallHelper.AccountingEntry_Generate6ColumnTrialBalance);//new SystemQuery { BranchId = model.SystemQuery.BranchId, FileType = model.SystemQuery.FileType, AccountIds = model.SystemQuery.AccountIds, FromDate = model.SystemQuery.FromDate, ToDate = model.SystemQuery.ToDate });

                                }
                                break;
                            case "BS":
                                {
                                    string fileTitle = $"BalanceSheet_{model.SystemQuery.FromDate.ToString("ddMMyyyy")}_{model.SystemQuery.ToDate.ToString("ddMMyyyy")}";
                                    var DocModel = (await _accountingEntryServices.GetAllFSDocument()).Where(x => x.name.ToUpper() == "BALANCESHEET").First();
                                    var modelx = new BSQuery { BranchId = model.SystemQuery.BranchId, Date = model.SystemQuery.ToDate, DocumentId = DocModel.id };
                                    var account = await _acountServices.GenerateBalanceSheet(modelx);
                                    this.HttpContext.Session["rptSource"] = account;
                                    string ReportName = $"BalanceSheet.rpt";
                                    if (account == null)
                                    {
                                        this.HttpContext.Session["rptSource"] = "empty";

                                    }
                                    this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                    this.HttpContext.Session["dtoPasser"] = modelx;
                                    this.HttpContext.Session["rptType"] = model.SystemQuery.FileType;
                                    this.HttpContext.Session["ReportName"] = $"{ReportName}";
                                    this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Accounting/{ReportName}";

                                }
                                break;
                            case "PANDL":
                                {
                                    string fileTitle = $"InComeStatement{model.SystemQuery.FromDate}_{model.SystemQuery.ToDate}";
                                    var account = await _acountServices.GenerateIncomeStatement(model.SystemQuery);
                                    this.HttpContext.Session["rptSource"] = account;
                                    if (!account.Any())
                                    {
                                        this.HttpContext.Session["rptSource"] = "empty";
                                    }
                                    this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                                }
                                break;

                        }

                        return Json(new { success = true, message = "Report generated successfully" }, JsonRequestBehavior.AllowGet);
                    }

                    // Return a JSON result

                }
                else
                {
                    // Handle non-AJAX requests if needed
                    // You might want to redirect here or return a view
                    return RedirectToAction("Index");
                }


            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }
        [HttpGet]
        public async Task<ActionResult> GenerateGLByBranchId(string branchId,string fileType)
        {
            try
            {

                string fileTitle = $"GeneralLedger_{DateTime.UtcNow.ToString("yyyyMMddhhmmss")}";

                var account = await _acountServices.GenerateAccountLedger(new GLQuery { BranchId = branchId, FileType = fileType });
                this.HttpContext.Session["rptSource"] = account;
                string ReportName = $"GeneralLedger.rpt";
                if (!account.Any())
                {
                    this.HttpContext.Session["rptSource"] = "empty";
                }
                this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                this.HttpContext.Session["rptType"] = $"{fileType}";
                this.HttpContext.Session["ReportName"] = $"{ReportName}";
                this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Accounting/GeneralLedger.rpt";
                return Json(account, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }
        [HttpGet]
        public async Task<ActionResult> GenerateLiaisonLedgerByBranchId(string branchId, string fileType)
        {
            try
            {

                string fileTitle = $"LiaisonLedger_{DateTime.UtcNow.ToString("yyyyMMddhhmmss")}";

                var account = await _acountServices.GenerateLiaisonLedgerForBranch(branchId);
                this.HttpContext.Session["rptSource"] = account;
                string ReportName = $"LiaisonLedger.rpt";
                if (!account.Any())
                {
                    this.HttpContext.Session["rptSource"] = "empty";
                }
                this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                this.HttpContext.Session["rptType"] = $"{fileType}";
                this.HttpContext.Session["ReportName"] = $"{ReportName}";
                this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Accounting/LiaisonLedger.rpt";
                return Json(account, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }
        [HttpGet]
        public async Task<ActionResult> GenerateJEByBranchId(string branchId, string fileType,DateTime DateFrom,DateTime DateTo)
        {
            try
            {

                string fileTitle = $"JournalEntries_{DateTime.UtcNow.ToString("yyyyMMddhhmmss")}";

                var account = await _acountServices.GenerateJournalEntry(new JEQuery { BranchId = branchId, FileType = fileType , FromDate=DateFrom,ToDate= DateTo });
                this.HttpContext.Session["rptSource"] = account;
                string ReportName = $"JournalEntries.rpt";
                if (!account.Any())
                {
                    this.HttpContext.Session["rptSource"] = "empty";
                }
                this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                this.HttpContext.Session["rptType"] = $"{fileType}";
                this.HttpContext.Session["ReportName"] = $"{ReportName}";
                this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Accounting/JournalEntries.rpt";
                return Json(account, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }
        [HttpPost]
        public async Task<ActionResult> GetReport(string rptType = null, string ReportName = null, string serviceoption = null, string reportpath = null, string fileTitle = null, string ReadOptions = null, string KEY = null, string path = null, string yearID = null, string datefrom = null, string dateto = null)
        {
            if (path == "export_generalLedger")
            {
                //var GenerateAccountingLedger = await _acountServices.GenerateAccountingLedger();
                //this.HttpContext.Session["rptSource"] = GenerateAccountingLedger;
                //if (!GenerateAccountingLedger.Any())
                //{
                //    this.HttpContext.Session["rptSource"] = "empty";
                //}
                //this.HttpContext.Session["rptType"] = rptType;
                //this.HttpContext.Session["ReportName"] = $"{ReportName}.rpt";
                //this.HttpContext.Session["rptpath"] = $"~/{reportpath}/" + ReportName + ".rpt";
                //this.HttpContext.Session["rpttitle"] = $"{fileTitle}";


            }
            else if (path == "by_date_history")
            {
                //this.HttpContext.Session["rptSource"] = _helper._object.Receipts;
                //this.HttpContext.Session["DateFrom"] = datefrom;
                //this.HttpContext.Session["DateTo"] = dateto;
            }


            return Json("", JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public async Task<ActionResult> GenerateBalanceSheet(string branchId, string fileType, DateTime DateFrom, DateTime DateTo)
        {
            try
            {
                var branch= await _branchServices.GetBranch(branchId);
                string fileTitle = $"BalanceSheet_{branch.Name}_{DateTime.UtcNow.ToString("yyyyMMddhhmmss")}";

                var account = await _acountServices.GenerateBalanceSheet(new BSQuery { BranchId = branchId,Date = DateTo, DocumentId = "DOC298781794326" });
                this.HttpContext.Session["rptSource"] = account;
                string ReportName = $"Balance Sheet.rpt";
                if (account!=null)
                {
                    this.HttpContext.Session["rptSource"] = "empty";
                }
                this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
                this.HttpContext.Session["rptType"] = $"{fileType}";
                this.HttpContext.Session["ReportName"] = $"{ReportName}";
                this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Accounting/Balance Sheet.rpt";
                return Json(account, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }


    }
}