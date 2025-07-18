using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.BusinessService.DailyCollectionServices;
using CBS.BusinessService.Session;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DailyCollectionData;
using CBS.FrontDesk.Data.Entity.DailyCollectorManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
// 
namespace CBS.FrontDesk.UI.Controllers
{
    //DailyAgentManagement/AddOrUpdate
    public class DailyAgentManagementController : BaseController
    {
        private BranchServices _branchService;
        private AgentServices _agentServices;
        private UserManagementServices _userServices;
        private AgentAccountServices _agentAccountServices;
        public DailyAgentManagementController()
        {
            _branchService = new BranchServices();
            _agentServices = new AgentServices();
            _userServices = new UserManagementServices();
            _agentAccountServices = new AgentAccountServices();


        }
        public async Task<ActionResult> Index()
        {
            await GetList();
            return View(new DailyCollectionConfiguration());
        }
        public async Task<ActionResult> CommissionPayment()
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

        private async Task<ActionResult> GetServiceAction(string path, string partialView, string KEY, string serviceOption)
        {
            if (serviceOption == "agent")
            {
                if (path == "list")
                {
                    var data = await _agentServices.GetAgents();

                    var sysData = new DailyAgentManagement { Agents = data.ToList() };
                    return PartialView(partialView, sysData);

                }
                else if (path == "new")
                {


                    return PartialView(partialView, new DailyAgentManagement { Agent = new Data.Entity.DailyCollectionData.Agent() });

                }
                else
                {

                    var data = await _agentServices.GetAgentById(KEY);
                    return PartialView(partialView, new DailyAgentManagement { Agent = data });

                }


            }
            else
            {
                if (path == "list")
                {
                    var data = await _agentAccountServices.GetAgentAccounts();

                    var sysData = new DailyAgentManagement { AgentAccounts = data.ToList() };
                    return PartialView(partialView, sysData);

                }
                else if (path == "new")
                {


                    return PartialView(partialView, new DailyAgentManagement { AgentAccount = new Data.Entity.DailyCollectionData.AgentAccount() });

                }
                else
                {
                    var data = await _agentAccountServices.GetAgentAccountById(KEY);
                    return PartialView(partialView, new DailyAgentManagement { AgentAccount = data });

                }

            }
        }
        private dynamic BuildOperationTypes()
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();
            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "", Value = $"Select Option" });

            //selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "Subscription", Value = $"SUB" });
            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "Deposit", Value = $"CashIn" });

            return selectListItems;

        }
        private async Task<List<System.Web.WebPages.Html.SelectListItem>> BuildDailyCollectorAsync(List<DailyCollectorInfo> listOfCollector)
        {
        
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();

            foreach (var item in listOfCollector)
            {
                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.name , Value = item.userId.ToString() });

            }

            return selectListItems;

        }
        private async Task GetList()
        {
            var BranList = (await _branchService.GetBranches()).ToList();
            ViewBag.Branches = BuildBranch(BranList);
            ViewBag.OperationTypes = BuildOperationTypes();
            //ViewBag.DailyCollectors =await BuildDailyCollectorAsync();
        }
        private dynamic BuildBranch(List<Branch> listOfItems)
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();
            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "", Value = $"Select BranchCode" });
            foreach (var item in listOfItems)
            {
                if (!item.BranchCode.Equals("000"))
                {
                    selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = $"{item.Name}", Value = item.Id });
                }

            }
            return selectListItems;

        }
        [HttpPost]
        public async Task<ActionResult> PayDailyCollectorCommission(CollectorDto collector)
        {
            var sessionKey = "rptSource" + _agentServices.GetUserID();
            var modelData = this.HttpContext.Session[sessionKey] as PayDailyCollectorCommission;

            if (modelData == null)
            {
                return Json(new { success = false, message = "Session expired or report data not found." }, JsonRequestBehavior.AllowGet);
            }

            var result = await _agentServices.PayAgentActivitiesAsync(modelData);

            if (result != null)
            {
                return Json(new { success = true, data = result, message = "Payment successfully processed." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Error occurred while processing payment." }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public async Task<JsonResult> RetrieveDailyCollectionDashboardActivitiesAsync(string Month, string BranchId, string CollectorId)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(Month) || BranchId.IsNullOrEmpty() || CollectorId.IsNullOrEmpty())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid parameters supplied."
                    }, JsonRequestBehavior.AllowGet);
                }

                // Example: Fetch data from service/repository
                var activities = await _agentServices.GetAllActivitiesAsync((new DailyCollectionDashboardActivitiesQuery
                {
                    Month = Month,
                    BranchId = BranchId,
                    CollectorId = CollectorId,

                }).ConvertToDailyCollectionActivitiesQuery());

                if (activities == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No activities found for the provided parameters."
                    }, JsonRequestBehavior.AllowGet);
                }


                return Json(new
                {
                    success = true,
                    data = activities
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing your request."
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetActiveAgentBYBranch(string BranchId, string month)
        {
            int Year = 0; int Month = 0;
            try
            {
                // Validate input
                if (BranchId.IsNullOrEmpty())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid parameters supplied."
                    }, JsonRequestBehavior.AllowGet);
                }

                // Example: Fetch data from service/repository
                if (!string.IsNullOrEmpty(month))
                {
                    var monthParts = month.Split('-');
                    if (monthParts.Length == 2)
                    {
                        if (int.TryParse(monthParts[0], out int year) && int.TryParse(monthParts[1], out int months))
                        {
                            Year = year;
                            Month = months;
                        }
                        else
                        {
                            throw new FormatException($"Invalid month format: {month}. Expected format: YYYY-MM");
                        }
                    }
                    else
                    {
                        throw new FormatException($"Invalid month format: {month}. Expected format: YYYY-MM");
                    }
                }
                var activities = await _agentServices.GetAgentActiveAgent(BranchId, Month, Year);
                if (activities == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No activities found for the provided parameters."
                    }, JsonRequestBehavior.AllowGet);
                }

                var dee = await BuildDailyCollectorAsync(activities);
                return Json(new
                {
              
                    data = dee
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing your request."
                }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpGet]
        public async Task<JsonResult> RetrieveAgentActivitiesAsync(string Month, string BranchId, string CollectorId, string OperationType, string MemberReference)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(Month) || BranchId.IsNullOrEmpty() || CollectorId.IsNullOrEmpty())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid parameters supplied."
                    }, JsonRequestBehavior.AllowGet);
                }

                // Example: Fetch data from service/repository
                var activities = await _agentServices.GetAgentActivitiesAsync(new CollectorSalaryInfo
                {
                    Month = Month,
                    BranchId = BranchId,
                    CollectorId = CollectorId,
                    OperationType = OperationType,
                    MemberReference = MemberReference
                });

                if (activities == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No activities found for the provided parameters."
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    this.HttpContext.Session["rptSource" + _agentServices.GetUserID()] = activities.ConvertToPayDailyCollectorCommission();
                }

                return Json(new
                {
                    success = true,
                    data = activities
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing your request."
                }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public async Task<JsonResult> RetrieveFinancialActivitiesStatics(string Month, string BranchId, string CollectorId)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(Month) || BranchId.IsNullOrEmpty() || CollectorId.IsNullOrEmpty())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid parameters supplied."
                    }, JsonRequestBehavior.AllowGet);
                }

                // Example: Fetch data from service/repository
                var activities = await _agentServices.GetAgentActivitiesAsync(new CollectorSalaryInfo
                {
                    Month = Month,
                    BranchId = BranchId,
                    CollectorId = CollectorId,

                });

                if (activities == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No activities found for the provided parameters."
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    this.HttpContext.Session["rptSource" + _agentServices.GetUserID()] = activities.ConvertToPayDailyCollectorCommission();
                }

                return Json(new
                {
                    success = true,
                    data = activities
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing your request."
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private async Task<Func<Task<ExecutionMessages>>> GetInsertServiceActionAsync(string serviceOption, DailyAgentManagement model)
        {
            if (serviceOption == "agent")
            {

                return () => _agentServices.Create(model.Agent);

            }
            else if (serviceOption == "agentAccount")
            {
                return () => _agentAccountServices.Create(model.AgentAccount);
            }

            else
            {
                return null;
            }
        }
    }
}