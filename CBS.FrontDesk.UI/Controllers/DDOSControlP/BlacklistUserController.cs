using CBS.BusinessService.Config;
using CBS.BusinessService.RequestLoggerServicesP;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.RequestManagement;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.DDOSControlP
{
    [CheckSessionTimeOutAttribute]

    public class BlacklistUserController : BaseController
    {
        // GET: BlacklistUser
        private readonly RateLimitedUserService _membersServices;
        private readonly UserManagementServices _userManagementServices;
        public BlacklistUserController(RateLimitedUserService membersServices, UserManagementServices userManagementServices)
        {
            _membersServices = membersServices;
            _userManagementServices=userManagementServices;
        }

        public async Task<ActionResult> Index()
        {
            ViewBag.Users=await _userManagementServices.GetUserDropDownList();
            return View(new RateLimitedUser());
        }

        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(RateLimitedUser model)
        {
            Func<Task<ExecutionMessages>> serviceAction = null;

            serviceAction = GetInsertServiceAction(model);

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

        private Func<Task<ExecutionMessages>> GetInsertServiceAction(RateLimitedUser model)
        {
            return () => _membersServices.Add(model.BlockRequest);

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
        [HttpGet]
        public async Task<ActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return HttpNotFound("ID is required.");

            var entry = await _membersServices.GetByIdAsync(id);
            if (entry == null)
                return HttpNotFound("Blacklist entry not found.");

            return PartialView("_Details", entry);
        }
        private Func<Task<PartialViewResult>> GetServiceAction(string path, string partialView, string key)
        {
            if (path == "list")
            {
                return async () =>
                {
                    var data = await _membersServices.GetAllAsync();
                    return PartialView(partialView, data.ToList());

                };
            }
            else if (path == "new")
            {
                return async () => PartialView(partialView, new RateLimitedUser());
            }
            else
            {
                return async () => PartialView(partialView, await _membersServices.GetByIdAsync(key));
            }
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _membersServices.DeleteAsync(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

        }
    }
}