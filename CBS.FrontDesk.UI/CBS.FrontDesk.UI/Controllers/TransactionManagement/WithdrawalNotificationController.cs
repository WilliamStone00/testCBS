using CBS.BusinessService.Accounts;
using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity.LoanConf;
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
    public class WithdrawalNotificationController : BaseController
    {
        // GET: WithdrawalNotification
        private readonly WithdrawalNotificationServices _services;
        private readonly IndividualProfileServices _individualProfileServices;

        public WithdrawalNotificationController(WithdrawalNotificationServices services, IndividualProfileServices individualProfileServices = null)
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
            if (model.WithdrawalNotification.Id == null)
            {
                var data = await _services.Create(model.WithdrawalNotification);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else
            {
                return await Update(model.WithdrawalNotification);
            }
        }
        [HttpPost]
        public async Task<ActionResult> Update(WithdrawalNotification model)
        {

            var data = await _services.Update(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }
        [HttpPost]
        public async Task<ActionResult> ApproveOrReject(WithdrawalNotificationForm model)
        {

            var data = await _services.ApproveOrRejectWithdrawalNotification(model.WithdrawalNotification);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "list")
            {
                var notifications = await _services.GetWithdrawalNotifications();
                var form = new WithdrawalNotificationForm { WithdrawalNotifications = notifications.ToList() };
                return PartialView(partialView, form);
            }
            else if (path == "pendinglisting")
            {
                var notifications = await _services.GetPendingWithdrawalNotifications();
                var form = new WithdrawalNotificationForm { WithdrawalNotifications = notifications.ToList() };
                return PartialView(partialView, form);
            }
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
                var withdrawalNotification = await _services.GetBalanceOfLoanAndSaving(KEY, individual.CustomerList.LegalForm);
                var withdrawalNotifications = await _services.GetWithdrawalNotificationsByMemberId(KEY);
                withdrawalNotification.Customer = individual.CustomerList;
                var form = new WithdrawalNotificationForm { WithdrawalNotification = withdrawalNotification, Customer = individual.CustomerList, WithdrawalNotifications = withdrawalNotifications.ToList() };
                return PartialView(partialView, form);
            }
            else if (path == "customerrequest")
            {
                var withdrawalNotifications = await _services.GetWithdrawalNotificationsByMemberId(KEY);
                var form = new WithdrawalNotificationForm { WithdrawalNotifications = withdrawalNotifications.ToList() };
                return PartialView(partialView, form);
            }
            else if (path == "detail")
            {
                ViewBag.Key = KEY;
                var withdrawalNotification = await _services.GetWithdrawalNotification(KEY);
                var individual = await _individualProfileServices.GetCustomerLight(withdrawalNotification.CustomerId);
                var form = new WithdrawalNotificationForm { WithdrawalNotification = withdrawalNotification, Customer = individual.CustomerList };
                return PartialView(partialView, form);
            }
            else if (path == "get_to_approve")
            {
                ViewBag.Key = KEY;
                var withdrawalNotification = await _services.GetWithdrawalNotification(KEY);
                var individual = await _individualProfileServices.GetCustomerLight(withdrawalNotification.CustomerId);
                var withdrawalNotifications = await _services.GetWithdrawalNotificationsByMemberId(withdrawalNotification.CustomerId);
                var form = new WithdrawalNotificationForm { WithdrawalNotification = withdrawalNotification, Customer = individual.CustomerList, WithdrawalNotifications = withdrawalNotifications.ToList() };
                ViewBag.Status = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "Rejected", Text = "Rejected" },
                        new SelectListItem { Value = "Approved", Text = "Approved", Selected = true } // Default selection
                    };
                return PartialView(partialView, form);

            }
            else
            {
                ViewBag.Key = KEY;
                var withdrawalNotification = await _services.GetWithdrawalNotification(KEY);
                var individual = await _individualProfileServices.GetCustomerLight(withdrawalNotification.CustomerId);
                var withdrawalNotifications = await _services.GetWithdrawalNotificationsByMemberId(withdrawalNotification.CustomerId);
                var form = new WithdrawalNotificationForm { WithdrawalNotification = withdrawalNotification, Customer = individual.CustomerList, WithdrawalNotifications = withdrawalNotifications.ToList() };
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