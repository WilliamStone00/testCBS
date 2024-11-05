using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.ThirdPartyBankAccount;
using CBS.BusinessService.ThirdPartyInstitutionAccount;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.CashMovementTracker;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{

    [CheckSessionTimeOutAttribute]
    public class CashMovementTrackerController : BaseController
    {
        private readonly CashMovementTrackerServices _cashMovementTrackerServices;
        private readonly AccountingEntryServices _accountingEntryServices;
        private readonly BranchServices _branchService;
        private readonly UserManagementServices _userService;
        private readonly CashMovementTrackingConfigurationServices _cashMovementTrackingConfigurationServices;
        public CashMovementTrackerController(CashMovementTrackerServices CollateralServices, AccountingEntryServices entryServices , BranchServices branchServices, UserManagementServices services, CashMovementTrackingConfigurationServices cashMovementTrackingConfigurationServices)
        {
            _userService = services;
            _cashMovementTrackerServices = CollateralServices;
            _accountingEntryServices = entryServices;
            _branchService = branchServices;
            _cashMovementTrackingConfigurationServices = cashMovementTrackingConfigurationServices;
        }

        // GET: CashMovementTracker
        public ActionResult Index()
        {
          
            return View(new CashMovementConfiguration());
        }

        public async Task<ActionResult> SettingCashTracker()
        {
 
            return View(new CashMovementConfiguration { CashReplenimentRequestDtos = await _accountingEntryServices.GetAllCashReplenimentRequest(), DepositNotificationDto =   await _accountingEntryServices.GetAllDepositNotificationRequest() }) ;
        }
        [HttpPost]
        public async Task<ActionResult> Create(CashMovementTracker model)
        {
            if (model.Id == null)
            {
                if (ModelState.IsValid)
                {
                    var data = await _cashMovementTrackerServices.Create(model);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
            }
            else
            {
                return await Update(model);
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpGet]
        public async Task<ActionResult> GetAllBranchUser(string branchId)
        {
            if (!string.IsNullOrEmpty(branchId))
            {
                var listOfusers = await _userService.GetUsers();
                var listOfUserByBranch = listOfusers.Where(x => x.BranchID == branchId).ToList();
                var data = BuildDropDown(GenerateBranchUserListView(listOfUserByBranch));
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, status = false, message = "Fill the required fields." });
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetCashTrackingConfiguration(string branchId,string referenceId,string transType, string movementType)
        {
            if (!string.IsNullOrEmpty(branchId)&& !string.IsNullOrEmpty(referenceId) && !string.IsNullOrEmpty(transType) && !string.IsNullOrEmpty(movementType))
            {
                if (transType.Equals("CashRepleniment"))
                {
                    var request = _accountingEntryServices.GetCashReplenimentRequest(referenceId);
                }
                else
                {

                }
                //var data = BuildDropDown(GenerateBranchUserListView(listOfUserByBranch));
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, status = false, message = "Fill the required fields." });
            }
        }
        [HttpPost]
        public async Task<ActionResult> Update(CashMovementTracker model)
        {
            if (ModelState.IsValid)
            {
                var data = await _cashMovementTrackerServices.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {

            // await GetList();
            if (path == "list")
            {
                List<CashMovementTracker> CashMovementTrackingConfigurations = new List<CashMovementTracker>();
                var movements = (await _cashMovementTrackerServices.GetCashMovementTracker()).ToList();
                var datas = await _accountingEntryServices.GetAllCashReplenimentRequest();
                var branches = (await _branchService.GetBranches()).ToList();
                List<Branch> branchList = branches;
                var users = (await _userService.GetUsers()).ToList();
                var usersing = users;
                var moveCashConfig =( await   _cashMovementTrackingConfigurationServices.GetCashMovementTrackingConfiguration()).ToList();
                var  result = from move in movements  
                              join req in datas on move.ReferenceId equals req.ReferenceId
                              join config in moveCashConfig on move.CashMovementTrackingConfigurationId equals config.Id
                              join branchFrom in branches on req.BranchId equals branchFrom.Id    
                              join branch  in branchList on config.To equals branch.Id
                              join user in users on move.DoneBy equals user.id.ToString()
                              join userx in usersing on move.DoneBy equals userx.id.ToString()
                              select new CashMovementDataStatus
                              {
                                 Id = move.Id,
                                 MovementType = config.MovementType,
                                 BranchName = branchFrom.BranchCode + "-" + branchFrom.Name,
                                 DoneBy = user.lastName+" " +user.firstName,
                                 Amount = req.AmountApproved,
                                 Destination = branch.Name,
                                 DoneAt = move.StartTime,
                                 ExpiresAt = move.EndTime,
                                 CreatedBy = userx.lastName + " " + userx.firstName
                              };
                return PartialView(partialView, new CashMovementConfiguration { CashMovementDataStatus = result.ToList() });
            }

            else if (path == "new")
            {
                await GetList();

                return PartialView(partialView, new CashMovementConfiguration { CashMovementTracker=new CashMovementTracker() });
            }
            else
            {


                var movements = (await _cashMovementTrackingConfigurationServices.GetCashMovementTrackingConfiguration(KEY));
                return PartialView(partialView, new CashMovementConfiguration { CashMovementTrackingConfiguration = movements });
            }
        }


        public async Task GetList()
        {
            var DebitAccounts = new List<Data.Account>();
            var listBranch = await _branchService.GetBranches();
            ViewBag.Branches = BuildDropDown(GenerateBranchListView(listBranch.ToList()));


        }
        private IEnumerable<StringValues> GenerateBranchListView(List<Branch> branches)
        {
            List<StringValues> stringValues = new List<StringValues>();
            foreach (var branch in branches)
            {

                stringValues.Add(new StringValues(branch.Id, branch.Name));
            }
            return stringValues;
        }
        private IEnumerable<StringValues> GenerateBranchUserListView(List<User> branches)
        {
            List<StringValues> stringValues = new List<StringValues>();
            foreach (var branch in branches)
            {

                stringValues.Add(new StringValues(branch.id.ToString(), branch.lastName+" "+branch.firstName));
            }
            return stringValues;
        }
        private List<SelectListItem> BuildDropDown(IEnumerable<StringValues> stringValues)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var item in stringValues)
            {

                list.Add(new SelectListItem { Text = item.Text, Value = item.Value });

            }

            return list;
        }

    }
}