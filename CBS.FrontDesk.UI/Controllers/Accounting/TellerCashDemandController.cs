using CBS.BusinessService.Accounting;
using CBS.BusinessService;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    public class TellerCashDemandController : BaseController
    {
        public TellerCashReplenishmentServices Service { get; private set; }

        public TellerCashDemandController()
        {
           Service= new TellerCashReplenishmentServices();
        }
        public async Task<ActionResult> Index()

        {
       await GetList();
            return View();
        }


        public async Task GetList(string language = "En")
        {

            ViewBag.OpeningOfDayId = await Service.GetCashReplenimentRequestId();
    
        }


        public async Task<ActionResult> CreateCashReplenishmentRequest()
        {
            await GetList();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateCashReplenishmentRequest(CashInfusionRequest model)
        {
            if (ModelState.IsValid)
            {
               var result=  await Service.CreateCashReplenishmentRequest(model);
                if (result.MessageStatus.Equals("Failed"))
                {
                    var datass = DetailsDto.SetDefault(model);
                    return View("Failed_Request_View", datass);
                }
                else
                {
                   
                    return View("Successfull_Request_View", model);
                }

            }

            var datas = DetailsDto.SetDefault(model);
            return View("Failed_Request_View", datas);
        }

        [HttpGet]
        public async Task<ActionResult> GetCashReplenimentRequest(string KEY)
        {
        
            var datas = await Service.GetCashReplenimentRequest(KEY);

     

            return View(datas);
        }
        [HttpGet]
        public async Task<ActionResult> GetAllCashReplenimentRequestData()
        {
            var datas = await Service.GetAllCashReplenimentRequest();
         
                       
            return View(datas);
        }

        [HttpGet]
        public async Task<ActionResult> CreateApprovalRequest()
        {

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateApprovalRequest(Approval model)

        {
         
            if (ModelState.IsValid)
            {
                var datas = await Service.CreateApprovalRequest(model);
                if (datas.MessageStatus.Equals("Failed"))
                {
                    var datasw = await Service.GetCashReplenimentRequest(model.id);
                    return View("Failed_RequestApproval_View", datasw);
                }
                else
                {
                    var datasw = await Service.GetCashReplenimentRequest(model.id); 
                    return View("Successfull_RequestApproval_View", datasw);
                }

            }
            else
            {
                var datasw = await Service.GetCashReplenimentRequest(model.id); 
                return View("Failed_RequestApproval_View", datasw);
            }




        }
    }
}