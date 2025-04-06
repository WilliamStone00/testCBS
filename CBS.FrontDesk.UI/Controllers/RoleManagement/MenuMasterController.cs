using CBS.BusinessService;
using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using Microsoft.Ajax.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace CBS.FrontDesk.UI.Controllers.RoleManagement
{
    //[CheckSessionTimeOutAttribute]

    public class MenuMasterController : BaseController
    {
        // GET: MenuMaster
        private readonly MenuMasterServices _menuMasterServices;
        public MenuMasterController(MenuMasterServices menuMasterServices)
        {
            _menuMasterServices = menuMasterServices;

        }

        public async Task<ActionResult> Index()
        {
            //await GetList();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(MenuMaster model)
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


        private Func<Task<ExecutionMessages>> GetInsertServiceAction(string serviceOption, MenuMaster model)
        {
            return () => _menuMasterServices.Create(model);
        }

        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(string serviceOption, MenuMaster model)
        {
            return () => _menuMasterServices.Update(model);
        }


        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            if (path!="list")
            {
                await GetList();
            }

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
                    var data = await _menuMasterServices.GetMenuMasters();
                    return PartialView(partialView, data);

                };
            }
            else if (path == "get")
            {
                return async () => PartialView(partialView, await _menuMasterServices.GetMenuMaster(key));
            }
            else
            {
                return async () =>
                {
                    return PartialView(partialView, new MenuMaster());

                };
            }
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _menuMasterServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public async Task<bool> GetList()
        {
            var stringValues = await _menuMasterServices.GetMenuMastersDropDowns();
            ViewBag.MenuMasters = stringValues.ToList();
            return true;
        }
    }
}