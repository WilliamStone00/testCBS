using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    public class EndOfDayOperationController : BaseController
    {
        // GET: EndOfDayOperation
        private readonly SubTellerEndOfDayServices _subTellerEndOfDay;
        private readonly PrimaryTellerEndOfDayServices _primaryTellerEndOfDayServices;

        public EndOfDayOperationController(SubTellerEndOfDayServices services, PrimaryTellerEndOfDayServices primaryTellerEndOfDayServices)
        {
            _subTellerEndOfDay = services;
            _primaryTellerEndOfDayServices = primaryTellerEndOfDayServices;
        }

        public async Task<ActionResult> Index()
        {
            return View(new EndOfTheDay());
        }

        public async Task<ActionResult> Primary()
        {
            return View(new EndOfTheDay());
        }
        public async Task<ActionResult> SubTeller()
        {
            return View(new EndOfTheDay());
        }

        [HttpPost]
        public async Task<ActionResult> Primary(EndOfTheDay model)
        {
            var data = await _primaryTellerEndOfDayServices.EndTheDay(model.EndOfDayPrimaryTellerCommand);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

        }
        [HttpPost]
        public async Task<ActionResult> SubTeller(EndOfTheDay model)
        {
            var data = await _subTellerEndOfDay.EndTheDay(model.EndOfDaySubTellerCommand);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

        }
    }
}