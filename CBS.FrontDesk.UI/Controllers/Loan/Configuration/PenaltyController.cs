using CBS.BusinessService.Accounting;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Configuration
{
    //[CheckSessionTimeOutAttribute]

    public class PenaltyController : BaseController
    {
        // GET: Penalty
        private readonly PenaltyServices _PenaltyServices;
        private readonly LoanProductServices _loanProductServices;
        public PenaltyController(PenaltyServices PenaltyServices, LoanProductServices accountingServices)
        {
            _PenaltyServices = PenaltyServices;
            _loanProductServices = accountingServices;
        }
        public async Task<ActionResult> Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> AddOrEdit(Penalty model)
        {

            if (ModelState.IsValid)
            {
                if (model.Id==null)
                {
                    var data = await _PenaltyServices.Create(model);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

                }
                var execution = await _PenaltyServices.Update(model);
                return Json(new { success = execution.Result, status = execution.MessageStatus, message = Messaging.MessageResult(execution) });

            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "list")
            {
                var data = await _PenaltyServices.GetPenaltys();
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                await GetValues();
                return PartialView(partialView, new Penalty());
            }
            else
            {
                await GetValues();
                var Penalty = await _PenaltyServices.GetPenalty(KEY);
                return PartialView(partialView, Penalty);
            }
        }
        [HttpPost]
        public async Task<JsonResult> DataTable()
        {
            var penalties = await _PenaltyServices.GetPenaltys();

            // If paging/sorting/filtering were implemented, apply here
            var data = penalties.Select(p => new
            {
                Id = p.Id,
                PenaltyName = p.PenaltyName,
                PenaltyType = p.PenaltyType,
                PenaltyValue = p.PenaltyValue,
                IsRate = p.IsRate,
                CalculatePenaltyOn = p.CalculatePenaltyOn,
                IsEnabled = p.IsEnabled
            }).ToList();

            return Json(new
            {
                data = data,
                recordsTotal = data.Count,
                recordsFiltered = data.Count
            });
        }

        public async Task<bool> GetValues()
        {
            var productEnumAgregates = await _loanProductServices.GetLoanProductEnumAggregates();
            ViewBag.CalculateInterestOn = productEnumAgregates.CalculateInterestOn;
            ViewBag.PenaltyTypes = _loanProductServices.GetPenaltyTypes().ToList(); 
            return true;
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _PenaltyServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}