using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace CBS.BusinessService.Loan.Config
{
    public class WriteOffLoanServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public WriteOffLoanServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objWriteOffLoanConfiguration = await GetWriteOffLoanConfiguration(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_WriteOffLoan, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objWriteOffLoanConfiguration.LoanId}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objWriteOffLoanConfiguration, false, $"{objWriteOffLoanConfiguration.LoanId}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        
        public async Task<IEnumerable<WriteOffLoan>> GetWriteOffLoanConfigurations()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<WriteOffLoan>>>(APICallHelper.GetAllWriteOffLoans);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<WriteOffLoan>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<WriteOffLoan> GetWriteOffLoanConfiguration(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<WriteOffLoan>>(string.Format(APICallHelper.Get_Update_Delete_WriteOffLoan, id));
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
        public async Task<ExecutionMessages> Create(WriteOffLoan model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<WriteOffLoan>>(APICallHelper.CreateWriteOffLoan, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.LoanId}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.LoanId, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(WriteOffLoan model)
        {
            try
            {

                var WriteOffLoanConfiguration = await GetWriteOffLoanConfiguration(model.Id);
                if (WriteOffLoanConfiguration != null)
                {
                    WriteOffLoanConfiguration.LoanId = model.LoanId;
                    WriteOffLoanConfiguration.OutstandingLoanBalance = model.OutstandingLoanBalance;
                    WriteOffLoanConfiguration.AccruedInterests = model.AccruedInterests;
                    WriteOffLoanConfiguration.AccruedPenalties = model.AccruedPenalties;
                    WriteOffLoanConfiguration.PastDueDays = model.PastDueDays;
                    WriteOffLoanConfiguration.OverduePrincipal = model.OverduePrincipal;
                    WriteOffLoanConfiguration.BankId = model.BankId;
                    WriteOffLoanConfiguration.BranchId = model.BranchId;
                    WriteOffLoanConfiguration.OrganizationId = model.OrganizationId;
                    WriteOffLoanConfiguration.AccountingRuleId = model.AccountingRuleId;
                    WriteOffLoanConfiguration.Comment = model.Comment;
                    WriteOffLoanConfiguration.WriteOffMethod = model.WriteOffMethod;
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<WriteOffLoan>>(string.Format(APICallHelper.Get_Update_Delete_WriteOffLoan, model.Id), WriteOffLoanConfiguration);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.LoanId}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.LoanId, MessagesResults.Failed,
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
