
using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
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

    public class FeePolicyController : BaseController
    {
        // GET: FeePolicy
        private readonly FeePolicyServices _services;
        private readonly OperationFeeServices _feeServices;
        private readonly BankServices _bankServices;
        private readonly BranchServices _branchServices;
        private readonly ChartOfAccountServicesAnnex _accountingServices;

        public FeePolicyController(FeePolicyServices services, OperationFeeServices feeServices = null, BankServices bankServices = null, BranchServices branchServices = null, ChartOfAccountServicesAnnex accountingServices = null)
        {
            _services = services;
            _feeServices = feeServices;
            _bankServices = bankServices;
            _branchServices = branchServices;
            _accountingServices = accountingServices;
        }

        public async Task<ActionResult> Index()
        {
            await GetEventNames("FEE");
            return View(new FeePolicy());
        }
        [HttpPost]
        public async Task<ActionResult> Create(FeePolicy model)
        {
            if (model.Id== null)
            {
                var data = await _services.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else
            {
                return await Update(model);
            }
        }
        [HttpPost]
        public async Task<ActionResult> Update(FeePolicy model)
        {
            var data = await _services.Update(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }
        private async Task GetEventNames(string operationType)
        {
            var fees = await _feeServices.GetFees();
            var Banks = await _bankServices.GetBanks();
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            ViewBag.Fees = fees;
            ViewBag.Banks = Banks;

            ViewBag.EventCodes = await _accountingServices.GetEventNames(operationType);

        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "list")
            {
                var data = await _services.GetFeePolicys();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                await GetEventNames("FEE");
                return PartialView(partialView, new FeePolicy());
            }
            else
            {
                await GetEventNames("FEE");

                ViewBag.Key = KEY;
                var feePolicy = await _services.GetFeePolicy(KEY);
                ViewBag.Base = feePolicy.Fee.FeeType;
                ViewBag.MemberShip = feePolicy.Fee.OperationFeeType;
                return PartialView(partialView, feePolicy);

            }
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetFee(string Key)
        {
            var data = await _feeServices.GetFee(Key);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> Ajaxloader(string Key, string path)
        {
            if (Key != null)
            {
                var listing = await _branchServices.GetBranchesByBankId(Key);
                return Json(listing, JsonRequestBehavior.AllowGet);

            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }
    }

}