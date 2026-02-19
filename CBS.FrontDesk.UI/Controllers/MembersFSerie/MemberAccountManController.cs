using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Accounts.MemberReceiptsP;
using CBS.BusinessService.Config;
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
        private readonly BranchServices _branchServices;
        private readonly AccountManagementService _accountManagementService;

        public MemberAccountManController(AccountManagementService accountManagementService, BranchServices branchServices)
        {
            _accountManagementService = accountManagementService;
            _branchServices = branchServices;
        }

        public ActionResult Index()
        {
            return View();
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
            return true;

        }

        [HttpPost]
        public async Task<JsonResult> LoadData(ManageAccountmanaQuery query)
        {
            try
            {
                var result = await _accountManagementService.GetDataTableAsync(query);

                // 🔥 Deserialize ONLY the array of records
                var response = JsonConvert.DeserializeObject<List<ManageAccountStatusCommand>>(JsonConvert.SerializeObject(result.data)
                );

                return Json(new
                {
                    draw = result.data,
                    recordsTotal = result.data,
                    recordsFiltered = result.data,
                    data = response
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = query?.DataTableOptions?.draw ?? "1",
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

        [HttpPost]
        public async Task<ActionResult> MakeDecision(Decission model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            // Convert string to enum
            if (!Enum.TryParse<AccountWorkflowStatus>(
                    model.Decision,
                    true,
                    out var workflowStatus))
            {
                return Json(new
                {
                    success = false,
                    message = $"Invalid decision value: {model.Decision}"
                });
            }

            // Replace string value with numeric string (if backend expects int)
            model.Decision = ((int)workflowStatus).ToString();

            // Send whole object
            var result = await _accountManagementService.MakeDecisionAsync(model);

            return Json(new
            {
                success = result.Result,
                message = Messaging.MessageResult(result)
            });
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