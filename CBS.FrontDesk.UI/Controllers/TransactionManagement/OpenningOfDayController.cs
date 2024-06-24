using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
   
    [CheckSessionTimeOutAttribute]

    public class OpenningOfDayController : BaseController
    {
        // GET: OpenningOfDay
        private readonly TellerProvissioningServices _services;
        private readonly PrimaryTellerCashReplenishmentServices _primaryTellerCashReplenishmentServices;
        private readonly AccountServices _accountServices;

        public OpenningOfDayController(TellerProvissioningServices services, PrimaryTellerCashReplenishmentServices primaryTellerCashReplenishmentServices = null, AccountServices accountServices = null)
        {
            _services = services;
            _primaryTellerCashReplenishmentServices = primaryTellerCashReplenishmentServices;
            _accountServices = accountServices;
        }

        public async Task<ActionResult> Index()
        {
            var data = await _primaryTellerCashReplenishmentServices.GetAllPendingPrimaryTellerCashReplenishments();
            return View(new OpeningOfTheDay { CashReplenishmentPrimaryTellers = data.ToList() });
        }

        public async Task<ActionResult> Primary()
        {
            try
            {
                ViewBag.Option = "Primary";
                var account = await _accountServices.GetTellerAccount(new GetTellerAccountBalanceQuery("N/A", true));
                ViewBag.Error = account.ErrorMessage;
                ViewBag.HasError = account.HasError;
                return View(new OpeningOfTheDay { OpenningOfDayRequest = new OpenningOfDayRequest { Comment = $"As Primary Teller, {Session["FullName"].ToString()} is commencing operations for the day on [{DateTime.Now}] with a total balance of {account.Balance.ToString("#,##0")}.", Amount = account.Balance, InitialAmount = account.Balance, CurrencyNotes = new Data.Entity.SavingProducts.AccountActivation.CurrencyNotes() } });

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public async Task<ActionResult> SubTeller()
        {
            ViewBag.Option = "SubTeller";
            var account = await _accountServices.GetTellerAccount(new GetTellerAccountBalanceQuery("N/A", false));
            ViewBag.Error = account.ErrorMessage;
            ViewBag.HasError = account.HasError;
            if (account.Balance==0 && !account.HasError)
            {
                ViewBag.HasNoBalance = true;
                ViewBag.Error = $"The current balance of Teller {account.AccountName} is 0. Kindly make a cash request";
            }
            

            return View(new OpeningOfTheDay { OpenningOfDayRequest = new OpenningOfDayRequest { Amount = account.Balance, InitialAmount = account.Balance, CurrencyNotes = new Data.Entity.SavingProducts.AccountActivation.CurrencyNotes(), Comment = $"As Sub-Teller {Session["FullName"].ToString()}, I hereby commence today's operations on [{DateTime.Now}] with an opening balance of {account.Balance.ToString("#,##0")}." }
        });
        }

        [HttpPost]
        public async Task<ActionResult> OpenTheDay(OpeningOfTheDay model)
        {
            if (model.Option=="Primary")
            {
                var data = await _services.PrimaryTellerProvision(model.OpenningOfDayRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.Option == "SubTeller")
            {
                var data = await _services.SubTellerProvision(model.OpenningOfDayRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            return Json(new { success = false, status = false, message = "Invalid choise" });

        }


        public async Task<ActionResult> Ajaxloader(string Key, string path)
        {
            if (Key != null)
            {
                if (path == "")
                {
                    var listing = await _services.GetUserTellerRole(Key);
                    return Json(listing, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var listing = await _services.GetSubTellers(Key);
                    return Json(listing, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public async Task<bool> GetList()
        {
            var TellerRoles = await _services.GetUserTellerRole();
            var SubTellers = await _services.GetSubTellers();
            var PrimaryTeller = await _services.GetPrimaryTellers();
            ViewBag.TellerRoles = TellerRoles;
            ViewBag.SubTellers = SubTellers;
            ViewBag.PrimaryTeller = PrimaryTeller;
            return true;
        }
    }

}