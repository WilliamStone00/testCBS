using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using System.Web.Services.Description;

namespace CBS.FrontDesk.UI.Controllers
{
    public class OperationController : BaseController
    {
        // GET: Operation
        private readonly AccountServices _acountServices;
        private readonly TellerProvissioningServices _services;
        public OperationController(AccountServices acountServices, TellerProvissioningServices services = null)
        {
            _acountServices = acountServices;
            _services = services;
        }
        public async Task<ActionResult> AccountDetails(string KEY = null)
        {
            var account = await _acountServices.GetAccountByAccountNumber(KEY);
            return View(account);
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            try
            {
                ViewBag.KEY = KEY;
                if (path == "transactions")
                {

                }
                else
                {
                    var account = await _acountServices.GetAccountByAccountNumber(KEY);
                    ViewBag.Sources = _acountServices.GetPaymentSources();
                    return PartialView(partialView, account);
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
        public async Task<ActionResult> Deposit(Account model)
        {
            if (model.OperationType == "Deposit")
            {

                var data = await _acountServices.Deposit(model.DepositRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.OperationType == "InitialDeposit")
            {
                var data = await _acountServices.MakeInitialDeposit(model.AccountActivationRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.OperationType == "Withdrawal")
            {
                var data = await _acountServices.Withdrawal(model.WithdrawalRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.OperationType == "Transfer")
            {
                var data = await _acountServices.Transfer(model.TransferRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else
            {
                return Json(new { success = false, status = false, message = "Fill the required fields." });
            }
        }
        public async Task<ActionResult> Transfer()
        {
            var account = await _acountServices.SourceAndDestinationAccount();
            var conf = await _acountServices.GetSavingConfigurationAggregates();
            ViewBag.Source = account;
            ViewBag.Destination = account;
            ViewBag.TransferTypes = conf.transferTypes;
            return View(new Account());
        }
    }
}