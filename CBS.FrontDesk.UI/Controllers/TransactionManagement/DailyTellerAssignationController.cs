using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
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
    public class DailyTellerAssignationController : BaseController
    {
        // GET: DailyTellerAssignation
        private readonly DailyTellerServices _services;
        private readonly TellerServices _tellerServices;
        private readonly BranchServices _branchServices;
        public DailyTellerAssignationController(DailyTellerServices services, TellerServices tellerServices = null, BranchServices branchServices = null)
        {
            _services = services;
            _tellerServices = tellerServices;
            _branchServices = branchServices;
        }

        public async Task<ActionResult> Index()
        {
            await LoadDropdowns();
            return View();
        }
       
        [HttpPost]
        public async Task<ActionResult> Create(DailyTeller model)
        {
            if (model.Id == null)
            {
                var data = await _services.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else
            {
                return await Update(model);
            }
        }
        [HttpPost]
        public async Task<ActionResult> Update(DailyTeller model)
        {
            var data = await _services.Update(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string dateFrom = null, string dateTo = null)
        {
            if (path == "search")
            {
                if (KEY!=string.Empty)
                {
                    var data = await _services.GetDailyTellers(_services.GetDateTime(dateFrom), _services.GetDateTime(dateTo),KEY);
                    return PartialView(partialView, data.ToList());

                }
                else
                {
                    var data = await _services.GetDailyTellers(_services.GetDateTime(dateFrom), _services.GetDateTime(dateTo));
                    return PartialView(partialView, data.ToList());

                }
            }

            else if (path == "new")
            {
                await LoadDropdowns();
                return PartialView(partialView, new DailyTeller());
            }
            else
            {
                ViewBag.Key = KEY;
                await LoadDropdowns();
                var dailyTeller = await _services.GetDailyTeller(KEY);
                dailyTeller.UserId = $"{dailyTeller.UserId}@{dailyTeller.UserName}";
                return PartialView(partialView, dailyTeller);

            }


        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

        public async Task LoadDropdowns()
        {
            var Users = await _services.LoadDailyUsers();
            var Tellers = await _tellerServices.GetTellersStringValuesAsync();
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;

            ViewBag.Users = Users;
            ViewBag.Tellers = Tellers;
        }
        //public async Task<ActionResult> Loa(string Key)
        //{
        //    var Users = await _services.LoadDailyUsers();
        //    var Tellers = await _tellerServices.GetTellersStringValuesAsync();
        //    var Branches = await _branchServices.GetBranches();
        //    ViewBag.Users = Users;
        //    ViewBag.Tellers = Tellers;
        //    ViewBag.Branches = Branches;

        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}
    }

}