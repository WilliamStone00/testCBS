using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.MembersAccountSettings.policy;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement.Policy
{
    public class MemberAccountActivationPolicyController : BaseController
    {
        // GET: TellerManagement
        private readonly MemberAccountActivationPolicyServices _tellerServices;
        public MemberAccountActivationPolicyController(MemberAccountActivationPolicyServices tellerServices)
        {
            _tellerServices = tellerServices;
           
        }

        public async Task<ActionResult> Index()
        {
            return View(new MemberAccountActivationPolicy());
        }

        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(MemberAccountActivationPolicy model)
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

        private Func<Task<ExecutionMessages>> GetInsertServiceAction(string serviceOption, MemberAccountActivationPolicy model)
        {
          
            return () => _tellerServices.Create(model);
        }

        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(string serviceOption, MemberAccountActivationPolicy model)
        {
            return () => _tellerServices.Update(model);
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
                    var data = await _tellerServices.GetMemberAccountActivationPolicys();
                    var sysData = data;
                    return PartialView(partialView, sysData);

                };
            }
            else if (path == "new")
            {
                return async () => PartialView(partialView, new MemberAccountActivationPolicy());
            }
            else
            {
                return async () => PartialView(partialView, await _tellerServices.GetMemberAccountActivationPolicy(key));
            }
        }

       
        public async Task<ActionResult> Ajaxloader(string Key)
        {
            var listing = await _tellerServices.GetMemberAccountActivationPolicy(Key);
            return Json(listing, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> Delete(string id)
        {
            var data = await _tellerServices.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}