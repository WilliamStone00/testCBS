
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
    public class OldLoanAccountingMapingServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;
        private readonly BranchServices _branchServices;

        public OldLoanAccountingMapingServices(BranchServices branchServices)
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices = branchServices;
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objFee = await GetOldLoanAccountingMaping(id);
                var inResponse = await _transactionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_OldLoanAccountingMaping, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objFee, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<OldLoanAccountingMaping>> GetOldLoanAccountingMapings()
        {
            try
            {
                // Fetch the OldLoanAccountingMaping data from the API
                var OldLoanAccountingMapingResponse = await _transactionApiHelper.GetAsync<ResponseObject<List<OldLoanAccountingMaping>>>(APICallHelper.GetAllOldLoanAccountingMaping);

                if (OldLoanAccountingMapingResponse.IsSuccess)
                {
                    // Fetch branches data
                    //var branches = await _branchServices.GetBranches();

                    // Extract fee policies and branches from the responses
                    var oldLoanAccountings = OldLoanAccountingMapingResponse.ApiResponseData.Data;

                    // Map branch information to fee policies
                    //var mappedPolicies = (from ol in oldLoanAccountings
                    //                      join branch in branches on ol.BranchId equals branch.Id into branchGroup
                    //                      from branch in branchGroup.DefaultIfEmpty()
                    //                      select new OldLoanAccountingMaping
                    //                      {
                    //                          Id = ol.Id,
                    //                          ChartOfAccountIdForCapital = ol.ChartOfAccountIdForCapital,
                    //                          ChartOfAccountIdForInterest = ol.ChartOfAccountIdForInterest,
                    //                          ChartOfAccountIdForVAT = ol.ChartOfAccountIdForVAT,
                    //                          LoanTypeName = ol.LoanTypeName,
                    //                          BranchId = ol.BranchId,
                    //                          BranchName = branch != null ? branch.Name : "N/A", // Map branch name if found
                    //                          BranchCode = branch != null ? branch.BranchCode : "N/A", // Map branch code if found
                    //                      }).ToList();

                    return oldLoanAccountings;
                }

                // Return an empty list if the response is not successful
                return new List<OldLoanAccountingMaping>();
            }
            catch (Exception ex)
            {
                // Log and handle the exception
                throw;
            }
        }
        public async Task<OldLoanAccountingMaping> GetOldLoanAccountingMaping(string id)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<OldLoanAccountingMaping>>(string.Format(APICallHelper.Get_Update_Delete_OldLoanAccountingMaping, id));
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
        public async Task<ExecutionMessages> Create(OldLoanAccountingMaping model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<OldLoanAccountingMaping>>(APICallHelper.CreateOldLoanAccountingMaping, model);
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
        public async Task<ExecutionMessages> Update(OldLoanAccountingMaping model)
        {
            try
            {
                var oldLoanAccountingMaping = await GetOldLoanAccountingMaping(model.Id);
                if (oldLoanAccountingMaping != null)
                {
                    oldLoanAccountingMaping.ChartOfAccountIdForCapital = model.ChartOfAccountIdForCapital;
                    oldLoanAccountingMaping.ChartOfAccountIdForVAT = model.ChartOfAccountIdForVAT;
                    oldLoanAccountingMaping.ChartOfAccountIdForInterest = model.ChartOfAccountIdForInterest;
                    //oldLoanAccountingMaping.BranchId = model.BranchId;
                    var response = await _transactionApiHelper.PutAsync<ServiceResponse<OldLoanAccountingMaping>>(string.Format(APICallHelper.Get_Update_Delete_OldLoanAccountingMaping, model.Id), oldLoanAccountingMaping);
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
