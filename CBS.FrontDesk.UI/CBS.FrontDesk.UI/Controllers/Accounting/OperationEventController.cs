
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.BusinessService;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    [CheckSessionTimeOutAttribute]
    public class OperationEventController : BaseController
    {
        // GET: OperationEvent
        private readonly OperationEventServices _operationEventServices;
        public OperationEventController(OperationEventServices operationEventServices)
        {
            _operationEventServices = operationEventServices;
        }

        public async Task<ActionResult> Index()
        {
            GetList();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(OperationEvent model)
        {
            if (ModelState.IsValid)
            {
                var data = await _operationEventServices.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(OperationEvent model)
        {
            if (ModelState.IsValid)
            {
                var data = await _operationEventServices.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            GetList();
            if (path == "list")
            {
                var data = await _operationEventServices.GetOperationEvents();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                return PartialView(partialView, new OperationEvent());
            }
            else
            {
                var OperationEvent = await _operationEventServices.GetOperationEvent(KEY);
                return PartialView(partialView, OperationEvent);

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _operationEventServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public void GetList()
        {
           
            //ViewBag.Branches = _operationEventServices.GetAllBranchesByBankId(_operationEventServices.BankId);
          
        }
    }
}