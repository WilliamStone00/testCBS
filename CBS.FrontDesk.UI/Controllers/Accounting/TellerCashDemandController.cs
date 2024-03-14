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
using System.Web.UI.WebControls;

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
            List<SelectListItem> list = new List<SelectListItem>();
            ViewBag.OpeningOfDayId = await Service.GetCashReplenimentRequestId();
            if (ViewBag.OpeningOfDayId.Count == 0)
            {
                list.Add(new SelectListItem { Text = "Id001", Value = "Current Opening Reference" });
                ViewBag.OpeningOfDayId = list;
            }
            ViewBag.Decisions = BuildMenuViewBag();
        }
        private dynamic BuildMenuViewBag()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            list.Add(new SelectListItem { Text = $"Approve", Value = "Approve" });

            list.Add(new SelectListItem { Text = $"Rejected", Value = "Rejected" });


            return list;
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
                    var datas = DetailsDto.SetDefault(model);
                    return View("Failed_Request_View", datas);
                }
                else
                {
              
                    return View("Successfull_Request_View", result.Data);
                }

            }

            var dataccs = DetailsDto.SetDefault(model);
            return View("Failed_Request_View", dataccs);
        }

        [HttpGet]
        public async Task<ActionResult> GetCashReplenimentRequest(string KEY)
        {
            await GetList();
  
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