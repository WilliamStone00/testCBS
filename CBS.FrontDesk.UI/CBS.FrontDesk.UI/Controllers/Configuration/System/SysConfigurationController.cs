using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.Config.System;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Configuration.System
{
    [CheckSessionTimeOutAttribute]

    public class SysConfigurationController : BaseController
    {
        // GET: SysConfiguration
        private readonly SysConfigurationServices _SysConfigurationServices;
        public SysConfigurationController( SysConfigurationServices SysConfigurationServices)
        {
            _SysConfigurationServices = SysConfigurationServices;

        }

        public async Task<ActionResult> Index()
        {
            return View(new SysConfiguration());
        }
        private Func<Task<ExecutionMessages>> GetInsertServiceAction(string serviceOption, SysConfiguration model)
        {
            return () => _SysConfigurationServices.Create(model);

        }

        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(SysConfiguration model)
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

    
        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(string serviceOption, SysConfiguration model)
        {
            return () => _SysConfigurationServices.Update(model);
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
            if (serviceOption == "SysConfiguration")
            {
                if (path == "list")
                {
                    return async () =>
                    {
                        var data = await _SysConfigurationServices.GetSysConfigurations();
                        var sysData = data.ToList();
                        return PartialView(partialView, sysData);

                    };
                }
                else if (path == "new")
                {
                    return async () => PartialView(partialView, new SysConfiguration());
                }
                else
                {
                    return async () => PartialView(partialView, await _SysConfigurationServices.GetSysConfiguration(key));
                }

            }
            return null;
        }

        
    }
}