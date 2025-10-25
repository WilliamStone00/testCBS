using CBS.API.Helper;
using CBS.BusinessService.AccountingV2;
using CBS.BusinessService.AccountingV2.ConfigurationsManualEntry;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.ChangeNumber;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.ConfigurationManualEntry
{
    public class ConfigurationManualEntryController : Controller
    {
        public readonly ConfigurationManualEntryService _configurationManualEntryService;
        public ConfigurationManualEntryController(ConfigurationManualEntryService configurationManualEntryService)
        {
            _configurationManualEntryService = configurationManualEntryService;

        }

        // GET: ConfigurationManualEntry
        public ActionResult Index()
        {
            return View();
        }
  


        //[HttpPost]
        //public async Task<ActionResult> Update(ConfigurationManualEntries model)
        //{
        //    var data = await _configurationManualEntryService.Update(model);
        //    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        //}


        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "list")
            {
                 var data = await _configurationManualEntryService.GetAllAsync();
                //var data = _configurationManualEntryService.GetSampleEntries();
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                return PartialView(partialView, new ConfigurationManualEntries());
            }
            else
            {
                var data = await _configurationManualEntryService.GetData(KEY);
                return PartialView(partialView, data);

            }
        }

        [HttpGet]
        public async Task<ActionResult> GetDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(400, "Configuration ID is required");

            try
            {
                var entry = await _configurationManualEntryService.GetData(id);
                if (entry == null)
                    return HttpNotFound("Configuration not found");

                // Return the partial view that will be injected into the modal
                return PartialView("_Create", entry);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateConfigurationManualEntry(ConfigurationManualEntries model)
        {
            if (model == null)
                return Json(new { success = false, message = "⚠️ Invalid or empty model." });

            try
            {
                var result = await _configurationManualEntryService.UpdateConfigurationManualEntryAsync(model);

                // Check if response is null or failed
                if (result == null || !result.IsSuccess)
                    return Json(new { success = false, message = result?.Message ?? "❌ Failed to update entry." });

                return Json(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"❌ Error: {ex.Message}" });
            }
        }



    }
}