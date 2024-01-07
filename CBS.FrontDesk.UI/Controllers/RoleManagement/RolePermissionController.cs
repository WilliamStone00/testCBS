using CBS.BusinessService;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.RoleManagement
{
    public class RolePermissionController : Controller
    {
        // GET: RolePermission
        private readonly RolePermissionServices _services;
        public RolePermissionController(RolePermissionServices services)
        {
            _services = services;
        }

        public async Task<ActionResult> Index()
        {

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RolePermissionRequestCommand model)
        {
            if (ModelState.IsValid)
            {
                var data = await _services.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(RolePermissionRequestCommand model)
        {
            if (ModelState.IsValid)
            {
                var data = await _services.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "list")
            {
                var data = await _services.GetRolePermissions();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                return PartialView(partialView, new RolePermissionRequestCommand());
            }
            else if(path == "getby_roleID")
            {
                var Tax = await _services.GetRolePermissions(KEY);
                return PartialView(partialView, Tax);

            }
            else
            {
                var Tax = await _services.GetRolePermission(KEY);
                return PartialView(partialView, Tax);

            }
        }

        public async Task<ActionResult> Delete(List<string> KEYS)
        {
            var data = await _services.Delete(KEYS);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}