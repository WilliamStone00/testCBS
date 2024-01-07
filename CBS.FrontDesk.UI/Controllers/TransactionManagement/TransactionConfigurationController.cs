using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Loan.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    public class TransactionConfigurationController : BaseController
    {
        // GET: TransactionConfiguration
        // GET: Tax
        private readonly SavingProductServices _savingProductServices;
        private readonly TellerServices _tellerServices;
        private readonly DepositLimitServices _depositLimitServices;
        private readonly TransferLimitServices _transferLimitServices;
        private readonly WithdrawalLimitServices _withdrawalLimitServices;
        private readonly AccountingServices _accountingServices;
        public TransactionConfigurationController(SavingProductServices savingProductServices, AccountingServices accountingServices, TellerServices tellerServices, DepositLimitServices depositLimitServices, TransferLimitServices transferLimitServices, WithdrawalLimitServices withdrawalLimitServices)
        {
            _savingProductServices = savingProductServices;
            _accountingServices = accountingServices;
            _tellerServices = tellerServices;
            _depositLimitServices = depositLimitServices;
            _transferLimitServices = transferLimitServices;
            _withdrawalLimitServices = withdrawalLimitServices;
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
            if (serviceOption == "savingproduct")
            {
                return () => _savingProductServices.Create(model.SavingProduct);
            }
            else if (serviceOption == "depositlimit")
            {
                return () => _depositLimitServices.Create(model.DepositLimit);
            }
            else if (serviceOption == "transferlimit")
            {
                return () => _transferLimitServices.Create(model.TransferLimit);
            }
            else if (serviceOption == "withdrawallimit")
            {
                return () => _withdrawalLimitServices.Create(model.WithdrawalLimit);
            }
            else if (serviceOption == "teller")
            {
                return () => _tellerServices.Create(model.Teller);
            }
            else
            {
                return null;
            }
        }

        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(string serviceOption, SavingConfiguration model)
        {
            if (serviceOption == "savingproduct")
            {
                return () => _savingProductServices.Update(model.SavingProduct);
            }
            else if (serviceOption == "depositlimit")
            {
                return () => _depositLimitServices.Update(model.DepositLimit);
            }
            else if (serviceOption == "transferlimit")
            {
                return () => _transferLimitServices.Update(model.TransferLimit);
            }
            else if (serviceOption == "withdrawallimit")
            {
                return () => _withdrawalLimitServices.Update(model.WithdrawalLimit);
            }
            else if (serviceOption == "teller")
            {
                return () => _tellerServices.Update(model.Teller);
            }
            else
            {
                return null;
            }
        }







        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption=null)
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

        private Func<Task<PartialViewResult>> GetServiceAction(string path, string partialView, string key,string serviceOption)
        {
            if (serviceOption == "savingproduct")
            {
                if (path == "list")
                {
                    return async () =>
                    {
                        var data = await _savingProductServices.GetSavingProducts();
                        var sysData = new SavingConfiguration { SavingProducts = data.ToList() };
                        return PartialView(partialView, sysData);
                    };
                }
                else if (path == "new")
                {
                    return async () => PartialView(partialView, new SavingConfiguration {SavingProduct=new SavingProduct()});
                }
                else
                {
                    return async () => PartialView(partialView, new SavingConfiguration {SavingProduct= await _savingProductServices.GetSavingProduct(key) });
                }
            }
            else if (serviceOption == "depositlimit")
            {
                if (path == "list")
                {
                    return async () =>
                    {
                        var data = await _depositLimitServices.GetDepositLimits();
                        var sysData = new SavingConfiguration { DepositLimits = data.ToList() };
                        return PartialView(partialView, sysData);
                    };
                }
                else if (path == "new")
                {
                    return async () => PartialView(partialView, new SavingConfiguration { DepositLimit = new DepositLimit() });
                }
                else
                {
                    return async () => PartialView(partialView, new SavingConfiguration { DepositLimit = await _depositLimitServices.GetDepositLimit(key) });
                }

            }
            else if (serviceOption == "transferlimit")
            {
                if (path == "list")
                {
                    return async () =>
                    {
                        var data = await _transferLimitServices.GetTransferLimits();
                        var sysData = new SavingConfiguration { TransferLimits = data.ToList() };
                        return PartialView(partialView, sysData);
                    };
                }
                else if (path == "new")
                {
                    return async () => PartialView(partialView, new SavingConfiguration { TransferLimit = new TransferLimit() });
                }
                else
                {
                    return async () => PartialView(partialView, new SavingConfiguration { TransferLimit = await _transferLimitServices.GetTransferLimit(key) });
                }

            }
            else if (serviceOption == "withdrawallimit")
            {
                if (path == "list")
                {
                    return async () =>
                    {
                        var data = await _withdrawalLimitServices.GetWithdrawalLimits();
                        var sysData = new SavingConfiguration { WithdrawalLimits = data.ToList() };
                        return PartialView(partialView, sysData);
                    };
                }
                else if (path == "new")
                {
                    return async () => PartialView(partialView, new SavingConfiguration { WithdrawalLimit = new WithdrawalLimit() });
                }
                else
                {
                    return async () => PartialView(partialView, new SavingConfiguration { WithdrawalLimit = await _withdrawalLimitServices.GetWithdrawalLimit(key) });
                }

            }
            else if (serviceOption == "teller")
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
            return null;
        }

        //public async Task<ActionResult> Delete(string KEY)
        //{
        //    var data = await _TaxServices.Delete(KEY);
        //    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        //}
        public async Task<bool> GetList()
        {   var conf = await _savingProductServices.GetSavingConfigurationAggregates();
            ViewBag.Products = await _savingProductServices.GetSavingProducts();
            ViewBag.DepositLimitTypes = conf.depositTypes.ToList();
            ViewBag.TransferLimitTypes = conf.transactionTypes.ToList();
            ViewBag.WithdrawalLimitTypes = conf.withdrawalTypes.ToList();
            return true;
        }
    }
}