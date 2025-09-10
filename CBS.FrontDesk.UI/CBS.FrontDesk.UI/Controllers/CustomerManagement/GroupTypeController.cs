using CBS.BusinessService.Accounting;
using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.MembersAccountSettings.policy;
using CBS.FrontDesk.Data.Entity.CustomerManagement.Grouping;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.CustomerManagement
{
    [CheckSessionTimeOutAttribute]
    public class GroupTypeController : BaseController
    {
        // GET: GroupType
        private readonly GroupTypeServices _groupTypeServices;
        public GroupTypeController(GroupTypeServices tellerServices)
        {
            _groupTypeServices = tellerServices;
        }

        public async Task<ActionResult> Index()
        {
            return View(new GroupType());
        }

        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(GroupType model)
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

        private Func<Task<ExecutionMessages>> GetInsertServiceAction(string serviceOption, GroupType model)
        {

            return () => _groupTypeServices.Create(model);
        }

        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(string serviceOption, GroupType model)
        {
            return () => _groupTypeServices.Update(model);
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
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

                    var data = await _groupTypeServices.GetGroupTypes();
                    var sysData = data;
                    return PartialView(partialView, sysData);

                };
            }
            else if (path == "new")
            {
                return async () =>
                {
                    return PartialView(partialView, new GroupType());
                };
            }
            else
            {

                return async () =>
                {
                    return PartialView(partialView, await _groupTypeServices.GetGroupType(key));
                };
            }
        }
        public async Task<ActionResult> Delete(string id)
        {
            var data = await _groupTypeServices.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}