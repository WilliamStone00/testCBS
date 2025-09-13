
using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.AndriodAppVersioning;
using CBS.BusinessService.ChangeNumber;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.AndriodApp;
using CBS.FrontDesk.Data.Entity.ChangeNumber;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.VaultManagement;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AndriodVersioning
{


 
    [CheckSessionTimeOutAttribute]
    public class AndriodAppVersioningController : BaseController
    {
        private readonly AndriodVersionConfigurationServices _services;

        public AndriodAppVersioningController(AndriodVersionConfigurationServices services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
          
        }

        public ActionResult Index()
        {
            return View(new AndriodVersionConfiguration());
        }


        public ActionResult Initialization()
        {
            return View(new AndriodVersionConfiguration());
        }

        // 🔹 Handles Creating or Updating an Android Version
        [HttpPost]
        public async Task<ActionResult> Create(AndriodVersionConfiguration model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }
                model.Id=model.Id==null ? "n/a" : model.Id;
                var data = await _services.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            catch (Exception ex)
            {
                // Log error and return a friendly message
                return Json(new { success = false, message = "An unexpected error occurred. Please try again later." });
            }
        }

        // 🔹 Loads Data for Views Dynamically
        public async Task<ActionResult> InitializeData(string key = null, string partialView = null, string path = null)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return Json(new { success = false, message = "Invalid request" });

                switch (path.ToLower())
                {
                    case "list":
                        var versions = await _services.GetAllAndroidVersions();
                        return PartialView(partialView, versions);

                    case "new":
                        return PartialView(partialView, new AndriodVersionConfiguration());

                    default:
                        var versionDetail = await _services.GetAndroidVersionById(key);
                        return PartialView(partialView, versionDetail);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An unexpected error occurred. Please try again later." });
            }
        }

        // 🔹 Deletes an Android Version
        public async Task<ActionResult> Delete(string key)
        {
            try
            {
                var result = await _services.Delete(key);
                return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to delete the version." }, JsonRequestBehavior.AllowGet);
            }
        }

        // 🔹 Retrieves Detailed Information for an Android Version
        public async Task<ActionResult> GetAndroidVersionDetail(string key)
        {
            try
            {
                var version = await _services.GetAndroidVersionById(key);
                if (version == null)
                    return HttpNotFound();

                return PartialView("_AndroidVersionConfigurationDetailsPartial", version);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to retrieve version details." }, JsonRequestBehavior.AllowGet);
            }
        }
    }

}