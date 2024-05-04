using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Configuration.Organ
{
    [CheckSessionTimeOutAttribute]

    public class BranchManagementController : BaseController
    {
        // GET: BranchManagement
        private readonly BankServices _bankServices;
        private readonly BranchServices _branchServices;
        public BranchManagementController(BranchServices branchServices, BankServices bankServices)
        {
            _branchServices = branchServices;
            _bankServices = bankServices;

        }

        public async Task<ActionResult> Index()
        {
            //await GetList();
            return View(new BankConfig());
        }

        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(BankConfig model)
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

        private Func<Task<ExecutionMessages>> GetInsertServiceAction(string serviceOption, BankConfig model)
        {
            if (serviceOption == "bank")
            {
                return () => _bankServices.Create(model.Bank);
            }
            else 
            {
                return () => _branchServices.Create(model.Branch);
            }
           
        }
        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(string serviceOption, BankConfig model)
        {
            if (serviceOption == "bank")
            {
                return () => _bankServices.Update(model.Bank);
            }
            else if (serviceOption == "update_bank_logo")
            {
                model.CustomerDocumentRequest.CustomerID = model.Bank.Id;
                model.CustomerDocumentRequest.ServiceTypeType = "Bank_Logo";
                return () => _bankServices.UploadBankLogo(model.CustomerDocumentRequest);

            }
            else if (serviceOption == "branch")
            {
                return () => _branchServices.Update(model.Branch);
            }
            else if (serviceOption == "update_branch_logo")
            {
                model.CustomerDocumentRequest.CustomerID = model.Branch.Id;
                model.CustomerDocumentRequest.ServiceTypeType = "Branch_Logo";
                return () => _branchServices.UploadBranchLogo(model.CustomerDocumentRequest);

            }
            else
            {
                return null;
            }
        }



        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            //await GetList();
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
            if (serviceOption == "bank")
            {
                if (path == "list")
                {
                    return async () =>
                    {
                        var data = await _bankServices.GetBanks();
                        var sysData = new BankConfig { Banks = data.ToList() };
                        return PartialView(partialView, sysData);

                    };
                }
                else if (path == "new")
                {
                    return async () => PartialView(partialView, new BankConfig { Bank = new Bank() });
                }
                else
                {
                    return async () => PartialView(partialView, new BankConfig { Bank = await _bankServices.GetBank(key) });
                }

            }
            else if (serviceOption == "branch")
            {
                if (path == "list")
                {
                    return async () =>
                    {
                        var data = await _branchServices.GetBranches();
                        var sysData = new BankConfig { Branches = data.ToList() };
                        return PartialView(partialView, sysData);

                    };
                }
                else if (path == "new")
                {
                    return async () => PartialView(partialView, new BankConfig { Branch = new Branch { BankId = Session["BankID"].ToString() } });
                }
                else
                {
                    return async () => PartialView(partialView, new BankConfig { Branch = await _branchServices.GetBranch(key) });
                }

            }
            return null;
        }

        public async Task<bool> GetList()
        {
            var banks = await _bankServices.GetBanks();
            ViewBag.Banks = banks.ToList();
            return true;
        }
        public async Task<ActionResult> Ajaxloader(string Key)
        {
            var listing = await _branchServices.GetBranchByBankID(Key);
            return Json(listing, JsonRequestBehavior.AllowGet);
        }
    }
}