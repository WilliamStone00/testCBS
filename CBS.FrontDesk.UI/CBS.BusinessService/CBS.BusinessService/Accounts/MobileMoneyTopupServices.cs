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
    public class MobileMoneyTopupServices : BaseService
    {
        private readonly ApiCallerHelper _transactionBaseConfigApiHelper;
        private readonly BranchServices _branchServices;
        public MobileMoneyTopupServices()
        {
            _transactionBaseConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices = new BranchServices();
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var inResponse = await _transactionBaseConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.DeleteMobileMoneyCashTopup, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{id}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(null, false, $"{id}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }


        public async Task<IEnumerable<MobileMoneyCashTopup>> GetMobileMoneyCashTopups(GetAllMobileMoneyCashTopupQuery getAllMobileMoney)
        {
            try
            {
                // Call the API to get all pending cash replenishments
                var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<List<MobileMoneyCashTopup>>>(APICallHelper.GetAllMobileMoneyCashTopup, getAllMobileMoney);
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
                                   select MapMobileMoneyCashTopupWithBranch(teller, branch);

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
                            .Select(teller => MapMobileMoneyCashTopupWithBranch(teller, branch));

                        return filteredTellers;
                    }
                }

                // If API call fails or data is null, return an empty enumerable
                return Enumerable.Empty<MobileMoneyCashTopup>();
            }
            catch (Exception ex)
            {
                // Log and rethrow exception
                // You may want to handle or log the exception more gracefully here
                throw;
            }
        }



        private MobileMoneyCashTopup MapMobileMoneyCashTopupWithBranch(MobileMoneyCashTopup mobileMoneyCashTopup, Branch branch)
        {
            return new MobileMoneyCashTopup
            {
                Id = mobileMoneyCashTopup.Id,
                Amount = mobileMoneyCashTopup.Amount,  // Updated from RequestedAmount to Amount
                OperatorType = mobileMoneyCashTopup.OperatorType,
                SourceType = mobileMoneyCashTopup.SourceType,
                RequestDate = mobileMoneyCashTopup.RequestDate,
                BranchId = mobileMoneyCashTopup.BranchId,
                BankId = mobileMoneyCashTopup.BankId,
                RequestNote = mobileMoneyCashTopup.RequestNote,
                RequestInitiatedBy = mobileMoneyCashTopup.RequestInitiatedBy,
                RequestApprovalDate = mobileMoneyCashTopup.RequestApprovalDate,
                RequestApprovedBy = mobileMoneyCashTopup.RequestApprovedBy,
                RequestApprovalStatus = mobileMoneyCashTopup.RequestApprovalStatus,
                RequestApprovalNote = mobileMoneyCashTopup.RequestApprovalNote,
                RequestReference = mobileMoneyCashTopup.RequestReference,
                MobileMoneyTransactionId = mobileMoneyCashTopup.MobileMoneyTransactionId,
                MobileMoneyMemberReference = mobileMoneyCashTopup.MobileMoneyMemberReference,
                AccountNumber = mobileMoneyCashTopup.AccountNumber,
                PhoneNumber = mobileMoneyCashTopup.PhoneNumber,
                TellerId = mobileMoneyCashTopup.TellerId,
                Branch = branch,
                BranchName = branch.Name,
                StrRequestApprovalDate = GetDateFormat(mobileMoneyCashTopup.RequestApprovalDate),
                StrRequestDate = GetDateFormat(mobileMoneyCashTopup.RequestDate),
                Teller = mobileMoneyCashTopup.Teller,
                ValidateMobileMoneyCashTopup = new ValidateMobileMoneyCashTopup { Id = mobileMoneyCashTopup.Id }
            };
        }



        public async Task<MobileMoneyCashTopup> GetMobileMoneyCashTopup(string id)
        {
            try
            {
                var cusResponseObject = await _transactionBaseConfigApiHelper.GetAsync<ResponseObject<MobileMoneyCashTopup>>(string.Format(APICallHelper.GetMobileMoneyCashTopup, id));
                if (cusResponseObject.IsSuccess)
                {
                    var branch = await _branchServices.GetBranch(cusResponseObject.ApiResponseData.Data.BranchId);
                    var CashReplenishmentPrimaryTeller = MapMobileMoneyCashTopupWithBranch(cusResponseObject.ApiResponseData.Data, branch);
                    return CashReplenishmentPrimaryTeller;

                }
                return new MobileMoneyCashTopup { RequestApprovalNote = cusResponseObject.Message, Id = "0" };
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ExecutionMessages> Create(AddMobileMoneyCashTopup model)
        {
            try
            {
                var response = await _transactionBaseConfigApiHelper.PostAsync<ServiceResponse<MobileMoneyCashTopup>>(APICallHelper.CreateMobileMoneyCashTopup, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> ValidateRequest(ValidateMobileMoneyCashTopup validateMobileMoney)
        {
            try
            {

                var response = await _transactionBaseConfigApiHelper.PutAsync<ServiceResponse<MobileMoneyCashTopup>>(string.Format(APICallHelper.ValidateMobileMoneyCashTopup, validateMobileMoney.Id), validateMobileMoney);

                if (response.IsSuccess)
                {
                    // Successful update
                    return GetExecutionMessages(response, true, null, MessagesResults.Success,
                                                 ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    // Failed update
                    return GetExecutionMessages(validateMobileMoney, false, null, MessagesResults.Failed,
                                                 ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                return GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                                            SystemMessageStatus.Failed.ToString(), ex);
            }

        }

    }

}
