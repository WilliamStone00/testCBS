using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.CorrespondingBankAccount;
using CBS.BusinessService.Services;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.CashMovementTracker;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CorrespondingBankManaagement;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    [CheckSessionTimeOutAttribute]
    public class CashMovementTrackerConfigurationController : BaseController
    {
        private readonly BranchServices _branchService;
        private readonly CashMovementTrackingConfigurationServices _cashMovementTrackingConfigurationServices;
        private readonly CorrespondingBankBranchServices _correspondingBankBranchServices;
        private readonly CorrespondingBankServices _correspondingBankServices;
        private readonly UserManagementServices _userService;
        private readonly BankingZoneServices _bankingZoneServices;
        private readonly BankZoneBranchServices _bankZoneBranchServices;
        public CashMovementTrackerConfigurationController()
        {
            _branchService = new BranchServices();
            _cashMovementTrackingConfigurationServices = new CashMovementTrackingConfigurationServices();
            _correspondingBankBranchServices = new CorrespondingBankBranchServices();
            _correspondingBankServices = new CorrespondingBankServices();
            _bankingZoneServices = new BankingZoneServices();
            _bankZoneBranchServices = new BankZoneBranchServices();
        }
        // GET: CashMovementTrackerConfiguration
        public async Task<ActionResult> Index()
        {
            await GetList();

            return View(new CashMovementConfiguration());
        }
        public List<StringValues> GetBankingZone(List<BankingZoneDto> bankBranches)
        {
            List<StringValues> selectListItems = new List<StringValues>();

            var collections = GetUniqueZoneName(bankBranches);
            foreach (var item in collections)
            {
                selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
            }


            return selectListItems;
        }
        private List<BankingZoneDto> GetUniqueZoneName(List<BankingZoneDto> bankBranches)
        {
            List<BankingZoneDto> List = new List<BankingZoneDto>();
            foreach (var item in bankBranches)
            {
                if (List.Find(x => x.Name.Equals(item.Name)) == null)
                {
                    List.Add(item);
                }

            }
            return List;
        }
        public async Task<ActionResult> AddOrUpdate(CashMovementConfiguration model)
        {

            if (model.Action.Equals("insert"))
            {
                var datac = await _cashMovementTrackingConfigurationServices.Create(model.CashMovementTrackingConfiguration);
                return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });


            }
            else
            {
                var datac = await _cashMovementTrackingConfigurationServices.Update(model.CashMovementTrackingConfiguration);
                return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });


            }



        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {

            // await GetList();
            if (path == "list")
            {
                List<CashMovementTrackingConfiguration> CashMovementTrackingConfigurations = new List<CashMovementTrackingConfiguration>();
                var movements = (await _cashMovementTrackingConfigurationServices.GetCashMovementTrackingConfiguration());
                var datas = movements.Where(x => x.MovementType == "Branch-To-Branch");
                var branches = (await _branchService.GetBranches()).ToList();
                var result = from request in datas
                             join branchFrom in branches on request.From equals branchFrom.Id
                             join branchTo in branches on request.To equals branchTo.Id
                             select new CashMovementTrackingConfiguration
                             {
                                 Id = request.Id,
                                 MovementType = request.MovementType,
                                 From = branchFrom.BranchCode + "-" + branchFrom.Name,
                                 To = branchTo.BranchCode + "-" + branchTo.Name,
                                 Duration = request.Duration,

                             };
                CashMovementTrackingConfigurations = result.ToList();
                var datas3pp = movements.Where(x => x.MovementType == "Branch-To-Bank");
                var datas3ppBank = (await _branchService.GetBranches()).ToList();
                var result2 = from request in datas3pp
                              join branchFrom in branches on request.From equals branchFrom.Id
                              join bankTo in datas3ppBank on request.To equals bankTo.Id
                              select new CashMovementTrackingConfiguration
                              {
                                  Id = request.Id,
                                  MovementType = request.MovementType,
                                  From = branchFrom.BranchCode + "-" + branchFrom.Name,
                                  To = bankTo.BranchCode + "-" + bankTo.Name,
                                  Duration = request.Duration,

                              };

                CashMovementTrackingConfigurations.AddRange(result2.ToList());
                return PartialView(partialView, new CashMovementConfiguration { CashMovementTrackingConfigurationData = CashMovementTrackingConfigurations });
            }

            else if (path == "new")
            {
                await GetList();

                return PartialView(partialView, new CashMovementConfiguration { });
            }
            else
            {


                var movements = (await _cashMovementTrackingConfigurationServices.GetCashMovementTrackingConfiguration(KEY));
                return PartialView(partialView, new CashMovementConfiguration { CashMovementTrackingConfiguration = movements });
            }
        }


        private dynamic BuildMovemenType()
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>
            {
            new System.Web.WebPages.Html.SelectListItem {Text  = "Branch-To-Branch", Value = "Branch To Branch" },
            new System.Web.WebPages.Html.SelectListItem { Text = "Branch-To-Bank", Value = "Branch To Bank" },
                        new System.Web.WebPages.Html.SelectListItem { Text = "Bank-To-Branch", Value = "Bank To Branch" },
            };
            return selectListItems;
        }
        private async Task GetList()
        {
            ViewBag.MovementTypes = BuildMovemenType();
            ViewBag.Branches = BuildBranch((await _branchService.GetBranches()).ToList());
            ViewBag.BankingZoneId = GetBankingZone(await _bankingZoneServices.GetBankingZone());
        }
        private dynamic BuildBranch(List<Branch> listOfItems)
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();
            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "", Value = $"Select BranchCode" });
            foreach (var item in listOfItems)
            {
                if (!item.BranchCode.Equals("000"))
                {
                    selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.BranchCode, Value = $"{item.BranchCode} - {item.Name}" });
                }

            }
            return selectListItems;

        }
        [HttpGet]
        public async Task<ActionResult> GetBuildThirdPartyBank()
        {
            try
            {
                var listOfAccounts = (await _branchService.GetBranches()).ToList();
                var listx = GenerateBranchListView(listOfAccounts);
                var data = BuildDropDown(listx);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = false, message = "Fill the required fields." });

            }
        }

        [HttpGet]
        public async Task<ActionResult> GetBuildBranches()
        {
            try
            {
                var listOfAccounts = (await _branchService.GetBranches()).ToList();
                var listx = GenerateBranchListView(listOfAccounts);
                var data = BuildDropDown(listx);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = false, message = "Fill the required fields." });

            }
        }
        private List<StringValues> GenerateBranchListView(List<Branch> branches)
        {
            List<StringValues> stringValues = new List<StringValues>();
            foreach (var branch in branches)
            {

                stringValues.Add(new StringValues(branch.Id, branch.Name));
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
        private dynamic BuildThirdPartyBank(List<Branch> listOfItems)
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();
            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "xxx", Value = $"Select ThirdPartyBank" });
            foreach (var item in listOfItems)
            {
                if (!item.BranchCode.Equals("000"))
                {
                    selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.BranchCode, Value = $"xxxx - {item.Name}" });
                }

            }
            return selectListItems;

        }
    }
}