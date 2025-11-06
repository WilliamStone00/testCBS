using CBS.BusinessService.Accounting_V2;
using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Accounting_V2.Pendingaccounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AffiliateAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.PendingAccount;
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
    [CheckSessionTimeOut]
    public class AffiliateAccountController : Controller
    {

        private readonly AffiliateAccountService _AffiliateAccountService;
        private readonly AffiliateAccountMockService _affiliateAccountMockService;
        private readonly AffiliateService _AffiliateService;
        private readonly BranchServices _branchServices;
        private readonly ChartOfAccountsV2Service _chartOfAccountsV;
        private readonly PendingAccountsService _pendingAccountsService;


        /// <summary>
        /// Injects the required AffiliateController via dependency injection.
        /// </summary>
        /// <param name="CategoryConfigService">The service for cheque admin operations.</param>
        public AffiliateAccountController(AffiliateAccountService affiliateaccountService, BranchServices branchServices, AffiliateService affiliateService, AffiliateAccountMockService affiliateAccountMockService, ChartOfAccountsV2Service chartOfAccountsV, PendingAccountsService pendingAccountsService)
        {
            _AffiliateAccountService = affiliateaccountService;
            _branchServices = branchServices;
            _AffiliateService = affiliateService;
            _affiliateAccountMockService = affiliateAccountMockService;
            _chartOfAccountsV = chartOfAccountsV;
            _pendingAccountsService = pendingAccountsService;
        }

        [HttpGet]
        public async Task<ActionResult> DetailsPartial(string id)
        {

            // _affiliateService should be injected via constructor as IAffiliateAccountService
            var node = await _AffiliateAccountService.GetByIdAsync(id);
            if (node == null) return PartialView("_AffiliateModals", new AffiliateAccountTreeViewModel());

            // get full flat list (service can supply)
            var flat = await _AffiliateAccountService.GetAsync();

            var parents = _AffiliateAccountService.GetParentChain(id, flat);
            var childrenTree = _AffiliateAccountService.BuildChildrenTree(id, flat);

            var vm = new AffiliateAccountTreeViewModel
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
            await loader();
            return View();

        }


        [HttpGet]
        public async Task<ActionResult> ListByClass(string cls)
        {
            var items = await _AffiliateAccountService.GetAllAffiliateAccountListByClass(cls);
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

           var affiliate = await _AffiliateService.GetAffiliatesAsync();
            ViewBag.Affiliates = affiliate;

            //var affiliate = await _AffiliateService.GetAsync();
            //ViewBag.Affiliates = affiliate;

            var Chartofaccount = await _AffiliateAccountService.GetAllAffiliateAccounts1();
            ViewBag.ChartAccount = Chartofaccount;

            var pcmfs = await _chartOfAccountsV.GetAllPCMFAccounts();
            ViewBag.HoPcmfAccounts = pcmfs;

            var classes = _chartOfAccountsV.GetAllClass2();
            ViewBag.Classes = classes;

            ViewBag.Languages = new List<SelectListItem>
            {
                new SelectListItem { Text = "English", Value = "en" },
                new SelectListItem { Text = "French",  Value = "fr" }
            };
            return true;

        }

        [HttpPost]
        public async Task<JsonResult> LoadAffiliateData(AffiliateAccountQuery query)
        {
            //await loader();
            try
            {
                query.AffiliateId = "1";
                var data = await _AffiliateAccountService.GetcategoryDataTableAsync(query);

                var Affiliate = JsonConvert.DeserializeObject<List<CBS.FrontDesk.Data.Entity.Accounting_V2.AffiliateAccount.AffiliateAccountDto>>(JsonConvert.SerializeObject(data.data));

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
                var data = await _AffiliateAccountService.GetAsync();
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                var model = new PendingAccountRequest
                {
                    Scope = "Affiliate",
                    Language = _AffiliateAccountService.GetUserLanguage(),
                    RequiresMapping = false
                };

                if (string.Equals(serviceOption, "root", StringComparison.OrdinalIgnoreCase))
                {
                    model.IsOrigin = true;
                    model.ParentId = null;             // root
                                                       // Optionally set a default Affiliate if you have 1; else leave for dropdown
                                                       // model.AffiliateId = ...
                                                       // preload any defaults (Class, etc.) if you have policy
                }
                else if (!string.IsNullOrWhiteSpace(KEY))
                {
                    // creating a child under existing parent
                    var parent = await _AffiliateAccountService.GetByIdAsync(KEY);
                    if (parent != null)
                    {
                        model.ParentId = parent.Id;
                        model.Class = parent.Class;                 // inherit class
                        model.AffiliateId = parent.AffiliateId;      // inherit affiliate
                        model.ParentAccountNumber = parent.Code;     // inherit affiliate
                        model.HoPcmfAccountId = parent.HoPcmfAccountId;
                        model.Code = AccountManagementPositionCalculator.ComposeChildCode(parent.Code, "");
                    }
                }


                return PartialView(partialView, model);
            }
            else 
            {
                // Get the AffiliateAccountDto from service
                var entity = await _AffiliateAccountService.GetByIdAsync(KEY);

                if (entity == null)
                {
                    // Handle not found case
                    return Content("Affiliate not found");
                }

                return PartialView(partialView, entity);
            }
        }


        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(PendingAccountRequest model)
        {
            // If modelstate invalid -> return JSON with structured errors so client can show appalert + inline message
                if (!ModelState.IsValid)
                {
                    var errors = GetModelStateErrors();
                    var errorsHtml = BuildErrorsHtml(errors);

                    return Json(new
                    {
                        success = false,
                        validation = true,
                        message = "Validation failed.",
                        errors,
                        errorsHtml
                    });
                }

                var result = await _pendingAccountsService.CreateAsync(model);

                // Map your ExecutionMessages -> JSON. Adjust field names as needed based on your ExecutionMessages.
                // Here I assume result.Result is a bool indicating success and result may carry messages.
                if (result.Result)
                {
                    return Json(new
                    {
                        success = true,
                        message = Messaging.MessageResult(result),
                        // optionally instruct client to reload the listing:
                        reloadDataView = "Yes"
                    });
                }
                else
                {
                    // If your ExecutionMessages contain field-level validation info, add it to the JSON here.
                    // For now return a structured failure so client can decide (validation=false means keep form open)
                    return Json(new
                    {
                        success = false,
                        validation = false,
                        message = Messaging.MessageResult(result)
                        // optionally: errors = ..., errorsHtml = ...
                    });
                }
          /*  }
            else
            {
                return await Update(model);
            }*/
        }

        // Collect ModelState errors into a dictionary
        private IDictionary<string, string[]> GetModelStateErrors()
        {
            return ModelState
                .Where(kvp => kvp.Value.Errors != null && kvp.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? (e.Exception?.Message ?? "Invalid value") : e.ErrorMessage).ToArray()
                );
        }

        // Build a small HTML unordered list for display inside appalert / inline summary
        private string BuildErrorsHtml(IDictionary<string, string[]> errors)
        {
            if (errors == null || errors.Count == 0) return string.Empty;

            var sb = new System.Text.StringBuilder();
            sb.Append("<ul style=\"margin:0;padding-left:18px;\">");
            foreach (var kv in errors)
            {
                // take last path part as friendly name (e.g. "Code" from "model.Code")
                var keyParts = kv.Key.Split('.');
                var friendlyKey = keyParts.Length > 0 ? keyParts.Last() : kv.Key;

                foreach (var msg in kv.Value)
                {
                    sb.AppendFormat("<li><strong>{0}:</strong> {1}</li>", System.Net.WebUtility.HtmlEncode(friendlyKey), System.Net.WebUtility.HtmlEncode(msg));
                }
            }
            sb.Append("</ul>");
            return sb.ToString();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(PendingAccountRequest model)
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

