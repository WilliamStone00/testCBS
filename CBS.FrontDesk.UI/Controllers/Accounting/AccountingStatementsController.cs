using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data.Entity.Accounting;
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

        public AccountingStatementsController()
        {
            _acountServices = new AccountingStatementService();
        }
        // GET: AccountingStatements
        public async Task<ActionResult> Index()
        {

            return View();
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            try
            {
                ViewBag.KEY = KEY;
                if (path == "export_generalLedger")
                {
                    var account = await _acountServices.GenerateAccountingLedger();
                    return PartialView(partialView, account);
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
        public async Task<ActionResult> GetReport(string rptType = null, string ReportName = null, string serviceoption = null, string reportpath = null, string fileTitle = null, string ReadOptions = null, string KEY = null, string path = null, string yearID = null, string datefrom = null, string dateto = null)
        {
            if (path == "export_generalLedger")
            {
                var GenerateAccountingLedger = await _acountServices.GenerateAccountingLedger();
                this.HttpContext.Session["rptSource"] = GenerateAccountingLedger;
                if (!GenerateAccountingLedger.Any())
                {
                    this.HttpContext.Session["rptSource"] = "empty";
                }
                this.HttpContext.Session["rptType"] = rptType;
                this.HttpContext.Session["ReportName"] = $"{ReportName}.rpt";
                this.HttpContext.Session["rptpath"] = $"~/{reportpath}/" + ReportName + ".rpt";
                this.HttpContext.Session["rpttitle"] = $"{fileTitle}";

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