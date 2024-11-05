
using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounts
{
    //WithdrawalNotification
    public class FeePolicyServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;
        private readonly BranchServices _branchServices;

        public FeePolicyServices(BranchServices branchServices)
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices = branchServices;
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objFee = await GetFeePolicy(id);
                var inResponse = await _transactionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_FeePolicy, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"Policy", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objFee, false, $"Policy", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<FeePolicy>> GetFeePolicys()
        {
            try
            {
                // Fetch the FeePolicy data from the API
                var feePolicyResponse = await _transactionApiHelper.GetAsync<ResponseObject<List<FeePolicy>>>(APICallHelper.GetAllFeePolicy);

                if (feePolicyResponse.IsSuccess)
                {
                    // Fetch branches data
                    var branches = await _branchServices.GetBranches();

                    // Extract fee policies and branches from the responses
                    var policies = feePolicyResponse.ApiResponseData.Data;

                    // Map branch information to fee policies
                    var mappedPolicies = (from policy in policies
                                         join branch in branches on policy.BranchId equals branch.Id into branchGroup
                                         from branch in branchGroup.DefaultIfEmpty()
                                         select new FeePolicy
                                         {
                                             Id = policy.Id,
                                             FeeId = policy.FeeId,
                                             AmountFrom = policy.AmountFrom,
                                             AmountTo = policy.AmountTo,
                                             Value = policy.Value, IsCentralised= policy.IsCentralised,
                                             Charge = policy.Charge,
                                             BranchId = policy.BranchId,
                                             BankId = policy.BankId,
                                             BranchName = branch != null ? branch.Name : "N/A", // Map branch name if found
                                             BranchCode = branch != null ? branch.BranchCode : "N/A", // Map branch code if found
                                             EventCode = policy.EventCode,
                                             Fee = policy.Fee
                                         }).ToList();

                    return mappedPolicies;
                }

                // Return an empty list if the response is not successful
                return new List<FeePolicy>();
            }
            catch (Exception ex)
            {
                // Log and handle the exception
                throw;
            }
        }
        public async Task<FeePolicy> GetFeePolicy(string id)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<FeePolicy>>(string.Format(APICallHelper.Get_Update_Delete_FeePolicy, id));
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
        public async Task<ExecutionMessages> Create(FeePolicy model)
        {
            try
            {
                
                // Make an API call to create an individual profile
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<FeePolicy>>(APICallHelper.CreateFeePolicy, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Policy", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, "Policy", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(FeePolicy model)
        {
            try
            {
                var Fee = await GetFeePolicy(model.Id);
                if (Fee != null)
                {
                    if (Fee.Fee.FeeType=="Range")
                    {
                        Fee.AmountFrom = model.AmountFrom;
                        Fee.AmountTo = model.AmountTo;
                        Fee.Charge = model.Charge;
                        Fee.BranchId = model.BranchId;
                        Fee.BankId = model.BankId;
                        Fee.IsCentralised = model.IsCentralised;
                        if (model.EventCode!=string.Empty)
                        {
                            Fee.EventCode = model.EventCode;

                        }
                    }
                    else
                    {
                        Fee.Value = model.Value;
                        Fee.BranchId = model.BranchId;
                        Fee.BankId = model.BankId;
                        Fee.IsCentralised = model.IsCentralised;
                        if (model.EventCode != string.Empty)
                        {
                            Fee.EventCode = model.EventCode;

                        }
                    }
                    var response = await _transactionApiHelper.PutAsync<ServiceResponse<FeePolicy>>(string.Format(APICallHelper.Get_Update_Delete_FeePolicy, model.Id), Fee);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{Fee.Fee.Name}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, Fee.Fee.Name, MessagesResults.Failed,
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
