using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    [CheckSessionTimeOutAttribute]

    public class EndOfDayOperationController : BaseController
    {
        // GET: EndOfDayOperation
        private readonly SubTellerEndOfDayServices _subTellerEndOfDay;
        private readonly PrimaryTellerEndOfDayServices _primaryTellerEndOfDayServices;
        private readonly SavingProductServices _savingProductServices;
        private readonly AccountServices _acountServices;

        public EndOfDayOperationController(SubTellerEndOfDayServices services, PrimaryTellerEndOfDayServices primaryTellerEndOfDayServices, SavingProductServices savingProductServices = null, AccountServices acountServices = null)
        {
            _subTellerEndOfDay = services;
            _primaryTellerEndOfDayServices = primaryTellerEndOfDayServices;
            _savingProductServices = savingProductServices;
            _acountServices = acountServices;
        }

        public async Task<ActionResult> Index()
        {

            return View(new EndOfTheDay());
        }

        //public async Task<ActionResult> Primary()
        //{
        //    var primaryTellerProvisionings = await _primaryTellerEndOfDayServices.GetPrimaryTellerHistories();
        //    return View(new EndOfTheDay { PrimaryTellerProvisioningHistories = primaryTellerProvisionings.ToList() });
        //}2
        public async Task<ActionResult> Primary()
        {
            try
            {
                //var primaryTellerProvisionings = await _primaryTellerEndOfDayServices.GetPrimaryTellerHistories();
                ViewBag.Option = "Primary";
                var account = await _acountServices.GetTellerAccount(new GetTellerAccountBalanceQuery("N/A", true, false, true));
                ViewBag.Error = account.ErrorMessage;
                ViewBag.HasError = account.HasError;
                return View(new EndOfTheDay { CloseOfDayRequest = new CloseOfDayRequest { Amount = account.Balance, CashAtHand = account.Balance, CurrencyNotes = new Data.Entity.SavingProducts.AccountActivation.CurrencyNotes(), ClossedStatus = "Pending", Comment = $"As Primary Teller, {Session["FullName"].ToString()} is concluding operations for the day on [{DateTime.Now}] with a final total balance of {account.Balance.ToString("#,##0")}." } });
                //85,222,000.0
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        //
        //public async Task<ActionResult> SubTeller()
        //{

        //    var tellerProvisioningDtos = await _subTellerEndOfDay.GetSubTellerHistories();
        //    return View(new EndOfTheDay { SubTellerProvioningHistories = tellerProvisioningDtos.ToList() });
        //}

        public async Task<ActionResult> SubTeller()
        {
            ViewBag.Option = "SubTeller";
            var account = await _acountServices.GetTellerAccount(new GetTellerAccountBalanceQuery("N/A", false));
            ViewBag.Error = account.ErrorMessage;
            ViewBag.HasError = account.HasError;
            if (account.Balance == 0 && !account.HasError)
            {
                ViewBag.HasNoBalance = true;
                ViewBag.Error = $"The current balance of Teller {account.AccountName} is 0. Kindly make a cash request";
            }


            return View(new EndOfTheDay { CloseOfDayRequest = new CloseOfDayRequest { Amount = account.Balance, CashAtHand = account.Balance, CurrencyNotes = new Data.Entity.SavingProducts.AccountActivation.CurrencyNotes(), ClossedStatus = "Pending", Comment = $"As Sub-Teller, {Session["FullName"].ToString()} is concluding operations for the day on [{DateTime.Now}] with a final total balance of {account.Balance.ToString("#,##0")}." } });
        }


        public async Task<ActionResult> SubtellerVerification()
        {

            var tellerProvisioningDtos = await _subTellerEndOfDay.GetSubTellerHistories();
            return View(new EndOfTheDay { SubTellerProvioningHistories = tellerProvisioningDtos.ToList() });
        }
        public async Task<ActionResult> Accountant()
        {
            var tellerProvisioningDtos = await _primaryTellerEndOfDayServices.GetPrimaryTellerHistoriesByBranchID();
            return View(new EndOfTheDay { PrimaryTellerProvisioningHistories = tellerProvisioningDtos.ToList() });
        }



        [HttpPost]
        public async Task<ActionResult> CloseTheDay(EndOfTheDay model)
        {
            if (model.Option == "Primary")
            {
                var data = await _primaryTellerEndOfDayServices.EndTheDay(model.CloseOfDayRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.Option == "SubTeller")
            {
                var data = await _subTellerEndOfDay.EndTheDay(model.CloseOfDayRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.Option == "Verification")
            {
                var data = await _primaryTellerEndOfDayServices.EndTheDaySubTellerVerification(model.EndOfDayBySubTellerIDCommand);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            return Json(new { success = false, status = false, message = "Invalid choise" });

        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            try
            {//GetPrimaryTellerHistoriesByBranchID

                ViewBag.KEY = KEY;
                await GetList();
                if (serviceOption == "close_subteller")
                {
                    var command = await _subTellerEndOfDay.GetDailyOperationToClose(KEY);
                    return PartialView(partialView, command);
                }
                else
                {
                    if (path == "subteller_validation")
                    {
                        var command = await _primaryTellerEndOfDayServices.GetDailyOperationToClose(KEY);
                        return PartialView(partialView, command);
                    }
                    else
                    {
                        var provisioningDto = await _primaryTellerEndOfDayServices.GetDailyOperation(KEY);
                        return PartialView(partialView, provisioningDto);
                    }

                }

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
            if (path == "export_tellers_transactions")
            {
                var command = await _primaryTellerEndOfDayServices.GetDailyOperationToClose(KEY);
                this.HttpContext.Session["rptSource"] = _acountServices.GetTransactionHistoryExports(command.TransactionHistories);
                if (!command.TransactionHistories.Any())
                {
                    this.HttpContext.Session["rptSource"] = "empty";
                }
                this.HttpContext.Session["rptType"] = rptType;
                this.HttpContext.Session["ReportName"] = $"{ReportName}.rpt";
                this.HttpContext.Session["rptpath"] = $"~/{reportpath}/" + ReportName + ".rpt";
                this.HttpContext.Session["rpttitle"] = $"{fileTitle}";

            }
            return Json("", JsonRequestBehavior.AllowGet);

        }

        public async Task<bool> GetList()
        {
            var conf = await _savingProductServices.GetSavingConfigurationAggregates();
            ViewBag.Statuses = conf.primaryTellerEODStatuses.ToList();
            ViewBag.ClosedStatus = conf.accountantEODStatuses.ToList();
            ViewBag.Statuses = conf.Statuses.ToList();
            return true;
        }
    }
}