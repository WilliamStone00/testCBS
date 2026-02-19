using CBS.BusinessService.Accounts;
using CBS.BusinessService.Accounts.MemberReceiptsP;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.MemberAccountManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Controllers.Accounting_V2.Affiliate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace CBS.FrontDesk.UI.Controllers.MembersFSerie
{
    public class MemberAccountManController : Controller
    {

        private readonly AccountManagementService _accountManagementService;

        public MemberAccountManController(AccountManagementService accountManagementService)
        {
            _accountManagementService = accountManagementService;
        }

        public ActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public async Task<ActionResult> List()
        {
            return View();
        }


        [HttpPost]
        public async Task<JsonResult> LoadData(ManageAccountmanaQuery query)
        {
            try
            {
                var data = await _accountManagementService.GetDataTableAsync(query);

                var Response = JsonConvert.DeserializeObject<List<ManageAccountStatusCommand>>(JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = Response
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
                var data = await _accountManagementService.GetAsync();
                return PartialView(partialView, data);

            }
            //GetRolePermissions
            else if (path == "new")
            {
                return PartialView(partialView, new ManageAccountStatusCommand());
            }

            else
            {
                var data = await _accountManagementService.GetByIdAsync(KEY);
                return PartialView(partialView, data);

            }
        }
     

        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(ManageAccountStatusCommand model)
        {
            // Use IsNullOrWhiteSpace so empty string Ids don't behave like null
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Validation failed." });

                var result = await _accountManagementService.CreateAsync(model);
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
        public async Task<ActionResult> Update(ManageAccountStatusCommand model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _accountManagementService.UpdateAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpGet]
        // [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

            var result = await _accountManagementService.DeleteAsync(KEY);

            // Map to simple JSON shape the client expects. Adjust if result has different property names.
            bool success = result?.Result ?? false;
            string message = Messaging.MessageResult(result) ?? "Operation completed.";

            return Json(new { success = success, message = message }, JsonRequestBehavior.AllowGet);
        }
    }
}