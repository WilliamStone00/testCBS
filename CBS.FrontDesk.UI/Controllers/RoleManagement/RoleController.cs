using CBS.BusinessService;
using CBS.BusinessService.Accounting;

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
    //[CheckSessionTimeOutAttribute]

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
            return View(new RolePermissionManagement { RolePermissions = data.ToList() });
        }
        //
        [HttpPost]
        public async Task<ActionResult> Create(RolePermissionManagement model)
        {
            if (ModelState.IsValid)
            {
                var data = await _services.Create(model.Role);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(RolePermissionManagement model)
        {
            if (ModelState.IsValid)
            {
                var data = await _services.Update(model.Role);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            if (path == "list")
            {
                if (serviceOption== "view_role_permission")
                {
                    var data = await _services.GetRolePermissions(KEY);

                    return PartialView(partialView, new RolePermissionManagement { RolePermissions = data.ToList() });
                }
                else
                {
                    var data = await _services.GetRoles();
                    return PartialView(partialView, new RolePermissionManagement { Roles = data.ToList() });
                }
           
            }
            //GetRolePermissions
            else if (path == "new")
            {
                return PartialView(partialView, new RolePermissionManagement());
            }
      
            else
            {
                var role = await _services.GetRole(KEY);
                return PartialView(partialView, new RolePermissionManagement { Role = role });

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> DeleteRolePersmission(string KEY)
        {
            var data = await _services.DeleteRolePermission(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

    }
}