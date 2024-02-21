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
            var primaryTellerProvisionings = await _primaryTellerEndOfDayServices.GetPrimaryTellerHistories();
            return View(new EndOfTheDay { PrimaryTellerProvisioningHistories= primaryTellerProvisionings.ToList()});
        }
        public async Task<ActionResult> SubTeller()
        {

            var tellerProvisioningDtos = await _subTellerEndOfDay.GetSubTellerHistories();
            return View(new EndOfTheDay { SubTellerProvioningHistories = tellerProvisioningDtos.ToList() });
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
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            try
            {
                ViewBag.KEY = KEY;
                if (serviceOption == "subteller")
                {
                    if (path=="details")
                    {
                        var provisioningDto = await _subTellerEndOfDay.GetDailyOperation(KEY);
                        return PartialView(partialView, new EndOfTheDay { SubTellerProvioningHistory = provisioningDto });
                    }
                    else
                    {
                        var command = await _subTellerEndOfDay.GetDailyOperationToClose(KEY);
                        return PartialView(partialView, new EndOfTheDay {  EndOfDaySubTellerCommand = command });
                    }
                }
                else
                {
                    var provisioningDto = await _primaryTellerEndOfDayServices.GetDailyOperation(KEY);
                    return PartialView(partialView, new EndOfTheDay { PrimaryTellerProvisioningHistory = provisioningDto });
                }

            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = ex.Message; // Store error message
                return RedirectToAction("Index", "Error"); // Redirect to error page
            }
        }
    }
}