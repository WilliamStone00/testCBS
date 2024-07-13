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
using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.Config;
using System.Web.Services.Description;
using System.Web.ModelBinding;
using System.Data;
using CBS.FrontDesk.Data;
using System.Security.Cryptography.Xml;
using CBS.FrontDesk.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    [CheckSessionTimeOutAttribute]
    public class CashFlowManagementController : BaseController
    {
        private readonly AccountingServices _AccountServices;
        private readonly AccountingEntryServices _accountingEntryServices;
        private readonly BranchServices branchServices;
        public CashFlowManagementController()
        {
            _AccountServices = new AccountingServices();
            _accountingEntryServices = new AccountingEntryServices();
            branchServices = new BranchServices();
        }
        // GET: BankingOperation


        public async Task GetList(string language = "En")
        {
            var DebitAccounts = new List<Data.Account>();
            var listBranch = await branchServices.GetBranches();
            ViewBag.Branches = BuildDropDown(GenerateBranchListView(listBranch.ToList()));

            ViewBag.Accounts = BuildDropDown(GenerateAccountListView(DebitAccounts));
            ViewBag.Decisions = BuildMenuViewBag();
            var models = BuildMenuViewBag(await _accountingEntryServices.GetCashReplenimentCurrentOpenOfDayHistoryRequestId());


            ViewBag.OpeningOfDayId = models;//BuildMenuViewBag(await _accountingEntryServices.GetCashReplenimentCurrentOpenOfDayHistoryRequestId());

            if (models.Count() == 0)
            {
                ViewBag.OpeningOfDayId = new List<StringValues> { new StringValues { Text = "Id001", Value = "Current Opening Reference" } };
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
        private IEnumerable<StringValues> GenerateBranchListView(List<Branch> branches)
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
        private List<SelectListItem> BuildMenuViewBag(IEnumerable<CashRoot> debitAccounts)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var item in debitAccounts)
            {

                list.Add(new SelectListItem { Text = item.id, Value = item.text });

            }

            return list;
        }

        private dynamic BuildMenuViewBagCurrency(List<CBS.FrontDesk.Data.Entity.Accounting.Currency> currencies)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var item in currencies)
            {

                list.Add(new SelectListItem { Text = item.Code, Value = item.Name + "-" + item.Symbol });

            }

            return list;
        }
        private dynamic BuildMenuViewBag(IEnumerable<ChartOfAccount> debitAccounts, string language = "En")
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var item in debitAccounts)
            {
                if (language == "En")
                    list.Add(new SelectListItem { Text = item.Id, Value = item.AccountNumber + "-" + item.LabelEn });
                else
                {
                    list.Add(new SelectListItem { Text = item.Id, Value = item.AccountNumber + "-" + item.LabelFr });

                }
            }

            return list;
        }


        private dynamic BuildMenuViewBag()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            list.Add(new SelectListItem { Text = $"Approve", Value = "Approve" });

            list.Add(new SelectListItem { Text = $"Rejected", Value = "Rejected" });

            list.Add(new SelectListItem { Text = $"Redirect-To-Branch", Value = "Redirect-To-Branch" });
            return list;
        }

        [HttpGet]
        public async Task<ActionResult> CreateCashReplenishmentRequest()
        {
            await GetList();
            return View(new CashDemandDataEntity());
        }
        [HttpGet]
        public async Task<ActionResult> CreateBankCashOut()
        {
            await GetList();
            return View(new CashDemandDataEntity());
        }
        public async Task<ActionResult> CreateBankCashOutApproval(string referenceId)
        {
            var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(_AccountServices.GetBranchID());
            var datas = await _accountingEntryServices.GetCashReplenimentRequest(referenceId);
            var BankCashOut = new BankCashOut();
             BankCashOut.ReferenceId = referenceId;
            BankCashOut.Amount= datas.AmountApproved;
            ViewBag.Accounts = BuildDropDown(GenerateAccountListView(listOfAccounts));
            return View(new CashDemandDataEntity { BankCashOut = BankCashOut });
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            await GetList();
            return View(new CashDemandDataEntity());
        }
        [HttpGet]
        public async Task<ActionResult> GetAllBranchAccountUsedToCreditCashFlow(string branchId)
        {
            if (!string.IsNullOrEmpty(branchId))
            {
                var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(branchId);

                var data = BuildDropDown(GenerateAccountListView(listOfAccounts));
                //jjjj
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = false, status = false, message = "Fill the required fields." });
            }

      
        }
        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(CashDemandDataEntity model)
        {
            if (model.ServiceOption.Equals("CashInfusionModel"))
            {
                if (model.Action.Equals("insert"))
                {
                    var datac = await _accountingEntryServices.CashReplenishmentRequest(model.CashInfusionModel);

                    return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

                }
                else
                {
                    //update
                    var datac = await _accountingEntryServices.Update(model.CashInfusionModel);
                    return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

                }
            }
            else if (model.ServiceOption.Equals("CashRequestApproval"))
            {

                model.CashReplenimentRequestdto.IsApproved = model.CashReplenimentRequestdto.ApprovedBy == "Approve" ? true :(model.CashReplenimentRequestdto.ApprovedBy == "Redirect-To-Branch") ? true : false;
              
                model.CashReplenimentRequestdto.CashRequisitionType =  (model.CashReplenimentRequestdto.ApprovedBy == "Redirect-To-Branch") ? "ORDER" : "REQUEST";
                model.CashReplenimentRequestdto.ParentCashReplenishId = model.CashReplenimentRequestdto.CashRequisitionType=="ORDER"? model.CashReplenimentRequestdto.Id:"NONE";
                var approval = model.CashReplenimentRequestdto.ConvertToCashApprovalResponse(_AccountServices.GetBranchCode());
                approval .CorrespondingBranchId= (model.CashReplenimentRequestdto.ApprovedBy == "Redirect-To-Branch") ? model.CashReplenimentRequestdto.CorrespondingBranchId: model.CashReplenimentRequestdto.BranchId;
                approval.CashRequisitionType = model.CashReplenimentRequestdto.CashRequisitionType;
                var datac = await _accountingEntryServices.CreateApprovalRequest(approval);

                return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

            }
            else if (model.ServiceOption.Equals("bankCashOutApproval")) //(path == "")
            {
                model.BankCashOut.TransactionType = "CASH OUT";
                model .BankCashOut.Id =  BaseUtilities.GenerateInsuranceUniqueNumber(15, "BCO");
                var datac = await _accountingEntryServices.CreateBankCashTransaction(model.BankCashOut);
                 
                return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

            }
            else
            {
                var datac = await _accountingEntryServices.CashReplenishmentRequest(model.CashInfusionModel);

                return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });


            }

        }
        //GetBankTransactionAccountById

        [HttpGet]
        public async Task<ActionResult> GetBankTransactionByReferenceId(string referenceId)
        {
            if (!string.IsNullOrEmpty(referenceId))
            {
                var CashRepleniment = await _accountingEntryServices.GetCashReplenishmentRequestIdReference(referenceId);
                var BankTransaction = await _accountingEntryServices.GetBankTransactionByReferenceId(referenceId);
                var account = await _AccountServices.GetAccount(BankTransaction.AccountId);
                return Json(new { BankTransaction = BankTransaction, Cashreplenishment = CashRepleniment,Account=account }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = false, status = false, message = "Fill the required fields." });
            }


        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {

            // await GetList();
            if (path == "list")
            {
                List<CashReplenimentRequestDto> cashRepleniments = new List<CashReplenimentRequestDto>();

                var datas = await _accountingEntryServices.GetAllCashReplenimentRequest();
                var dataUser = (await _accountingEntryServices.GetUserList()).ToList();
                var branches = (await branchServices.GetBranches()).ToList();
                var result = from request in datas
                             join user in dataUser on request.IssuedBy equals user.id.ToString()
                             join branch in branches on request.BranchId equals branch.Id
                             select new CashReplenimentRequestDto
                             {
                                 Id = request.Id,
                                 ReferenceId = request.ReferenceId,
                                 AmountRequested = request.AmountRequested,
                                 BranchOffice = branch.Name,
                                 RequestMessage = request.RequestMessage,
                                 IssuedBy = user.name + "," + user.roleName,
                                 IssuedDate = request.IssuedDate,
                                 AmountApproved = request.AmountApproved,
                                 ApprovedBy = request.ApprovedBy,
                                 ApprovedDate = request.ApprovedDate,
                                 IsApproved = request.IsApproved,
                                 CurrencyCode = request.CurrencyCode,
                                 Status = request.Status,
                                 ApprovedMessage = request.ApprovedMessage
                             };
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.ListCashReplenimentRequestDto = result.ToList();


                return PartialView(partialView, cashDemandDataEntity);
            }
            else if (path == "new")
            {
                await GetList();
                return PartialView(partialView, new CashDemandDataEntity { CashReplenimentRequest = new CashReplenimentRequest(), CashInfusionModel = new CashInfusion() });
            }
            else if (path == "update")
            {
                await GetList();
                var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.CashInfusionModel = OperationEventAttribute.ConvertToCashInfusionModel();
                return PartialView(partialView, cashDemandDataEntity);

            }
            else if (path == "status")
            {
                //await GetList();
                var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.CashReplenimentRequestdto = OperationEventAttribute.ConvertToCashReplenimentRequestDto();

                return PartialView(partialView, cashDemandDataEntity);

            }
            else if (path == "complete_status")
            {
                //await GetList();
                var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.CashReplenimentRequestdto = OperationEventAttribute.ConvertToCashReplenimentRequestDto();

                return PartialView(partialView, cashDemandDataEntity);

            }
            else if (path == "approve")
            {
                await GetList();
                var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.CashReplenimentRequestdto = OperationEventAttribute.ConvertToCashReplenimentRequestDto();
                //cashDemandDataEntity.CashReplenimentRequestdto.ApprovedMessage = $"I {_AccountServices.GetUserFullName()} Approved you withdraw XAF {cashDemandDataEntity.CashReplenimentRequestdto.AmountRequested.ToString("N")} from the bank in favour" +
                //                                                                 $" of Vault of {_AccountServices.GetBranchName()}";
                cashDemandDataEntity.CashReplenimentRequestdto.BranchOffice =( await branchServices.GetBranches()).Where(po=>po.Id.Equals(OperationEventAttribute.BranchId)).FirstOrDefault().Name;
              var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(OperationEventAttribute.BranchId);
                ViewBag.Accounts = BuildDropDown(GenerateAccountListView(listOfAccounts));
                ViewBag.Decisions = BuildMenuViewBag();
                return PartialView(partialView, cashDemandDataEntity);

            }
            else if (path == "bankCashOutApproval")
            {
                await GetList();
                var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.BankCashOut = new BankCashOut();
                cashDemandDataEntity.BankCashOut.ReferenceId = OperationEventAttribute.ReferenceId;
                cashDemandDataEntity.BankCashOut.Description = "";
                cashDemandDataEntity.BankCashOut.Amount = OperationEventAttribute.AmountApproved;
                //cashDemandDataEntity.BankCashOut.ValueDate = DateTime.Now.Date;
                //cashDemandDataEntity.CashReplenimentRequestdto.ApprovedMessage = $"I {_AccountServices.GetUserFullName()} Approved you withdraw XAF {cashDemandDataEntity.CashReplenimentRequestdto.AmountRequested.ToString("N")} from the bank in favour" +
                //                                                                 $" of Vault of {_AccountServices.GetBranchName()}";
                var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(OperationEventAttribute.CorrespondingBranchId);
                ViewBag.Accounts = BuildDropDown(GenerateAccountListView(listOfAccounts));

                return PartialView(partialView, cashDemandDataEntity);

            }
            else if (path == "BranchToBranchTransfer")
            {
                await GetList();
                var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.CashReplenimentRequestdto = OperationEventAttribute.ConvertToCashReplenimentRequestDto();
                //cashDemandDataEntity.CashReplenimentRequestdto.ApprovedMessage = $"I {_AccountServices.GetUserFullName()} Approved you withdraw XAF {cashDemandDataEntity.CashReplenimentRequestdto.AmountRequested.ToString("N")} from the bank in favour" +
                //                                                                 $" of Vault of {_AccountServices.GetBranchName()}";
                cashDemandDataEntity.CashReplenimentRequestdto.BranchOffice = (await branchServices.GetBranches()).Where(po => po.Id.Equals(OperationEventAttribute.BranchId)).FirstOrDefault().Name;
                var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(OperationEventAttribute.BranchId);
                ViewBag.Accounts = BuildDropDown(GenerateAccountListView(listOfAccounts));
                ViewBag.Decisions = BuildMenuViewBag();
                return PartialView(partialView, cashDemandDataEntity);

            }

            else
            {
                await GetList();
                var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.CashInfusionModel = OperationEventAttribute.ConvertToCashInfusionModel();
                return PartialView(partialView, cashDemandDataEntity);

            }
        }






        public async Task<ActionResult> CreateCashReplenishmentRequest(CashDemandDataEntity model)
        {
            if (ModelState.IsValid)
            {
                model.CashInfusionModel.ReferenceNumber = model.CashInfusionModel.CurrentOpenOfDayHistoryId;
                var data = await _accountingEntryServices.CashReplenishmentRequest(model.CashInfusionModel);
                if (data.MessageStatus.Equals("Failed"))
                {
                    return View("Failed_Request_View", model.CashInfusionModel.ConvertToCashReplenimentRequest());
                }
                else
                {

                    var datasw = await _accountingEntryServices.GetCashReplenimentReferenceRequest(model.CashInfusionModel.ReferenceNumber);
                    datasw.CurrentOpenOfDayHistoryId = model.CashInfusionModel.CurrentOpenOfDayHistoryId;
                    CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity { CashReplenimentRequest = datasw };
                    return View("Successfull_Request_View", cashDemandDataEntity);
                }

            }

            return View("Failed_Request_View", model.CashInfusionModel.ConvertToCashReplenimentRequest());
        }

        [HttpGet]
        public async Task<ActionResult> GetCashReplenimentRequest(string KEY)
        {
            await GetList();
            var datas = await _accountingEntryServices.GetCashReplenimentRequest(KEY);

            CashReplenimentRequestDto model = datas.ConvertToCashReplenimentRequestDto();
            CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity { CashReplenimentRequestdto = model };
            return View(cashDemandDataEntity);

        }
        [HttpGet]
        public async Task<ActionResult> GetAllCashReplenimentRequestData()
        {
            var datas = await _accountingEntryServices.GetAllCashReplenimentRequest();
            var dataUser = (await _accountingEntryServices.GetUserList()).ToList();
            var result = from request in datas
                         join user in dataUser on request.IssuedBy equals user.id.ToString()
                         select new CashReplenimentRequest
                         {
                             Id = request.Id,
                             ReferenceId = request.ReferenceId,
                             AmountRequested = request.AmountRequested,
                             AmountApproved = request.AmountApproved,
                             RequestMessage = request.RequestMessage,
                             IssuedBy = user.name + "," + user.roleName,
                             IssuedDate = request.IssuedDate,
                             ApprovedBy = request.ApprovedBy,
                             ApprovedDate = request.ApprovedDate,
                             IsApproved = request.IsApproved,
                             CurrencyCode = request.CurrencyCode,
                             Status = request.Status,
                             ApprovedMessage = request.ApprovedMessage
                         };
            CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
            cashDemandDataEntity.ListCashReplenimentRequest = result.ToList();
            return View(cashDemandDataEntity);
        }

        [HttpGet]
        public async Task<ActionResult> GetCashReplenimentRequestDataAwaitingApproval()
        {
            List<CashReplenimentRequestDto> cashReplenimentRequestDtos = new List<CashReplenimentRequestDto>();
            //CreateBankCashOut
            var Id = _AccountServices.GetBranchID();
            var datas = (await _accountingEntryServices.GetAllCashReplenimentRequest()).Where(pi => pi.BranchId.Equals(Id) && pi.CashRequisitionType.Equals(CashRequisitionType.REQUEST.ToString()));

            var dataUser = (await _accountingEntryServices.GetUserList()).ToList();
            var result = from request in datas
                         join user in dataUser on request.ApprovedBy equals user.id.ToString()
                         select new CashReplenimentRequest
                         {
                             Id = request.Id,
                             ReferenceId = request.ReferenceId,
                             AmountRequested = request.AmountRequested,
 
                             ApprovedBy = user.name + "," + user.roleName,

                             AmountApproved = request.AmountApproved,
                             ApprovedDate = request.ApprovedDate,
                             Status = request.Status,
                             CashReplishmentRequestStatus= request.CashReplishmentRequestStatus,
                             ApprovedMessage = request.ApprovedMessage
                         };
            CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
            cashDemandDataEntity.ListCashReplenimentRequest = result.ToList();
            return View(cashDemandDataEntity);
        }

        [HttpGet]
        public async Task<ActionResult> GetCashReplenimentRequestDataAwaitingApprovalCompleted()
        {
            List<CashReplenimentRequestDto> cashReplenimentRequestDtos = new List<CashReplenimentRequestDto>();
            //CreateBankCashOut
            var list = await _accountingEntryServices.GetAllCashRequestApprovalQuery();
            var datas = list.Where(pi => pi.CorrespondingBranchId.Equals(_AccountServices.GetBranchID())&&pi.CashRequisitionType.Equals(CashRequisitionType.ORDER.ToString()));
            var dataUser = (await _accountingEntryServices.GetUserList()).ToList();
            var result = from request in datas
                         join user in dataUser on request.ApprovedBy equals user.id.ToString()
                         select new CashReplenimentRequest
                         {
                             Id = request.Id,
                             ReferenceId = request.ReferenceId,
                             AmountRequested = request.AmountRequested,

                             ApprovedBy = user.name + "," + user.roleName,

                             AmountApproved = request.AmountApproved,
                             ApprovedDate = request.ApprovedDate,
                             Status = request.Status,
                             ApprovedMessage = request.ApprovedMessage
                         };
            CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
            cashDemandDataEntity.ListCashReplenimentRequest = result.ToList();
            return View(cashDemandDataEntity);
        }


        [HttpGet]
        public async Task<ActionResult> CreateApprovalRequest()
        {

            return View();
        }

    }
}