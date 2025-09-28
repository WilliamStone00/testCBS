using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.CashDeskOperations
{
    [CheckSessionTimeOutAttribute]
    public class OtherTransactionController : BaseController
    {
        // GET: OtherTransaction
        private readonly OtherTransactionServices _OtherTransactionServices;
        private readonly AccountServices _accountingServices;
        private readonly BranchServices _branchServices;

        public OtherTransactionController(AccountServices accountingServices, OtherTransactionServices OtherTransactionServices, BranchServices branchServices = null)
        {
            _accountingServices = accountingServices;
            _OtherTransactionServices = OtherTransactionServices;
            _branchServices = branchServices;
        }

        public async Task<ActionResult> Index()
        {
            await GetList();
            return View(new CashDesk());
        }

        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(CashDesk model)
        {
            Func<Task<ExecutionMessages>> serviceAction = null;

            if (model.Action == "insert")
            {
                serviceAction = GetInsertServiceAction(model);
            }
            //else
            //{
            //    serviceAction = GetUpdateServiceAction(model.ServiceOption, model);
            //}

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

      

        private Func<Task<ExecutionMessages>> GetInsertServiceAction(CashDesk model)
        {
            return null;

            //return () => _OtherTransactionServices.Create(model.OtherTransaction);
        }

        //private Func<Task<ExecutionMessages>> GetUpdateServiceAction(string serviceOption, CashDesk model)
        //{
        //    return () => _OtherTransactionServices.Update(model.OtherTransaction);
        //}

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
                    var data = await _OtherTransactionServices.GetOtherTransactions();
                    var sysData = new CashDesk { OtherTransactions = data.ToList() };
                    return PartialView(partialView, sysData);

                };
            }
            else if (path == "new")
            {
                return async () => PartialView(partialView, new CashDesk { OtherTransaction = new OtherTransaction() });
            }
            else
            {
                return async () => PartialView(partialView, new CashDesk { OtherTransaction = await _OtherTransactionServices.GetOtherTransaction(key) });
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
            var data = await _OtherTransactionServices.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

    }

}