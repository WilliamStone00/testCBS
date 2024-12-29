using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.CorrespondingBankAccount;
using CBS.BusinessService.Services;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity.CorrespondingBankManaagement;
using Microsoft.Extensions.Primitives;
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
namespace CBS.FrontDesk.UI.Controllers
{
    public class CorrespondingBankManagementController : BaseController
    {
        private readonly CorrespondingBankBranchServices _correspondingBankBranchServices;
        private readonly CorrespondingBankServices _correspondingBankServices;
        private readonly UserManagementServices _userService;

        public CorrespondingBankManagementController()
        {
            _correspondingBankBranchServices = new CorrespondingBankBranchServices();
            _userService = new UserManagementServices();
            _correspondingBankServices = new CorrespondingBankServices();
        }
        // GET: CorrespondingBank
        public async Task<ActionResult> Index()
        {
            await GetList();
            return View(new CorrespondingBankConfiguration());
        }
        public async Task<ActionResult> ManageCorrespondingBank(string Key)
        {
            var model = await _correspondingBankServices.GetCorrespondingBank(Key);
            ViewBag.Regions = await _correspondingBankBranchServices.GetValueOption("REGION");
            return View(new CorrespondingBankConfiguration { CorrespondingBankBranches = await GetBranches(Key), CorrespondingBank = model});
        }

        private async Task GetList()
        {
            ViewBag.BankTypes = await _correspondingBankBranchServices.GetBanktypeAsync();
            ViewBag.Regions = await _correspondingBankBranchServices.GetValueOption("REGION");



        }
       // 

        public async Task<ActionResult> GetValueOption(string switch_on, string Id)
        {


            try
            {
                //var number = chartOfAccountNumber.Length==1? chartOfAccountNumber: chartOfAccountNumber.Substring(0, 1);

               
                var dataList = await  _correspondingBankBranchServices.GetValueOption(switch_on,Id);


                return Json(dataList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
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
                   
                    var sysData = new CorrespondingBankConfiguration { CorrespondingBanks = DataSet.ToList()};
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
                    var bankBranchDtos =await GetBranches("");
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

            return null;
        }
        public async Task<ActionResult> Delete(string KEY, string serviceOption)
        {


            if (serviceOption == "correspondingBank")
            {
                
                    var data = await _correspondingBankServices.Delete(KEY);
                 

            }
            else if (serviceOption == "correspondingBankBranch")
            {
                var data = await _correspondingBankBranchServices.Delete(KEY);
            }
             return null ;

        }

        public async Task<List<CorrespondingBankBranchDto>> GetBranches(string KEY)
        {
            var dataTowns = (await _correspondingBankBranchServices.GetLocationInfo()).Towns;
            var DataSetBankBranch = KEY == "" ? await _correspondingBankBranchServices.GetCorrespondingBankBranch(): await _correspondingBankBranchServices.GetCorrespondingBankBranchByBankId(KEY);
            var DataSetBank =  await _correspondingBankServices.GetCorrespondingBank() ;
            List<CorrespondingBankBranchDto> bankBranchDtos = (from bankBranch in DataSetBankBranch
                                                               join bank in DataSetBank on bankBranch.CorrespondingBankId equals bank.Id
                                                               join town in dataTowns  on bankBranch.TownId equals town.Id
                                                               select new CorrespondingBankBranchDto
                                                               {
                                                                   Id = bankBranch.Id,
                                                                   BranchName = bankBranch.BranchName,
                                                                   BankCode = bank.Code,
                                                                   BankName = bank.Name,
                                                                   FocalPointContact = bankBranch.FocalPointContact,
                                                                   FocalPointName = bankBranch.FocalPointName,
                                                                   TownName = town.Name
                                                               }).ToList();
            return bankBranchDtos ;

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