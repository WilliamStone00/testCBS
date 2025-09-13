using CBS.BusinessService.Accounts;
using CBS.BusinessService.CustomerManagement;
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
    public class ChargesWaivedController : BaseController
    {
        // GET: ChargesWaived
        private readonly IndividualProfileServices _individualProfileServices;
        private readonly ChargesWaivedServices _services;

        public ChargesWaivedController(ChargesWaivedServices services, IndividualProfileServices individualProfileServices = null)
        {
            _services = services;
            _individualProfileServices = individualProfileServices;
        }
        public async Task<ActionResult> Index()
        {
            return View();
        }
        public async Task<ActionResult> Requests()
        {
            return View();
        }
        public async Task<ActionResult> Pending()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(WithdrawalNotificationForm model)
        {
            if (model.ChargesWaived.Id == null)
            {
                var data = await _services.Create(model.ChargesWaived);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else
            {
                return await Update(model.ChargesWaived);
            }
        }
        [HttpPost]
        public async Task<ActionResult> Update(ChargesWaived model)
        {

            var data = await _services.Update(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }
        
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "list")
            {
                var notifications = await _services.GetChargesWaiveds();
                var form = new WithdrawalNotificationForm { ChargesWaiveds = notifications.ToList() };
                return PartialView(partialView, form);
            }
            //else if (path == "pendinglisting")
            //{
            //    var notifications = await _services.GetPendingWithdrawalNotifications();
            //    var form = new WithdrawalNotificationForm { WithdrawalNotifications = notifications.ToList() };
            //    return PartialView(partialView, form);
            //}
            else if (path == "search")
            {
                if (KEY == null || KEY == "")
                {
                    ViewBag.message = "Empty data was submited. Please enter search criterial";
                    return PartialView("_DataNotFound", new WithdrawalNotificationForm());
                }
                var individualProfile = await _individualProfileServices.GetSingleCustomer(KEY);
                if (individualProfile == null)
                {
                    ViewBag.message = $"{KEY} was not found in the database.";
                    return PartialView("_DataNotFound", new WithdrawalNotificationForm());
                }
                var form = new WithdrawalNotificationForm { Customer = individualProfile };
                return PartialView(partialView, form);
            }
            else if (path == "request")
            {
                var individual = await _individualProfileServices.GetCustomerLight(KEY);
                var form = new WithdrawalNotificationForm { ChargesWaived = new ChargesWaived { CustomerId= individual .CustomerList.CustomerId}, Customer = individual.CustomerList};
                return PartialView(partialView, form);
            }
            else if (path == "customerrequest")
            {
                var chargesWaiveds = await _services.GetChargesWaiveds(KEY);
                var form = new WithdrawalNotificationForm { ChargesWaiveds = chargesWaiveds.ToList() };
                return PartialView(partialView, form);
            }
            else if (path == "detail")
            {
                ViewBag.Key = KEY;
                var chargesWaived = await _services.GetChargesWaived(KEY);
                var individual = await _individualProfileServices.GetCustomerLight(chargesWaived.CustomerId);
                var form = new WithdrawalNotificationForm { Customer = individual.CustomerList, ChargesWaived= chargesWaived };
                return PartialView(partialView, form);
            }
           
            else
            {
                ViewBag.Key = KEY;
                var chargesWaived = await _services.GetChargesWaived(KEY);
                var individual = await _individualProfileServices.GetCustomerLight(chargesWaived.CustomerId);
                var form = new WithdrawalNotificationForm { Customer = individual.CustomerList, ChargesWaived = chargesWaived };
                return PartialView(partialView, form);

            }

        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

    }

}