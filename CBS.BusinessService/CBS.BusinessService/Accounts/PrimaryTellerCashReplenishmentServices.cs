using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounts
{
    public class PrimaryTellerCashReplenishmentServices : BaseService
    {
        private readonly ApiCallerHelper _transactionBaseConfigApiHelper;
        private readonly BranchServices _branchServices;
        private readonly UserManagementServices _userManagementServices;
        private readonly TellerServices _tellerServices;

        public PrimaryTellerCashReplenishmentServices(UserManagementServices userManagementServices = null, TellerServices tellerServices = null)
        {
            _transactionBaseConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices = new BranchServices();
            _userManagementServices = userManagementServices;
            _tellerServices = tellerServices;
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objCashReplenishmentPrimaryTeller = await GetCashReplenishmentPrimaryTeller(id);
                var inResponse = await _transactionBaseConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_PrimaryTellerCashReplenishment, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objCashReplenishmentPrimaryTeller.TransactionReference}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objCashReplenishmentPrimaryTeller, false, $"{objCashReplenishmentPrimaryTeller.TransactionReference}", MessagesResults.Failed,
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
        public async Task<IEnumerable<CashReplenishmentPrimaryTeller>> GetAllPendingPrimaryTellerCashReplenishments()
        {
            try
            {
                // Call the API to get all pending cash replenishments
                var couApiResponse = await _transactionBaseConfigApiHelper.GetAsync<ResponseObject<List<CashReplenishmentPrimaryTeller>>>(APICallHelper.GetAllApendingPrimaryTellerCashReplenishment);

                // Check if the API call was successful and data is not null
                if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                {
                    var tellers = await _tellerServices.GetTellers();
                    IEnumerable<Branch> branches;
                    string branchId;

                    // Determine if the user is from the head office or not
                    if (IsHeadOffice())
                    {
                        // If the user is from head office, get all branches
                        branches = await _branchServices.GetBranches();
                    }
                    else
                    {
                        // If the user is not from head office, get the branch of the user
                        branchId = GetBranchID();
                        branches = new List<Branch> { await _branchServices.GetBranch(branchId) };
                    }

                    var cashReplenishmentPrimaryTellers = couApiResponse.ApiResponseData.Data;

                    // Join cash replenishments with branches and tellers, and map them
                    var data = from teller in cashReplenishmentPrimaryTellers
                               join branch in branches on teller.BranchId equals branch.Id
                               join teler in tellers on teller.TellerId equals teler.id
                               select MapCashReplenishmentPrimaryTellerWithBranch(teller, branch, teler);

                    return data;
                }

                // If API call fails or data is null, return an empty enumerable
                return Enumerable.Empty<CashReplenishmentPrimaryTeller>();
            }
            catch (Exception ex)
            {
                // Log and rethrow exception
                // You may want to handle or log the exception more gracefully here
                throw;
            }
        }

        public async Task<IEnumerable<CashReplenishmentPrimaryTeller>> GetCashReplenishmentPrimaryTellers(DateTime dateFrom, DateTime dateTo, string branchId)
        {
            try
            {
                // Create query parameters with dates and branch ID
                var query = new QueryParamWithDates { DateFrom = dateFrom, DateTo = dateTo, BranchId = branchId };

                // Call the API to get cash replenishment data
                var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<List<CashReplenishmentPrimaryTeller>>>(APICallHelper.GetAllPrimaryTellerCashReplenishmentByBranch, query);

                // GetAllowAnonymous tellers and branches
                var tellers = await _tellerServices.GetTellers();
                var branch = await _branchServices.GetBranch(branchId);

                // Check if the API call was successful and data is not null
                if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                {
                    // Extract cash replenishment data
                    var cashReplenishmentPrimaryTellers = couApiResponse.ApiResponseData.Data;

                    // Join cash replenishments with tellers and branches and map them
                    var data = from cashReplenishment in cashReplenishmentPrimaryTellers
                               join teller in tellers on cashReplenishment.TellerId equals teller.id
                               select MapCashReplenishmentPrimaryTellerWithBranch(cashReplenishment, branch, teller);

                    return data;
                }

                // If API call fails or data is null, return an empty enumerable
                return Enumerable.Empty<CashReplenishmentPrimaryTeller>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }



        public async Task<IEnumerable<CashReplenishmentPrimaryTeller>> GetCashReplenishmentPrimaryTellers(DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                List<CashReplenishmentPrimaryTeller> filteredTellers = new List<CashReplenishmentPrimaryTeller>();

                // Check if the user is from the head office
                if (IsHeadOffice())
                {
                    // Retrieve all branches
                    var branches = await _branchServices.GetBranches();

                    // Construct query parameters with date range
                    var query = new QueryParamWithDates { DateFrom = dateFrom, DateTo = dateTo };

                    // Retrieve CashReplenishmentPrimaryTeller data from the API
                    var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<List<CashReplenishmentPrimaryTeller>>>(APICallHelper.GetAllPrimaryTellerCashReplenishment, query);

                    // Check if the API call was successful and data is not null
                    if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                    {
                        // Retrieve all tellers
                        var tellers = await _tellerServices.GetTellers();

                        // Filter and map CashReplenishmentPrimaryTeller data with branches and tellers
                        filteredTellers = (from ctrp in couApiResponse.ApiResponseData.Data
                                           join branch in branches on ctrp.BranchId equals branch.Id
                                           let teller = tellers.FirstOrDefault(t => t.id == ctrp.TellerId)
                                           where teller != null
                                           select MapCashReplenishmentPrimaryTellerWithBranch(ctrp, branch, teller)).ToList();
                    }
                }
                else
                {
                    // Retrieve branch ID
                    var branchId = GetBranchID();

                    // Construct query parameters with date range and branch ID
                    var query = new QueryParamWithDates { DateFrom = dateFrom, DateTo = dateTo, BranchId = branchId };

                    // Retrieve CashReplenishmentPrimaryTeller data by branch from the API
                    var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<List<CashReplenishmentPrimaryTeller>>>(APICallHelper.GetAllPrimaryTellerCashReplenishmentByBranch, query);

                    // Check if the API call was successful and data is not null
                    if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                    {
                        // Retrieve branch information
                        var branch = await _branchServices.GetBranch(branchId);

                        // Retrieve all tellers
                        var tellers = await _tellerServices.GetTellers();

                        // Filter and map CashReplenishmentPrimaryTeller data with branch and tellers
                        filteredTellers = (from ctrp in couApiResponse.ApiResponseData.Data
                                           let teller = tellers.FirstOrDefault(t => t.id == ctrp.TellerId)
                                           where teller != null
                                           select MapCashReplenishmentPrimaryTellerWithBranch(ctrp, branch, teller)).ToList();
                    }
                }

                return filteredTellers;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        private CashReplenishmentPrimaryTeller MapCashReplenishmentPrimaryTellerWithBranch(CashReplenishmentPrimaryTeller cashReplenishmentPrimaryTeller, Branch branch, Teller teller)
        {
            return new CashReplenishmentPrimaryTeller
            {
                Id = cashReplenishmentPrimaryTeller.Id,
                TellerId = cashReplenishmentPrimaryTeller.TellerId,
                Status = cashReplenishmentPrimaryTeller.Status,
                BranchId = cashReplenishmentPrimaryTeller.BranchId,
                RequestedAmount = cashReplenishmentPrimaryTeller.RequestedAmount,
                ConfirmedAmount = cashReplenishmentPrimaryTeller.ConfirmedAmount,
                RequesterUserId = cashReplenishmentPrimaryTeller.RequesterUserId,
                ApprovedBy = cashReplenishmentPrimaryTeller.ApprovedBy,
                ApprovedByUserId = cashReplenishmentPrimaryTeller.ApprovedByUserId,
                ApprovedDate = cashReplenishmentPrimaryTeller.ApprovedDate,
                InitializeDate = cashReplenishmentPrimaryTeller.InitializeDate,
                ApprovedComment = cashReplenishmentPrimaryTeller.ApprovedComment,
                Requetcomment = cashReplenishmentPrimaryTeller.Requetcomment,
                ApprovedStatus = cashReplenishmentPrimaryTeller.ApprovedStatus,
                TransactionReference = cashReplenishmentPrimaryTeller.TransactionReference,
                RequesterName = cashReplenishmentPrimaryTeller.RequesterName,
                Teller = teller,
                Branch = branch, AccountingPostingReference=cashReplenishmentPrimaryTeller.AccountingPostingReference,
                CurrencyNote = cashReplenishmentPrimaryTeller.CurrencyNote
            };
        }




        public async Task<CashReplenishmentPrimaryTeller> GetCashReplenishmentPrimaryTeller(string id)
        {
            try
            {
                var cusResponseObject = await _transactionBaseConfigApiHelper.GetAsync<ResponseObject<CashReplenishmentPrimaryTeller>>(string.Format(APICallHelper.Get_Update_Delete_PrimaryTellerCashReplenishment, id));

                if (cusResponseObject.IsSuccess)
                {
                    var tellers = await _tellerServices.GetTellers();
                    var branch = await _branchServices.GetBranch(cusResponseObject.ApiResponseData.Data.BranchId);
                    var teller = tellers.FirstOrDefault(t => t.id == cusResponseObject.ApiResponseData.Data.TellerId);

                    if (teller != null)
                    {
                        return MapCashReplenishmentPrimaryTellerWithBranch(cusResponseObject.ApiResponseData.Data, branch, teller);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }


        public async Task<ExecutionMessages> Create(CashReplenishmentPrimaryTeller model)
        {
            try
            {
                var response = await _transactionBaseConfigApiHelper.PostAsync<ServiceResponse<CashReplenishmentPrimaryTeller>>(APICallHelper.PrimaryTellerCashReplenishmentRequest, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.RequestedAmount}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.RequestedAmount}", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> ValidateRequest(CashReplenishmentPrimaryTeller model)
        {
            try
            {
                var (isValid, discrepancyMessage) = ValidateDenominations(model.CurrencyNote, model.RequestedAmount);

                if (!isValid)
                {
                    GetExecutionMessages(model, false, $"{model.RequestedAmount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Amount entered must be greater than 0");
                    return ExecutionMessage;
                }
                model.ApprovedStatus = model.Status ? "Approved" : "Pending";
                var cashReplenishmentSubTeller = await GetCashReplenishmentPrimaryTeller(model.Id);

                if (cashReplenishmentSubTeller != null)
                {
                    cashReplenishmentSubTeller.ConfirmedAmount = model.RequestedAmount;
                    cashReplenishmentSubTeller.ApprovedComment = model.ApprovedComment;
                    cashReplenishmentSubTeller.ApprovedStatus = model.ApprovedStatus;
                    cashReplenishmentSubTeller.CurrencyNote = model.CurrencyNote;
                    var response = await _transactionBaseConfigApiHelper.PostAsync<ServiceResponse<CashReplenishmentPrimaryTeller>>(APICallHelper.ValidatePrimaryTellerCashReplenishment, cashReplenishmentSubTeller);

                    if (response.IsSuccess)
                    {
                        // Successful update
                        return GetExecutionMessages(response, true, $"{cashReplenishmentSubTeller.RequesterName}", MessagesResults.Success,
                                                     ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    }
                    else
                    {
                        // Failed update
                        return GetExecutionMessages(model, false, $"{cashReplenishmentSubTeller.RequesterName}", MessagesResults.Failed,
                                                     ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                return GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                                            SystemMessageStatus.Failed.ToString(), ex);
            }

            return ExecutionMessage;
        }
    }

}
