using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AffiliateAccount;
using CBS.FrontDesk.Data.Message;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.AffiliateAccounts
{
    //[CheckSessionTimeOut]
    public class AffiliateAccountController : Controller
    {

        private readonly AffiliateAccountService _AffiliateAccountService;
        private readonly AffiliateAccountMockService _affiliateAccountMockService;
        private readonly AffiliateService _AffiliateService;
        private readonly BranchServices _branchServices;
        

        /// <summary>
        /// Injects the required AffiliateController via dependency injection.
        /// </summary>
        /// <param name="CategoryConfigService">The service for cheque admin operations.</param>
        public AffiliateAccountController(AffiliateAccountService affiliateaccountService, BranchServices branchServices, AffiliateService affiliateService, AffiliateAccountMockService affiliateAccountMockService)
        {
            _AffiliateAccountService = affiliateaccountService;
            _branchServices = branchServices;
            _AffiliateService = affiliateService;
            _affiliateAccountMockService = affiliateAccountMockService;
        }

        [HttpGet]
        public async Task<ActionResult> DetailsPartial(string id)
        {

            // _affiliateService should be injected via constructor as IAffiliateAccountService
            var node = await _affiliateAccountMockService.GetByIdAsync(id);
            if (node == null) return PartialView("_AffiliateModals", new AffiliateTreeViewModel());

            // get full flat list (service can supply)
            var flat = await _affiliateAccountMockService.GetAllAsync();

            var parents = _affiliateAccountMockService.GetParentChain(id, flat);
            var childrenTree = _affiliateAccountMockService.BuildChildrenTree(id, flat);

            var vm = new AffiliateTreeViewModel
            {
                Node = node,
                ParentChain = parents,
                Children = childrenTree
            };

            return PartialView("_AffiliateModals", vm);
        }


        [HttpGet]
        public async Task<ActionResult> List()
        {
            //await loader();
            return View();

        }

        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            var affiliate = await _affiliateAccountMockService.GetAffiliatesFromMockAsync();
            ViewBag.Affiliates = affiliate;

            var Chartofaccount = await _affiliateAccountMockService.GetAffiliatesFromMockAsync();
            ViewBag.HoPcmfAccountId = Chartofaccount;
            return true;
        }

        [HttpPost]
        public async Task<JsonResult> LoadAffiliateData(AffiliateAccountQuery query)
        {
            //await loader();
            try
            {

                var data = await _affiliateAccountMockService.GetcategoryDataTableAsync(query);

                var Affiliate = JsonConvert.DeserializeObject<List<Data.Entity.Accounting_V2.Affiliate.Affiliateresponse>>(JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.draw,
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


        //public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        //{
        //    await loader();
        //    if (path == "list")
        //    {
        //        var data = await _affiliateAccountMockService.GetAsync();
        //        return PartialView(partialView, data);

        //    }
        //    //GetRolePermissions
        //    else if (path == "new")
        //    {
        //        var model = new AddAffiliateAccountCommand();

        //        if (!string.IsNullOrWhiteSpace(KEY))
        //        {
        //            // set ParentId so the view receives it in the hidden field
        //            model.ParentId = KEY;
        //        }
        //        return PartialView(partialView, model);
        //    }

        //    else
        //    {
        //        var data = await _affiliateAccountMockService.GetByIdAsync(KEY);
        //        return PartialView(partialView, data);

        //    }
        //}

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            await loader();

            if (path == "list")
            {
                var data = await _affiliateAccountMockService.GetAsync();
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                var model = new AddAffiliateAccountCommand();
                if (!string.IsNullOrWhiteSpace(KEY))
                {
                    model.ParentId = KEY;
                }
                return PartialView(partialView, model);
            }
            else // This handles the "get" path for editing
            {
                // Get the Affiliateresponse from service
                var entity = await _affiliateAccountMockService.GetByIdAsync(KEY);

                if (entity == null)
                {
                    // Handle not found case
                    return Content("Affiliate not found");
                }

                // MAP Affiliateresponse to AddAffiliateAccountCommand
                var model = new AddAffiliateAccountCommand
                {
                    Id = entity.Id,
                    Code = entity.Code,
                    Class = entity.Class,
                    NameEn = entity.NameEn,
                    NameFr = entity.NameFr,
                    IsActive = entity.IsActive,
                    PostingAllowed = entity.PostingAllowed,
                    ParentId = entity.ParentId,
                    // Note: AffiliateId and HoPcmfAccountId might need different mapping
                    // since they don't exist in Affiliateresponse
                    AffiliateId = entity.Id, // Or map appropriately
                    HoPcmfAccountId = entity.ParentId // Or map appropriately
                };

                return PartialView(partialView, model);
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(AddAffiliateAccountCommand model)
        {
            // Use IsNullOrWhiteSpace so empty string Ids don't behave like null
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Validation failed." });

                var result = await _AffiliateAccountService.CreateAsync(model);
                return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
            }
            else
            {
                // IMPORTANT: return the ActionResult from Update
                return await Update(model);
            }

            // unreachable now but keep for safety (or remove)
            // return Json(new { success = false, status = false, message = "Fillsss the required fields." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(AddAffiliateAccountCommand model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _AffiliateAccountService.UpdateAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpGet]
        // [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

            var result = await _AffiliateAccountService.DeleteAsync(KEY);

            // Map to simple JSON shape the client expects. Adjust if result has different property names.
            bool success = result?.Result ?? false;
            string message = Messaging.MessageResult(result) ?? "Operation completed.";

            return Json(new { success = success, message = message }, JsonRequestBehavior.AllowGet);
        }

    }
}

