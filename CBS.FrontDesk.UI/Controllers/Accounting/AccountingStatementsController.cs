using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    public class AccountingStatementsController : BaseController
    {
        private readonly AccountingStatementService _acountServices;
        private readonly BranchServices _branchServices;
        private readonly AccountingServices _accountingServices;
        public AccountingStatementsController()
        {
            _acountServices = new AccountingStatementService();
            _branchServices = new BranchServices();
            _accountingServices = new AccountingServices();
        }
        // GET: AccountingStatements
        public async Task<ActionResult> Index()
        {
            ViewBag.FileTypes = BuildDropDown(GenerateFIleType());
            ViewBag.ReportTypes = BuildDropDown(GenerateReportType());
            ViewBag.Accounts = BuildDropDown(GenerateAccountsListView((await _accountingServices.GetAllAccounting())));
            ViewBag.Branches = BuildDropDown(GenerateBranchListView((await _branchServices.GetBranches()).ToList()));
            return View();
        }

        private IEnumerable<StringValues> GenerateAccountsListView(List<Account> accounts)
        {
            List<StringValues> stringValues = new List<StringValues>();
            foreach (var branch in accounts)
            {

                stringValues.Add(new StringValues(branch.Id, branch.AccountHolder));
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
            stringValues.Add(new StringValues("EXCEL", "PDF"));
            
            return stringValues;
        }
        private IEnumerable<StringValues> GenerateReportType()
        {
            List<StringValues> stringValues = new List<StringValues>();
            stringValues.Add(new StringValues("GL", "General Ledger"));
            stringValues.Add(new StringValues("LL", "Liaison Ledger"));
            stringValues.Add(new StringValues("LLA", "Liaison Ledger An Account"));
            stringValues.Add(new StringValues("TB6", "Trial Balance 6C"));
            stringValues.Add(new StringValues("TB4", "Trial Balance 4C"));
            stringValues.Add(new StringValues("BS", "Balance Sheet"));
            stringValues.Add(new StringValues("PANDL", "Profit and loss"));
            return stringValues;
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
                ViewBag.KEY = KEY;
                if (path == "export_generalLedger")
                {
                    //var account = await _acountServices.GenerateAccountingLedgerForAnumber();
                    //return PartialView(partialView, account);
                }
                else
                {
                    //var account = await _acountServices.GetAccountByAccountNumber(KEY);
                    //ViewBag.Sources = _acountServices.GetPaymentSources();
                    //return PartialView(partialView, account);
                }

                return PartialView(KEY, partialView);
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
            if (String.IsNullOrEmpty(model.SystemQuery.BranchId))
            {
                switch (model.SystemQuery.ReportType)
                {
                    case "GL": 
                        {
                            var account = await _acountServices.GenerateAccountingLedgerForAnumber(model.SystemQuery);
                            //return PartialView(partialView, account);
                        } break;
                    case "LL": 
                        { 
                        
                        } break;
                    case "TB4":
                        {

                        }
                        break;
                    case "TB6": 
                        {

                        } break;
                    case "BS": 
                        {
                        
                        } break;
                    case "PANDL": 
                        { 
                        
                        } break;
                }
            }
            else {
                switch (model.SystemQuery.ReportType)
                {
                    case "GL":
                        {
                            var account = await _acountServices.GenerateAccountingLedgerForAnumber(model.SystemQuery);
                            //return PartialView(partialView, account);
                        }
                        break;
                    case "LL":
                        {

                        }
                        break;
                    case "TB4":
                        {

                        }
                        break;
                    case "TB6":
                        {

                        }
                        break;
                    case "BS":
                        {

                        }
                        break;
                    case "PANDL":
                        {

                        }
                        break;
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);

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
    }
}