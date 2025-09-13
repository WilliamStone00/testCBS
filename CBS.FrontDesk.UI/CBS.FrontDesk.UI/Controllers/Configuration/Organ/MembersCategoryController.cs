using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
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

    public class MembersCategoryController : BaseController
    {
        // GET: MembersCategory
        private readonly CustomerCategoryServices _membersServices;
        public MembersCategoryController(CustomerCategoryServices membersServices)
        {
            _membersServices = membersServices;

        }

        public async Task<ActionResult> Index()
        {
            return View(new CustomerCategory());
        }

        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(CustomerCategory model)
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

        private Func<Task<ExecutionMessages>> GetInsertServiceAction(CustomerCategory model)
        {
            return () => _membersServices.Create(model);

        }
        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(CustomerCategory model)
        {
            return () => _membersServices.Update(model);
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
                    var data = await _membersServices.GetCustomerCategories();
                    return PartialView(partialView, data.ToList());

                };
            }
            else if (path == "new")
            {
                return async () => PartialView(partialView, new CustomerCategory());
            }
            else
            {
                return async () => PartialView(partialView, await _membersServices.GetCustomerCategory(key));
            }
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _membersServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

        }
    }
}