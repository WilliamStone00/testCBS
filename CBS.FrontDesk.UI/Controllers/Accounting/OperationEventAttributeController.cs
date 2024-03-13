
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CBS.BusinessService.Accounting;

using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.BusinessService;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    public class OperationEventAttributeController : BaseController
    {
        // GET: OperationEventAttribute
        private readonly OperationEventAttributeServices _Services;
        private readonly OperationEventServices _operationEventServices;

        public OperationEventAttributeController(OperationEventAttributeServices OperationEventAttributeAttributeServices, OperationEventServices opservice)
        {
            _Services = OperationEventAttributeAttributeServices;
            _operationEventServices = opservice;
        }

        public async Task<ActionResult> Index()
        {
           await GetList();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(OperationEventAttribute model)
        {
            if (ModelState.IsValid)
            {
                var data = await _Services.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(OperationEventAttribute model)
        {
            if (ModelState.IsValid)
            {
                var data = await _Services.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            GetList();
            if (path == "list")
            {
                var attributes = await _Services.GetOperationEventAttributes();

                var events = await _operationEventServices.GetOperationEvents();
                var data = ConvertToDtos(attributes.ToList(), events.ToList());
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                return PartialView(partialView, new OperationEventAttribute());
            }
            else
            {
                var OperationEventAttribute = await _Services.GetOperationEventAttribute(KEY);
                return PartialView(partialView, OperationEventAttribute);

            }
        }

       
        public List<OperationEventAttributeDto> ConvertToDtos(List<OperationEventAttribute> attributes, List<OperationEvent> events)
        {
            return (from a in attributes
                join e in events on a.OperationEventId equals e.Id
                select new OperationEventAttributeDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    OperationEventName = e.OperationEventName
                }).ToList();
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _Services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public async Task GetList()
        {

         var Branches = await _operationEventServices.GetOperationEvents();
            ViewBag.OperationEvents = Branches;

        }
    }
}