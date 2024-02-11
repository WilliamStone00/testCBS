using CBS.BusinessService.Accounting;
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
    public class TellerManagementController : BaseController
    {
        // GET: TellerManagement
        private readonly TellerServices _tellerServices;
        private readonly ChartOfAccountServicesAnnex _accountingServices;
        public TellerManagementController(ChartOfAccountServicesAnnex accountingServices, TellerServices tellerServices)
        {
            _accountingServices = accountingServices;
            _tellerServices = tellerServices;
           
        }

        public async Task<ActionResult> Index()
        {
            await GetList();
            return View(new SavingConfiguration());
        }

        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(SavingConfiguration model)
        {
            Func<Task<ExecutionMessages>> serviceAction = null;

            if (model.Action == "insert")
            {
                serviceAction = GetInsertServiceAction(model.ServiceOption, model);
            }
            else
            {
                serviceAction = GetUpdateServiceAction(model.ServiceOption, model);
            }

            if (serviceAction != null)
            {
                try
                {
                    var data = await serviceAction();
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
                }
            }

            return Json(new { success = false, status = false, message = "Invalid option selected." });
        }

        private Func<Task<ExecutionMessages>> GetInsertServiceAction(string serviceOption, SavingConfiguration model)
        {
          
            return () => _tellerServices.Create(model.Teller);
        }

        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(string serviceOption, SavingConfiguration model)
        {
            return () => _tellerServices.Update(model.Teller);
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            await GetList();
            Func<Task<PartialViewResult>> serviceAction = GetServiceAction(path, partialView, KEY, serviceOption);

            if (serviceAction != null)
            {
                var partialResult = await serviceAction();

                if (partialResult != null)
                {
                    return partialResult;
                }
            }

            return HttpNotFound(); // Or return a default view for handling unknown paths
        }

        private Func<Task<PartialViewResult>> GetServiceAction(string path, string partialView, string key, string serviceOption)
        {
            if (path == "list")
            {
                return async () =>
                {
                    var data = await _tellerServices.GetTellers();
                    var sysData = new SavingConfiguration { Tellers = data.ToList() };
                    return PartialView(partialView, sysData);

                };
            }
            else if (path == "new")
            {
                return async () => PartialView(partialView, new SavingConfiguration { Teller = new Teller() });
            }
            else
            {
                return async () => PartialView(partialView, new SavingConfiguration { Teller = await _tellerServices.GetTeller(key) });
            }
        }

        public async Task<bool> GetList()
        {
            var chartOfAccounts = await _accountingServices.GetChartOfAccounts();
            ViewBag.chartOfAccounts = chartOfAccounts.ToList();
            ViewBag.OperationEventAttributes = await _accountingServices.GetEventAttributeByOperationTypeID();
            return true;
        }
        public async Task<ActionResult> Ajaxloader(string Key)
        {
            var listing = await _accountingServices.GetEventAttributeByOperationTypeID(Key);
            return Json(listing, JsonRequestBehavior.AllowGet);
        }
    }
}