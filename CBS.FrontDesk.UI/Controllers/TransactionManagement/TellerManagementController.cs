using CBS.BusinessService.Accounting;
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
using ZXing;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    [CheckSessionTimeOutAttribute]

    public class TellerManagementController : BaseController
    {
        // GET: TellerManagement
        private readonly TellerServices _tellerServices;
        private readonly ChartOfAccountServicesAnnex _accountingServices;
        private readonly BranchServices _branchServices;
        private readonly AccountServices _accountServices;
        
        private readonly IUserManagementServices _userManagementServices;
        public TellerManagementController(ChartOfAccountServicesAnnex accountingServices, TellerServices tellerServices, BranchServices branchServices = null, IUserManagementServices userManagementServices = null, AccountServices accountServices = null)
        {
            _accountingServices = accountingServices;
            _tellerServices = tellerServices;
            _branchServices = branchServices;
            _userManagementServices = userManagementServices;
            _accountServices=accountServices;
        }

        public async Task<ActionResult> Index()
        {
            await GetList();
            return View(new SavingConfiguration());
        }
        public async Task<ActionResult> MMConfiguration(string key)
        {
            var teller = await _tellerServices.GetTeller(key);
            var mobileMoneyTellerConfiguration = _tellerServices.MapTellerToMobileMoneyCommand(teller);
            ViewBag.EventCodes = await _accountingServices.GetEventNames("FEE");
            ViewBag.ChartOfAccounts = await _accountingServices.GetChartOfAccounts();
            ViewBag.Users = await _userManagementServices.GetUserDropDownList();
            var data = new SavingConfiguration { Teller = teller, MobileMoneyTellerConfiguration = mobileMoneyTellerConfiguration };
            return View(data);
        }
        [HttpPost]
        public async Task<JsonResult> ToggleRemoteClose(ToggleRemoteCloseDto request)
        {
            if (string.IsNullOrWhiteSpace(request.TellerId))
            {
                return Json(new
                {
                    success = false,
                    message = "❌ Teller ID is required."
                });
            }

            try
            {
               
                // 🔁 Call update method with toggle mode
                var result = await _tellerServices.Update(null, request, true);

                return Json(new
                {
                    success = result.Result,
                    status = result.MessageStatus,
                    message = Messaging.MessageResult(result)
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"❌ An unexpected error occurred while toggling remote close: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<JsonResult> LinkCollectorTransit(LinkCollectorTransitCommand request)
        {
            if (string.IsNullOrWhiteSpace(request.TellerId))
            {
                return Json(new
                {
                    success = false,
                    message = "❌ Teller ID is required."
                });
            }

            try
            {

                // 🔁 Call update method with toggle mode
                var result = await _tellerServices.Update(request);

                return Json(new
                {
                    success = result.Result,
                    status = result.MessageStatus,
                    message = Messaging.MessageResult(result)
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"❌ An unexpected error occurred while toggling remote close: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<JsonResult> InitializeAccount(InitializeAccountCommand command)
        {
            try
            {
              var  result= await _accountServices.InitialiseBalances(command);

                return Json(new
                {
                    success = result.Result,
                    status = result.MessageStatus,
                    message = Messaging.MessageResult(result)
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"❌ An unexpected error occurred while initialising balance in operation: {ex.Message}"
                });
            }
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
                if (model.Action == "update")
                {
                    serviceAction = GetUpdateServiceAction(model.ServiceOption, model);

                }
                else
                {
                    serviceAction = UpdateMobileMoneyCOnfigurations(model.ServiceOption, model);
                }
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
            return () => _tellerServices.Update(model.Teller,null,false);
        }

        private Func<Task<ExecutionMessages>> UpdateMobileMoneyCOnfigurations(string serviceOption, SavingConfiguration model)
        {
            return () => _tellerServices.UpdateMobileMoneyConfiguration(model.MobileMoneyTellerConfiguration,model.Action);
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
            //var chartOfAccounts = await _accountingServices.GetChartOfAccounts();
            //ViewBag.chartOfAccounts = chartOfAccounts.ToList();
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;

            //ViewBag.OperationEventAttributes = await _accountingServices.GetEventAttributeByOperationTypeID();
            return true;
        }
       
        public async Task<ActionResult> Delete(string id)
        {
            var data = await _tellerServices.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        
    }
}