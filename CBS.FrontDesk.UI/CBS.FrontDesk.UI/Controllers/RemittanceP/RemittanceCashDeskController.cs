using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace CBS.FrontDesk.UI.Controllers.RemittanceP
{
    [CheckSessionTimeOutAttribute]

    public class RemittanceCashDeskController : BaseController
    {
        private readonly CashDeskServices _cashDeskService;
        private readonly AccountingServices _accountingServices;
        private readonly RemittanceServices _remittanceServices;

        //RemittanceCashDesk
        public RemittanceCashDeskController(CashDeskServices cashDeskService = null, AccountingServices accountingServices = null, RemittanceServices remittanceServices = null)
        {
            _cashDeskService = cashDeskService;
            _accountingServices = accountingServices;
            _remittanceServices=remittanceServices;
        }
        // GET: RemittanceCashDesk
        public ActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<ActionResult> GenerateOTPRemittance(GenerateRemittanceOTPCommand model)
        {
            var data = await _remittanceServices.GenerateOTPRemittance(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

        }
       
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = "_DataNotFound", string path = null, string QueryString = null)
        {
            try
            {
                var cashDesk = new CashDesk();
                ViewBag.KEY = KEY;
                if (path=="")
                {
                    path = "cashin";
                    var remittanceWildQuery = new GetAllRemittanceWildQuery { Approved=true, QueryString="ReferenceNumber", QueryValue=KEY };
                    var remittances = await _remittanceServices.SearchRemittance(remittanceWildQuery);
                    if (!remittances.Any())
                    {
                        ViewBag.message = $"{KEY} was not found in the database.";
                        return PartialView("_DataNotFound", new CashDesk());

                    }
                    if (remittances.FirstOrDefault().Status!="Paid")
                    {
                        ViewBag.Operation = path;
                        cashDesk = await _cashDeskService.GetCashDeskRemittance(remittances.FirstOrDefault());
                        return PartialView(partialView, cashDesk);
                    }
                    else
                    {
                        ViewBag.Operation = "cashout";
                        cashDesk = await _cashDeskService.GetCashDeskRemittance(remittances.FirstOrDefault());
                        return PartialView(partialView, cashDesk);
                    }
                }
                if (path == "search")
                {
                    path="cashin";
                    if (KEY == null || KEY == "")
                    {
                        ViewBag.message = "Empty data was submited. Please enter search criterial";
                        return PartialView("_DataNotFound", new CashDesk());
                    }

                    var remittanceWildQuery = new GetAllRemittanceWildQuery { Approved=true, QueryString="ReferenceNumber", QueryValue=KEY };
                    var remittances = await _remittanceServices.SearchRemittance(remittanceWildQuery);
                    if (!remittances.Any())
                    {
                        ViewBag.message = $"{KEY} was not found in the database.";
                        return PartialView("_DataNotFound", new CashDesk());

                    }
                    ViewBag.Operation = path;
                    cashDesk = await _cashDeskService.GetCashDeskRemittance(remittances.FirstOrDefault());
                    return PartialView(partialView, cashDesk);



                }
                else if (path == "cashin" || path == "cashout")
                {

                    if (KEY == null || KEY == "")
                    {
                        ViewBag.message = "Empty data was submited. Please enter search criterial";
                        return PartialView("_DataNotFound", new CashDesk());
                    }

                    cashDesk = await _cashDeskService.GetCashDeskRemittance(null, KEY);
                    if (cashDesk == null)
                    {
                        var remittanceWildQuery = new GetAllRemittanceWildQuery { Approved=true, QueryString="ReferenceNumber", QueryValue=KEY };
                        var remittances = await _remittanceServices.SearchRemittance(remittanceWildQuery);
                        if (!remittances.Any())
                        {
                            ViewBag.message = $"{KEY} was not found in the database.";
                            return PartialView("_DataNotFound", new CashDesk());

                        }
                        ViewBag.Operation = path;
                        cashDesk = await _cashDeskService.GetCashDeskRemittance(remittances.FirstOrDefault());
                        return PartialView(partialView, cashDesk);

                    }

                    ViewBag.Operation = path;
                    return PartialView(partialView, cashDesk);

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
                if (deposits != null)
                {
                    var data = await _cashDeskService.BulkDeposi(deposits);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

                }
                return Json(new { success = false, status = false, message = $"No data was submitted." });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
            }
        }
        [HttpPost]
        public async Task<ActionResult> GetReport(string path)
        {
            if (path=="loan")
            {
                this.HttpContext.Session["rptType"] = "ReportParameterLess";
                this.HttpContext.Session["ReportName"] = $"MainReportLoan.rpt";
                this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/Payment/Loan/MainReportLoan.rpt";
                this.HttpContext.Session["rpttitle"] = $"MemberLoanReceipts";
                return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                this.HttpContext.Session["rptType"] = "ReportParameterLess";
                this.HttpContext.Session["ReportName"] = $"MainReport.rpt";
                this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/Payment/MainReport.rpt";
                this.HttpContext.Session["rpttitle"] = $"MemberReceipts";
                return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public async Task<ActionResult> GetReportOtherPayment()
        {
            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = $"OtherTransactionReciept.rpt";
            this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/Reciepts/OtherTransactionReciept.rpt";
            this.HttpContext.Session["rpttitle"] = $"MemberReceiptsOtherPayment";
            return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);

        }

    }
}