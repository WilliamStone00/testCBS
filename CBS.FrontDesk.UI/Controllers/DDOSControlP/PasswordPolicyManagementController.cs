using CBS.BusinessService.Config;
using CBS.BusinessService.RequestLoggerServicesP;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity.RequestManagement;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace CBS.FrontDesk.UI.Controllers.DDOSControlP
{
    [CheckSessionTimeOutAttribute]
    public class PasswordPolicyManagementController : BaseController
    {
        private readonly BranchServices _branchServices;
        private readonly PasswordPolicyManagentService _passwordPolicyManagentService;
        public PasswordPolicyManagementController(BranchServices branchServices, PasswordPolicyManagentService passwordPolicyManagentService)
        {
            _branchServices=branchServices;
            _passwordPolicyManagentService=passwordPolicyManagentService;
        }
        // GET: PasswordPolicyManagement
        public async Task<ActionResult> Index()
        {
            var model = new IdleTime();
            model.PasswordPoliciesJson = JsonConvert.SerializeObject(IdleTime.GetDefaultPolicies(), Formatting.Indented);
            ViewBag.Branches=await _branchServices.GetBranches();
            return View(model);
        }
        public async Task<ActionResult> Update(string key)
        {
            var data = await _passwordPolicyManagentService.GetByIdAsync(key);
            ViewBag.Branches=await _branchServices.GetBranches();
            return View(data);
        }
        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(IdleTime model)
        {
            
            Func<Task<ExecutionMessages>> serviceAction = null;

            if (model.Action == "insert")
            {
                serviceAction = GetInsertServiceAction(model);
            }
            else
            {
                serviceAction = GetUpdateServiceAction(model);
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

        private Func<Task<ExecutionMessages>> GetInsertServiceAction(IdleTime model)
        {
            return () => _passwordPolicyManagentService.Add(model);

        }
        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(IdleTime model)
        {
            return () => _passwordPolicyManagentService.Update(model);
        }


        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            Func<Task<PartialViewResult>> serviceAction = GetServiceAction(path, partialView, KEY);

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

        private Func<Task<PartialViewResult>> GetServiceAction(string path, string partialView, string key)
        {
            if (path == "list")
            {
                return async () =>
                {
                    var data = await _passwordPolicyManagentService.GetAllAsync();
                    return PartialView(partialView, data.ToList());

                };
            }
            else
            {

                return async () =>
                {
                    var data=await _passwordPolicyManagentService.GetByIdAsync(key);
                    ViewBag.Branches=await _branchServices.GetBranches();
                    return PartialView(partialView, data);

                };
            }
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _passwordPolicyManagentService.DeleteAsync(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

        }
    }
}