using BusinessServices;
using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Accounting_V2.API;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity.Accounting_V2.API;
using CBS.FrontDesk.Data.Message;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.API
{
    public class APIKeyController : Controller
    {
        private readonly ApiKeyService _apiKeyService;
        private readonly UserManagementServices _userManagementServices;
        private readonly BaseService _BaseService;

        public APIKeyController(ApiKeyService apiKeyService, UserManagementServices userManagementServices, BaseService baseService)
        {
            _apiKeyService = apiKeyService;
            _userManagementServices = userManagementServices;
            _BaseService  = baseService;
        }

        public async Task<ActionResult> Index()
        {
            await loader();
            return View(new CreateApiKeyRequest());
        }

        public async Task<bool> loader()
        {
         
            var thirdparty = await _userManagementServices.GetUserbyrole();
            ViewBag.thirdparty = thirdparty;
            return true;

        }

        [HttpGet]
        public async Task<ActionResult> List()
        {
            return View(new List<ApiKey>());
        }

        [HttpPost]
        public async Task<JsonResult> LoadData(ApiKeyQuery query)
        {
            try
            {
                var data = await _apiKeyService.GetDataTableAsync(query);
                var Affiliate = JsonConvert.DeserializeObject<List<ApiKey>>(JsonConvert.SerializeObject(data.data));

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
            if (path == "list")
            {
                var role =  _BaseService.GetRoleName();
                var data = await _apiKeyService.GetAsync();
                foreach (var item in data)
                {
                    item.UserRole = "ThirdPartyProviders";
                }

                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                return PartialView(partialView, new CreateApiKeyRequest());
            }
            else if (path == "details")
            {
                var data = await _apiKeyService.GetByIdAsync(KEY);
                return PartialView(partialView, data);
            }
            else if (path == "renew")
            {
                var data = await _apiKeyService.GetByIdAsync(KEY);
                var renewModel = new RenewApiKeyRequest { Id = data?.Id ?? KEY, UserName = data.UserName };
                return PartialView(partialView, renewModel);
            }
            else if (path == "revoke")
            {
                var data = await _apiKeyService.GetByIdAsync(KEY);
                var revokeModel = new RevokeApiKeyRequest { Id = data?.Id ?? KEY, UserName = data.UserName };
                return PartialView(partialView, revokeModel);
            }
            else if (path == "change-status")
            {
                var data = await _apiKeyService.GetByIdAsync(KEY);
                var statusModel = new ChangeStatusRequest
                {
                    Id = data?.Id ?? KEY,
                    IsActive = !(data?.IsActive ?? false),// Toggle current status
                    UserName = data.UserName
                };
                return PartialView(partialView, statusModel);
            }
            else
            {
                var data = await _apiKeyService.GetByIdAsync(KEY);
                return PartialView(partialView, data);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateApiKeyRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _apiKeyService.CreateAsync(model);

            // Return the raw key in the response
            return Json(new
            {
                success = result.Result,
                message = Messaging.MessageResult(result),
                data = new { rawKey = result.Data?.ToString() } // Assuming the raw key is in result.Data
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Renew(RenewApiKeyRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _apiKeyService.RenewAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeStatus(ChangeStatusRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _apiKeyService.ChangeStatusAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Revoke(RevokeApiKeyRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _apiKeyService.RevokeAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpGet]
        public async Task<JsonResult> GetByUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return Json(new { success = false, message = "Validation failed, Enter required field" }, JsonRequestBehavior.AllowGet);
            }

            var data = await _apiKeyService.GetByUserNameAsync(userName);
            return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetApiGuide(string apiKey = "")
        {
            ViewBag.ApiKey = string.IsNullOrEmpty(apiKey) ? "your-generated-api-key-here" : apiKey;
            return PartialView("_ApiIntegrationGuide");
        }

        [HttpPost]
  
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

            var result = await _apiKeyService.DeleteAsync(KEY);
            bool success = result?.Result ?? false;
            string message = Messaging.MessageResult(result) ?? "Operation completed.";

            return Json(new { success = success, message = message }, JsonRequestBehavior.AllowGet);
        }
    }
}