using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CashCeilingManagement;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml;
using Microsoft.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.BusinessService.Accounts
{
    public class DailyTellerServices : BaseService
    {
        private readonly ApiCallerHelper _transactionBaseConfigApiHelper;
        private readonly BranchServices _branchServices;
        private readonly TellerProvissioningServices _tellerProvissioningServices;
        private readonly TellerServices _tellerServices;
        private readonly UserManagementServices _userManagementServices;
        public DailyTellerServices(TellerProvissioningServices tellerProvissioningServices = null, TellerServices tellerServices = null, UserManagementServices userManagementServices = null)
        {
            _transactionBaseConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices = new BranchServices();
            _tellerProvissioningServices = tellerProvissioningServices;
            _tellerServices = tellerServices;
            _userManagementServices = userManagementServices;
        }



        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objDailyTeller = await GetDailyTeller(id);
                var inResponse = await _transactionBaseConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_DailyTeller, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objDailyTeller.UserName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objDailyTeller, false, $"{objDailyTeller.UserName}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        //
        //public async Task<IEnumerable<OpenningAnclClossingTillDto>> TellerOpenningAndClossingOfDay(GetTellerOpenningAndClossingQuery getTellerOpenning)
        //{
        //    try
        //    {
        //        var apiUrl = APICallHelper.TellerOpenningAndClossingQuery;

        //        var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<List<OpenningAnclClossingTillDto>>>(apiUrl, getTellerOpenning);

        //        if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData!=null)
        //        {


        //            if (getTellerOpenning.ByBracnch)
        //            {
        //                var branch = _branchServices.GetBranch(getTellerOpenning.BranchId);
        //                return couApiResponse.ApiResponseData.Data;
        //            }
        //            else
        //            {
        //                var branches = _branchServices.GetBranches();
        //                return couApiResponse.ApiResponseData.Data;
        //            }

        //        }
        //        else
        //        {
        //            return Enumerable.Empty<OpenningAnclClossingTillDto>();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        public async Task<IEnumerable<OpenningAnclClossingTillDto>> TellerOpenningAndClossingOfDay(GetTellerOpenningAndClossingQuery getTellerOpenning)
        {
            try
            {
                var apiUrl = APICallHelper.TellerOpenningAndClossingQuery;
                if (getTellerOpenning.BranchId==null)
                {
                    getTellerOpenning.BranchId = "N/A";
                }

                var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<List<OpenningAnclClossingTillDto>>>(apiUrl, getTellerOpenning);

                if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                {
                    // Initialize branch details
                    var branches = new Dictionary<string, Branch>();

                    // Fetch branch information based on the query
                    if (getTellerOpenning.ByBracnch)
                    {
                        // Fetch details for a single branch
                        var branch = await _branchServices.GetBranch(getTellerOpenning.BranchId);
                        branches[branch.Id] = branch;
                    }
                    else
                    {
                        // Fetch details for all branches
                        var allBranches = await _branchServices.GetBranches();
                        foreach (var branch in allBranches)
                        {
                            branches[branch.Id] = branch;
                        }
                    }

                    var dtos = couApiResponse.ApiResponseData.Data;

                    // Map branch details to each DTO
                    foreach (var dto in dtos)
                    {
                        if (branches.TryGetValue(dto.BranchCode, out var branch))
                        {
                            dto.BranchName = branch.Name;
                            dto.BranchCode = branch.BranchCode;
                            dto.BranchAddress = branch.Address;
                            dto.BranchTelephone = branch.Telephone;
                            dto.HeadOfficeName = branch.Bank.Name;
                            dto.HeadOfficeAddress = branch.Bank.Address;
                            dto.HeadOfficeTelephone = branch.Bank.Telephone;
                            dto.HeadOfficeEmail = branch.Bank.Email;
                            dto.HeadOfficeWebSite = branch.Bank.WebSite;
                            dto.HeadOfficeInitial = branch.Bank.BankInitial;
                            dto.HeadOfficeCode = branch.Bank.BankCode;
                        }
                    }

                    return dtos;
                }
                else
                {
                    return Enumerable.Empty<OpenningAnclClossingTillDto>();
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw;
            }
        }

        public async Task<IEnumerable<TillOpenAndClossingDS>> TillCashStatus(GetTillStatusQuery getTellerOpenning)
        {
            try
            {
                var apiUrl = APICallHelper.GetTillCashStatus;
                if (getTellerOpenning.QueryParameter == null)
                {
                    getTellerOpenning.QueryParameter = "N/A";
                }
                var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<List<TellerProvioningHistory>>>(apiUrl, getTellerOpenning);
                if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                {
                    // Initialize branch details
                    List<Branch> branches = new List<Branch>();

                    // Fetch branch information based on the query
                    if (getTellerOpenning.ByBranch)
                    {
                        // Fetch details for a single branch
                        var branch = await _branchServices.GetBranch(getTellerOpenning.QueryParameter);
                        branches.Add(branch);
                    }
                    else
                    {
                        // Fetch details for all branches
                        var allBranches = await _branchServices.GetBranches();
                        branches = allBranches.ToList();
                    }

                    var tellerProvioningHistories = couApiResponse.ApiResponseData.Data;
                    var tillOpenAndClossingDs = _tellerProvissioningServices.MapToTillOpenAndClossingDS(tellerProvioningHistories, branches);
                    return tillOpenAndClossingDs;
                }
                else
                {
                    HttpContext.Current.Session["ErrorMessage"] = couApiResponse.Message;
                    return Enumerable.Empty<TillOpenAndClossingDS>();
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw;
            }
        }

        public async Task<IEnumerable<StringValues>> LoadDailyUsers()
        {
            try
            {
                var users = await _tellerProvissioningServices.GetUserTellerRoleDropDown();
                return users;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }


        public async Task<IEnumerable<DailyTeller>> GetDailyTellers(string branchId)
        {
            try
            {
                var Tellers = await _tellerServices.GetTellers();

                var query = new QueryParamWithDates { BranchId = branchId };
                var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<List<DailyTeller>>>(APICallHelper.GetAllDailyTellerByBranch, query);

                if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                {
                    var branch = await _branchServices.GetBranch(branchId);
                    var dailyTellers = couApiResponse.ApiResponseData.Data;


                    var filteredTellers = dailyTellers.Where(x => x.BranchId == branchId)
                                                      .Select(dailyTeller => MapDailyTellerWithBranch(dailyTeller, branch, Tellers.Where(t => t.id == dailyTeller.TellerId).FirstOrDefault()));

                    return filteredTellers;
                }


                return Enumerable.Empty<DailyTeller>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }


        public async Task<IEnumerable<DailyTeller>> GetDailyTellers(DateTime DateFrom, DateTime DateTo)
        {
            try
            {
                var Tellers = await _tellerServices.GetTellers();

                if (IsHeadOffice())
                {
                    var query = new QueryParamWithDates { DateFrom = DateFrom, DateTo = DateTo };
                    var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<List<DailyTeller>>>(APICallHelper.GetAllDailyTeller, query);

                    if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                    {
                        var branches = await _branchServices.GetBranches();

                        var dailyTellers = couApiResponse.ApiResponseData.Data;
                        var data = from dailyTeller in dailyTellers
                                   join branch in branches on dailyTeller.BranchId equals branch.Id
                                   join teller in Tellers on dailyTeller.TellerId equals teller.id
                                   select MapDailyTellerWithBranch(dailyTeller, branch, teller);

                        return data;
                    }
                }
                else
                {
                    var query = new QueryParamWithDates { DateFrom = DateFrom, DateTo = DateTo, BranchId = GetBranchID() };
                    var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<List<DailyTeller>>>(APICallHelper.GetAllDailyTellerByBranch, query);

                    if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                    {
                        var branch = await _branchServices.GetBranch(GetBranchID());
                        var dailyTellers = couApiResponse.ApiResponseData.Data;
                        var filteredTellers = dailyTellers.Where(x => x.BranchId == GetBranchID())
                                                          .Select(dailyTeller => MapDailyTellerWithBranch(dailyTeller, branch, Tellers.Where(t => t.id == dailyTeller.TellerId).FirstOrDefault()));

                        return filteredTellers;
                    }
                }

                return Enumerable.Empty<DailyTeller>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<ExportTellerGL>> GetDailyOperation(GetAllTellerOperationsQuery operationsQuery)
        {
            try
            {
                //operationsQuery.TellerId = "N/A";
                operationsQuery.QueryString = operationsQuery.QueryString == null ? "all" : operationsQuery.QueryString;
                operationsQuery.BranchId = operationsQuery.BranchId == null ? "N/A" : operationsQuery.BranchId;
                operationsQuery.IsByBranch = true;
                operationsQuery.IsByDate = true;
                var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<List<TellerOperationGL>>>(APICallHelper.GetTellerDailyOperations, operationsQuery);
                if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                {
                    var GL = couApiResponse.ApiResponseData.Data;
                    var branch = await _branchServices.GetBranch(operationsQuery.BranchId);
                    var exportTellerGLs = MapToExportTellerGL(GL, branch, "N/A", "N/A");

                    return exportTellerGLs;
                }
                return Enumerable.Empty<ExportTellerGL>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public ExportTellerGL MapToExportTellerGL(TellerOperationGL tellerOperationGL, string branchName, string tellerName, string userName)
        {
            return new ExportTellerGL
            {
                Date = tellerOperationGL.Date,
                Naration = tellerOperationGL.Naration,
                BalanceBF = tellerOperationGL.BalanceBF,
                Debit = tellerOperationGL.Debit,
                Credit = tellerOperationGL.Credit,
                Balance = tellerOperationGL.Balance,
                BranchName = branchName,
                TellerName = tellerName,
                AccountNumber=tellerOperationGL.AccountNumber,
                AccountType=tellerOperationGL.AccountType,
                UserName = userName
            };
        }
        private string CleanMemberName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "N/A";

            return name
                .Replace("MobileMoneyMTN", "")
                .Replace("OrangeMoney", "")
                .Trim();
        }

        private string FormatAccountType(string accountType)
        {
            if (string.IsNullOrWhiteSpace(accountType)) return "N/A";

            if (accountType.Contains("MTN"))
                return "MoMo";
            if (accountType.Contains("Orange"))
                return "OM";
            if (accountType.Contains("PreferenceShare"))
                return "P-Share";
            if (accountType.Contains("MemberShare"))
                return "O-Share";
            if (accountType.Contains("Daily"))
                return "DCA";

            return accountType;
        }
        //
        public List<ExportTellerGL> MapToExportTellerGL(List<TellerOperationGL> tellerOperationGLList, Branch branch, string tellerName, string userName)
        {
            List<ExportTellerGL> exportTellerGLList = new List<ExportTellerGL>();

            decimal openingBalance = tellerOperationGLList.FirstOrDefault()?.OpeningBalance ?? 0;
            decimal totalCredit = tellerOperationGLList.Sum(item => item.Credit);
            decimal totalDebit = tellerOperationGLList.Sum(item => item.Debit);
            decimal closingBalance = openingBalance + totalCredit - totalDebit;
            int totalTransactions = tellerOperationGLList.Count;

            var orderedTellerOperations = tellerOperationGLList
                .OrderBy(t => t.EntryDate)
                .ThenBy(t => t.TransactionRef ?? "")
                .ToList();

            foreach (var tellerOperationGL in orderedTellerOperations)
            {
                exportTellerGLList.Add(new ExportTellerGL
                {
                    Date = tellerOperationGL.Date,
                    Naration = tellerOperationGL.Naration ?? "",
                    BalanceBF = tellerOperationGL.BalanceBF,
                    Debit = tellerOperationGL.Debit,
                    Credit = tellerOperationGL.Credit,
                    Balance = tellerOperationGL.Balance,
                    BranchName = branch.Name ?? "N/A",
                    LogoUrl = branch.Bank?.LogoUrl ?? "",
                    TellerName = tellerOperationGL.TellerName ?? "N/A",
                    AccountNumber = tellerOperationGL.AccountNumber ?? "N/A",
                    TransactionType = tellerOperationGL.TransactionType ?? "N/A",
                    Description = tellerOperationGL.Description ?? "",
                    MemberAccountNumber = tellerOperationGL.MemberAccountNumber ?? "N/A",
                    MemberId = tellerOperationGL.MemberId ?? "N/A",
                    UserName = userName ?? "N/A",
                    BranchAddress = branch.Address ?? "",
                    BranchCode = branch.BranchCode ?? "N/A",
                    BranchTel = branch.Telephone ?? "N/A",
                    HeadOffice = branch.Bank?.Name ?? "N/A",
                    DailyReferences = tellerOperationGL.DailyReferences ?? "N/A",

                    // Summary fields
                    OpeningBalance = openingBalance,
                    TotalCredit = totalCredit,
                    TotalDebit = totalDebit,
                    AccountType = FormatAccountType(tellerOperationGL.AccountType),
                    Amount = tellerOperationGL.Amount,
                    BranchId = tellerOperationGL.BranchId ?? "N/A",
                    CashierName = tellerOperationGL.CashierName ?? "N/A",
                    EntryDate = tellerOperationGL.EntryDate,
                    MemberName = CleanMemberName(tellerOperationGL.MemberName),
                    TellerID = tellerOperationGL.TellerID ?? "N/A",
                    TransactionRef = tellerOperationGL.TransactionRef ?? "N/A",
                    TransactionReference = tellerOperationGL.TransactionReference ?? "N/A",
                    ClosingBalance = closingBalance,
                    TotalTransactions = totalTransactions
                });
            }

            return exportTellerGLList;
        }
        private DailyTeller MapDailyTellerWithBranch(DailyTeller dailyTeller, Branch branch, Teller teller)
        {
            return new DailyTeller
            {
                Id = dailyTeller.Id,
                UserId = dailyTeller.UserId,
                UserName = dailyTeller.UserName,
                ProvisionedBy = dailyTeller.ProvisionedBy,
                TellerId = dailyTeller.TellerId,
                Status = dailyTeller.Status,
                IsPrimary = dailyTeller.IsPrimary,
                BranchId = dailyTeller.BranchId,
                MaximumWithdrawalAmount = dailyTeller.MaximumWithdrawalAmount,
                MaximumCeilin = dailyTeller.MaximumCeilin,
                Teller = teller,
                Branch = branch,
                PrimaryTellerProvisioningHistories = dailyTeller.PrimaryTellerProvisioningHistories,
                SubTellerProvioningHistories = dailyTeller.SubTellerProvioningHistories
            };
        }


        public async Task<DailyTeller> GetDailyTeller(string id)
        {
            try
            {
                var cusResponseObject = await _transactionBaseConfigApiHelper.GetAsync<ResponseObject<DailyTeller>>(string.Format(APICallHelper.Get_Update_Delete_DailyTeller, id));
                if (cusResponseObject.IsSuccess)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<DailyTeller> GetDailyTellerUser(string tellerType)
        {
            try
            {
                GetDailyTellerByUserIdQuery allCashCeilingRequestsQuery = new GetDailyTellerByUserIdQuery { TellerType=tellerType, UserId=GetUserID() };
                var queryString = ToQueryString(allCashCeilingRequestsQuery);
                var fullUrl = $"{APICallHelper.GetDailyTellerUser}?{queryString}";
                var response = await _transactionBaseConfigApiHelper.GetAsync<ResponseObject<DailyTeller>>(fullUrl);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ExecutionMessages> Create(DailyTeller model)
        {
            try
            {

                string[] parts = model.UserId.Split('@');
                model.UserId = parts[0];
                model.UserName = parts[1];
                model.ProvisionedBy = GetUserFullName();
                var user = await _userManagementServices.GetUser(model.UserId);
                model.UserBranchId = user.BranchID;
                // Make an API call to create an individual profile
                var response = await _transactionBaseConfigApiHelper.PostAsync<ServiceResponse<DailyTeller>>(APICallHelper.CreateDailyTeller, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.UserName}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.UserName}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> Update(DailyTeller model)
        {
            try
            {
                string[] parts = model.UserId.Split('@');
                model.UserId = parts[0];
                model.UserName = parts[1];
                var DailyTeller = await GetDailyTeller(model.Id);
                if (DailyTeller != null)
                {
                    var user = await _userManagementServices.GetUser(model.UserId);
                    model.UserBranchId = user.BranchID;
                    //DailyTeller.IsPrimary = model.IsPrimary;
                    //DailyTeller.MaximumCeilin = model.MaximumCeilin;
                    //DailyTeller.MaximumWithdrawalAmount = model.MaximumWithdrawalAmount;
                    DailyTeller.ProvisionedBy = GetUserFullName();
                    DailyTeller.Status = model.Status;
                    //DailyTeller.TellerId = model.TellerId;
                    DailyTeller.UserId = model.UserId;
                    DailyTeller.UserBranchId = model.UserBranchId;
                    //DailyTeller.BranchId = model.BranchId;
                    DailyTeller.UserName = model.UserName;
                    var response = await _transactionBaseConfigApiHelper.PutAsync<ServiceResponse<DailyTeller>>(string.Format(APICallHelper.Get_Update_Delete_DailyTeller, model.Id), DailyTeller);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{DailyTeller.UserName}", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{DailyTeller.UserName}", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }

            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }

    }

}
