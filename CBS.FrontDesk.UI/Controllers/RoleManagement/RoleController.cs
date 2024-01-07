using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Loan.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.RoleManagement
{
    public class RoleController : BaseController
    {
        // GET: Role
        private readonly RoleServices _services;
        public RoleController(RoleServices services)
        {
            _services = services;
        }

        public async Task<ActionResult> Index()
        {
            
            return View();
        }
        public async Task<ActionResult> RolePermission(string KEY)
        {
            var data = await _services.GetRolePermissions(KEY);
            return View(data);
        }
        //
        [HttpPost]
        public async Task<ActionResult> Create(Role model)
        {
            if (ModelState.IsValid)
            {
                var data = await _services.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(Role model)
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
                var data = await _services.GetRoles();
                return PartialView(partialView, data);
            }
            //GetRolePermissions
            else if (path == "new")
            {
                return PartialView(partialView, new Role());
            }
            else if (path == "set_permission")
            {
                var data = await _services.GetRolePermissions(KEY);
                return PartialView(partialView, data);
            }
            else
            {
                var Tax = await _services.GetRole(KEY);
                return PartialView(partialView, Tax);

            }
        }

        public async Task<ActionResult> Delete(string KEYS)
        {
            var data = await _services.Delete(KEYS);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
       
    }
}