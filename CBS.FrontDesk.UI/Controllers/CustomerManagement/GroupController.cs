using CBS.BusinessService.Accounting;
using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity.CustomerManagement.Grouping;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.CustomerManagement
{
    [CheckSessionTimeOutAttribute]
    public class GroupController : BaseController
    {        // GET: Group

        private readonly GroupTypeServices _groupTypeServices;
        private readonly GroupServices _groupServices;
        private readonly IndividualProfileServices _individualProfileServices;
        public GroupController(GroupServices tellerServices, GroupTypeServices groupTypeServices = null, IndividualProfileServices individualProfileServices = null)
        {
            _groupServices = tellerServices;
            _groupTypeServices = groupTypeServices;
            _individualProfileServices = individualProfileServices;
        }

        public async Task<ActionResult> Index()
        {
            await GetViewBags();

            var statuses = await _individualProfileServices.GetAllAsync(); // however you fetch them

            ViewBag.MemberStatuses = statuses?
                .Select(s => new SelectListItem
                {
                    Value = s.Id,
                    Text = $"[{s.Name}] - [{s.Description}]"
                })
                .OrderBy(x => x.Text)
                .ToList()
                ?? new List<SelectListItem>();

            var politicalstatus = await _individualProfileServices.GetAllPoliticalAsync(); // however you fetch them

            ViewBag.politicalstatuses = politicalstatus?
                .Select(s => new SelectListItem
                {
                    Value = s.Value,
                    Text = $"[{s.Text}] - [{s.Value}]"
                })
                .OrderBy(x => x.Text)
                .ToList()
                ?? new List<SelectListItem>();
            return View(new Group());
        }
        public async Task<ActionResult> Details(string KEY)
        {

            var groupmanagement = await _groupServices.GetGroupManagement(KEY);
            var members = await _individualProfileServices.GetIndividualProfileByBranch();
            ViewBag.Members = await _individualProfileServices.GetMembers(members.ToList());
            await GetViewBags();

            return View(groupmanagement);
        }
        public async Task<ActionResult> Listing()
        {
            var data = await _groupServices.GetGroups();
            return View(data);
        }

        [HttpGet]
        public async Task<ActionResult> LoadData(string searchCriteria = "All")
        {
            try
            {
                var dataTable = await _groupServices.GetDataTable(GetDataTableOptions(), searchCriteria);
                return Json(new { draw = dataTable.draw, recordsFiltered = dataTable.recordsTotal, recordsTotal = dataTable.recordsTotal, data = dataTable.data }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw;
            }
        }
       

        //
        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(Group model)
        {
            Func<Task<ExecutionMessages>> serviceAction = null;

            serviceAction = GetInsertServiceAction(model.ServiceOption, model);

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
        [HttpPost]
        public async Task<ActionResult> Update(GroupManagement model)
        {
            Func<Task<ExecutionMessages>> serviceAction = null;

            serviceAction = GetUpdateServiceAction(model);

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

        private Func<Task<ExecutionMessages>> GetInsertServiceAction(string serviceOption, Group model)
        {

            return () => _groupServices.Create(model);
        }

        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(GroupManagement model)
        {
            return () => _groupServices.Update(model);
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
            if (path == "list")
            {
                return async () =>
                {

                    var data = await _groupServices.GetGroups();
                    var sysData = data;
                    return PartialView(partialView, sysData);

                };
            }
            else if (path == "new")
            {
                return async () =>
                {
                    await GetViewBags();
                    return PartialView(partialView, new Group());
                };
            }
            else
            {
                return async () =>
                {
                    await GetViewBags();
                    return PartialView(partialView, await _groupServices.GetGroup(key));
                };
            }
        }
        public async Task<ActionResult> Delete(string id)
        {
            var data = await _groupServices.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        private async Task GetViewBags()
        {

            ViewBag.GroupTypes = await _groupTypeServices.GetGroupTypes();
            
            var agrAggregates = await _individualProfileServices.GetAggregates();
            ViewBag.Banks = agrAggregates.Banks;
            ViewBag.Branches = agrAggregates.Branches;
            ViewBag.EconomicActivities = agrAggregates.EconomicActivities;
            ViewBag.Countries = agrAggregates.Countries;
            ViewBag.Regions = agrAggregates.Regions;
            ViewBag.Divisions = agrAggregates.Divisions;
            ViewBag.Subdivisions = agrAggregates.Subdivisions;
            ViewBag.Towns = agrAggregates.Towns;
            ViewBag.Savings = agrAggregates.Savings;
            ViewBag.Organizations = agrAggregates.Organizations;
            ViewBag.bankingRelationships = agrAggregates.CustomerDefaultEnum.bankingRelationships;
            ViewBag.genders = agrAggregates.CustomerDefaultEnum.genders;
            ViewBag.membershipApprovalStatuses = agrAggregates.CustomerDefaultEnum.membershipApprovalStatuses;
            ViewBag.activeStatuses = agrAggregates.CustomerDefaultEnum.activeStatuses;
            ViewBag.workingStatuses = agrAggregates.CustomerDefaultEnum.workingStatuses;
            ViewBag.legalForms = agrAggregates.CustomerDefaultEnum.legalForms;
            ViewBag.formalOrInformalSectors = agrAggregates.CustomerDefaultEnum.formalOrInformalSectors;
            ViewBag.maritalStatuses = agrAggregates.CustomerDefaultEnum.maritalStatuses;
            ViewBag.languages = _individualProfileServices.GetLanguages();
            ViewBag.Categories = agrAggregates.CustomerDefaultEnum.customerCategories;
            ViewBag.relationships = agrAggregates.CustomerDefaultEnum.relationships;

        }
    }
}