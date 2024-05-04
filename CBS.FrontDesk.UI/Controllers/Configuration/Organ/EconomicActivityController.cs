using CBS.BusinessService.Config;
using CBS.BusinessService.Config.Localization;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Configuration.Organ
{
    [CheckSessionTimeOutAttribute]

    public class EconomicActivityController : BaseController
    {
        // GET: EconomicActivity
        private readonly EconomicActivityServices _economyServices;
        public EconomicActivityController(EconomicActivityServices economyServices)
        {
            _economyServices = economyServices;

        }

        public async Task<ActionResult> Index()
        {
            return View(new EconomicActivity());
        }

        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(EconomicActivity model)
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

        private Func<Task<ExecutionMessages>> GetInsertServiceAction(EconomicActivity model)
        {
            return () => _economyServices.Create(model);

        }
        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(EconomicActivity model)
        {
            return () => _economyServices.Update(model);
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
                    var data = await _economyServices.GetEconomicActivities();
                    return PartialView(partialView, data.ToList());

                };
            }
            else if (path == "new")
            {
                return async () => PartialView(partialView, new EconomicActivity());
            }
            else
            {
                return async () => PartialView(partialView, await _economyServices.GetEconomicActivity(key));
            }
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _economyServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

        }

    }
}