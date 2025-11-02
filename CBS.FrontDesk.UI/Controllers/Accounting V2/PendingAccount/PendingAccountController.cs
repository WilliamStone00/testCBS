using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.Pendingaccounts;
using CBS.BusinessService.CheckManagementSystem.Operations.ChequeRequestService;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AffiliateAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.BranchAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.FileUpload;
using CBS.FrontDesk.Data.Entity.Accounting_V2.PendingAccount;
using CBS.FrontDesk.Data.Message;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using ZXing;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.PendingAccount
{
    public class PendingAccountController : Controller
    {
        private readonly AffiliateAccountService _AffiliateAccountService;
        private readonly AffiliateAccountMockService _affiliateAccountMockService;
        private readonly AffiliateService _AffiliateService;
        private readonly BranchServices _branchServices;
        private readonly PendingAccountsService _pendingAccount;


        /// <summary>
        /// Injects the required AffiliateController via dependency injection.
        /// </summary>
        /// <param name="CategoryConfigService">The service for cheque admin operations.</param>
        public PendingAccountController(PendingAccountsService pendingacountsservice, AffiliateAccountService affiliateaccountService, BranchServices branchServices, AffiliateService affiliateService, AffiliateAccountMockService affiliateAccountMockService)
        {
            _AffiliateAccountService = affiliateaccountService;
            _branchServices = branchServices;
            _AffiliateService = affiliateService;
            _affiliateAccountMockService = affiliateAccountMockService;
            _pendingAccount = pendingacountsservice;
        }

        [HttpGet]
        public async Task<ActionResult> List()
        {
            await loader();
            return View();

        }

        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            var affiliate = await _AffiliateService.GetAsync();
            ViewBag.Affiliates = affiliate;

            var Chartofaccount = await _affiliateAccountMockService.GetAffiliatesFromMockAsync();
            ViewBag.HoPcmfAccountId = Chartofaccount;

            var affiliateAccounts = await _AffiliateAccountService.GetAffiliatesFromEndpointAsync();
            ViewBag.HoPcmfAccountId = affiliateAccounts;

            return true;

        }

        [HttpPost]
        public async Task<JsonResult> LoadData(PendingAccountQuery query)
        {
            //await loader();
            try
            {

                var data = await _pendingAccount.DataTableAsync(query);

                var Affiliate = JsonConvert.DeserializeObject<List<PendingAccountDto>>(JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.Options.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = Affiliate
                });
            }
            catch (Exception ex)
            {
                // return a DataTables-compatible empty result on error
                return Json(new
                {
                    draw = query?.Options?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {

            if (path == "new")
            {
                await loader();
                return PartialView(partialView, new BranchAccountResponse());
            }
            else
            {

                if (path == "reject" || path == "validate")
                {
                    var model = new RequestAction
                    {
                        Id = KEY,
                        ActionType=serviceOption
                        
                    };
                    return PartialView(partialView, model);
                }
                else
                {
                    var data = await _pendingAccount.GetByIdAsync(KEY);
                    return PartialView(partialView, data);
                }


            }
        }

        public ActionResult HandleAction(string id, string actionType)
        {
            // You can fetch the model or data based on ID and action type
            var model = new RequestAction
            {
                Id = id
            };

            // Return a partial view
            return PartialView("_ActionModel", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(BranchAccountResponse model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _pendingAccount.UpdateAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }



        [HttpPost]
        public async Task<ActionResult> ValidateRequest(RequestAction dto)
        {
            
            if (string.IsNullOrEmpty(dto.Id))
                return Json(new { success = false, message = "Invalid request ID." });

            var success = await _pendingAccount.ValidateRequestAsync(dto);
            return Json(new { success = success.Result, message = Messaging.MessageResult(success) });
        }


        [HttpPost]
        public async Task<ActionResult> RejectRequest(RequestAction dto)
        {
            if (string.IsNullOrEmpty(dto.Id) || string.IsNullOrEmpty(dto.RejectionReason))
                return Json(new { success = false, message = "Request ID and reason are required." });

            var success = await _pendingAccount.RejectRequestAsync(dto);
            return Json(new { success = success.Result, message = Messaging.MessageResult(success) });
        }
    }
}
