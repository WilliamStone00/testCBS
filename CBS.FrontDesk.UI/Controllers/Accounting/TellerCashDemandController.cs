using CBS.BusinessService.Accounting;
using CBS.BusinessService;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using CBS.BusinessService.Accounts;
using System.Web.Services.Description;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using System.Web.Configuration;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    [CheckSessionTimeOutAttribute]
    public class TellerCashDemandController : BaseController
    {
        public TellerCashReplenishmentServices Service { get; set; }
        private AccountingEntryServices _accountingEntry { get; set; }
        public BranchServices branchServices { get; set; }
        private readonly AccountingServices _AccountServices;
        public TellerCashDemandController()
        {
            Service = new TellerCashReplenishmentServices();
            branchServices = new BranchServices();
            _accountingEntry = new AccountingEntryServices();
            _AccountServices = new AccountingServices();
        }
        public async Task<ActionResult> Index()

        {
            await GetList();
            return View(new CashDemandDataEntity());
        }

        public async Task<ActionResult> Delete(string KEY, string serviceOption)
        {


            if (serviceOption == "cashrequest")
            {
                var data = await Service.Delete(KEY);
                return Json(new { success = data, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);


            }

            else
            {
                return null;
            }

        }
        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(CashDemandDataEntity model)
        {
            if (model.ServiceOption.Equals("cashRequest"))
            {
                if (model.Action.Equals("insert"))
                {
                    var datac = await Service.CreateCashReplenishmentRequest(model.CashInfusionRequest);

                    return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

                }
                else
                {
                    //update
                    var datac = await Service.Update(model.CashInfusionModel);
                    return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

                }
            }
            else if (model.ServiceOption.Equals("CashRequestApproval"))
            {

                var datac = await Service.CreateApprovalRequest(model.Approval);

                return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

            }
            else
            {

                return Json(new { success = true, status = "No processing done", message = Messaging.MessageResult(new ExecutionMessages()) });


            }

        }
        private async Task<PartialViewResult> GetServiceAction(string path, string partialView, string key, string serviceOption)
        {
            if (serviceOption == "cashRequest")
            {
                if (path == "list")
                {
                    var dataChart = await Service.GetAllCashReplenimentRequest();

                    var sysData = new CashDemandDataEntity { DetailsDtos = dataChart.ToList() };
                    return PartialView(partialView, sysData);
                }

                else if (path == "new")
                {

                    await GetList();
                    return PartialView(partialView, new CashDemandDataEntity { });
                }
                else
                {
                    var data = await Service.GetCashReplenimentRequest(key);
                    return PartialView(partialView, new CashDemandDataEntity { DetailsDto = data });

                }


            }

            return null;
        }

        public async Task GetList(string language = "En")
        {
            List<SelectListItem> list = new List<SelectListItem>();
            ViewBag.OpeningOfDayId = await Service.GetCashReplenimentRequestId();
            if (ViewBag.OpeningOfDayId.Count == 0)
            {
                list.Add(new SelectListItem { Text = "Id001", Value = "Current Opening Reference" });
                ViewBag.OpeningOfDayId = list;
            }
            ViewBag.Decisions = BuildMenuViewBag();
        }
        private dynamic BuildMenuViewBag()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            list.Add(new SelectListItem { Text = $"Approve", Value = "Approve" });

            list.Add(new SelectListItem { Text = $"Rejected", Value = "Rejected" });


            return list;
        }

        public async Task<ActionResult> CreateCashReplenishmentRequest()
        {
            await GetList(); 
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateCashReplenishmentRequest(CashDemandDataEntity model)
        {
          
                var result = await Service.CreateCashReplenishmentRequest(model.CashInfusionRequest);
                if (result.MessageStatus.Equals("Failed"))
                {
                    var datas = DetailsDto.SetDefault(model.CashInfusionRequest);
                    return View("Failed_Request_View", datas);
                }
                else
                {
                    return View("Successfull_Request_View", result.Data);
                }
          
        }


        public async Task<ActionResult> GetCashReplenimentRequest(string KEY)
        {
            //await GetList(); 
            var datas = await Service.GetCashReplenimentRequest(KEY);
            return Json(datas, JsonRequestBehavior.AllowGet);


        }
        [HttpGet]
        public async Task<ActionResult> GetAllCashReplenimentRequestData()
        {
            var datas = await Service.GetAllCashReplenimentRequest();


            return View(datas);
        }

        [HttpGet]
        public async Task<ActionResult> CreateApprovalRequest()
        {

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateApprovalRequest(Approval model)

        {

            if (ModelState.IsValid)
            {
                var datas = await Service.CreateApprovalRequest(model);
                if (datas.MessageStatus.Equals("Failed"))
                {
                    var datasw = await Service.GetCashReplenimentRequest(model.id);
                    return View("Failed_RequestApproval_View", datasw);
                }
                else
                {
                    var datasw = await Service.GetCashReplenimentRequest(model.id);
                    return View("Successfull_RequestApproval_View", datasw);
                }

            }
            else
            {
                var datasw = await Service.GetCashReplenimentRequest(model.id);
                return View("Failed_RequestApproval_View", datasw);
            }




        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {

            // await GetList();
            if (path == "list")
            {

                var datas = await Service.GetAllCashReplenimentRequest();
                //var dataUser = (await _accountingEntry.GetUserList()).ToList();
                //var branches = (await branchServices.GetBranches()).ToList();
                //var result = (from request in datas
                //             join user in dataUser on request.requesterUserId equals user.id.ToString()

                //             select new DetailsDto
                //             {
                //                 id = request.id,
                //                 //re = request.ReferenceId,
                //                 requestedAmount = request.requestedAmount,
                //                 BranchOffice = "",
                //                 requetcomment = request.requetcomment,
                //                 requesterUserId = user.name + "," + user.roleName,
                //                 RequestDate = request.RequestDate,
                //                 approvedBy = request.approvedBy,
                //                 approvedDate = request.approvedDate,
                //                 approvedStatus = request.approvedStatus,
                //                 approvedComment = request.approvedComment
                //             }).ToList();
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.DetailsDtos = datas;


                return PartialView(partialView, cashDemandDataEntity);
            }
            else if (path == "new")
            {
                await GetList();
                return PartialView(partialView, new CashDemandDataEntity { CashReplenimentRequest = new CashReplenimentRequest(), CashInfusionModel = new CashInfusion(), CashInfusionRequest = new CashInfusionRequest() });
            }
            else if (path == "update")
            {
                await GetList();
                var OperationEventAttribute = await _accountingEntry.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.CashInfusionModel = OperationEventAttribute.ConvertToCashInfusionModel();
                return PartialView(partialView, cashDemandDataEntity);

            }
            else if (path == "status")
            {
                await GetList();
                var OperationEventAttribute = await _accountingEntry.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.CashReplenimentRequestdto = OperationEventAttribute.ConvertToCashReplenimentRequestDto();

                return PartialView(partialView, cashDemandDataEntity);

            }
            else if (path == "approve")
            {

                var OperationEventAttribute = await _accountingEntry.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.CashReplenimentRequestdto = OperationEventAttribute.ConvertToCashReplenimentRequestDto();
                //var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(OperationEventAttribute.BranchId);
                //ViewBag.Accounts = BuildDropDown(GenerateAccountListView(listOfAccounts));
                //ViewBag.Decisions = BuildMenuViewBag();
                return PartialView(partialView, cashDemandDataEntity);

            }
            else
            {
                await GetList();
                var OperationEventAttribute = await _accountingEntry.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.CashInfusionModel = OperationEventAttribute.ConvertToCashInfusionModel();
                return PartialView(partialView, cashDemandDataEntity);

            }
        }
        private IEnumerable<StringValues> GenerateAccountListView(List<Data.Account> accounts)
        {
            List<StringValues> stringValues = new List<StringValues>();
            foreach (var branch in accounts)
            {

                stringValues.Add(new StringValues(branch.Id, $"{branch.AccountNumber}-{branch.AccountName}- {branch.CurrentBalance}"));
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