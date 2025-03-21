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
using CBS.FrontDesk.Data.UserManagement;
using DocumentFormat.OpenXml.EMMA;
using Azure.Core;
using CBS.BusinessService.Services;
using CBS.FrontDesk.Data.Entity.CorrespondingBankManaagement;
using Hangfire.Storage.Monitoring;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    [CheckSessionTimeOutAttribute]
    public class CashFlowManagementController : BaseController
    {
        private const string EventCode = "Vault_To_Liaison";
        private readonly AccountingServices _AccountServices;
        private readonly AccountingEntryServices _accountingEntryServices;
        private readonly BranchServices branchServices; 
                    private readonly BankZoneBranchServices _bankZoneBranchServices;
        private readonly AccountingEntryRuleService _Service;
        public CashFlowManagementController()
        {
            _AccountServices = new AccountingServices();
            _accountingEntryServices = new AccountingEntryServices();
            branchServices = new BranchServices();
            _Service = new AccountingEntryRuleService();
            _bankZoneBranchServices = new BankZoneBranchServices();
        }
        // GET: BankingOperation


        public async Task GetList(string language = "En")
        {
            var DebitAccounts = new List<Data.Account>();
            var listBranch = await branchServices.GetBranches();
            ViewBag.Branches = BuildDropDown(GenerateBranchBranchCode(listBranch.ToList()));

            ViewBag.Accounts = BuildDropDown(GenerateAccountListView(DebitAccounts));
          

        }
        private IEnumerable<StringValues> GenerateAccountListView(List<Data.Account> accounts)
        {
            List<StringValues> stringValues = new List<StringValues>();
            foreach (var branch in accounts)
            {

                stringValues.Add(new StringValues(branch.Id, $"{branch.AccountNumber}-{branch.AccountName}- {branch.CurrentBalance:N2} FCFA"));
            }
            return stringValues;
        }
        private IEnumerable<StringValues> GenerateBranchListView(List<Branch> branches)
        {
            List<StringValues> stringValues = new List<StringValues>();
            foreach (var branch in branches)
            {

                stringValues.Add(new StringValues(branch.Id,branch.BranchCode+"-"+ branch.Name));
            }
            return stringValues;
        }
        private IEnumerable<StringValues> GenerateBranchBranchCode(List<Branch> branches)
        {
            List<StringValues> stringValues = new List<StringValues>();
            //var collections = branches.Where(x => x.IsHavingBank == true);
            foreach (var branch in branches)
            {

                stringValues.Add(new StringValues(branch.Id, branch.BranchCode + "-" + branch.Name));
            }
            return stringValues;
        }
        private async Task<IEnumerable<StringValues>> GenerateBranchInZoneCode(List<Branch3ppBranch> branches, string excludeBranchId)
        {
            List<StringValues> stringValues = new List<StringValues>();
            List<Branch> allBranch =( await  branchServices.GetBranches()).ToList();
            var finalCollection = from b in allBranch
                                  join branch in branches on b.Id equals branch.Id
                                  select new Branch3ppBranch
                                  {
                                      Id = branch.Id,
                                      Code = branch.Code,
                                      Name = branch.Name
                                  };
            foreach (var branch in finalCollection)
            {
                if (branch.Id == excludeBranchId)
                    continue;
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
        //AccountPolicySetting
        private dynamic BuildMenuViewBagDeposit(string branchId)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            list.Clear();
            list.Add(new SelectListItem { Text = $"Rejected", Value = "Rejected" });
            list.Add(new SelectListItem { Text = $"RedirectToBranchBTB", Value = "Redirected for inter-branch-transfer" });
            list.Add(new SelectListItem { Text = $"Approved@"+ branchId, Value = "Approve for bank deposit" });

            return list;
        }
        private dynamic BuildMenuViewBag(Branch branch)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            if (branch.IsHavingBank)
            {
                list.Clear();
                list.Add(new SelectListItem { Text = $"Rejected", Value = "Rejected" });
                list.Add(new SelectListItem { Text = $"RedirectToBranchBCO", Value = "Redirected for bank cash out" });
                list.Add(new SelectListItem { Text = $"RedirectToBranchBTB", Value = "Redirected for inter-branch-transfer" });
                list.Add(new SelectListItem { Text = $"Approved", Value = "Approve for bank cash Out" });
            }
            else
            {
                list.Clear();
                list.Add(new SelectListItem { Text = $"Rejected", Value = "Rejected" });
                list.Add(new SelectListItem { Text = $"RedirectToBranchBCO", Value = "Redirected for bank cash out" });
                list.Add(new SelectListItem { Text = $"RedirectToBranchBTB", Value = "Redirected for inter branch transfer" });
            }

           
       

            return list;
        }

        [HttpGet]
        public async Task<ActionResult> CreateCashReplenishmentRequest()
        {
            await GetList();
            if (_AccountServices.IsHeadOffice())
            {
                var model = new CashDemandDataEntity();
                model.CashInfusionModel = new CashInfusion();
                model.CashInfusionModel.RequestMessage = $"I {_AccountServices.GetUserFullName()} i want more cash as soon as possible";
                ViewBag.IsAuthourized = false;
                ViewBag.Error = _AccountServices.GetUserFullName() + ", You are not authourized to perform this cash request kindly contact the system admin for advice";
                return View(model);
            }
            else
            {
                var model = new CashDemandDataEntity();
                model.CashInfusionModel = new CashInfusion();
                model.CashInfusionModel.RequestMessage = $"I {_AccountServices.GetUserFullName()} i want more cash as soon as possible";
                ViewBag.IsAuthourized = true   ;
                return View(model);
            }
           
        }
        [HttpGet]
        public async Task<ActionResult> DepositRequest()
        {
            var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(_AccountServices.GetBranchID());
            var model = new DepositNotification();
            model.HasBankAccount = listOfAccounts.Any();
            ViewBag.Accounts = BuildDropDown(GenerateAccountListView(listOfAccounts));
            ViewBag.Branches = BuildDropDown(GenerateBranchListView((await branchServices.GetBranches()).ToList()));

            return View(new CashDemandDataEntity { DepositNotificationDto = new DepositNotificationDto(), DepositNotification = model });
        }
        [HttpGet]
        public async Task<ActionResult> CashInfusion()
        {
            #region MyRegion
            //var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(cashReplenishmentId);

            ////cashDemandDataEntity.BankCashOut.ValueDate = DateTime.Now.Date;
            ////cashDemandDataEntity.CashReplenimentRequestdto.ApprovedMessage = $"I {_AccountServices.GetUserFullName()} Approved you withdraw XAF {cashDemandDataEntity.CashReplenimentRequestdto.AmountRequested.ToString("N")} from the bank in favour" +
            ////                                                                 $" of Vault of {_AccountServices.GetBranchName()}";
            //if (OperationEventAttribute.CashRequisitionType.Equals("REQUEST"))
            //{
            //    var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(OperationEventAttribute.BranchId);
            //    ViewBag.Accounts = BuildDropDown(GenerateAccountListView(listOfAccounts));
            //}
            //else
            //{
            //    var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(OperationEventAttribute.correpondingBranchId);
            //    ViewBag.Accounts = BuildDropDown(GenerateAccountListView(listOfAccounts));
            //}

            //var account = (await _AccountServices.GetAllAccounting()).Where(pp => pp.AccountNumberCU.EndsWith(_AccountServices.GetBranchCode()) && pp.Account3 == "451" && pp.LiaisonId == _AccountServices.GetBranchID());
            //string Name, balance = string.Empty;
            //string Id = string.Empty;
            //if (account.Any())
            //{
            //    var acc = account.FirstOrDefault();
            //    Name = $"451[***]{_AccountServices.GetBranchCode()}-{acc.AccountName}";
            //    balance = (account.Sum(ff=> Convert.ToDecimal(ff.CurrentBalance))).ToString();
            //    Id = acc.Id;
            //}
            //else
            //{
            //    balance = "0";
            //    Name = $"No Vault Found";
            //}

            //var mOdelsd = new CashClearing();
            ////mOdelsd.ExpectedAmount = Convert.ToDecimal( balance);
            //mOdelsd.AccountInfo = Name;
            //return View(new CashDemandDataEntity { CashClearing = mOdelsd }); 
            #endregion
            List<CashReplenimentRequest> cashReplenimentRequestDtos = new List<CashReplenimentRequest>();
            //CreateBankCashOut
            var Id = _AccountServices.GetBranchID();
            var datas = (await _accountingEntryServices.GetAllCashReplenimentRequest()).Where(pi => pi.BranchId.Equals(Id) && pi.CashRequisitionType.Equals(CashRequisitionType.REQUEST.ToString()) && pi.Status.Equals(CashReplishmentRequestStatus.Awaiting_Branch_CashClearing.ToString()));

            var dataUser = (await _accountingEntryServices.GetUserList()).ToList();
            var result = from request in datas
                         join user in dataUser on request.ApprovedBy equals user.id.ToString()
                         select new CashReplenimentRequest
                         {
                             Id = request.Id,
                             ReferenceId = request.ReferenceId,
                             AmountRequested = request.AmountRequested,

                             TempId2 = user.name + "," + user.roleName,
                             HasAccount56 = false,
                             AmountApproved = request.AmountApproved,
                             ApprovedDate = request.ApprovedDate,
                             Status = request.Status,
                             CashReplishmentRequestStatus = request.CashReplishmentRequestStatus,
                             ApprovedMessage = request.ApprovedMessage
                         };
            CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
            foreach (var item in result.ToList())
            {
                item.HasAccount56 = await CheckIfBranchHasBankAccountAsync(_AccountServices.GetBranchID());
                cashReplenimentRequestDtos.Add(item);
            }
            cashDemandDataEntity.ListCashReplenimentRequest = cashReplenimentRequestDtos;
            return View(cashDemandDataEntity);
        }
        [HttpGet]
        public async Task<ActionResult> CreateBankCashOut000mm()
        {
            await GetList();
            var Accountnumber = "571010" + _AccountServices.GetBranchCode() + "000";
            var account = (await _AccountServices.GetAccountByAccountNumber(Accountnumber));
            string name, balance = string.Empty;
            string Id = string.Empty;
            if (account != null)
            {
                var acc = account;
                name = $"{acc.AccountNumber}-{acc.AccountName}";
                balance = acc.CurrentBalance.ToString();
                Id = acc.Id;
            }
            else
            {
                balance = "0";
                name = $"No Vault Found";
            }
            ViewBag.AccountName = name;
            ViewBag.AccountBalance = balance;
            var listBranch = await branchServices.GetLiaison();
            ViewBag.Liaisons = BuildDropDown(GenerateBranchListView(listBranch.ToList()));
            var mOdelsd = new BranchToBranchTransfer();
            mOdelsd.Balance = balance;
            mOdelsd.Accountinfor = name;
            //mOdelsd.AccountId = Id;
            return View(new CashDemandDataEntity { BranchToBranchTransfer = mOdelsd });
        }

        public async Task<ActionResult> CreateBankCashOut()
        {
            List<CashReplenimentRequest> cashReplenimentRequestDtos = new List<CashReplenimentRequest>();
            //CreateBankCashOut
            var Id = _AccountServices.GetBranchID();
            var datas = (await _accountingEntryServices.GetAllCashReplenimentRequest()).Where(pi => pi.CorrespondingBranchId.Equals(Id) && pi.Status.Equals(CashReplishmentRequestStatus.RedirectToBranchBCO.ToString()));

            var dataUser = (await _accountingEntryServices.GetUserList()).ToList();
            var result = from request in datas
                         join user in dataUser on request.ApprovedBy equals user.id.ToString()
                         select new CashReplenimentRequest
                         {
                             Id = request.Id,
                             ReferenceId = request.ParentCashReplenishId,
                             AmountRequested = request.AmountRequested,

                             TempId2 = user.name + "," + user.phoneNumber,
                             HasAccount56 = false,
                             AmountApproved = request.AmountApproved,
                             ApprovedDate = request.ApprovedDate,
                             Status = request.Status,
                             CashReplishmentRequestStatus = request.CashReplishmentRequestStatus,
                             ApprovedMessage = request.ApprovedMessage
                         };
            CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
            foreach (var item in result.ToList())
            {
                item.HasAccount56 = await CheckIfBranchHasBankAccountAsync(item.BranchId);
                cashReplenimentRequestDtos.Add(item);
            }
            cashDemandDataEntity.ListCashReplenimentRequest = cashReplenimentRequestDtos;
            return View(cashDemandDataEntity);
        }
        public async Task<ActionResult> CreateBankCashOutApproval(string referenceId)
        {
            var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(_AccountServices.GetBranchID());
            var datas = await _accountingEntryServices.GetCashReplenimentRequest(referenceId);
            var BankCashOut = new BankCashOut();
            BankCashOut.ReferenceId = referenceId;
            BankCashOut.Amount = Convert.ToDecimal(datas.AmountApproved);
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
        public async Task<ActionResult> GetAllBranchAccountUsedToCreditCashFlow(string branchId,string optionQuery)
        {

            branchId = (branchId == "Approved") ? _AccountServices.GetBranchID() : branchId;
            if (!string.IsNullOrEmpty(branchId))
            {
                var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(branchId);
                if (optionQuery== "RedirectToBranchBCO")
                {
                    listOfAccounts = listOfAccounts.Where(c => c.Account2 == "56").ToList();
                }
                else if (optionQuery == "RedirectToBranchBTB")
                {
                    listOfAccounts = listOfAccounts.Where(c => c.AccountNumber == "57101").ToList();
                }
             

                var data = BuildDropDown(GenerateAccountListView(listOfAccounts));

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
            else if (model.ServiceOption.Equals("DepositNotification"))
            {

                if (model.Action.Equals("insert"))
                {


                    var datac = await _accountingEntryServices.DepositNotificationRequest(model.DepositNotification);

                    return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

                }
                else
                {
                    //update
                    var datac = await _accountingEntryServices.UpdateDepositNotificationRequest(model.DepositNotification);
                    return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

                }
            }
            else if (model.ServiceOption.Equals("DepositNotificationApproval"))
            {
                model.DepositApproval.Status = model.DepositNotificationDto.ApprovedBy.Split('@')[0];
                model.DepositApproval.CorrepondingBranchId = model.DepositNotificationDto.correpondingBranchId;
                model.DepositApproval.BankAccountId = model.DepositNotificationDto.Temp3;
                model.DepositApproval.ApprovedMessage = model.DepositNotificationDto.ApprovedMessage;
                model.DepositApproval.Id = model.DepositNotificationDto.Id;
                var datac = await _accountingEntryServices.DepositNotificationApprovalRequest(model.DepositApproval);

                return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

            }
            else if (model.ServiceOption.Equals("CashRequestApproval"))
            {
                model.CashReplenimentRequestdto.IsApproved = model.CashReplenimentRequestdto.TempId2.Contains ("RedirectToBranch") ? true : model.CashReplenimentRequestdto.TempId2 == "Approved" ? true : false;
                model.CashReplenimentRequestdto.CashRequisitionType = model.CashReplenimentRequestdto.TempId2.Contains("RedirectToBranch") ? CashRequisitionType.ORDER.ToString() : CashRequisitionType.REQUEST.ToString();
                model.CashReplenimentRequestdto.Status = model.CashReplenimentRequestdto.TempId2;
                model.CashReplenimentRequestdto.CorrespondingBranchId = (model.CashReplenimentRequestdto.CashRequisitionType == CashRequisitionType.ORDER.ToString()) ? model.CashReplenimentRequestdto.CorrespondingBranchId : _accountingEntryServices.GetBranchID();
                model.CashReplenimentRequestdto.TempId3= model.CashReplenimentRequestdto.TempId3;
                var datac = await _accountingEntryServices.CreateApprovalRequest(model.CashReplenimentRequestdto.ConvertToCashApprovalResponse(_AccountServices.GetBranchCode()), model.CashReplenimentRequestdto.BranchId == _AccountServices.GetBranchID());

                    return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });
 
 
            }
            else if (model.ServiceOption.Equals("bankCashOutApproval")) //(path == "")
            {
                model.BankCashOut.TransactionType = "CASH OUT";
                model.BankCashOut.Id = BaseUtilities.GenerateInsuranceUniqueNumber(15, "BCO");
                model.BankCashOut.Balance = (await _AccountServices.GetAccount(model.BankCashOut.FromAccountId)).CurrentBalance.ToString();
        
                var accountList = (await _AccountServices.GetAccountInfoByEventCode(new EventRequest { EventCode = "Bank_To_Transit", ToBranchCode = _AccountServices.GetBranchCode(), ToBranchId = _AccountServices.GetBranchID() }));
                var fromAccount = accountList.Where(x => x.Type.ToLower() == "source").FirstOrDefault();
                var toAccount = accountList.Where(x => x.Type.ToLower() == "destination").FirstOrDefault();
                
                model.BankCashOut.ToAccountId = toAccount.Id;
                var datac = await _accountingEntryServices.CreateBankCashTransaction(model.BankCashOut);
                return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

            }
            else if (model.ServiceOption.Equals("BranchToBranchTransfer")) //(path == "")
            {
                string reference = string.Empty;
                if (model.BranchToBranchTransfer.ConvertToTransferData().Amount==0)
                {
                    return Json(new { success = false, status = "success", message = "You have not computed any denomination" });
                }
                var datac = await _accountingEntryServices.CreateBranchToBranchTransferTransaction(model.BranchToBranchTransfer);

                return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

            }
            else if (model.ServiceOption.Equals("CashClearing")) //(path == "")
            {
                var datac = await _accountingEntryServices.CreateCashClearingTransaction(model.CashClearing);

                return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

            }
            else if (model.ServiceOption.Equals("UploadBankDepositReceipt"))
            {



                var datac = await _accountingEntryServices.UploadBankDepositTransactionReciept(model.UploadBankReciept);

                return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });


            }
            else if (model.ServiceOption.Equals("BankDepositCashClearing"))
            {

                var datac = await _accountingEntryServices.BankDepositCashClearingTransaction(model.BankDepositCashClearing);

                return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

            }
            else
            {
                //var datac = await _accountingEntryServices.CashReplenishmentRequest(model.CashInfusionModel);

                //return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

                var datac = await _accountingEntryServices.UploadBankDepositTransactionReciept(model.UploadBankReciept);

                return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });
            }

        }
        [HttpGet]
        public async Task<ActionResult> GetBankTransactionByReferenceId(string referenceId)
        {
            if (!string.IsNullOrEmpty(referenceId))
            {
                var CashRepleniment = await _accountingEntryServices.GetCashReplenimentRequest(referenceId);
                var BankTransaction = await _accountingEntryServices.GetBankTransactionByReferenceId(CashRepleniment.Id);
                var account = await _AccountServices.GetAccount(BankTransaction.AccountId);
                return Json(new { BankTransaction = BankTransaction, Cashreplenishment = CashRepleniment, Account = account }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, status = false, message = "Fill the required fields." });
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetBankTransactionByDepositId(string referenceId)
        {
            if (!string.IsNullOrEmpty(referenceId))
            {
                var CashRepleniment = await _accountingEntryServices.GetDepositNotificationRequest(referenceId);
                var BankTransaction = await _accountingEntryServices.GetBankTransactionByReferenceId(CashRepleniment.Id);
                var account = await _AccountServices.GetAccount(BankTransaction.AccountId);
                BankTransaction.TransactionType = (await branchServices.GetBranch(BankTransaction.BranchId)).Name;
                return Json(new { BankTransaction = BankTransaction, Cashreplenishment = CashRepleniment, Account = account }, JsonRequestBehavior.AllowGet);
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
                             join userA in dataUser on request.ApprovedBy equals userA.id.ToString()
                             join branch in branches on request.BranchId equals branch.Id
                             select new CashReplenimentRequestDto
                             {
                                 Id = request.Id,
                                 ReferenceId = request.ReferenceId,
                                 IsOwnerOfTheRequest =request.BranchId==_AccountServices.GetBranchID(),
                                 AmountRequested = request.AmountRequested,
                                 IsRedirectedTo = request.CorrespondingBranchId == _AccountServices.GetBranchID(),

                                 BranchOffice = branch.Name,
                                 RequestMessage = request.RequestMessage,
                                 IssuedBy = user.name + "," + user.phoneNumber,
                                 IssuedDate = request.IssuedDate,
                                 AmountApproved = request.AmountApproved,
                                 ApprovedBy = userA.name + "," + userA.phoneNumber,
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
            else if (path == "DepositNotificationlist")
            {
                List<DepositNotificationDto> cashRepleniments = new List<DepositNotificationDto>();

                var datas = await _accountingEntryServices.GetAllDepositNotificationRequest();
                var dataUser = (await _accountingEntryServices.GetUserList()).ToList();
                var branches = (await branchServices.GetBranches()).ToList();
                var result = from request in datas
                             join user in dataUser on request.IssuedBy equals user.id.ToString()
                             join userA in dataUser on request.ApprovedBy equals userA.id.ToString()
                             join branch in branches on request.BranchId equals branch.Id
                             select new DepositNotificationDto
                             {
                                 Id = request.Id,

                                 Amount = request.Amount,
                                 BranchOffice = branch.Name,
                                 Message = request.Message,
                                 Temp1 = user.name + "," + user.phoneNumber,
                                 IssueDate = request.IssueDate,
                                 Temp2 = userA.name + "," + userA.phoneNumber,
                                 ApprovedDate = request.ApprovedDate,
                                 IsApproved = request.IsApproved,

                                 Status = request.Status,
                                 ApprovalKey = request.ApprovalKey
                             };
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.ListDepositNotificationDto = result.ToList();


                return PartialView(partialView, cashDemandDataEntity);
            }
            else if (path == "newDepositNotification")
            {
                var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(_AccountServices.GetBranchID());
                var model = new DepositNotification();
                model.HasBankAccount = listOfAccounts.Any();
                ViewBag.Accounts = BuildDropDown(GenerateAccountListView(listOfAccounts));
                ViewBag.Branches = BuildDropDown(GenerateBranchListView((await branchServices.GetBranches()).ToList()));
                return PartialView(partialView, new CashDemandDataEntity { DepositNotificationDto = new DepositNotificationDto(), DepositNotification = model });
            }
            else if (path == "new")
            {
                await GetList();
                var model = new CashInfusion();
                return PartialView(partialView, new CashDemandDataEntity { CashReplenimentRequest = new CashReplenimentRequest(), CashInfusionModel = model });
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
            else if (path == "approveDepositNotification")
            {
               
            
                     var OperationEventAttribute = await _accountingEntryServices.GetDepositNotificationRequest(KEY);

                ViewBag.DepositDecisions = BuildMenuViewBagDeposit(OperationEventAttribute.BranchId);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.DepositNotificationDto = OperationEventAttribute;
                cashDemandDataEntity.DepositNotificationDto.BranchOffice = (await branchServices.GetBranches()).Where(po => po.Id.Equals(OperationEventAttribute.BranchId)).FirstOrDefault().Name;
                var listBranch = await _bankZoneBranchServices.GetAllBranchPresentInZoneByParticipant(OperationEventAttribute.BranchId, "BRANCH");
                ViewBag.ZoneBranch = BuildDropDown(await GenerateBranchInZoneCode(listBranch, OperationEventAttribute.BranchId));
                if (branchServices.IsHeadOffice()==false)
                {
                    ViewBag.IsAuthourized = false;
                    ViewBag.Error = _AccountServices.GetUserFullName() + ", You are not authourized to perform this transaction kindly contact the " + (await branchServices.GetBranch(OperationEventAttribute.BranchId)).Name;
                    return PartialView(partialView, cashDemandDataEntity);
                }
                ViewBag.IsAuthourized = true;
                return PartialView(partialView, cashDemandDataEntity);

            }
            else if (path == "uploadBankDepositReceipt")
            {

                var OperationEventAttribute = await _accountingEntryServices.GetDepositNotificationRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.DepositNotificationDto = OperationEventAttribute;
                cashDemandDataEntity.UploadBankReciept.Id = OperationEventAttribute.Id;
                cashDemandDataEntity.DepositNotificationDto.BranchOffice = (await branchServices.GetBranches()).Where(po => po.Id.Equals(OperationEventAttribute.BranchId)).FirstOrDefault().Name;
                if (branchServices.GetBranchID() != OperationEventAttribute.BranchId)
                {
                    ViewBag.IsAuthourized = false;
                    ViewBag.Error = _AccountServices.GetUserFullName() + ", You are not authourized to perform this transaction kindly contact the " + (await branchServices.GetBranch(OperationEventAttribute.BranchId)).Name;
                    return PartialView(partialView, cashDemandDataEntity);
                }
                ViewBag.IsAuthourized = true;
                return PartialView(partialView, cashDemandDataEntity);

            }
            else if (path.Contains("approve"))
            {
                await GetList();
                var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.CashReplenimentRequestdto = OperationEventAttribute.ConvertToCashReplenimentRequestDto();
                cashDemandDataEntity.CashReplenimentRequestdto.HasAccount56 = await CheckIfBranchHasBankAccountAsync(_AccountServices.GetBranchID());
                cashDemandDataEntity.CashReplenimentRequestdto.ApprovedMessage = $"I {_AccountServices.GetUserFullName()} Approved you withdraw XAF {cashDemandDataEntity.CashReplenimentRequestdto.AmountRequested.ToString("N")} from the bank in favour" +
                    $" of Vault of {(await branchServices.GetBranch(OperationEventAttribute.BranchId)).Name}";
                var branch = (await branchServices.GetBranches()).Where(po => po.Id.Equals(OperationEventAttribute.BranchId)).FirstOrDefault();
                cashDemandDataEntity.CashReplenimentRequestdto.BranchOffice = branch.Name;
                cashDemandDataEntity.CashReplenimentRequestdto.AmountApproved = OperationEventAttribute.AmountRequested.ToString();
                if (branchServices.IsHeadOffice() == false)
                {
                    ViewBag.IsAuthourized = false;
                    ViewBag.Error = _AccountServices.GetUserFullName() + ", You are not authourized to perform this transaction kindly contact the head office ";
                    return PartialView(partialView, cashDemandDataEntity);
                }
                ViewBag.IsAuthourized = true;
                var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(OperationEventAttribute.BranchId);
                ViewBag.Accounts = BuildDropDown(GenerateAccountListView(listOfAccounts));
                var listBranch = await _bankZoneBranchServices.GetAllBranchPresentInZoneByParticipant(cashDemandDataEntity.CashReplenimentRequestdto.BranchId, "BRANCH");
                ViewBag.ZoneBranch = BuildDropDown(await GenerateBranchInZoneCode(listBranch, cashDemandDataEntity.CashReplenimentRequestdto.BranchId));
                ViewBag.Decisions = BuildMenuViewBag(branch);
                return PartialView(partialView, cashDemandDataEntity);

            }
            else if (path == "bankCashOutApproval")
            {
 

                var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.BankCashOut = new BankCashOut();
                cashDemandDataEntity.BankCashOut.ReferenceId = OperationEventAttribute.Id;
                cashDemandDataEntity.BankCashOut.FromAccountId = OperationEventAttribute.TempData;
   
                cashDemandDataEntity.BankCashOut.Amount =Convert.ToDecimal( OperationEventAttribute.AmountApproved);
                var user = await _accountingEntryServices.GetUser(OperationEventAttribute.ApprovedBy);
                cashDemandDataEntity.BankCashOut.ApprovedBy = $"{user.firstName} {user.lastName}";
                cashDemandDataEntity.BankCashOut.ApprovedDate = OperationEventAttribute.ApprovedDate.ToString();
                cashDemandDataEntity.BankCashOut.Description = $"I {_AccountServices.GetUserFullName()} was authorized to withdraw {(Convert.ToDecimal(OperationEventAttribute.AmountApproved).ToString("N"))} from the bank in favour" +
                $" of Vault of {_AccountServices.GetBranchName()}";
                var destinationBranchInfo = await branchServices.GetBranch(OperationEventAttribute.BranchId);
                var accountList = (await _AccountServices.GetAccountInfoByEventCode(new EventRequest { EventCode = "Transit_To_Vault", ToBranchCode = destinationBranchInfo.BranchCode, ToBranchId = destinationBranchInfo.Id }));
                var accountTo = accountList.Where(x => x.Type.ToLower() == "destination").FirstOrDefault();
                cashDemandDataEntity.BankCashOut.ToAccountId = accountTo.Id;
                if (branchServices.IsHeadOffice())
                {
                    //ViewBag.HasError= true;
                    ViewBag.IsAuthourized = false;
                    ViewBag.Error = _AccountServices.GetUserFullName() + ", You are not authourized to perform this transaction kindly contact the " + (await branchServices.GetBranch(OperationEventAttribute.BranchId)).Name;
                    return PartialView(partialView, cashDemandDataEntity);
                }
                ViewBag.IsAuthourized = true;
                var list = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(OperationEventAttribute.BranchId);
                if (OperationEventAttribute.CashRequisitionType.Equals("REQUEST"))
                {
                    var listOfAccounts = list;
                    ViewBag.Accounts = BuildDropDown(GenerateAccountListView(listOfAccounts));
                }
                else
                {
                    var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(OperationEventAttribute.CorrespondingBranchId);
                    ViewBag.Accounts = BuildDropDown(GenerateAccountListView(listOfAccounts));
                }
                var modsx = await _AccountServices.GetAccount(cashDemandDataEntity.BankCashOut.FromAccountId);
                cashDemandDataEntity.BankCashOut.FromAccountName = $"{modsx.AccountNumberCU} -{modsx.AccountName}:{modsx.CurrentBalance}";

                return PartialView(partialView, cashDemandDataEntity);

            }
            else if (path == "bankDepositCashClearing")
            {

                await GetList();
                var OperationEventAttribute = await _accountingEntryServices.GetDepositNotificationRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                var branch = await branchServices.GetBranch(OperationEventAttribute.BranchId);
                //var liaisonnumber = "451000" + branch.BranchCode + _AccountServices.GetBranchCode();
                //var account = (await _AccountServices.GetAllLiaisonAccount()).Where(pp => pp.AccountNumberCU == liaisonnumber);
                var accountList = (await _AccountServices.GetAccountInfoByEventCode(new EventRequest { EventCode = "Liaison_To_Vault", ToBranchCode = branch.BranchCode, ToBranchId = branch.Id }));
                var SourceBranch = await branchServices.GetBranch(cashDemandDataEntity.CashReplenimentRequestdto.CorrespondingBranchId);
                if (branchServices.GetBranchID() == OperationEventAttribute.BranchId)
                {
                    ViewBag.IsAuthourized = false;
                    ViewBag.Error = _AccountServices.GetUserFullName() + ", You are not authorized to perform this transaction kindly contact the " + (await branchServices.GetBranch(OperationEventAttribute.BranchId)).Name;
                    return PartialView(partialView, cashDemandDataEntity);
                }
                else
                {
                    ViewBag.IsAuthourized = true;
                    var liaisonAccount = accountList.Where(x => x.Type.ToLower() == "source").FirstOrDefault();
                    var sourceAccount = accountList.Where(x => x.Type.ToLower() == "destination").FirstOrDefault();

                    string name, balance = string.Empty;
                    string Id = string.Empty;
                    string IdAcc = string.Empty;
                    if (liaisonAccount != null && sourceAccount != null)
                    {
                        balance = (Convert.ToDecimal(liaisonAccount.CurrentBalance)).ToString();
                        Id = liaisonAccount.Id;
                        name = $"{liaisonAccount.AccountNumber}-{liaisonAccount.AccountName}>>{sourceAccount.AccountNumber}-{sourceAccount.AccountName}";
                        IdAcc = sourceAccount.Id;
                    }
                    else
                    {
                        balance = "0";
                        name = $"No Vault Found";
                        ViewBag.IsSystemError = true;
                        ViewBag.Error = _AccountServices.GetUserFullName() + ", there is no liaison account : 451000 between " + _AccountServices.GetBranchName() + " and " + (await branchServices.GetBranch(OperationEventAttribute.BranchId)).Name + " please kindly contact the head office ";

                    }

                    var mOdelsd = new BankDepositCashClearing();
                    //mOdelsd.AmountExpected = Convert.ToDecimal(balance) > 0 ? Convert.ToDecimal(balance).ToString("N") : $"({(Math.Abs(Convert.ToDecimal(balance))).ToString("N")})";
                    mOdelsd.AccountInfo = name;
                    mOdelsd.Amount = Convert.ToDecimal(OperationEventAttribute.Amount);
                    mOdelsd.Id = KEY;
                    mOdelsd.AmountExpected = (Convert.ToDecimal(OperationEventAttribute.Amount)).ToString("N");
                    mOdelsd.FromAccountId = Id;
                    mOdelsd.ToAccountId = IdAcc;
                    cashDemandDataEntity.BankDepositCashClearing = mOdelsd;
                    cashDemandDataEntity.DepositNotificationDto = OperationEventAttribute;
                    var userx = await _accountingEntryServices.GetUser(cashDemandDataEntity.DepositNotificationDto.ApprovedBy);
                    cashDemandDataEntity.DepositNotificationDto.Temp2 = userx.firstName + " " + userx.lastName + "," + userx.phoneNumber;
                    var entry = await _accountingEntryServices.GetAccountingEntriesByReferceId(OperationEventAttribute.Id);
                    //var user = await _accountingEntryServices.GetUser();
                    mOdelsd.TransferBy = OperationEventAttribute.IssuedBy;
                    mOdelsd.CreatedDate = OperationEventAttribute.ApprovedDate.ToString("dd-MM-yyyy hh:mm:ss");

                    cashDemandDataEntity.BankDepositCashClearing.Description = $"I {_AccountServices.GetUserFullName()} is performing this operation of cash clearing authorized by {cashDemandDataEntity.CashReplenimentRequest.TempId2}";
                    var btbDtoData = await _accountingEntryServices.GetCachedBranchToBranchTransferData(KEY);
                    cashDemandDataEntity.BankDepositCashClearing.CurrencyNotes = btbDtoData.CurrencyNotesRequest;
                }
            
                return PartialView(partialView, cashDemandDataEntity);
            }
            else if (path == "BranchToBranchTransfer")
            {
            
        
                var listBranch = await branchServices.GetLiaison();
                ViewBag.Liaisons = BuildDropDown(GenerateBranchListView(listBranch.ToList()));
                var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
             
                cashDemandDataEntity.CashReplenimentRequestdto = OperationEventAttribute.ConvertToCashReplenimentRequestDto();
                var user = await _accountingEntryServices.GetUser(OperationEventAttribute.ApprovedBy);
                var destinationBranchInfo = listBranch.Where(x => x.Id == cashDemandDataEntity.CashReplenimentRequestdto.BranchId).FirstOrDefault();
                var accountList = (await _AccountServices.GetAccountInfoByEventCode(new EventRequest { EventCode = "Vault_To_Liaison", ToBranchCode = destinationBranchInfo.BranchCode, ToBranchId=destinationBranchInfo.Id }));
                
                cashDemandDataEntity.CashReplenimentRequestdto.ApprovedBy = user.name + "," + user.phoneNumber + " ";
                var DestinationBranch = await branchServices.GetBranch(cashDemandDataEntity.CashReplenimentRequestdto.BranchId);
                cashDemandDataEntity.CashReplenimentRequestdto.BranchOffice = DestinationBranch.Name;
                var SourceBranch = await branchServices.GetBranch(cashDemandDataEntity.CashReplenimentRequestdto.CorrespondingBranchId);
                if (branchServices.IsHeadOffice())
                {
                    ViewBag.IsAuthourized = false;
                    ViewBag.Error = _AccountServices.GetUserFullName() + ", You are not authourized to perform this transaction kindly contact the head office ";
                    return PartialView(partialView, cashDemandDataEntity);
                }

                ViewBag.IsAuthourized = true;


                await GetList();

                string name, balance = string.Empty;
                string Id = "", idB = "";
                var liaisonAccount = accountList.Where(x => x.Type.ToLower() == "destination").FirstOrDefault();
                var sourceAccount = accountList.Where(x => x.Type.ToLower() == "source").FirstOrDefault();
                if (liaisonAccount != null&& sourceAccount!=null)
                {
                  
                    //var AccountnumberCD = $"451000{SourceBranch.BranchCode}{DestinationBranch.BranchCode}";
                    //var accountb = (await _AccountServices.GetAccountByAccountNumber(AccountnumberCD));
      
                    name =$"{sourceAccount.AccountNumber}-{sourceAccount.AccountName}>>{liaisonAccount.AccountNumber}-{liaisonAccount.AccountName}";
                    balance = liaisonAccount.CurrentBalance;
                    Id = liaisonAccount.Id;
                    //idB = accountb.Id;
                    

                }
                else
                {
                    balance = "0";
                    name = $"No Vault Found";
                    ViewBag.IsSystemError = true;
                    ViewBag.Error = _AccountServices.GetUserFullName() + ", there is no vault account : 57101 for "+ _AccountServices.GetBranchName()+ " please kindly contact the head office ";

                }
                var mOdelsd = new BranchToBranchTransfer();
                mOdelsd.Balance = balance.ToString();
                mOdelsd.Accountinfor = name;
                mOdelsd.FromAccountId = sourceAccount.Id;
                mOdelsd.ReferenceId = cashDemandDataEntity.CashReplenimentRequestdto.Id;
                mOdelsd.ToAccountId = liaisonAccount.Id;
                cashDemandDataEntity.BranchToBranchTransfer = mOdelsd;
                cashDemandDataEntity.BranchToBranchTransfer.Description = $"I {_AccountServices.GetUserFullName()} is performing this operation authorized by {cashDemandDataEntity.CashReplenimentRequestdto.ApprovedBy}";
                return PartialView(partialView, cashDemandDataEntity);

            }
            else if (path == "cashInFusion")
            {
                await GetList();
                var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                var branch = await branchServices.GetBranch(OperationEventAttribute.CorrespondingBranchId);
                //var liaisonnumber = "451000" + branch.BranchCode + _AccountServices.GetBranchCode();
                //var account = (await _AccountServices.GetAllLiaisonAccount()).Where(pp => pp.AccountNumberCU == liaisonnumber);
                var accountList = (await _AccountServices.GetAccountInfoByEventCode(new EventRequest { EventCode = "Liaison_To_Vault", ToBranchCode = branch.BranchCode, ToBranchId = branch.Id }));
                var SourceBranch = await branchServices.GetBranch(cashDemandDataEntity.CashReplenimentRequestdto.CorrespondingBranchId);
                if (branchServices.GetBranchID() != OperationEventAttribute.BranchId)
                {
                    ViewBag.IsAuthourized = false;
                    ViewBag.Error = _AccountServices.GetUserFullName() + ", You are not authourized to perform this transaction kindly contact the " + (await branchServices.GetBranch(OperationEventAttribute.BranchId)).Name;
                    return PartialView(partialView, cashDemandDataEntity);
                }
                ViewBag.IsAuthourized = true;
                var liaisonAccount = accountList.Where(x => x.Type.ToLower() == "source").FirstOrDefault();
                var sourceAccount = accountList.Where(x => x.Type.ToLower() == "destination").FirstOrDefault();

                string name, balance = string.Empty;
                string Id = string.Empty;
                string IdAcc = string.Empty;
                if (liaisonAccount != null && sourceAccount != null)
                {
                    balance = (Convert.ToDecimal(liaisonAccount.CurrentBalance)).ToString();
                    Id = liaisonAccount.Id;
                    name = $"{liaisonAccount.AccountNumber}-{liaisonAccount.AccountName}>>{sourceAccount.AccountNumber}-{sourceAccount.AccountName}";
                    IdAcc = sourceAccount.Id;
                }
                else
                {
                    balance = "0";
                    name = $"No Vault Found";
                    ViewBag.IsSystemError = true;
                    ViewBag.Error = _AccountServices.GetUserFullName() + ", there is no liaison account : 451000 between " + _AccountServices.GetBranchName() + " and " + (await branchServices.GetBranch(OperationEventAttribute.BranchId)).Name + " please kindly contact the head office ";

                }

                var mOdelsd = new CashClearing();
                mOdelsd.ExpectedAmount = Convert.ToDecimal(balance) > 0 ? Convert.ToDecimal(balance).ToString("N") : $"({(Math.Abs(Convert.ToDecimal(balance))).ToString("N")})";
                mOdelsd.AccountInfo = name;
                mOdelsd.ReferenceId = KEY;
                mOdelsd.AmountExpected = Convert.ToDecimal(OperationEventAttribute.AmountApproved);
                mOdelsd.FromAccountId = Id;
                mOdelsd.ToAccountId = IdAcc;
                cashDemandDataEntity.CashClearing = mOdelsd;
                cashDemandDataEntity.CashReplenimentRequest = OperationEventAttribute;
                var userx = await _accountingEntryServices.GetUser(cashDemandDataEntity.CashReplenimentRequest.ApprovedBy);
                cashDemandDataEntity.CashReplenimentRequest.TempId2 = userx.firstName + " " + userx.lastName + "," + userx.phoneNumber;
                var entry = await _accountingEntryServices.GetAccountingEntriesByReferceId(OperationEventAttribute.Id);
                var user = await _accountingEntryServices.GetUser(entry.FirstOrDefault().CreatedBy);
                mOdelsd.TransferBy = user.firstName + " " + user.lastName + "," + user.phoneNumber;
                mOdelsd.CreatedDate = entry.FirstOrDefault().CreatedDate;

                cashDemandDataEntity.CashClearing.Description = $"I {_AccountServices.GetUserFullName()} is performing this operation of cash clearing authorized by {cashDemandDataEntity.CashReplenimentRequest.TempId2}";
                var btbDtoData = await _accountingEntryServices.GetCachedBranchToBranchTransferData(KEY);
                cashDemandDataEntity.CashClearing.CurrencyNotes = btbDtoData.CurrencyNotesRequest;
                return PartialView(partialView, cashDemandDataEntity);


            }
            else if (path == "depositRequest")
            {

                var OperationEventAttribute = await _accountingEntryServices.GetDepositNotificationRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.DepositNotificationDto = OperationEventAttribute;
                cashDemandDataEntity.DepositNotificationDto.BranchOffice = (await branchServices.GetBranch(OperationEventAttribute.BranchId)).Name;
                return PartialView(partialView, cashDemandDataEntity);

            }
            else if (path == "cashreplenishmentstatus")
            {

                var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
               
                cashDemandDataEntity.CashReplenimentRequestdto = OperationEventAttribute.ConvertToCashReplenimentRequestDto();
                var user = await _accountingEntryServices.GetUser(cashDemandDataEntity.CashReplenimentRequestdto.IssuedBy);
                cashDemandDataEntity.CashReplenimentRequestdto.TempId1 = user.firstName + " " + user.lastName + "," + user.phoneNumber;
 

                if (OperationEventAttribute.IsApproved)
                {
                    var userc = await _accountingEntryServices.GetUser(cashDemandDataEntity.CashReplenimentRequestdto.ApprovedBy);
                    cashDemandDataEntity.CashReplenimentRequestdto.TempId2 = userc.firstName + " " + userc.lastName + "," + userc.phoneNumber;
                    //var user = await _accountingEntryServices.GetUser(cashDemandDataEntity.DepositNotificationDto.ApprovedBy);
 
                }


                var entry = await _accountingEntryServices.GetAccountingEntriesByReferceId(OperationEventAttribute.Id);
                return PartialView(partialView, cashDemandDataEntity);

            }
            else if (path == "depositRequestStatus")
            {

                var OperationEventAttribute = await _accountingEntryServices.GetDepositNotificationRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();

                cashDemandDataEntity.DepositNotificationDto = OperationEventAttribute;

                cashDemandDataEntity.DepositNotification.Temp1 = cashDemandDataEntity.DepositNotificationDto.IssuedBy;

                if (OperationEventAttribute.IsApproved)
                {

                    var user = await _accountingEntryServices.GetUser(cashDemandDataEntity.DepositNotificationDto.ApprovedBy);
                    cashDemandDataEntity.DepositNotificationDto.ApprovedBy = user.firstName+" "+user.lastName+","+user.phoneNumber;

                }


                var entry = await _accountingEntryServices.GetAccountingEntriesByReferceId(OperationEventAttribute.Id);
                return PartialView(partialView, cashDemandDataEntity);

            }
            else
            {
                return PartialView(partialView);
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
        public async Task<ActionResult> BankDepositTracking()
        {
            IEnumerable<DepositNotificationDto> datas = new List<DepositNotificationDto>();
            List<DepositNotificationDto> DepositNotificationDtos = new List<DepositNotificationDto>();
            //CreateBankCashOut
            var Id = _AccountServices.GetBranchID();
            if (_accountingEntryServices.IsHeadOffice())
            {
                datas = (await _accountingEntryServices.GetAllDepositNotificationRequest());

            }
            else
            {
                datas = (await _accountingEntryServices.GetAllDepositNotificationRequest()).Where(pi => pi.BranchId.Equals(Id) && pi.Status == CashReplishmentRequestStatus.Awaiting_uploaded_bank_deposit_receipt.ToString());


            }

            var dataUser = (await _accountingEntryServices.GetUserList()).ToList();
            var result = from request in datas
                         join user in dataUser on request.ApprovedBy equals user.id.ToString()
                         select new DepositNotificationDto
                         {
                             Id = request.Id,

                             Message = request.Message,
                             IssuedBy = request.IssuedBy,
                             IssueDate = request.IssueDate,
                             ApprovedBy = request.ApprovedBy,
                             IsApproved = request.IsApproved,
                             Amount = request.Amount,
                             ApprovedDate = request.ApprovedDate,
                             Status = request.Status,
                             BranchId = request.BranchId,
                             ApprovedMessage = request.ApprovedMessage
                         };
            CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
            foreach (var item in datas.ToList())
            {
                item.BranchOffice = (await branchServices.GetBranch(item.BranchId)).Name;
                item.HasBankAccount = await CheckIfBranchHasBankAccountAsync(item.BranchId);
                var userx = await _accountingEntryServices.GetUser(item.IssuedBy);
                item.Temp1 = userx.firstName + "," + userx.lastName;
                if (item.IsApproved)
                {
                    var user0x = await _accountingEntryServices.GetUser(item.ApprovedBy);
                    item.Temp2 = user0x.firstName + "," + user0x.lastName;
                }

                DepositNotificationDtos.Add(item);
            }
            cashDemandDataEntity.ListDepositNotificationDto = DepositNotificationDtos;

            ViewBag.IsAuthourized = true;

            return View(cashDemandDataEntity);

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
            var datasList = (await _accountingEntryServices.GetBranches()).ToList();
     
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
                             TempId1 = user.name + "," + user.roleName,
                             IssuedDate = request.IssuedDate,
                             IsOwner = _accountingEntryServices.GetBranchID()== request.BranchId,
                             IsRedirectedTo = _accountingEntryServices.GetBranchID()==request.CorrespondingBranchId,
                             HasAccount56 = datasList.Find(x => x.Id == request.BranchId).IsHavingBank,
                             ApprovedBy = request.ApprovedBy,
                             ApprovedDate = request.ApprovedDate,
                             IsApproved = request.IsApproved,
                             CurrencyCode = request.CurrencyCode,
                             BranchOffice = datasList.Find(x => x.Id == request.BranchId).Name,
                             Status = request.Status,
                             ApprovedMessage = request.ApprovedMessage
                         };
            CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
            cashDemandDataEntity.ListCashReplenimentRequest = result.ToList();
            return View(cashDemandDataEntity);
        }


        [HttpGet]
        public async Task<ActionResult> GetAllDepositRequestData()
        {
            var datasList = (await _accountingEntryServices.GetBranches()).ToList();

            var datas = await _accountingEntryServices.GetAllDepositNotificationRequest();
            var dataUser = (await _accountingEntryServices.GetUserList()).ToList();
            var result = from request in datas
                         join user in dataUser on request.IssuedBy equals user.id.ToString()
                         select new DepositNotificationDto

                         {
                             Id = request.Id,
                     
                             Amount = request.Amount,
                            
                             Message = request.Message,
                             IssuedBy = user.name + "," + user.roleName,
                            IssueDate = request.IssueDate,
                             IsOwner = _accountingEntryServices.GetBranchID() == request.BranchId,
                             HasAccount56 = datasList.Find(x => x.Id == request.BranchId).IsHavingBank,
                             ApprovedBy = request.ApprovedBy,
                             ApprovedDate = request.ApprovedDate,
                             IsApproved = request.IsApproved,
                             //CurrencyCode = request.CurrencyCode,
                             BranchOffice = datasList.Find(x => x.Id == request.BranchId).Name,
                             Status = request.Status,
                             ApprovedMessage = request.ApprovedMessage
                         };
            CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
            cashDemandDataEntity.ListDepositNotificationDto = result.ToList();
            return View(cashDemandDataEntity);
        }

        [HttpGet]
        public async Task<ActionResult> GetCashReplenimentRequestDataAwaitingApproval()
        {
            List<CashReplenimentRequestDto> cashReplenimentRequests = new List<CashReplenimentRequestDto>();
            //CreateBankCashOut
            var Id = _AccountServices.GetBranchID();
            var datas = (await _accountingEntryServices.GetAllCashReplenimentRequest()).Where(xc => xc.CashRequisitionType.Equals(CashRequisitionType.REQUEST.ToString()));

            CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
            foreach (var item in datas.ToList())
            {
                var itemdto = item.ConvertToCashReplenimentRequestDto();
                itemdto.HasAccount56 = await CheckIfBranchHasBankAccountAsync(itemdto.BranchId);
                var userx = await _accountingEntryServices.GetUser(item.IssuedBy);
                itemdto.TempId1 = userx.firstName + "," + userx.lastName;
                itemdto.IsOwnerOfTheRequest = itemdto.BranchId == _AccountServices.GetBranchID();
                if (item.IsApproved)
                {
                    var userxc = await _accountingEntryServices.GetUser(item.ApprovedBy);
                    itemdto.TempId2 = userxc.firstName + "," + userxc.lastName;
                }

                cashReplenimentRequests.Add(itemdto);
            }
            cashDemandDataEntity.ListCashReplenimentRequestDto = cashReplenimentRequests;
            return View(cashDemandDataEntity);
        }


        [HttpGet]
        public async Task<ActionResult> GetCashDepositDataAwaitingApproval()
        {
            List<DepositNotificationDto> datas = new List<DepositNotificationDto>();
            List<DepositNotificationDto> DepositNotificationDtos = new List<DepositNotificationDto>();
            //CreateBankCashOut
            var Id = _AccountServices.GetBranchID();
            if (_accountingEntryServices.IsHeadOffice())
            {
                datas = (await _accountingEntryServices.GetAllDepositNotificationRequest());

            }
            else
            {
                //datas = (await _accountingEntryServices.GetAllDepositNotificationRequest());//.Where(pi => pi.BranchId.Equals(Id)&&pi.Status==CashReplishmentRequestStatus.Pending.ToString());
                //Redirected request 
                datas.AddRange((await _accountingEntryServices.GetAllDepositNotificationRedirectionRequest()).Where(x => x.correpondingBranchId.Equals(_accountingEntryServices.GetBranchID())));


            }

            var dataUser = (await _accountingEntryServices.GetUserList()).ToList();
            var result = from request in datas
                         join user in dataUser on request.ApprovedBy equals user.id.ToString()
                         select new DepositNotificationDto
                         {
                             Id = request.Id,

                             Message = request.Message,
                             IssuedBy = request.IssuedBy,
                             IssueDate = request.IssueDate,
                             ApprovedBy = request.ApprovedBy,
                             IsApproved = request.IsApproved,
                             Amount = request.Amount,
                             ApprovedDate = request.ApprovedDate,
                             Status = request.Status,
                             BranchId = request.BranchId,
                             ApprovedMessage = request.ApprovedMessage
                         };
            CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
            foreach (var item in datas.ToList())
            {
                item.BranchOffice = (await branchServices.GetBranch(item.BranchId)).Name;
                item.HasBankAccount = await CheckIfBranchHasBankAccountAsync(item.BranchId);
                var userx = await _accountingEntryServices.GetUser(item.IssuedBy);
                item.Temp1 = userx.firstName + "," + userx.lastName;
                if (item.IsApproved)
                {
                    var user0x = await _accountingEntryServices.GetUser(item.ApprovedBy);
                    item.Temp2 = user0x.firstName + "," + user0x.lastName;
                }

                DepositNotificationDtos.Add(item);
            }
            cashDemandDataEntity.ListDepositNotificationDto = DepositNotificationDtos;

            ViewBag.IsAuthourized = true;

            return View(cashDemandDataEntity);
        }

        private async Task<bool> CheckIfBranchHasBankAccountAsync(string vBranchId)
        {
            var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(vBranchId);
            return listOfAccounts.Any();
        }

        [HttpGet]
        public async Task<ActionResult> CompleteBankDepositOperation()
        {
            IEnumerable<DepositNotificationDto> datas = new List<DepositNotificationDto>();
            List<DepositNotificationDto> DepositNotificationDtos = new List<DepositNotificationDto>();
            //CreateBankCashOut
            var Id = _AccountServices.GetBranchID();
            if (_accountingEntryServices.IsHeadOffice())
            {
                datas = (await _accountingEntryServices.GetAllDepositNotificationRequest())
                    .Where(x=>x.Status.Trim().Equals(CashReplishmentRequestStatus.Awaiting_uploaded_bank_deposit_receipt));

            }
            else
            {
                datas = (await _accountingEntryServices.GetAllDepositNotificationRequest())
                     .Where(x => x.Status.Trim().Equals(CashReplishmentRequestStatus.Awaiting_uploaded_bank_deposit_receipt.ToString()));


            }

            var dataUser = (await _accountingEntryServices.GetUserList()).ToList();
            var result = from request in datas
                         join user in dataUser on request.ApprovedBy equals user.id.ToString()
                         select new DepositNotificationDto
                         {
                             Id = request.Id,

                             Message = request.Message,
                             IssuedBy = request.IssuedBy,
                             IssueDate = request.IssueDate,
                             ApprovedBy = request.ApprovedBy,
                             IsApproved = request.IsApproved,
                             Amount = request.Amount,
                             ApprovedDate = request.ApprovedDate,
                             Status = request.Status,
                             BranchId = request.BranchId,
                             ApprovedMessage = request.ApprovedMessage
                         };
            CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
            foreach (var item in datas.ToList())
            {
                item.BranchOffice = (await branchServices.GetBranch(item.BranchId)).Name;
                item.HasBankAccount = await CheckIfBranchHasBankAccountAsync(item.BranchId);
                var userx = await _accountingEntryServices.GetUser(item.IssuedBy);
                item.Temp1 = userx.firstName + "," + userx.lastName;
                if (item.IsApproved)
                {
                    var user0x = await _accountingEntryServices.GetUser(item.ApprovedBy);
                    item.Temp2 = user0x.firstName + "," + user0x.lastName;
                }

                DepositNotificationDtos.Add(item);
            }
            cashDemandDataEntity.ListDepositNotificationDto = DepositNotificationDtos;

            ViewBag.IsAuthourized = true;
            return View(cashDemandDataEntity);
        }


        [HttpGet]
        public async Task<ActionResult> CreateApprovalRequest()
        {

            return View();
        }

    }
}