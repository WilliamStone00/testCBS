using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.BusinessService.DailyCollectionServices;
using CBS.BusinessService.Session;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DailyCollectorManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers
{
    public class DailyAgentManagementController : BaseController
    {
        private BranchServices _branchService;
        private AgentServices _agentServices;
        private AgentAccountServices _agentAccountServices;
        public DailyAgentManagementController()
        {
            _branchService = new BranchServices();
            _agentServices = new AgentServices();
            _agentAccountServices = new AgentAccountServices();


        }
        public async Task<ActionResult> Index()
        {
            await GetList();
            return View(new DailyAgentManagement());
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

        private async Task GetList()
        {
            var BranList = (await _branchService.GetBranches()).ToList();
            ViewBag.Branches = BuildBranch(BranList);
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
        [HttpPost]
     
        public async Task<ActionResult> AddOrUpdate(DailyAgentManagement model)
        {
            Func<Task<ExecutionMessages>> serviceAction = null;

            if (model.ServiceOption == "agent")
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
            else if(model.ServiceOption == "agentAccount")
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

        private async Task<Func<Task<ExecutionMessages>>> GetUpdateServiceActionAsync(string serviceOption, DailyAgentManagement model)
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