using CBS.BusinessService.Accounting;
using CBS.BusinessService.Config;
using CBS.BusinessService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Data.Entity.LoanConf;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;
using Microsoft.AspNet.SignalR;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    [CheckSessionTimeOutAttribute]

    public class NotificationController : BaseController
    {
        private readonly AccountingServices _AccountServices;
        private readonly AccountingEntryServices _accountingEntryServices;
        private readonly BranchServices branchServices;
        private readonly UserManagementServices _userServices;
        private readonly NotificationServices _notificationService;

        public NotificationController()
        {
            _AccountServices = new AccountingServices();
            _accountingEntryServices = new AccountingEntryServices();
            branchServices = new BranchServices();
            _userServices = new UserManagementServices();
            _notificationService = new NotificationServices();
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
        }
        private readonly IHubContext _hubContext;

     

        [HttpGet]
        public async Task<ActionResult> Index()
        {

            return View(new CashDemandDataEntity { UsersNotifications = await GetUserNotificationRequestFromDB() });
        }
        private List<SelectListItem> BuildDropDown(IEnumerable<StringValues> stringValues)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var item in stringValues)
            {

                list.Add(new SelectListItem { Text = item.Text, Value = item.Value });

            }

            return list;
        }/// <summary>
         /// 
         /// </summary>
         /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetUserNotificationRequest()
        {
           var notifications = await GetUserNotificationRequestFromDB();
            // or await joinedQuery.ToListAsync() if using EF Core
            // If there are new notifications, notify all clients
            if (notifications.Any())
            {
                _hubContext.Clients.All.ReceiveNotification();
            }

            return Json(notifications, JsonRequestBehavior.AllowGet);
         

        }


        public async Task<List<UsersNotification>> GetUserNotificationRequestForAjax()
        {

            var ModelRequest = await _accountingEntryServices.GetUserNotificationRequest();
            var UsersModel = await branchServices.GetBranches();
            var Users = await _userServices.GetUsers();
            var joinedQuery = from notification in ModelRequest
                              join branch in UsersModel on notification.BranchId equals branch.Id into branchJoin
                              from branch in branchJoin.DefaultIfEmpty()
                              join user in Users on notification.UserId equals user.id.ToString() into userJoin
                              from user in userJoin.DefaultIfEmpty()
                              select new UsersNotification
                              {
                                  Id = notification.Id,
                                  Action = notification.Action,
                                  ActionId = notification.ActionId,
                                  ActionUrl = string.Format(notification.ActionUrl, notification.ActionId),
                                  Timestamp = notification.CreatedDate.ToString("G"),
                                  IsActive = notification.IsActive,
                                  IsSeen = notification.IsSeen,
                                  BranchId = notification.BranchId,
                                  UserId = notification.UserId,
                                  BranchName = branch != null ? branch.Name : "Unknown Branch",
                                  UserName = user != null ? user.firstName + " " + user.lastName + "(" + user.phoneNumber + ")" : "Unknown User"
                              };

            var result = joinedQuery.ToList(); // or await joinedQuery.ToListAsync() if using EF Core

            return result;

        }
        public async Task<List<UsersNotification>> GetUserNotificationRequestFromDB()
        {

            var ModelRequest = await _accountingEntryServices.GetUserNotificationRequest();
            var UsersModel = await branchServices.GetBranches();
            var Users = await _userServices.GetUsers();
            var joinedQuery = from notification in ModelRequest
                              join branch in UsersModel on notification.BranchId equals branch.Id into branchJoin
                              from branch in branchJoin.DefaultIfEmpty()
                              join user in Users on notification.UserId equals user.id.ToString() into userJoin
                              from user in userJoin.DefaultIfEmpty()
                              select new UsersNotification
                              {
                                  Id = notification.Id,
                                  Action = notification.Action,
                                  ActionId = notification.ActionId,
                                  ActionUrl = string.Format(notification.ActionUrl, notification.ActionId),
                                  Timestamp = TimeSince(notification.CreatedDate),
                                  IsActive = notification.IsActive,
                                  IsSeen = notification.IsSeen,
                                  BranchId = notification.BranchId,
                                  UserId = notification.UserId,
                                  BranchName = branch != null ? branch.Name : "Unknown Branch",
                                  UserName = user != null ? user.firstName + " " + user.lastName + "(" + user.phoneNumber + ")" : "Unknown User"
                              };

            var result = joinedQuery.ToList(); // or await joinedQuery.ToListAsync() if using EF Core

            return result;

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
        private IEnumerable<StringValues> GenerateBranchListView(List<Data.Entity.Config.Branch> branches)
        {
            List<StringValues> stringValues = new List<StringValues>();
            foreach (var branch in branches)
            {

                stringValues.Add(new StringValues(branch.Id, branch.Name));
            }
            return stringValues;
        }

        public async Task<ActionResult> BankCashOutRequest(string Key, string BranchId)
        {
            CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
            var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(Key);
            if (_AccountServices.GetBranchID() != BranchId)
            {
                ViewBag.IsAuthourized = false;
                ViewBag.Error = _AccountServices.GetUserFullName() + ", You are not authourized to perform this transaction kindly inform branch manager of " + (await branchServices.GetBranch(BranchId)).Name;
                return View(cashDemandDataEntity);
            }
            ViewBag.IsAuthourized = true;
            var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(BranchId);
            var datas = await _accountingEntryServices.GetCashReplenimentRequest(Key);
            var TOAccounts = await _AccountServices.GetAllAccountForABranch(BranchId);
            var model = TOAccounts.Where(x => x.Account5.Equals("57101")).First();
            cashDemandDataEntity.CashReplenimentRequest = OperationEventAttribute;
            cashDemandDataEntity.BankCashOut.ReferenceId = Key;
            cashDemandDataEntity.BankCashOut.Amount = Convert.ToDecimal(datas.AmountApproved);
            cashDemandDataEntity.BankCashOut.ToAccountId = model.Id;
            ViewBag.Accounts = BuildDropDown(GenerateAccountListView(listOfAccounts));
            return View(cashDemandDataEntity);
        }


        private async Task<bool> CheckIfBranchHasBankAccountAsync(string vBranchId)
        {
            var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(vBranchId);
            return listOfAccounts.Any();
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
                                 AmountRequested = request.AmountRequested,
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
                                 IssuedBy = user.name + "," + user.phoneNumber,
                                 IssueDate = request.IssueDate,
                                 ApprovedBy = userA.name + "," + userA.phoneNumber,
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
                await GetList();
                var OperationEventAttribute = await _accountingEntryServices.GetDepositNotificationRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.DepositNotificationDto = OperationEventAttribute;
                cashDemandDataEntity.DepositNotificationDto.BranchOffice = (await branchServices.GetBranches()).Where(po => po.Id.Equals(OperationEventAttribute.BranchId)).FirstOrDefault().Name;
                //var userx = await _accountingEntryServices.GetUser(OperationEventAttribute.IssuedBy);
                //cashDemandDataEntity.CashReplenimentRequest.ApprovedBy = userx.firstName + " " + userx.lastName + "," + userx.phoneNumber;

                return PartialView(partialView, cashDemandDataEntity);

            }
            else if (path == "uploadBankDepositReceipt")
            {

                var OperationEventAttribute = await _accountingEntryServices.GetDepositNotificationRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.DepositNotificationDto = OperationEventAttribute;
                cashDemandDataEntity.UploadBankReciept.Id = OperationEventAttribute.Id;
                cashDemandDataEntity.DepositNotificationDto.BranchOffice = (await branchServices.GetBranches()).Where(po => po.Id.Equals(OperationEventAttribute.BranchId)).FirstOrDefault().Name;
                return PartialView(partialView, cashDemandDataEntity);

            }
            else if (path == ("approve"))
            {
                await GetList();
                var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.CashReplenimentRequestdto = OperationEventAttribute.ConvertToCashReplenimentRequestDto();
                cashDemandDataEntity.CashReplenimentRequestdto.ApprovedMessage = $"I {_AccountServices.GetUserFullName()} Approved you withdraw XAF {cashDemandDataEntity.CashReplenimentRequestdto.AmountRequested.ToString("N")} from the bank in favour" +
                                                                                 $" of Vault of {(await branchServices.GetBranch(OperationEventAttribute.BranchId)).Name}";
                cashDemandDataEntity.CashReplenimentRequestdto.BranchOffice = (await branchServices.GetBranches()).Where(po => po.Id.Equals(OperationEventAttribute.BranchId)).FirstOrDefault().Name;
                //var userx = await _accountingEntryServices.GetUser(OperationEventAttribute.IssuedBy);
                //cashDemandDataEntity.CashReplenimentRequest.ApprovedBy = userx.firstName + " " + userx.lastName + "," + userx.phoneNumber;
                var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(OperationEventAttribute.BranchId);
                ViewBag.Accounts = BuildDropDown(GenerateAccountListView(listOfAccounts));
                ViewBag.Decisions = BuildMenuViewBag();
                return PartialView(partialView, cashDemandDataEntity);

            }
            else if (path == "bankCashOutApproval")
            {
                var AccountnumberX = "571010" + _AccountServices.GetBranchCode() + "000";
                var account = (await _AccountServices.GetAccountByAccountNumber(AccountnumberX));

                var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.BankCashOut = new BankCashOut();
                cashDemandDataEntity.BankCashOut.ReferenceId = OperationEventAttribute.Id;
                cashDemandDataEntity.BankCashOut.ToAccountId = account.Id;
                cashDemandDataEntity.BankCashOut.Amount = Convert.ToDecimal(OperationEventAttribute.AmountApproved);
                var user = await _accountingEntryServices.GetUser(OperationEventAttribute.ApprovedBy);
                cashDemandDataEntity.BankCashOut.ApprovedBy = $"{user.firstName} {user.lastName}";
                cashDemandDataEntity.BankCashOut.ApprovedDate = OperationEventAttribute.ApprovedDate.ToString();
                cashDemandDataEntity.BankCashOut.Description = $"I {_AccountServices.GetUserFullName()} was authorized to withdraw {OperationEventAttribute.AmountApproved} from the bank in favour" +
                $" of Vault of {_AccountServices.GetBranchName()}";

                if (OperationEventAttribute.CashRequisitionType.Equals("REQUEST"))
                {
                    var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(OperationEventAttribute.BranchId);
                    ViewBag.Accounts = BuildDropDown(GenerateAccountListView(listOfAccounts));
                }
                else
                {
                    var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(OperationEventAttribute.CorrespondingBranchId);
                    ViewBag.Accounts = BuildDropDown(GenerateAccountListView(listOfAccounts));
                }


                return PartialView(partialView, cashDemandDataEntity);

            }
            else if (path == "BranchToBranchTransfer")
            {
                var Accountnumber = "571010" + _AccountServices.GetBranchCode() + "000";
                var account = (await _AccountServices.GetAccountByAccountNumber(Accountnumber));


                var listBranch = await branchServices.GetLiaison();
                ViewBag.Liaisons = BuildDropDown(GenerateBranchListView(listBranch.ToList()));

                //return View(new CashDemandDataEntity { BranchToBranchTransfer = mOdelsd });
                await GetList();
                var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.CashReplenimentRequestdto = OperationEventAttribute.ConvertToCashReplenimentRequestDto();
                var user = await _accountingEntryServices.GetUser(OperationEventAttribute.ApprovedBy);
                cashDemandDataEntity.CashReplenimentRequestdto.ApprovedBy = user.name + "," + user.phoneNumber + " ";
                var DestinationBranch = await branchServices.GetBranch(cashDemandDataEntity.CashReplenimentRequestdto.BranchId);
                cashDemandDataEntity.CashReplenimentRequestdto.BranchOffice = DestinationBranch.Name;
                var SourceBranch = await branchServices.GetBranch(cashDemandDataEntity.CashReplenimentRequestdto.CorrespondingBranchId);
                string name, balance = string.Empty;
                string Id = "", idB = "";
                if (account != null)
                {
                    var acc = account;
                    var AccountnumberCD = $"451000{SourceBranch.BranchCode}{DestinationBranch.BranchCode}";
                    var accountb = (await _AccountServices.GetAccountByAccountNumber(AccountnumberCD));
                    name = $"{acc.AccountNumberCU}-{acc.AccountName}>>450000{SourceBranch.BranchCode}{DestinationBranch.BranchCode}-{accountb.AccountName}";
                    balance = acc.CurrentBalance;
                    Id = acc.Id;
                    idB = accountb.Id;


                }
                else
                {
                    balance = "0";
                    name = $"No Vault Found";
                }
                var mOdelsd = new BranchToBranchTransfer();
                mOdelsd.Balance = balance.ToString();
                mOdelsd.Accountinfor = name;
                mOdelsd.FromAccountId = Id;
                mOdelsd.ReferenceId = cashDemandDataEntity.CashReplenimentRequestdto.Id;
                mOdelsd.ToAccountId = idB;
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
                var liaisonnumber = "451000" + branch.BranchCode + _AccountServices.GetBranchCode();
                var account = (await _AccountServices.GetAllLiaisonAccount()).Where(pp => pp.AccountNumberCU == liaisonnumber);
                string name, balance = string.Empty;
                string Id = string.Empty;
                string IdAcc = string.Empty;
                if (account.Any())
                {
                    var acc = account.FirstOrDefault();

                    balance = (account.Sum(ff => Convert.ToDecimal(ff.CurrentBalance))).ToString();
                    Id = acc.Id;
                    var AccountnumberBB = "571010" + _AccountServices.GetBranchCode() + "000";
                    var accountAcc = (await _AccountServices.GetAccountByAccountNumber(AccountnumberBB));
                    if (accountAcc.AccountNumberCU == "" && accountAcc.AccountName == null)
                    {
                        name = $"{acc.AccountNumberCU}:{branch.Name}-{acc.AccountName}>>There is no vault account for {_AccountServices.GetBranchName()}";
                    }
                    else
                    {
                        name = $"{acc.AccountNumberCU}:{branch.Name}-{acc.AccountName}>>{accountAcc.AccountNumberCU}-{accountAcc.AccountName}";
                    }

                    IdAcc = accountAcc.Id;
                }
                else
                {
                    balance = "0";
                    name = $"No Vault Found";
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
                cashDemandDataEntity.CashReplenimentRequest.ApprovedBy = userx.firstName + " " + userx.lastName + "," + userx.phoneNumber;
                var entry = await _accountingEntryServices.GetAccountingEntriesByReferceId(OperationEventAttribute.Id);
                var user = await _accountingEntryServices.GetUser(entry.FirstOrDefault().CreatedBy);
                mOdelsd.TransferBy = user.firstName + " " + user.lastName + "," + user.phoneNumber;
                mOdelsd.CreatedDate = entry.FirstOrDefault().CreatedDate;
                cashDemandDataEntity.CashClearing.Description = $"I {_AccountServices.GetUserFullName()} is performing this operation of cash clearing authorized by {cashDemandDataEntity.CashReplenimentRequest.ApprovedBy}";
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
            else
            {

                var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);
                CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
                cashDemandDataEntity.CashInfusionModel = OperationEventAttribute.ConvertToCashInfusionModel();
                cashDemandDataEntity.CashReplenimentRequestdto = OperationEventAttribute.ConvertToCashReplenimentRequestDto();
                var user = await _accountingEntryServices.GetUser(cashDemandDataEntity.CashReplenimentRequestdto.ApprovedBy);
                cashDemandDataEntity.CashReplenimentRequestdto.ApprovedBy = user.firstName + " " + user.lastName + "," + user.phoneNumber;
                var entry = await _accountingEntryServices.GetAccountingEntriesByReferceId(OperationEventAttribute.ReferenceId);
                return PartialView(partialView, cashDemandDataEntity);

            }
        }


        public async Task<ActionResult> ApproveCashRequest(string KEY)
        {


            await GetList();
            var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);
            CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
            cashDemandDataEntity.CashReplenimentRequestdto = OperationEventAttribute.ConvertToCashReplenimentRequestDto();
            cashDemandDataEntity.CashReplenimentRequestdto.ApprovedMessage = $"I {_AccountServices.GetUserFullName()} Approved you withdraw XAF {cashDemandDataEntity.CashReplenimentRequestdto.AmountRequested.ToString("N")} from the bank in favour" +
                                                                             $" of Vault of {(await branchServices.GetBranch(OperationEventAttribute.BranchId)).Name}";
            cashDemandDataEntity.CashReplenimentRequestdto.BranchOffice = (await branchServices.GetBranches()).Where(po => po.Id.Equals(OperationEventAttribute.BranchId)).FirstOrDefault().Name;
            //var userx = await _accountingEntryServices.GetUser(OperationEventAttribute.IssuedBy);
            //cashDemandDataEntity.CashReplenimentRequest.ApprovedBy = userx.firstName + " " + userx.lastName + "," + userx.phoneNumber;
            var listOfAccounts = await _AccountServices.GetAllBranchAccountUsedToCreditCashFlow(OperationEventAttribute.BranchId);
            if (branchServices.IsHeadOffice() == false)
            {
                ViewBag.IsAuthourized = false;
                ViewBag.Error = _AccountServices.GetUserFullName() + ", You are not authourized to perform this transaction kindly contact the head office ";
                return View(cashDemandDataEntity);
            }
            ViewBag.IsAuthourized = true;
            ViewBag.Accounts = BuildDropDown(GenerateAccountListView(listOfAccounts));

            return View(cashDemandDataEntity);


        }

        public async Task<ActionResult> BranchToBranchTransferRequest(string KEY, string BranchId)
        {
            CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
            var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);

            cashDemandDataEntity.CashReplenimentRequestdto = OperationEventAttribute.ConvertToCashReplenimentRequestDto();
            if (_AccountServices.GetBranchID() != BranchId)
            {
                ViewBag.IsAuthourized = false;
                ViewBag.Error = _AccountServices.GetUserFullName() + ", You are not authourized to perform this transaction kindly inform branch manager of " + (await branchServices.GetBranch(BranchId)).Name;
                return View(cashDemandDataEntity);
            }
            ViewBag.IsAuthourized = true;
            var Accountnumber = "571010" + _AccountServices.GetBranchCode() + "000";
            var account = (await _AccountServices.GetAccountByAccountNumber(Accountnumber));


            var listBranch = await branchServices.GetLiaison();
            ViewBag.Liaisons = BuildDropDown(GenerateBranchListView(listBranch.ToList()));

            //return View(new CashDemandDataEntity { BranchToBranchTransfer = mOdelsd });
            await GetList();

            var user = await _accountingEntryServices.GetUser(OperationEventAttribute.ApprovedBy);
            cashDemandDataEntity.CashReplenimentRequestdto.ApprovedBy = user.name + "," + user.phoneNumber + " ";
            var DestinationBranch = await branchServices.GetBranch(cashDemandDataEntity.CashReplenimentRequestdto.BranchId);
            cashDemandDataEntity.CashReplenimentRequestdto.BranchOffice = DestinationBranch.Name;
            var SourceBranch = await branchServices.GetBranch(BranchId);
            string name, balance = string.Empty;
            string Id = "", idB = "";
            if (account != null)
            {
                var acc = account;
                var AccountnumberCD = $"451000{SourceBranch.BranchCode}{DestinationBranch.BranchCode}";
                var accountb = (await _AccountServices.GetAccountByAccountNumber(AccountnumberCD));
                name = $"{acc.AccountNumberCU}-{acc.AccountName}>>450000{SourceBranch.BranchCode}{DestinationBranch.BranchCode}-{accountb.AccountName}";
                balance = acc.CurrentBalance;
                Id = acc.Id;
                idB = accountb.Id;


            }
            else
            {
                balance = "0";
                name = $"No Vault Found";
            }
            var mOdelsd = new BranchToBranchTransfer();
            mOdelsd.Balance = balance.ToString();
            mOdelsd.Accountinfor = name;
            mOdelsd.FromAccountId = Id;
            mOdelsd.ReferenceId = cashDemandDataEntity.CashReplenimentRequestdto.Id;
            mOdelsd.ToAccountId = idB;
            cashDemandDataEntity.BranchToBranchTransfer = mOdelsd;
            cashDemandDataEntity.BranchToBranchTransfer.Description = $"I {_AccountServices.GetUserFullName()} is performing this operation authorized by {cashDemandDataEntity.CashReplenimentRequestdto.ApprovedBy}";
            return View(cashDemandDataEntity);

            //BranchToBranchTransferRequest
        }


        public async Task<ActionResult> BranchCashClearingRequest(string KEY, string BranchId)
        {



            CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
            var OperationEventAttribute = await _accountingEntryServices.GetCashReplenimentRequest(KEY);

            cashDemandDataEntity.CashReplenimentRequestdto = OperationEventAttribute.ConvertToCashReplenimentRequestDto();
            if (_AccountServices.GetBranchID() != BranchId)
            {
                ViewBag.IsAuthourized = false;
                ViewBag.Error = _AccountServices.GetUserFullName() + ", You are not authourized to perform this transaction kindly inform branch manager of " + (await branchServices.GetBranch(BranchId)).Name;
                return View(cashDemandDataEntity);
            }
            ViewBag.IsAuthourized = true;
            await GetList();

            var branch = await branchServices.GetBranch(OperationEventAttribute.CorrespondingBranchId);
            var liaisonnumber = "451000" + branch.BranchCode + _AccountServices.GetBranchCode();
            var account = (await _AccountServices.GetAllLiaisonAccount()).Where(pp => pp.AccountNumberCU == liaisonnumber);
            string name, balance = string.Empty;
            string Id = string.Empty;
            string IdAcc = string.Empty;
            if (account.Any())
            {
                var acc = account.FirstOrDefault();

                balance = (account.Sum(ff => Convert.ToDecimal(ff.CurrentBalance))).ToString();
                Id = acc.Id;
                var AccountnumberBB = "571010" + _AccountServices.GetBranchCode() + "000";
                var accountAcc = (await _AccountServices.GetAccountByAccountNumber(AccountnumberBB));
                if (accountAcc.AccountNumberCU == "" && accountAcc.AccountName == null)
                {
                    name = $"{acc.AccountNumberCU}:{branch.Name}-{acc.AccountName}>>There is no vault account for {_AccountServices.GetBranchName()}.";
                    ViewBag.AccountNotFound = true;

                    ViewBag.Error = $"There is no vault account for {_AccountServices.GetBranchName()}.Inorder to procceed, Create the a vault account(571010) for {_AccountServices.GetBranchName()}";

                }
                else
                {
                    name = $"{acc.AccountNumberCU}:{branch.Name}-{acc.AccountName}>>{accountAcc.AccountNumberCU}-{accountAcc.AccountName}";
                }

                IdAcc = accountAcc.Id;
            }
            else
            {
                balance = "0";
                name = $"No Vault Found";
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
            cashDemandDataEntity.CashReplenimentRequest.ApprovedBy = userx.firstName + " " + userx.lastName + "," + userx.phoneNumber;
            var entry = await _accountingEntryServices.GetAccountingEntriesByReferceId(OperationEventAttribute.Id);
            var user = await _accountingEntryServices.GetUser(entry.FirstOrDefault().CreatedBy);
            mOdelsd.TransferBy = user.firstName + " " + user.lastName + "," + user.phoneNumber;
            mOdelsd.CreatedDate = entry.FirstOrDefault().CreatedDate;
            cashDemandDataEntity.CashClearing.Description = $"I {_AccountServices.GetUserFullName()} is performing this operation of cash clearing authorized by {cashDemandDataEntity.CashReplenimentRequest.ApprovedBy}";
            return View(cashDemandDataEntity);


        }
        //
        public async Task<ActionResult> BankDepositApprovalRequest(string KEY)
        {
            var listBranch = await branchServices.GetBranches();
            ViewBag.DepositDecisions = BuildMenuViewBagDeposit();
            ViewBag.Branches = BuildDropDown(GenerateBranchListView(listBranch.ToList()));
            var OperationEventAttribute = await _accountingEntryServices.GetDepositNotificationRequest(KEY);
            CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
            cashDemandDataEntity.DepositNotificationDto = OperationEventAttribute;
            cashDemandDataEntity.DepositNotificationDto.BranchOffice = (await branchServices.GetBranches()).Where(po => po.Id.Equals(OperationEventAttribute.BranchId)).FirstOrDefault().Name;
            if (branchServices.IsHeadOffice() == false)
            {
                ViewBag.IsAuthourized = false;
                ViewBag.Error = _AccountServices.GetUserFullName() + ", You are not authourized to perform this transaction kindly contact the " + (await branchServices.GetBranch(OperationEventAttribute.BranchId)).Name;
                return View(cashDemandDataEntity);
            }
            ViewBag.IsAuthourized = true;
            return View(cashDemandDataEntity);


        }

        public async Task<ActionResult> BankDepositCompletion(string KEY)
        {
            var listBranch = await branchServices.GetBranches();
            ViewBag.DepositDecisions = BuildMenuViewBagDeposit();
            ViewBag.Branches = BuildDropDown(GenerateBranchListView(listBranch.ToList()));
            var OperationEventAttribute = await _accountingEntryServices.GetDepositNotificationRequest(KEY);
            CashDemandDataEntity cashDemandDataEntity = new CashDemandDataEntity();
            cashDemandDataEntity.DepositNotificationDto = OperationEventAttribute;
            cashDemandDataEntity.UploadBankReciept.Id = OperationEventAttribute.Id;
            cashDemandDataEntity.DepositNotificationDto.BranchOffice = (await branchServices.GetBranches()).Where(po => po.Id.Equals(OperationEventAttribute.BranchId)).FirstOrDefault().Name;
            if (branchServices.IsHeadOffice())
            {
                ViewBag.IsAuthourized = false;
                ViewBag.Error = _AccountServices.GetUserFullName() + ", You are not authourized to perform this transaction kindly contact the " + (await branchServices.GetBranch(OperationEventAttribute.BranchId)).Name;
                return View(cashDemandDataEntity);
            }
            ViewBag.IsAuthourized = true;
            return View(cashDemandDataEntity);


        }
        public async Task GetList(string language = "En")
        {
            var DebitAccounts = new List<Data.Account>();
            var listBranch = await branchServices.GetBranches();
            ViewBag.Branches = BuildDropDown(GenerateBranchListView(listBranch.ToList()));

            ViewBag.Accounts = BuildDropDown(GenerateAccountListView(DebitAccounts));
            if (branchServices.IsHeadOffice() == false)
            {
                ViewBag.IsAuthourized = false;
                ViewBag.Error = _AccountServices.GetUserFullName() + ", You are not authourized to perform this transaction kindly contact the head office ";


            }
            ViewBag.Decisions = (branchServices.IsHeadOffice()) ? BuildMenuViewBag() : new List<SelectListItem>();
            ViewBag.IsAuthourized = true;
        }
        private dynamic BuildMenuViewBagDeposit()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            list.Add(new SelectListItem { Text = $"Approve", Value = "Approve" });

            list.Add(new SelectListItem { Text = $"Rejected", Value = "Rejected" });

            return list;
        }
        private dynamic BuildMenuViewBag()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            list.Add(new SelectListItem { Text = $"Approve", Value = "Approve Bank Cash Out" });

            list.Add(new SelectListItem { Text = $"Rejected", Value = "Rejected" });

            list.Add(new SelectListItem { Text = $"RedirectToBranch", Value = "Redirect-To-Branch" });
            return list;
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
                model.DepositApproval.IsApproved = model.DepositNotificationDto.ApprovedBy == "Approve" ? true : false;
                model.DepositApproval.BankAccountOwner = model.DepositNotificationDto.CorrespondingBranchId;
                model.DepositApproval.BankAccountId = model.DepositNotificationDto.BankAccountId;
                model.DepositApproval.ApprovedMessage = model.DepositNotificationDto.ApprovedMessage;
                model.DepositApproval.Id = model.DepositNotificationDto.Id;
                var datac = await _accountingEntryServices.DepositNotificationApprovalRequest(model.DepositApproval);

                return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

            }
            else if (model.ServiceOption.Equals("CashRequestApproval"))
            {
                model.CashReplenimentRequestdto.IsApproved = model.CashReplenimentRequestdto.TempId2 == "RedirectToBranch" ? true : model.CashReplenimentRequestdto.TempId2 == "Approve" ? true : false;
                model.CashReplenimentRequestdto.CashRequisitionType = (model.CashReplenimentRequestdto.TempId2 == "RedirectToBranch") ? CashRequisitionType.ORDER.ToString() : CashRequisitionType.REQUEST.ToString();
                model.CashReplenimentRequestdto.Status = model.CashReplenimentRequestdto.TempId2;
                model.CashReplenimentRequestdto.CorrespondingBranchId = (model.CashReplenimentRequestdto.CashRequisitionType == CashRequisitionType.ORDER.ToString()) ? model.CashReplenimentRequestdto.CorrespondingBranchId : _accountingEntryServices.GetBranchID();

                var datac = await _accountingEntryServices.CreateApprovalRequest(model.CashReplenimentRequestdto.ConvertToCashApprovalResponse(_AccountServices.GetBranchCode()), model.CashReplenimentRequestdto.BranchId == _AccountServices.GetBranchID());

                return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });
            }
            else if (model.ServiceOption.Equals("bankCashOutApproval")) //(path == "")
            {
                model.BankCashOut.TransactionType = "CASH OUT";
                model.BankCashOut.Id = BaseUtilities.GenerateInsuranceUniqueNumber(15, "BCO");
                model.BankCashOut.Balance = (await _AccountServices.GetAccount(model.BankCashOut.FromAccountId)).CurrentBalance;
                var datac = await _accountingEntryServices.CreateBankCashTransaction(model.BankCashOut);

                return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

            }
            else if (model.ServiceOption.Equals("BranchToBranchTransfer")) //(path == "")

            {
                string reference = string.Empty;

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
            else
            {
                var datac = await _accountingEntryServices.UploadBankDepositTransactionReciept(model.UploadBankReciept);

                return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });


            }

        }

        public void UpdateTimestamps(List<DateTime> timestamps)
        {
            foreach (var timestamp in timestamps)
            {
                string timeAgo = TimeSince(timestamp);
                Console.WriteLine(timeAgo); // Or update UI element as needed
            }
        }

        public string TimeSince(DateTime date)
        {
            TimeSpan timeDiff = DateTime.Now - date;
            int seconds = (int)timeDiff.TotalSeconds;

            int interval = seconds / 31536000;
            if (interval > 1)
            {
                return $"{interval} years ago";
            }
            interval = seconds / 2592000;
            if (interval > 1)
            {
                return $"{interval} months ago";
            }
            interval = seconds / 86400;
            if (interval > 1)
            {
                return $"{interval} days ago";
            }
            interval = seconds / 3600;
            if (interval > 1)
            {
                return $"{interval} hrs ago";
            }
            interval = seconds / 60;
            if (interval > 1)
            {
                return $"{interval} min ago";
            }
            return $"{Math.Floor(Convert.ToDouble(seconds))} seconds ago";
        }
    }


}