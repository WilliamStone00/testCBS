using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.CorrespondingBankAccount;
using CBS.BusinessService.Services;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity.CorrespondingBankManaagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CBS.FrontDesk.Data.Entity;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office2010.Excel;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data;
using System.Web.Services.Description;
using CBS.FrontDesk.Data.Message;
using StringValues = CBS.FrontDesk.Data.Entity.StringValues;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Config;
namespace CBS.FrontDesk.UI.Controllers
{
    public class CorrespondingBankManagementController : BaseController
    {
        private readonly CorrespondingBankBranchServices _correspondingBankBranchServices;
        private readonly CorrespondingBankServices _correspondingBankServices;
        private readonly UserManagementServices _userService;
        private readonly BankingZoneServices _bankingZoneServices;
        private readonly BankZoneBranchServices _bankZoneBranchServices;
        private readonly BranchServices _branchServices;
        public CorrespondingBankManagementController()
        {
            _correspondingBankBranchServices = new CorrespondingBankBranchServices();
            _userService = new UserManagementServices();
            _correspondingBankServices = new CorrespondingBankServices();
            _bankingZoneServices = new BankingZoneServices();
            _branchServices = new BranchServices();
            _bankZoneBranchServices = new BankZoneBranchServices();

        }
        // GET: CorrespondingBank
        public async Task<ActionResult> Index()
        {
            await GetList();
            return View(new CorrespondingBankConfiguration());
        }
        public async Task<ActionResult> ManageCorrespondingBank()
        {

            await GetList();
            return View(new CorrespondingBankConfiguration { CorrespondingBanks = (await _correspondingBankServices.GetCorrespondingBank()).ToList() });
        }

        private async Task GetList()
        {
            ViewBag.BankTypes = await _correspondingBankBranchServices.GetBanktypeAsync();
            ViewBag.Regions = await _correspondingBankBranchServices.GetValueOption("REGION");
            ViewBag.ThirdPartyInstitutions = GetCorrespondingBankBranchValueOption((await _correspondingBankServices.GetCorrespondingBank()).ToList());
            ViewBag.LocationType = await LocationTypeAsync();
            ViewBag.BankOrBrancheTypes = await ParticipantTypeAsync();
          
            ViewBag.BankingZones = GetBankingZone(await _bankingZoneServices.GetBankingZone());
        }
        public async Task<List<System.Web.WebPages.Html.SelectListItem>> LocationTypeAsync()
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();

            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "REGION", Value = "REGION" });
            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "DIVISION", Value = "DIVISION" });
            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "SUBDIVISION", Value = "SUBDIVISION" });
            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "TOWN", Value = "TOWN" });
            return selectListItems;
        }
        public async Task<List<System.Web.WebPages.Html.SelectListItem>> ParticipantTypeAsync()
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();

            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "CORRESPONDANT", Value = "CORRESPONDANT" });
            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "BRANCH", Value = "BRANCH" });

            return selectListItems;
        }
        public List<StringValues> GetCorrespondingBankBranchValueOption(List<CorrespondingBank> bankBranches)
        {
            List<StringValues> selectListItems = new List<StringValues>();

            if (bankBranches!=null)
            {
                foreach (var item in bankBranches)
                {
                    selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
                }
            }
            else
            {
                selectListItems.Add(new StringValues { Text = "XXXXX", Value = "No Corresponding Bank Exist" });
            }



            return selectListItems;
        }

        public List<StringValues> GetBranchListOption(List<Branch> bankBranches)
        {
            List<StringValues> selectListItems = new List<StringValues>();


            foreach (var item in bankBranches)
            {
                selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
            }


            return selectListItems;
        }
        public List<StringValues> GetBankBranch(List<CorrespondingBankBranchDto> bankBranches)
        {
            List<StringValues> selectListItems = new List<StringValues>();
            if (bankBranches != null)
            {
                foreach (var item in bankBranches)
                {
                    selectListItems.Add(new StringValues { Text = item.Id, Value = $"{item.BankName}-{item.BranchName}" });
                }
            }
            else
            {
                selectListItems.Add(new StringValues { Text = "XXXXX", Value = "No branch exist" });
            }
            return selectListItems;
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

        public async Task<ActionResult> GetValueOption(string switch_on, string Id)
        {


            try
            {
                //var number = chartOfAccountNumber.Length==1? chartOfAccountNumber: chartOfAccountNumber.Substring(0, 1);
                List<StringValues> dataList = new List<StringValues>();
                if (CheckIfZones(Id))
                {
                    dataList = await _correspondingBankBranchServices.GetLocationValueOption(switch_on, Id);
                }
                else
                {
                    dataList = await _correspondingBankBranchServices.GetValueOption(switch_on, Id);
                }



                return Json(dataList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }



        public async Task<ActionResult> loadBankZoneBranchType(string option)
        {


            try
            {
                //var number = chartOfAccountNumber.Length==1? chartOfAccountNumber: chartOfAccountNumber.Substring(0, 1);
                List<StringValues> dataList = new List<StringValues>();
                if (option.Equals("CORRESPONDANT"))
                {
                    var data = await GetBranches("");
                    dataList = GetBankBranch(data);
                }
                else
                {

                    var data = await _branchServices.GetBranches();
                    dataList = GetBranchListOption(data.ToList());
                }



                return Json(dataList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        private bool CheckIfZones(string id)
        {
            return id.Equals("DIVISION") || id.Equals("REGION") || id.Equals("SUBDIVISION") || id.Equals("TOWN");
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            await GetList();
            var partialResult = await GetServiceAction(path, partialView, KEY, serviceOption);

            return partialResult;
        }

        private async Task<PartialViewResult> GetServiceAction(string path, string partialView, string key, string serviceOption)
        {
            if (serviceOption == "correspondingBank")
            {
                if (path == "list")
                {
                    var DataSet = await _correspondingBankServices.GetCorrespondingBank();

                    var sysData = new CorrespondingBankConfiguration { CorrespondingBanks = DataSet.ToList() };
                    return PartialView(partialView, sysData);

                }

                else if (path == "new")
                {

                    return PartialView(partialView, new CorrespondingBankConfiguration { CorrespondingBank = new CorrespondingBank() });
                }
                else
                {
                    var data = await _correspondingBankServices.GetCorrespondingBank(key);

                    return PartialView(partialView, new CorrespondingBankConfiguration { CorrespondingBank = data });

                }


            }
            else if (serviceOption == "correspondingBankBranch")
            {
                if (path == "list")
                {
                    var bankBranchDtos = await GetBranches("");
                    var sysData = new CorrespondingBankConfiguration { CorrespondingBankBranches = bankBranchDtos };
                    return PartialView(partialView, sysData);

                }

                else if (path == "new")
                {

                    return PartialView(partialView, new CorrespondingBankConfiguration { CorrespondingBankBranch = new CorrespondingBankBranch() });
                }
                else
                {
                    var data = await _correspondingBankBranchServices.GetCorrespondingBankBranch(key);

                    return PartialView(partialView, new CorrespondingBankConfiguration { CorrespondingBankBranch = data });

                }


            }
            else if (serviceOption == "bankZoneBranch")
            {
                if (path == "list")
                {
                    var bankBranchDtos = await GetBankZoneBranches("");
                    var sysData = new CorrespondingBankConfiguration { BankZoneBranchs = bankBranchDtos };
                    return PartialView(partialView, sysData);

                }

                else if (path == "new")
                {

                    return PartialView(partialView, new CorrespondingBankConfiguration { BankZoneBranch = new BankZoneBranch() });
                }
                else
                {
                    var data = await _bankZoneBranchServices.GetBankZoneBranch(key);

                    return PartialView(partialView, new CorrespondingBankConfiguration { BankZoneBranch = data });

                }


            }
            else if (serviceOption == "bankingZone")
            {
                if (path == "list")
                {
                    var bankBranchDtos = await _bankingZoneServices.GetBankingZone();
                    var sysData = new CorrespondingBankConfiguration { BankingZones = bankBranchDtos };
                    return PartialView(partialView, sysData);

                }

                else if (path == "new")
                {

                    return PartialView(partialView, new CorrespondingBankConfiguration { BankingZone = new BankingZone() });
                }
                else
                {
                    var data = await _bankingZoneServices.GetBankingZone(key);

                    return PartialView(partialView, new CorrespondingBankConfiguration { BankingZone = data });

                }


            }
            return null;
        }
        public async Task<ActionResult> Delete(string KEY, string serviceOption)
        {


            if (serviceOption == "correspondingBank")
            {

                var data = await _correspondingBankServices.Delete(KEY);
                return Json(new { success = data, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);


            }
            else if (serviceOption == "correspondingBankBranch")
            {
                var data = await _correspondingBankBranchServices.Delete(KEY);
                return Json(new { success = data, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { success = "data", status = "Failed", message ="No element could be deleted successfully" }, JsonRequestBehavior.AllowGet);


        }

        public async Task<List<CorrespondingBankBranchDto>> GetBranches(string KEY)
        {
            var dataTowns = (await _correspondingBankBranchServices.GetLocationInfo()).Towns;
            var DataSetBankBranch = KEY == "" ? await _correspondingBankBranchServices.GetCorrespondingBankBranch() : await _correspondingBankBranchServices.GetCorrespondingBankBranchByBankId(KEY);
            var DataSetBank = await _correspondingBankServices.GetCorrespondingBank();
            List<CorrespondingBankBranchDto> bankBranchDtos = (from bankBranch in DataSetBankBranch
                                                               join bank in DataSetBank on bankBranch.ThirdPartyInstitutionId equals bank.Id
                                                               //join town in dataTowns  on bankBranch.TownId equals town.Id
                                                               select new CorrespondingBankBranchDto
                                                               {
                                                                   Id = bankBranch.Id,
                                                                   BranchName = bankBranch.BranchName,
                                                                   BankCode = bank.Code,
                                                                   BankName = bank.Name,
                                                                   FocalPointContact = bankBranch.FocalPointContact,
                                                                   FocalPointName = bankBranch.FocalPointName,
                                                                   TownName = bankBranch.TownName
                                                               }).ToList();
            return bankBranchDtos;

        }


        public async Task<List<BankZoneBranchDto>> GetBankZoneBranches(string KEY)
        {

            var DataSetBankBranch = KEY == "" ? await _bankZoneBranchServices.GetBankZoneBranch() : (await _bankZoneBranchServices.GetBankZoneBranch()).Where(x => x.Id.Equals(KEY));
            var zoneDataset = (await _bankingZoneServices.GetBankingZone());
            var B3ppDataset = (await _correspondingBankServices.GetCorrespondingBank());
            var B3ppBranchDataset = (await _correspondingBankBranchServices.GetCorrespondingBankBranch());
            List<BankZoneBranchDto> bankBranchDtos = (from bankBranch in DataSetBankBranch
                                                      join zone in zoneDataset on bankBranch.BankingZoneId equals zone.Id
                                                      join branch in B3ppBranchDataset on bankBranch.BranchId equals branch.Id
                                                      join bnk in B3ppDataset on branch.ThirdPartyInstitutionId equals bnk.Id
                                                      select new BankZoneBranchDto
                                                      {
                                                          Id = bankBranch.Id,
                                                          BranchName = branch.BranchName,
                                                          BankingZoneName = zone.Name,
                                                          BankName = bnk.Name,
                                                          Type = bankBranch.Type,
                                                          //FocalPointName = bankBranch.FocalPointName,
                                                          //TownName = bankBranch.TownName
                                                      }).ToList();
            return bankBranchDtos;

        }
        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(CorrespondingBankConfiguration model)
        {
            Func<Task<ExecutionMessages>> serviceAction = null;

            if (model.ServiceOption == "correspondingBank")
            {

                if (model.Action == "insert")
                {
                    serviceAction = await GetInsertServiceActionAsync(model.ServiceOption, model);
                }
                else
                {

                    serviceAction = GetUpdateServiceAction(model.ServiceOption, model);
                }


            }
            else if (model.ServiceOption == "correspondingBankBranch")
            {
                if (model.Action == "insert")
                {

                    serviceAction = await GetInsertServiceActionAsync(model.ServiceOption, model);
                }
                else
                {

                    serviceAction = GetUpdateServiceAction(model.ServiceOption, model);
                }
            }
            else if (model.ServiceOption == "bankingZone")
            {
                if (model.Action == "insert")
                {

                    serviceAction = await GetInsertServiceActionAsync(model.ServiceOption, model);
                }
                else
                {

                    serviceAction = GetUpdateServiceAction(model.ServiceOption, model);
                }
            }
            else if (model.ServiceOption == "bankZoneBranch")
            {
                if (model.Action == "insert")
                {

                    serviceAction = await GetInsertServiceActionAsync(model.ServiceOption, model);
                }
                else
                {

                    serviceAction = GetUpdateServiceAction(model.ServiceOption, model);
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

        private async Task<Func<Task<ExecutionMessages>>> GetInsertServiceActionAsync(string serviceOption, CorrespondingBankConfiguration model)
        {
            if (serviceOption == "correspondingBank")
            {
                return () => _correspondingBankServices.Create(model.CorrespondingBank);
            }

            else if (serviceOption == "correspondingBankBranch")
            {

                return () => _correspondingBankBranchServices.Create(model.CorrespondingBankBranch);
            }
            else if (serviceOption == "bankingZone")
            {

                return () => _bankingZoneServices.Create(model.BankingZone);
            }
            else if (serviceOption == "bankZoneBranch")
            {

                return () => _bankZoneBranchServices.Create(model.BankZoneBranchObj);
            }
            else
            {
                return null;
            }
        }
        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(string serviceOption, CorrespondingBankConfiguration model)
        {
            if (serviceOption == "correspondingBank")
            {
                return () => _correspondingBankServices.Update(model.CorrespondingBank);
            }

            else if (serviceOption == "correspondingBankBranch")
            {
                return () => _correspondingBankBranchServices.Update(model.CorrespondingBankBranch);
            }

            else
            {
                return null;
            }
        }
    }
}