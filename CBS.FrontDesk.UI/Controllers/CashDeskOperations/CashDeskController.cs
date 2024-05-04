using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.CashDeskOperations
{
    [CheckSessionTimeOutAttribute]

    public class CashDeskController : BaseController
    {
        private readonly CashDeskServices _cashDeskService;
        public CashDeskController(CashDeskServices cashDeskService = null)
        {
            _cashDeskService = cashDeskService;
        }
        // GET: CashDesk
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = "_DataNotFound", string path = null, string serviceOption = null)
        {
            try
            {
                ViewBag.KEY = KEY;
                if (path == "search")
                {
                    if (KEY == null || KEY == "")
                    {
                        ViewBag.message = "Empty data was submited. Please enter search criterial";
                        return PartialView("_DataNotFound", new CashDesk());
                    }
                    var cashDesk = await _cashDeskService.GetMember(KEY);
                    if (cashDesk == null)
                    {
                        ViewBag.message = $"{KEY} was not found in the database.";
                        return PartialView("_DataNotFound", new CashDesk());
                    }
                    return PartialView(partialView, cashDesk);


                }
                else if (path == "cashin" || path == "cashout" || path == "repayment")
                {
                    if (KEY == null || KEY == "")
                    {
                        ViewBag.message = "Empty data was submited. Please enter search criterial";
                        return PartialView("_DataNotFound", new CashDesk());
                    }

                    var cashDesk = await _cashDeskService.GetAccountByAccountNumberSearch(KEY, path);
                    if (cashDesk == null)
                    {

                        ViewBag.message = $"{KEY} was not found in the database.";
                        return PartialView("_DataNotFound", new CashDesk());
                    }
                    ViewBag.Operation = path;
                    return PartialView(partialView, cashDesk);

                }
                else if (path == "new_depositor")
                {
                    return PartialView(partialView, new CashDesk());

                }
                ViewBag.message = "Invalid option selected";
                return PartialView("_NoRecordFound", new CashDesk());

            }
            catch (Exception ex)
            {

                ViewBag.message = ex.Message; // Store error message
                return PartialView("_NoRecordFound", new CashDesk());
            }
        }
        [HttpPost]
        public async Task<ActionResult> PostRequestCash(List<BulkDeposit> deposits)
        {
            try
            {
                var data = await _cashDeskService.BulkDeposi(deposits);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
            }
        }
        [HttpPost]
        public async Task<ActionResult> GetReport()
        {
            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = $"Receipts.rpt";
            this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/Reciepts/Receipts.rpt";
            this.HttpContext.Session["rpttitle"] = $"MemberReceipts";
            return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);

        }

    }
}