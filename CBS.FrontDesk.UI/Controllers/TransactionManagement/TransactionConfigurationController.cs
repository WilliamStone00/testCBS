using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
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
    [CheckSessionTimeOutAttribute]

    public class TransactionConfigurationController : BaseController
    {
        // GET: TransactionConfiguration
        // GET: Tax
        private readonly SavingProductServices _savingProductServices;
        private readonly TellerServices _tellerServices;
        private readonly DepositLimitServices _depositLimitServices;
        private readonly TransferLimitServices _transferLimitServices;
        private readonly WithdrawalLimitServices _withdrawalLimitServices;
        private readonly ChartOfAccountServicesAnnex _accountingServices;
        private readonly ManagementFeeParameterServices _managementFeeParameterServices;
        private readonly ReopenFeeParameterServices _reopenFeeParameterServices;
        private readonly CloseFeeParameterServices _closeFeeParameterServices;
        private readonly EntryFeeParameterServices _entryFeeParameterServices;
        private readonly OperationFeeServices _operationFeeServices;
        private readonly SavingProductFeeServices _savingProductFeeServices;
        public TransactionConfigurationController(SavingProductServices savingProductServices, ChartOfAccountServicesAnnex accountingServices, TellerServices tellerServices, DepositLimitServices depositLimitServices, TransferLimitServices transferLimitServices, WithdrawalLimitServices withdrawalLimitServices, ManagementFeeParameterServices managementFeeParameterServices = null, ReopenFeeParameterServices reopenFeeParameterServices = null, CloseFeeParameterServices closeFeeParameterServices = null, EntryFeeParameterServices entryFeeParameterServices = null, OperationFeeServices operationFeeServices = null, SavingProductFeeServices savingProductFeeServices = null)
        {
            _savingProductServices = savingProductServices;
            _accountingServices = accountingServices;
            _tellerServices = tellerServices;
            _depositLimitServices = depositLimitServices;
            _transferLimitServices = transferLimitServices;
            _withdrawalLimitServices = withdrawalLimitServices;
            _managementFeeParameterServices = managementFeeParameterServices;
            _reopenFeeParameterServices = reopenFeeParameterServices;
            _closeFeeParameterServices = closeFeeParameterServices;
            _entryFeeParameterServices = entryFeeParameterServices;
            _operationFeeServices = operationFeeServices;
            _savingProductFeeServices = savingProductFeeServices;
        }

        public async Task<ActionResult> Index()
        {
            var savingProduct = await _savingProductServices.GetSavingProducts();
            return View(new SavingConfiguration { SavingProducts = savingProduct.ToList() });
        }
        public async Task<ActionResult> OrdinaryAccounts()
        {
            var savingProduct = await _savingProductServices.GetSavingProducts();
            return View(new SavingConfiguration { SavingProducts = savingProduct.ToList() });
        }
        //
        public async Task<ActionResult> PolicyandSharing(string Key, string serviceOption = null)
        {
            await GetListPolicies();
            var savingProduct = await _savingProductServices.GetSavingProduct(Key);
            var deposit = new DepositLimit { ProductId = savingProduct.Id };
            var transfer = new TransferLimit { productId = savingProduct.Id };
            var withdrawal = new WithdrawalLimit { ProductId = savingProduct.Id };
            return View(new SavingConfiguration { SavingProduct = savingProduct, DepositLimit = deposit, TransferLimit = transfer, WithdrawalLimit = withdrawal });
        }
        public async Task<ActionResult> AccountMapping(string Key, string serviceOption = null)
        {
            await GetChartOfAccounts();
            var savingProduct = await _savingProductServices.GetSavingProduct(Key);
            ViewBag.Fees = await _operationFeeServices.GetFees();
            await GetEventNames();
            ViewBag.FeeType = SavingProductFee.GetFeeTypeList();
            var savingProductFees = await _savingProductFeeServices.GetSavingProductFees(Key);
            return View(new SavingConfiguration { SavingProduct = savingProduct, SavingProductFee = new SavingProductFee { SavingProductId = savingProduct.Id }, SavingProductFees= savingProductFees.ToList() });
        }
        //
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
            else if (serviceOption == "CloseFeeParameter")
            {
                return () => _closeFeeParameterServices.Create(model.CloseFeeParameter);
            }
            else if (serviceOption == "EntryFeeParameter")
            {
                return () => _entryFeeParameterServices.Create(model.EntryFeeParameter);
            }
            else if (serviceOption == "ManagementFeeParameter")
            {
                return () => _managementFeeParameterServices.Create(model.ManagementFeeParameter);
            }
            else if (serviceOption == "ReopenFeeParameter")
            {
                return () => _reopenFeeParameterServices.Create(model.ReopenFeeParameter);
            }
            else if (serviceOption == "AccountMapping")
            {
                return () => _savingProductServices.UpdateProductAccountMapping(model.SavingProduct);
            }
            //accountmapping
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
            else if (serviceOption == "accountmapping")
            {
                return () => _savingProductServices.UpdateProductAccountMapping(model.SavingProduct);
            }
            else if (serviceOption == "mapformeventcharges")
            {
                return () => _savingProductServices.UpdateProductEventMapping(model.SavingProduct);
            }
            //
            else if (serviceOption == "transferlimit")
            {
                return () => _transferLimitServices.Update(model.TransferLimit);
            }
            else if (serviceOption == "withdrawallimit")
            {
                return () => _withdrawalLimitServices.Update(model.WithdrawalLimit);
            }
            else if (serviceOption == "CloseFeeParameter")
            {
                return () => _closeFeeParameterServices.Update(model.CloseFeeParameter);
            }
            else if (serviceOption == "EntryFeeParameter")
            {
                return () => _entryFeeParameterServices.Update(model.EntryFeeParameter);
            }
            else if (serviceOption == "ManagementFeeParameter")
            {
                return () => _managementFeeParameterServices.Update(model.ManagementFeeParameter);
            }
            else if (serviceOption == "ReopenFeeParameter")
            {
                return () => _reopenFeeParameterServices.Update(model.ReopenFeeParameter);
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


        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)

        {

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
            if (serviceOption == "savingproduct")
            {
                if (path == "list")
                {
                    //
                    return async () =>
                    {
                        var data = await _savingProductServices.GetSavingProducts();
                        var sysData = new SavingConfiguration { SavingProducts = data.ToList() };
                        return PartialView(partialView, sysData);
                    };
                }
                else if (path == "feemapping")
                {

                    return async () =>
                    {
                        await GetEventNames();
                        var savingProductFees = await _savingProductFeeServices.GetSavingProductFees(key);
                        return PartialView(partialView, new SavingConfiguration { SavingProductFees = savingProductFees.ToList() });

                    };
                }
                else if (path == "new")
                {

                    return async () =>
                    {
                        var conf = await _savingProductServices.GetSavingConfigurationAggregates();
                        ViewBag.Frequences = conf.freeQuencies.ToList();
                        ViewBag.Currencies = conf.currencies.ToList();
                        return PartialView(partialView, new SavingConfiguration { SavingProduct = new SavingProduct() });

                    };
                }
                else
                {
                    return async () =>
                    {
                        var conf = await _savingProductServices.GetSavingConfigurationAggregates();
                        ViewBag.Frequences = conf.freeQuencies.ToList();
                        ViewBag.Currencies = conf.currencies.ToList();
                        return PartialView(partialView, new SavingConfiguration { SavingProduct = await _savingProductServices.GetSavingProduct(key) });

                    };
                }
            }
            else if (serviceOption == "depositlimit")
            {
                if (path == "list")
                {
                    if (key != null)
                    {
                        return async () =>
                        {
                            var data = await _depositLimitServices.GetDepositLimits();
                            var datas = data.Where(x => x.ProductId == key).ToList();
                            var sysData = new SavingConfiguration { DepositLimits = datas };
                            return PartialView(partialView, sysData);
                        };
                    }
                    return async () =>
                    {
                        var data = await _depositLimitServices.GetDepositLimits();
                        var sysData = new SavingConfiguration { DepositLimits = data.ToList() };
                        return PartialView(partialView, sysData);
                    };
                }
                else if (path == "new")
                {
                    return async () =>
                    {
                        await GetList();
                        var savingProduct = await _savingProductServices.GetSavingProduct(key);
                        return PartialView(partialView, new SavingConfiguration { DepositLimit = new DepositLimit { ProductId = key }, SavingProduct = savingProduct });
                    };
                }
                else
                {
                    return async () =>
                    {
                        await GetList();
                        var data = await _depositLimitServices.GetDepositLimit(key);
                        var savingProduct = await _savingProductServices.GetSavingProduct(data.ProductId);
                        return PartialView(partialView, new SavingConfiguration { DepositLimit = data, SavingProduct = savingProduct });
                    };
                }

            }
            else if (serviceOption == "transferlimit")
            {
                if (path == "list")
                {
                    if (key != null)
                    {
                        return async () =>
                        {
                            var data = await _transferLimitServices.GetTransferLimits();
                            var sysData = new SavingConfiguration { TransferLimits = data.Where(x => x.productId == key).ToList() };
                            return PartialView(partialView, sysData);
                        };
                    }
                    return async () =>
                    {
                        var data = await _transferLimitServices.GetTransferLimits();
                        var sysData = new SavingConfiguration { TransferLimits = data.ToList() };
                        return PartialView(partialView, sysData);
                    };
                }
                else if (path == "new")
                {
                    return async () =>
                    {
                        await GetList();
                        var savingProduct = await _savingProductServices.GetSavingProduct(key);
                        return PartialView(partialView, new SavingConfiguration { TransferLimit = new TransferLimit { productId = key }, SavingProduct = savingProduct });
                    };
                }
                else
                {
                    return async () =>
                    {
                        await GetList();
                        var data = await _transferLimitServices.GetTransferLimit(key);
                        var savingProduct = await _savingProductServices.GetSavingProduct(data.productId);
                        return PartialView(partialView, new SavingConfiguration { TransferLimit = data, SavingProduct = savingProduct });
                    };
                }

            }
            else if (serviceOption == "withdrawallimit")
            {
                if (path == "list")
                {
                    if (key != null)
                    {
                        return async () =>
                        {
                            var data = await _withdrawalLimitServices.GetWithdrawalLimits();
                            var withdrawals = data.Where(x => x.ProductId == key).ToList();

                            //090491483100383
                            //090491483100383
                            var sysData = new SavingConfiguration { WithdrawalLimits = withdrawals };
                            return PartialView(partialView, sysData);
                        };
                    }
                    return async () =>
                    {
                        var data = await _withdrawalLimitServices.GetWithdrawalLimits();
                        var sysData = new SavingConfiguration { WithdrawalLimits = data.ToList() };
                        return PartialView(partialView, sysData);
                    };
                }
                else if (path == "new")
                {
                    return async () =>
                    {
                        var conf = await _savingProductServices.GetSavingConfigurationAggregates();
                        ViewBag.Frequences = conf.freeQuencies.ToList();
                        ViewBag.Currencies = conf.currencies.ToList();
                        ViewBag.WithdrawalLimitTypes = conf.withdrawalTypes.ToList();
                        var savingProduct = await _savingProductServices.GetSavingProduct(key);
                        return PartialView(partialView, new SavingConfiguration { WithdrawalLimit = new WithdrawalLimit { ProductId = key }, SavingProduct=savingProduct });
                    };
                }
                else
                {
                    return async () =>
                    {
                        var conf = await _savingProductServices.GetSavingConfigurationAggregates();
                        ViewBag.Frequences = conf.freeQuencies.ToList();
                        ViewBag.Currencies = conf.currencies.ToList();
                        ViewBag.WithdrawalLimitTypes = conf.withdrawalTypes.ToList();
                        var data = await _withdrawalLimitServices.GetWithdrawalLimit(key);
                        var savingProduct = await _savingProductServices.GetSavingProduct(data.ProductId);
                        return PartialView(partialView, new SavingConfiguration { WithdrawalLimit = data, SavingProduct = savingProduct });
                    };
                }

            }
      
            return null;
        }

        private async Task GetEventNames()
        {
            ViewBag.EventCodes = await _accountingServices.GetEventNames("INCOME");

        }

        public async Task<bool> GetList()
        {
            var conf = await _savingProductServices.GetSavingConfigurationAggregates();
            ViewBag.Products = await _savingProductServices.GetSavingProducts();
            var chartOfAccounts = await _accountingServices.GetChartOfAccounts();
            ViewBag.DepositLimitTypes = conf.depositTypes.ToList();
            ViewBag.TransferLimitTypes = conf.transferTypes.ToList();
            ViewBag.WithdrawalLimitTypes = conf.withdrawalTypes.ToList();
            ViewBag.Frequences = conf.freeQuencies.ToList();
            ViewBag.chartOfAccounts = chartOfAccounts.ToList();
            ViewBag.Currencies = conf.currencies.ToList();
            ViewBag.OperationEventAttributes = await _accountingServices.GetEventAttributeByOperationTypeID();
            ViewBag.operationAccounts = conf.operationAccounts.ToList();
            return true;
        }
        public async Task<bool> GetChartOfAccounts()
        {
            var chartOfAccounts = await _accountingServices.GetChartOfAccounts();
            ViewBag.chartOfAccounts = chartOfAccounts.ToList();
            return true;
        }
        public async Task<bool> GetListPolicies()
        {
            var conf = await _savingProductServices.GetSavingConfigurationAggregates();
            ViewBag.DepositLimitTypes = conf.depositTypes.ToList();
            ViewBag.TransferLimitTypes = conf.transferTypes.ToList();
            ViewBag.WithdrawalLimitTypes = conf.withdrawalTypes.ToList();
            ViewBag.Frequences = conf.freeQuencies.ToList();
            return true;
        }
        public async Task<ActionResult> Ajaxloader(string Key)
        {
            var listing = await _accountingServices.GetEventAttributeByOperationTypeID(Key);
            return Json(listing, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> Delete(string Key)
        {
            var data = await _savingProductServices.Delete(Key);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}