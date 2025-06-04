using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
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
    public class SubTellerCashReplenishmentServices : BaseService
    {
        private readonly ApiCallerHelper _transactionBaseConfigApiHelper;
        private readonly BranchServices _branchServices;
        public SubTellerCashReplenishmentServices()
        {
            _transactionBaseConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices = new BranchServices();
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objCashReplenishmentSubTeller = await GetCashReplenishmentSubTeller(id);
                var inResponse = await _transactionBaseConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_SubTellerCashReplenishment, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objCashReplenishmentSubTeller.RequesterUserId}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objCashReplenishmentSubTeller, false, $"{objCashReplenishmentSubTeller.RequesterUserId}", MessagesResults.Failed,
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

        //public async Task<IEnumerable<StringValues>> GetCashReplenishmentSubTellers()
        //{
        //    try
        //    {
        //        var users = await _userManagementServices.GetUserTellerRoleDropDown();
        //        return users;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log and handle exception
        //        throw;
        //    }
        //}

        //GetAllApendingSubTellerCashReplenishment
        public async Task<IEnumerable<CashReplenishmentSubTeller>> GetAllApendingSubTellerCashReplenishments()
        {
            try
            {
                // Call the API to get all pending cash replenishments
                var couApiResponse = await _transactionBaseConfigApiHelper.GetAsync<ResponseObject<List<CashReplenishmentSubTeller>>>(APICallHelper.GetAllApendingSubTellerCashReplenishment);

                // Check if the API call was successful and data is not null
                if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                {
                    if (IsHeadOffice())
                    {
                        // If the user is from head office, get all branches
                        var branches = await _branchServices.GetBranches();
                        var cashReplenishmentPrimaryTellers = couApiResponse.ApiResponseData.Data;

                        // Join cash replenishments with branches and map them
                        var data = from teller in cashReplenishmentPrimaryTellers
                                   join branch in branches on teller.BranchId equals branch.Id
                                   select MapCashReplenishmentSubTellerWithBranch(teller, branch);

                        return data;
                    }
                    else
                    {
                        // If the user is not from head office, get the branch of the user
                        var branch = await _branchServices.GetBranch(GetBranchID());
                        var cashReplenishmentPrimaryTellers = couApiResponse.ApiResponseData.Data;

                        // Filter cash replenishments for the user's branch and map them
                        var filteredTellers = cashReplenishmentPrimaryTellers
                            .Where(x => x.BranchId == branch.Id)
                            .Select(teller => MapCashReplenishmentSubTellerWithBranch(teller, branch));

                        return filteredTellers;
                    }
                }

                // If API call fails or data is null, return an empty enumerable
                return Enumerable.Empty<CashReplenishmentSubTeller>();
            }
            catch (Exception ex)
            {
                // Log and rethrow exception
                // You may want to handle or log the exception more gracefully here
                throw;
            }
        }

        public async Task<IEnumerable<CashReplenishmentSubTeller>> GetCashReplenishmentSubTellers(DateTime DateFrom, DateTime DateTo, string branchId)
        {
            try
            {
                var query = new QueryParamWithDates { DateFrom = DateFrom, DateTo = DateTo, BranchId = branchId };
                var queryString = ToQueryString(query);
                var fullUrl = $"{APICallHelper.GetAllSubTellerCashReplenishmentByBranch}?{queryString}";
                var couApiResponse = await _transactionBaseConfigApiHelper.GetAsync<ResponseObject<List<CashReplenishmentSubTeller>>>(fullUrl);

                if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                {
                    var branch = await _branchServices.GetBranch(branchId);
                    var CashReplenishmentSubTellers = couApiResponse.ApiResponseData.Data;
                    var filteredTellers = CashReplenishmentSubTellers.Where(x => x.BranchId == GetBranchID())
                                                      .Select(CashReplenishmentSubTeller => MapCashReplenishmentSubTellerWithBranch(CashReplenishmentSubTeller, branch));

                    return filteredTellers;
                }


                return Enumerable.Empty<CashReplenishmentSubTeller>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        //SubTellerCashReplenishment/Request/Pending
        public async Task<IEnumerable<CashReplenishmentSubTeller>> GetCashReplenishmentSubTellers(DateTime DateFrom, DateTime DateTo)
        {
            try
            {

                if (IsHeadOffice())
                {
                    var query = new QueryParamWithDates { DateFrom = DateFrom, DateTo = DateTo };
                    var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<List<CashReplenishmentSubTeller>>>(APICallHelper.GetAllSubTellerCashReplenishment, query);
                    if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                    {
                        var branches = await _branchServices.GetBranches();

                        var CashReplenishmentSubTellers = couApiResponse.ApiResponseData.Data;
                        var data = from CashReplenishmentSubTeller in CashReplenishmentSubTellers
                                   join branch in branches on CashReplenishmentSubTeller.BranchId equals branch.Id
                                   select MapCashReplenishmentSubTellerWithBranch(CashReplenishmentSubTeller, branch);

                        return data;
                    }
                }
                else
                {
                    var query = new QueryParamWithDates { DateFrom = DateFrom, DateTo = DateTo, BranchId = GetBranchID() };

                    var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<List<CashReplenishmentSubTeller>>>(APICallHelper.GetAllSubTellerCashReplenishmentByBranch, query);

                    if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                    {
                        var branch = await _branchServices.GetBranch(GetBranchID());
                        var CashReplenishmentSubTellers = couApiResponse.ApiResponseData.Data;
                        var filteredTellers = CashReplenishmentSubTellers.Where(x => x.BranchId == GetBranchID())
                                                          .Select(CashReplenishmentSubTeller => MapCashReplenishmentSubTellerWithBranch(CashReplenishmentSubTeller, branch));

                        return filteredTellers;
                    }
                }

                return Enumerable.Empty<CashReplenishmentSubTeller>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        private CashReplenishmentSubTeller MapCashReplenishmentSubTellerWithBranch(CashReplenishmentSubTeller cashReplenishmentSubTeller, Branch branch)
        {
            return new CashReplenishmentSubTeller
            {
                Id = cashReplenishmentSubTeller.Id,
                RequestedAmount = cashReplenishmentSubTeller.RequestedAmount,
                ConfirmedAmount = cashReplenishmentSubTeller.ConfirmedAmount,
                RequesterUserId = cashReplenishmentSubTeller.RequesterUserId,
                ApprovedBy = cashReplenishmentSubTeller.ApprovedBy,
                ApprovedByUserId = cashReplenishmentSubTeller.ApprovedByUserId,
                ApprovedDate = cashReplenishmentSubTeller.ApprovedDate,
                InitializeDate = cashReplenishmentSubTeller.InitializeDate,
                ApprovedComment = cashReplenishmentSubTeller.ApprovedComment,
                Requetcomment = cashReplenishmentSubTeller.Requetcomment,
                ApprovedStatus = cashReplenishmentSubTeller.ApprovedStatus,
                TellerId = cashReplenishmentSubTeller.TellerId,
                BranchId = cashReplenishmentSubTeller.BranchId,
                RequesterName = cashReplenishmentSubTeller.RequesterName,
                Approved = cashReplenishmentSubTeller.ApprovedStatus == "Approved" ? true : false,
                Branch = branch,
                TransactionReference = cashReplenishmentSubTeller.TransactionReference
            };
        }



        public async Task<CashReplenishmentSubTeller> GetCashReplenishmentSubTeller(string id)
        {
            try
            {
                var cusResponseObject = await _transactionBaseConfigApiHelper.GetAsync<ResponseObject<CashReplenishmentSubTeller>>(string.Format(APICallHelper.Get_Update_Delete_SubTellerCashReplenishment, id));
                if (cusResponseObject.IsSuccess)
                {
                    var branch = await _branchServices.GetBranch(cusResponseObject.ApiResponseData.Data.BranchId);
                    var CashReplenishmentPrimaryTeller = MapCashReplenishmentSubTellerWithBranch(cusResponseObject.ApiResponseData.Data, branch);
                    return CashReplenishmentPrimaryTeller;

                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ExecutionMessages> Create(CashReplenishmentSubTeller model)
        {
            try
            {
                model.RequesterUserId = GetUserID();
                model.RequesterName = GetUserFullName();
                var response = await _transactionBaseConfigApiHelper.PostAsync<ServiceResponse<CashReplenishmentSubTeller>>(APICallHelper.CreateSubTellerCashReplenishment, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.RequesterName}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.RequesterName}", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> ValidateRequest(CashReplenishmentSubTeller model)
        {
            try
            {

                var (isValid, discrepancyMessage) = ValidateDenominations(model.CurrencyNotes, model.RequestedAmount);

                if (!isValid)
                {
                    GetExecutionMessages(model, false, $"{model.RequestedAmount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Amount entered must be greater than 0");
                    return ExecutionMessage;
                }

                model.ApprovedStatus = model.Approved ? "Approved" : "Pending";
                var cashReplenishmentSubTeller = await GetCashReplenishmentSubTeller(model.Id);

                if (cashReplenishmentSubTeller != null)
                {
                    cashReplenishmentSubTeller.ConfirmedAmount = model.RequestedAmount;
                    cashReplenishmentSubTeller.ApprovedComment = model.ApprovedComment;
                    cashReplenishmentSubTeller.ApprovedStatus = model.ApprovedStatus;
                    cashReplenishmentSubTeller.CurrencyNotes = model.CurrencyNotes;
                    var response = await _transactionBaseConfigApiHelper.PutAsync<ServiceResponse<CashReplenishmentSubTeller>>(string.Format(APICallHelper.ValidateSubTellerCashReplenishment, model.Id), cashReplenishmentSubTeller);

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
