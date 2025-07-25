using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.DailyCollectionServices;
using CBS.BusinessService.LocalizationService;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Config;
 
using CBS.FrontDesk.Data.Message;
using DocumentFormat.OpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.DailyCollectorManagement
{
    public class DailyCollectionSettingController : BaseController
    {
        private BranchServices _branchService;
        private LocalizationService _localizationService;
        private CommissionSettingServices _commissionSettingServices;
        private AgentDailyCashLimitServices _agentDailyCashLimitServices;
        private UserManagementServices _userManagementServices;
        private ZoneServices _zoneServices;
        public DailyCollectionSettingController()
        {
            _branchService = new BranchServices();
            _localizationService = new LocalizationService();
            _commissionSettingServices = new CommissionSettingServices();
            _agentDailyCashLimitServices = new AgentDailyCashLimitServices();
            _zoneServices = new ZoneServices();
            _userManagementServices = new UserManagementServices();
        }
        // GET: DailyCollectionSetting/Index

        public async Task<ActionResult> Index()
        {
            await GetList();
            return View(new DailyCollectionConfiguration());
        }
        public async Task<ActionResult> CollectionZonification()
        {
            await GetList();
            return View(new DailyCollectionConfiguration());
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            await GetList();
            var partialResult = await GetServiceAction(path, partialView, KEY, serviceOption);

            return partialResult;
        }
        public async Task<ActionResult> DailySaverAccountUpload()
        {
            ViewBag.Branches = BuildMenuISViewBag((await _branchService.GetBranches()).ToList());
            ViewBag.BankName = _branchService.GetBankName();
            return View(new AccountingConfiguration { BranchId = _branchService.GetBranchID() });
        }
        private dynamic BuildMenuISViewBag(List<Branch> listOfItems)
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();
            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "", Value = $"Select Branch" });
            foreach (var item in listOfItems)
            {

                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value = $" {item.Name}" });


            }
            return selectListItems;

        }
        private async Task GetList()
        {
            var BranList = (await _branchService.GetBranches()).ToList();
            ViewBag.Branches = BuildBranch(BranList);
            ViewBag.OperationTypes = GetOperationTypes();
            ViewBag.ProviderTypes = GetProviderTypes();

        }
        [HttpGet]
        public async Task<JsonResult> GetAgentsByBranchId(string branchId)
        {
            try
            {
                if (string.IsNullOrEmpty(branchId))
                {
                    return Json(new List<SelectListItem>(), JsonRequestBehavior.AllowGet);
                }

                // Get all user roles and filter by branch and teller status
                var agents = (await _userManagementServices.GetUSerRoles())
                    .Where(u => u.branchId == branchId && u.RoleName == "Daily_Collector_Agent") // Filter by branch and tellers
                    .Select(a => new SelectListItem
                    {
                        Value = a.UserId.ToString(), // Using UserId as value
                        Text = $"{a.FirstName} {a.LastName} ({a.RoleName})" // Format: "John Doe (Teller)"
                    })
                    .OrderBy(a => a.Text) // Optional: sort alphabetically
                    .ToList();

                return Json(agents, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log error (consider using ILogger)
                // _logger.LogError(ex, "Error loading agents for branch {BranchId}", branchId);

                return Json(new { error = "Failed to load agents. Please try again." },
                          JsonRequestBehavior.AllowGet);
            }
        }
        public static List<SelectListItem> GetOperationTypes()
        {
            return new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Select Operation Type" },
            new SelectListItem { Value = "SUB", Text = "Subscription (SUB)" },
            new SelectListItem { Value = "DEP", Text = "Deposit (DEP)" },
            new SelectListItem { Value = "WDR", Text = "Withdrawal (WDR)" },
            new SelectListItem { Value = "LPR", Text = "Loan Repayment (LPR)" }
        };
        }

        public static List<SelectListItem> GetProviderTypes()
        {
            return new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Select Provider Type" },
            new SelectListItem { Value = "DestinationBranch", Text = "Destination Branch" },
            new SelectListItem { Value = "League", Text = "League" },
            new SelectListItem { Value = "AgentBranch", Text = "Agent Branch" },
            new SelectListItem { Value = "HeadOffice", Text = "Head Office" }
        };
        }

        private dynamic BuildBranch(List<Branch> listOfItems)
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();
            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "", Value = $"Select BranchCode" });
            foreach (var item in listOfItems)
            {
                if (!item.BranchCode.Equals("000"))
                {
                    selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value = $"{item.Name}" });
                }

            }
            return selectListItems;

        }
        private async Task<ActionResult> GetServiceAction(string path, string partialView, string KEY, string serviceOption)
        {
            if (serviceOption == "zone")
            {
                if (path == "list")
                {
                    var data = await _zoneServices.GetZones();

                    var sysData = new DailyCollectionConfiguration { Zones = data.ToList() };
                    return PartialView(partialView, sysData);

                }
                else if (path == "new")
                {


                    return PartialView(partialView, new DailyCollectionConfiguration { Zone = new Data.Entity.DailyCollectionData.Zone() });

                }
                else
                {
                    var data = await _zoneServices.GetZoneById(KEY);
                    return PartialView(partialView, new DailyCollectionConfiguration { Zone = data });

                }


            }
            else if (serviceOption == "commissionSetting")
            {
                if (path == "list")
                {
                    var data = await _commissionSettingServices.GetCommissionSettings();

                    var sysData = new DailyCollectionConfiguration { CommissionSettings = data.ToList() };
                    return PartialView(partialView, sysData);

                }

                else if (path == "new")
                {


                    return PartialView(partialView, new DailyCollectionConfiguration { CommissionSetting = new Data.Entity.DailyCollectionData.CommissionSetting() });

                }
                else
                {
                    var data = await _commissionSettingServices.GetCommissionSettingById(KEY);
                    return PartialView(partialView, new DailyCollectionConfiguration { CommissionSetting = data });

                }
            }
            else if (serviceOption == "agentDailyCashLimit")
            {
                if (path == "list")
                {
                    var data = await _agentDailyCashLimitServices.GetAgentDailyCashLimits();

                    var sysData = new DailyCollectionConfiguration { AgentDailyCashLimits = data.ToList() };
                    return PartialView(partialView, sysData);

                }

                else if (path == "new")
                {


                    return PartialView(partialView, new DailyCollectionConfiguration { AgentDailyCashLimit = new Data.Entity.DailyCollectionData.AgentDailyCashLimit() });

                }
                else
                {
                    var data = await _agentDailyCashLimitServices.GetAgentDailyCashLimitById(KEY);
                    return PartialView(partialView, new DailyCollectionConfiguration { AgentDailyCashLimit = data });

                }
            }
            else
            {
                return PartialView(partialView, new DailyCollectionConfiguration());
            }
        }


        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(DailyCollectionConfiguration model)
        {
            Func<Task<ExecutionMessages>> serviceAction = null;

            if (model.ServiceOption == "zone")
            {
                //return () => _AccountServices.Create(model.Account);
                if (model.Action == "insert")
                {
                    serviceAction = await GetInsertServiceActionAsync(model.ServiceOption, model);

                }
                else
                {
                    serviceAction = await GetUpdateServiceActionAsync(model.ServiceOption, model);
                }


            }
            else if (model.ServiceOption == "commissionSetting")
            {
                if (model.Action == "insert")
                {
                    serviceAction = await GetInsertServiceActionAsync(model.ServiceOption, model);
                }
                else
                {

                    serviceAction = await GetUpdateServiceActionAsync(model.ServiceOption, model);
                }
            }
            else if (model.ServiceOption == "agentDailyCashLimit")
            {
                if (model.Action == "insert")
                {
                    serviceAction = await GetInsertServiceActionAsync(model.ServiceOption, model);
                }
                else
                {

                    serviceAction = await GetUpdateServiceActionAsync(model.ServiceOption, model);
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

        private async Task<Func<Task<ExecutionMessages>>> GetInsertServiceActionAsync(string serviceOption, DailyCollectionConfiguration model)
        {
            //
            if (serviceOption == "zone")
            {

                return () => _zoneServices.Create(model.Zone);

            }
            else if (serviceOption == "agentDailyCashLimit")
            {
                return () => _agentDailyCashLimitServices.Create(model.AgentDailyCashLimit);
            }
            else if (serviceOption == "commissionSetting")
            {
                return () => _commissionSettingServices.Create(model.CommissionSetting);

            }
            else
            {
                return null;
            }
        }

        private async Task<Func<Task<ExecutionMessages>>> GetUpdateServiceActionAsync(string serviceOption, DailyCollectionConfiguration model)
        {
            if (serviceOption == "zone")
            {

                return () => _zoneServices.Update(model.Zone);
            }
            else if (serviceOption == "agentDailyCashLimit")
            {
                return () => _agentDailyCashLimitServices.Update(model.AgentDailyCashLimit);
            }
            else if (serviceOption == "commissionSetting")
            {
                return () => _commissionSettingServices.Update(model.CommissionSetting);
            }

            else
            {
                return null;
            }
        }


        public async Task<ActionResult> Delete(string KEY, string serviceOption)
        {


            if (serviceOption == "commissionSetting")
            {
                var data = await _commissionSettingServices.Delete(KEY);
                return Json(new { success = data, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);


            }
            else 
            {

                return null;
            }
        }
    }
}