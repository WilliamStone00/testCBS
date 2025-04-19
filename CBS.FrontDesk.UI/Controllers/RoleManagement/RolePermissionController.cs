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
    [CheckSessionTimeOutAttribute]

    public class RolePermissionController : BaseController
    {
        // GET: RolePermission
        private readonly RolePermissionServices _services;
        public RolePermissionController(RolePermissionServices services)
        {
            _services = services;
        }

        public async Task<ActionResult> Index()
        {
            await GetList();
            var data = await _services.GetAssignPermissions();
            return View(new PermissionMenuLoaderDto { PermissionMenuLoaders = data.ToList() });
        }

        //[HttpPost]
        //public async Task<ActionResult> AddOrUpdate([FromBody] PermissionMenuLoaderDto model)
        //{
        //    if (model.Action == "insert")
        //    {
        //        foreach (var roleId in model.roleIDs)
        //        {
        //            model.roleID = roleId;

        //            var serviceAction = GetInsertServiceAction(model.ServiceOption, model);
        //            if (serviceAction != null)
        //            {
        //                try
        //                {
        //                    var data = await serviceAction();
        //                    if (!data.Result)
        //                        return Json(new { success = false, message = Messaging.MessageResult(data) });
        //                }
        //                catch (Exception ex)
        //                {
        //                    return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
        //                }
        //            }
        //        }

        //        return Json(new { success = true, message = "Permissions successfully assigned to selected roles." });
        //    }

        //    return Json(new { success = false, message = "Invalid action." });
        //}

        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(PermissionMenuLoaderDto model)
        {
            Func<Task<ExecutionMessages>> serviceAction = null;

            if (model.Action == "insert")
            {
                serviceAction = GetInsertServiceAction(model.ServiceOption, model);
            }
            else
            {
                serviceAction = GetUpdateServiceAction(model.ServiceOption, model);
            }

            if (serviceAction != null)
            {
                try
                {
                    var data = await serviceAction();
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
                }
            }

            return Json(new { success = false, status = false, message = "Invalid option selected." });
        }
        
       
        private Func<Task<ExecutionMessages>> GetInsertServiceAction(string serviceOption, PermissionMenuLoaderDto model)
        {
            return () => _services.Create(model);
        }

        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(string serviceOption, PermissionMenuLoaderDto model)
        {
            return () => _services.Update(model);
        }


        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            await GetList();
            Func<Task<PartialViewResult>> serviceAction = GetServiceAction(path, partialView, KEY, serviceOption);

            if (serviceAction != null)
            {
                var partialResult = await serviceAction();

                if (partialResult != null)
                {
                    return partialResult;
                }
            }

            return HttpNotFound(); // Or return a default view for handling unknown paths
        }

        private Func<Task<PartialViewResult>> GetServiceAction(string path, string partialView, string key, string serviceOption)
        {
            if (path == "list")
            {
                return async () =>
                {
                    var data = await _services.GetRolePermissions();
                    return PartialView(partialView, data);

                };
            }
            else
            {
                return async () => PartialView(partialView, new PermissionMenuLoaderDto());
            }

        }

        public async Task<bool> GetList()
        {
          
            var stringValues = await _services.GetRolePermissions();
            ViewBag.Roles = stringValues.ToList();
            return true;
        }
    }
}