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
    public class LoanDeliquencyConfigurationController : BaseController
    {
        // GET: LoanDeliquencyConfiguration
        private readonly LoanDeliquencyConfigurationServices _LoanDeliquencyConfigurationServices;
        private readonly LoanProductServices _loanProductServices;
        public LoanDeliquencyConfigurationController(LoanDeliquencyConfigurationServices LoanDeliquencyConfigurationServices,LoanProductServices loanProductServices = null)
        {
            _LoanDeliquencyConfigurationServices = LoanDeliquencyConfigurationServices;
            _loanProductServices = loanProductServices;
        }

        public async Task<ActionResult> Index()
        {
            try
            {
                ViewBag.Key = null;
                await GetList();
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("InternalServer", "Error");
            }

        }
        [HttpPost]
        public async Task<ActionResult> Create(LoanDeliquencyConfiguration model)
        {
            if (model.Id == null)
            {
                var data = await _LoanDeliquencyConfigurationServices.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else
            {
                return await Update(model);
            }
        }
        [HttpPost]
        public async Task<ActionResult> Update(LoanDeliquencyConfiguration model)
        {
            var data = await _LoanDeliquencyConfigurationServices.Update(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {

 
            if (path == "list")
            {
                var data = await _LoanDeliquencyConfigurationServices.GetLoanDeliquencyConfigurations();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                await GetList();
                ViewBag.Key = null;
                return PartialView(partialView, new LoanDeliquencyConfiguration());
            }
            else
            {
                await GetList();
                ViewBag.Key = KEY;
                var LoanDeliquencyConfiguration = await _LoanDeliquencyConfigurationServices.GetLoanDeliquencyConfiguration(KEY);
                return PartialView(partialView, LoanDeliquencyConfiguration);

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _LoanDeliquencyConfigurationServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public async Task<bool> GetList()
        {
            var agreggates = await _loanProductServices.GetLoanProductEnumAggregates();
            ViewBag.LoanStatuses = agreggates.LoanDeliquenciesStatus;
            return true;
        }
    }

}