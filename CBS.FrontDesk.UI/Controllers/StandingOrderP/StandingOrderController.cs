using CBS.BusinessService;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.StandingOrderP
{
    [CheckSessionTimeOutAttribute]

    public class StandingOrderController : BaseController
    {
        // GET: StandingOrder
        private readonly IndividualProfileServices _services;
        private readonly StandingOrderServices _standingOrderServices;

        public StandingOrderController(IndividualProfileServices services, StandingOrderServices standingOrderServices)
        {
            _services = services;
            _standingOrderServices=standingOrderServices;
        }

       
        public ActionResult Index()
        {
            return View();
        }
      
       
        [HttpPost]
        public async Task<ActionResult> Create(StandingOrderCarrier model)
        {

            if (ModelState.IsValid)
            {
                if (model.Action=="insert")
                {
                    var data = await _standingOrderServices.Create(model.AddOrUpdateStandingOrderCommand);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data), activationid = data.SessionID });
                }
                else
                {
                    var data = await _standingOrderServices.Update(model.AddOrUpdateStandingOrderCommand);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data), activationid = data.SessionID });

                }

            }

            // Convert ModelState errors to a readable string
            var errors = ModelState
                .Where(ms => ms.Value.Errors.Count > 0)
                .SelectMany(ms => ms.Value.Errors.Select(e => e.ErrorMessage))
                .ToList();

            var errorMessage = string.Join("<br />", errors);

            return Json(new { success = false, status = false, message = errorMessage });
        }

        

       
        [HttpPost]
        public async Task<ActionResult> Update(StandingOrderCarrier model)
        {
            if (ModelState.IsValid)
            {
                var data = await _standingOrderServices.Update(model.AddOrUpdateStandingOrderCommand);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            // Convert ModelState errors to a readable string
            var errors = ModelState
                .Where(ms => ms.Value.Errors.Count > 0)
                .SelectMany(ms => ms.Value.Errors.Select(e => e.ErrorMessage))
                .ToList();

            var errorMessage = string.Join("<br />", errors);

            return Json(new { success = false, status = false, message = errorMessage });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
           
            if (path=="search")
            {
                ViewBag.Key=null;
                var data = await _services.GetCustomerLight(KEY);
                if (data!=null)
                {
                    var standingOrders = await _standingOrderServices.GetStandingOrderByMemberId(KEY);

                    return PartialView(partialView, new StandingOrderCarrier { Customer = data.CustomerList, AddOrUpdateStandingOrderCommand=new AddOrUpdateStandingOrderCommand { MemberId=data.CustomerList.CustomerId, MemberName=$"{data.CustomerList.FirstName} {data.CustomerList.LastName}"}, StandingOrders=standingOrders .ToList()});

                }
                else
                {
                    ViewBag.message = $"{KEY} was not found in the database.";

                    return PartialView("_DataNotFound", new StandingOrderCarrier());
                }
            }
            else if (path=="list")
            {
                var standingOrder = await _standingOrderServices.GetAllStandingOrder();
                var StandingOrderCarrier = new StandingOrderCarrier { StandingOrders=standingOrder.ToList()};
                return PartialView(partialView, StandingOrderCarrier);
            }
            else if (path=="my_orders")
            {
                var data = await _services.GetCustomerLight(KEY);
                if (data!=null)
                {
                    var standingOrders = await _standingOrderServices.GetStandingOrderByMemberId(KEY);
                    return PartialView(partialView, new StandingOrderCarrier {StandingOrders=standingOrders.ToList(), Customer=data.CustomerList });

                }
                else
                {
                    ViewBag.message = $"{KEY} was not found in the database.";

                    return PartialView("_DataNotFound", new StandingOrderCarrier());
                }
            }
            else
            {
                ViewBag.Key=KEY;
                var standingOrder = await _standingOrderServices.GetStandingOrder(KEY);
                var addOrUpdateStanding=_standingOrderServices.MapStandingOrderToCommand(standingOrder);
                var data = await _services.GetCustomerLight(standingOrder.MemberId);
                var StandingOrderCarrier = new StandingOrderCarrier { AddOrUpdateStandingOrderCommand=addOrUpdateStanding, StandingOrder=standingOrder, Customer=data.CustomerList };
                return PartialView(partialView, StandingOrderCarrier);
            }
        }
        public async Task<ActionResult> GetStandingOrderPartialView(string Key)
        {
            var data = await _standingOrderServices.GetStandingOrder(Key);
            if (data == null)
            {
                return HttpNotFound();
            }
            var standingOrders = await _standingOrderServices.GetStandingOrderByMemberId(data.MemberId);
            StandingOrderCarrier standingOrder = new StandingOrderCarrier();
            standingOrder.StandingOrders=standingOrders.ToList();
            standingOrder.StandingOrder=data;
            return PartialView("_StandingOrderDetailsPartial", standingOrder);
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _standingOrderServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        //[HttpGet]
        //public async Task<ActionResult> LoadData(string searchCriteria = "All")
        //{
        //    try
        //    {
        //        var dataTable = await _standingOrderServices.GetDataTable(GetDataTableOptions(), searchCriteria);
        //        return Json(new { draw = dataTable.draw, recordsFiltered = dataTable.recordsTotal, recordsTotal = dataTable.recordsTotal, data = dataTable.data }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}
        //[HttpPost]
        //public async Task<ActionResult> LoadDataSearch(string searchCriterial = "All")
        //{
        //    try
        //    {
        //        var dataTable = await _standingOrderServices.GetDataTable(GetDataTableOptions(), searchCriterial);
        //        return Json(new { draw = dataTable.draw, recordsFiltered = dataTable.recordsTotal, recordsTotal = dataTable.recordsTotal, data = dataTable.data });

        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}
    }

}