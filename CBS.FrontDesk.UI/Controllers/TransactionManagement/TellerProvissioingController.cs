using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.UserManagement;
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
    [CheckSessionTimeOutAttribute]

    public class TellerProvissioingController : BaseController
    {
        // GET: TellerProvissioing
        private readonly TellerProvissioningServices _services;
        public TellerProvissioingController(TellerProvissioningServices services)
        {
            _services = services;
        }

        public async Task<ActionResult> Index()
        {
            await GetList();
            return View(new OpeningOfTheDay());
        }

        public async Task<ActionResult> Primary()
        {
            await GetList();
            return View(new OpeningOfTheDay());
        }
        public async Task<ActionResult> SubTeller()
        {
            await GetList();
            return View(new OpeningOfTheDay());
        }

        [HttpPost]
        public async Task<ActionResult> Primary(OpeningOfTheDay model)
        {
            var data = await _services.PrimaryTellerProvision(model.PrimaryTellerProvissioning);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

        }
        [HttpPost]
        public async Task<ActionResult> SubTeller(OpeningOfTheDay model)
        {
            var data = await _services.SubTellerProvision(model.SubTellerProvissioning);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

        }

        public async Task<ActionResult> Ajaxloader(string Key, string path)
        {
            if (Key != null)
            {
                if (path=="")
                {
                    var listing = await _services.GetUserTellerRole(Key);
                    return Json(listing, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var listing = await _services.GetSubTellers(Key);
                    return Json(listing, JsonRequestBehavior.AllowGet);
                }
              
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public async Task<bool> GetList()
        {
            var TellerRoles = await _services.GetUserTellerRole();
            var SubTellers = await _services.GetSubTellers();
            var PrimaryTeller = await _services.GetPrimaryTellers();
            ViewBag.TellerRoles = TellerRoles;
            ViewBag.SubTellers = SubTellers;
            ViewBag.PrimaryTeller = PrimaryTeller;
            return true;
        }
    }
}