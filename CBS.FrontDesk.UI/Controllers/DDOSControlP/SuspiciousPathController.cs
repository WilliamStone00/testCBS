using CBS.BusinessService.Config;
using CBS.BusinessService.RequestLoggerServicesP;
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

    public class SuspiciousPathController : BaseController
    {
        // GET: SuspiciousPath
        private readonly SuspiciousPathService _services;
        public SuspiciousPathController(SuspiciousPathService membersServices)
        {
            _services = membersServices;

        }

        public async Task<ActionResult> Index()
        {
            return View(new SuspiciousPath());
        }

        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(SuspiciousPath model)
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

        private Func<Task<ExecutionMessages>> GetInsertServiceAction(SuspiciousPath model)
        {
            return () => _services.Add(new AddSuspiciousPathCommand { IsRegex=model.IsRegex, Pattern=model.Pattern});

        }
        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(SuspiciousPath model)
        {
            return () => _services.Update(new UpdateSuspiciousPathCommand { Pattern=model.Pattern, IsRegex=model.IsRegex, Id=model.Id});
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
                    var data = await _services.GetAllAsync();
                    return PartialView(partialView, data.ToList());

                };
            }
            else if (path == "new")
            {
                return async () => PartialView(partialView, new SuspiciousPath());
            }
            else
            {
                return async () => PartialView(partialView, await _services.GetSuspiciousPath(key));
            }
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.DeleteAsync(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

        }
    }
}