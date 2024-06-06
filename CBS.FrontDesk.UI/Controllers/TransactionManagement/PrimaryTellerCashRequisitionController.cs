using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
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
    public class PrimaryTellerCashRequisitionController : BaseController
    {
        // GET: PrimaryTellerCashRequisition
        private readonly PrimaryTellerCashReplenishmentServices _services;
        private readonly BranchServices _branchServices;

        public PrimaryTellerCashRequisitionController(PrimaryTellerCashReplenishmentServices services, BranchServices branchServices = null)
        {
            _services = services;
            _branchServices = branchServices;
        }
        //RequestValidation
        public async Task<ActionResult> Index()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View();
        }
        public async Task<ActionResult> PendingRequest()
        {
            var pending = await _services.GetAllPendingPrimaryTellerCashReplenishments();
            return View(pending.ToList());
        }
        //PendingRequest
        public async Task<ActionResult> RequestValidation(string ProvisionKey)
        {
            var cashReplenishmentSub = await _services.GetCashReplenishmentPrimaryTeller(ProvisionKey);
            cashReplenishmentSub.ApprovedComment = "Approved: Cash replenishment authorized to maintain optimal cash levels for customer service and operational efficiency.";
            cashReplenishmentSub.Status = true;
            return View(cashReplenishmentSub);
        }
        public async Task<ActionResult> Request()
        {
            return View(new CashReplenishmentPrimaryTeller());
        }
        [HttpPost]
        public async Task<ActionResult> Create(CashReplenishmentPrimaryTeller model)
        {
            if (model.Id == null)
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
        public async Task<ActionResult> Update(CashReplenishmentPrimaryTeller model)
        {
            var data = await _services.ValidateRequest(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string dateFrom = null, string dateTo = null)
        {
            if (path == "search")
            {
                if (KEY != string.Empty)
                {
                    var data = await _services.GetCashReplenishmentPrimaryTellers(_services.GetDateTime(dateFrom), _services.GetDateTime(dateTo), KEY);
                    return PartialView(partialView, data.ToList());

                }
                else
                {
                    var data = await _services.GetCashReplenishmentPrimaryTellers(_services.GetDateTime(dateFrom), _services.GetDateTime(dateTo));
                    return PartialView(partialView, data.ToList());

                }
            }

            else if (path == "new")
            {
                return PartialView(partialView, new CashReplenishmentPrimaryTeller());
            }
            else
            {
                var Guaranty = await _services.GetCashReplenishmentPrimaryTeller(KEY);
                return PartialView(partialView, Guaranty);

            }
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }


    }

}