using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Application
{
    public class LoanApplicationServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public LoanApplicationServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());

        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objLoanApplication = await GetLoanApplication(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_LoanApplication, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"Loan application", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objLoanApplication, false, $"Loan application", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<LoanApplication>> GetLoanApplications()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanApplication>>>(APICallHelper.GetAllLoanApplication);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<LoanApplication>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<LoanApplication>> GetLoanApplicationByCustomerID(string customerId)
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanApplication>>>(string.Format(APICallHelper.GetAllLoanApplicationByCustomerId,customerId));
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<LoanApplication>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<LoanApplication> GetLoanApplication(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<LoanApplication>>(string.Format(APICallHelper.Get_Update_Delete_LoanApplication, id));
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
        public async Task<ExecutionMessages> Create(LoanApplication model)
        {
            try
            {

                // Make an API call to create an individual profile
                model.BankId = GetBankID();
                model.BranchId = GetBranchID();
                model.OrganizationId = GetOrganizationID();
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<LoanApplication>>(APICallHelper.CreateLoanApplication, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Loan application", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, "Loan application", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> ValidaLoanApplication(LoanApplication model)
        {
            try
            {

                var LoanApplication = await GetLoanApplication(model.Id.ToString());
                if (LoanApplication != null)
                {
                    
                    
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<LoanApplication>>(string.Format(APICallHelper.ValidateLoanApplicationStatus, model.Id), LoanApplication);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"Loan application", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"Loan application", MessagesResults.Failed,
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
        //public async Task<ExecutionMessages> ChangeLoanStaus(string LoanApplicationID,bool status)
        //{
        //    try
        //    {

                

        //            var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<LoanApplication>>(string.Format(APICallHelper.ApproveLoanApplication, model.id), LoanApplication);
        //            if (response.IsSuccess)
        //            {
        //                // Successful creation
        //                GetExecutionMessages(response, true, $"Loan application", MessagesResults.Success,
        //                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
        //                return ExecutionMessage;
        //            }
        //            else
        //            {
        //                // Failed creation
        //                GetExecutionMessages(model, false, $"Loan application", MessagesResults.Failed,
        //                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
        //            }
               

        //    }
        //    catch (Exception ex)
        //    {
        //        // Log and handle exception
        //        GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
        //            SystemMessageStatus.Failed.ToString(), ex);
        //    }
        //    return ExecutionMessage;
        //}

    }

}
