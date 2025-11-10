using CBS.BusinessService.Accounting_V2.API;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.API;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Controllers.Accounting_V2.Affiliate;
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

            public APIKeyController(ApiKeyService apiKeyService)
            {
                _apiKeyService = apiKeyService;
            }

        public async Task<ActionResult> Index()
        {
            return View(new CreateApiKeyRequest());
        }


        [HttpGet]
        public async Task<ActionResult> List()
        {
            return View(new List<ApiKey>());
        }

        // Note: use [FromBody] so model binder reads the JSON DataTables sends.
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
            //await loader();
            if (path == "list")
            {
                var data = await _apiKeyService.GetAsync();
                return PartialView(partialView, data);

            }
            //GetRolePermissions
            else if (path == "new")
            {
                return PartialView(partialView, new CreateApiKeyRequest());
            }

            else
            {
                var data = await _apiKeyService.GetByIdAsync(KEY);
                return PartialView(partialView, data);

            }
        }

        [HttpPost]

        public async Task<ActionResult>Create(CreateApiKeyRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _apiKeyService.CreateAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }


        [HttpPost]
       
        public async Task<ActionResult> Renew(RenewApiKeyRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _apiKeyService.RenewAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpPost]
     
        public async Task<ActionResult> ChangeStatus(ChangeStatusRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _apiKeyService.ChangeStatusAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpPost]
      
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
                   return Json(new { success = false, message = "Validation failed, Enter rewuired field" }, JsonRequestBehavior.AllowGet);
                }

                var data = await _apiKeyService.GetByUserNameAsync(userName);
                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);          
           
        }

        [HttpGet]
        // [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

            var result = await _apiKeyService.DeleteAsync(KEY);

            // Map to simple JSON shape the client expects. Adjust if result has different property names.
            bool success = result?.Result ?? false;
            string message = Messaging.MessageResult(result) ?? "Operation completed.";

            return Json(new { success = success, message = message }, JsonRequestBehavior.AllowGet);
        }
    }
}