using CBS.BusinessService.Accounting_V2.AccountTypeGroup;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AccountTypeGroup;
using CBS.FrontDesk.Data.Message;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.AccountTypeGroup
{
    public class AccountTypeGroupController : Controller
    {
        private readonly AccountTypeGroupService _accountTypeGroupService;

        public AccountTypeGroupController(AccountTypeGroupService accountTypeGroupService)
        {
            _accountTypeGroupService = accountTypeGroupService;
        }

        public async Task<ActionResult> Index()
        {
            return View();
        }

        public async Task<ActionResult> List()
        {
            return View();
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            if (path == "list")
            {
                var data = await _accountTypeGroupService.GetAsync();
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                return PartialView(partialView, new AccountTypeGroupResponse());
            }
            else if (path == "edit" || path == "get")
            {
                var data = await _accountTypeGroupService.GetByIdAsync(KEY);
                return PartialView(partialView, data);
            }
            else
            {
                return PartialView(partialView, new AccountTypeGroupResponse());
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetAccountTypeGroups()
        {
            try
            {
                var groups = await _accountTypeGroupService.GetAsync();

                var result = groups.Select(g => new
                {
                    id = g.Id,
                    name = g.Name ?? string.Empty,
                    code = g.Code ?? string.Empty,
                    description = g.Description ?? string.Empty,
                    displayOrder = g.DisplayOrder,
                    isActive = g.IsActive,
                    statusBadge = g.StatusBadge,
                    statusClass = g.StatusClass,
                    accountTypesCount = g.AccountTypes?.Count ?? 0
                }).ToList();

                return Json(new { data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Error loading data" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetGroupDetails(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

            try
            {
                var group = await _accountTypeGroupService.GetByIdAsync(KEY);
                if (group == null)
                    return Json(new { success = false, message = "Account Type Group not found." }, JsonRequestBehavior.AllowGet);

                var result = new
                {
                    success = true,
                    data = new
                    {
                        id = group.Id,
                        name = group.Name,
                        code = group.Code,
                        description = group.Description,
                        displayOrder = group.DisplayOrder,
                        isActive = group.IsActive,
                        accountTypesCount = group.AccountTypes?.Count ?? 0
                    }
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading group details." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetGroupDetailsPartial(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Content("<div class='alert alert-danger'>Invalid ID provided.</div>");

            try
            {
                var group = await _accountTypeGroupService.GetByIdAsync(KEY);
                if (group == null)
                    return Content("<div class='alert alert-danger'>Account Type Group not found.</div>");

                return PartialView("_AccountTypeGroupDetails", group);
            }
            catch (Exception ex)
            {
                return Content("<div class='alert alert-danger'>Error loading group details.</div>");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOrUpdate(AccountTypeGroupResponse model)
        {
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                return await Create(model);
            }
            else
            {
                return await Update(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AccountTypeGroupResponse model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });          

            var result = await _accountTypeGroupService.CreateAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(AccountTypeGroupResponse model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });           

            var result = await _accountTypeGroupService.UpdateAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpGet]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

            var result = await _accountTypeGroupService.DeleteAsync(KEY);

            bool success = result?.Result ?? false;
            string message = Messaging.MessageResult(result) ?? "Operation completed.";

            return Json(new { success = success, message = message }, JsonRequestBehavior.AllowGet);
        }
    }
}