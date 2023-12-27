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

namespace CBS.FrontDesk.UI.Controllers
{
    public class OperationController : BaseController
    {
        // GET: TransactionManagement
        private readonly AccountServices _acountServices;

        public OperationController(AccountServices acountServices)
        {
            _acountServices = acountServices;
        }
        public async Task<ActionResult> AccountDetails(string KEY = null)
        {
            var account = await _acountServices.GetAccountByAccountNumber(KEY);
            return View(account);
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path=null)
        {
            ViewBag.KEY = KEY;
            if (path=="transactions")
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

        [HttpPost]
        public async Task<ActionResult> Deposit(Account model)
        {
            if (model.OperationType == "Deposit")
            {
                
                var data = await _acountServices.Deposit(model.DepositRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            //Withdrawal
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
            else
            {
                return Json(new { success = false, status = false, message = "Fill the required fields." });
            }
        }
    }
}