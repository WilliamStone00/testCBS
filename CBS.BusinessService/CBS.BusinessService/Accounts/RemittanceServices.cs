
using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
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

    public class RemittanceServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;
        private readonly BranchServices _branchServices;

        public RemittanceServices(BranchServices branchServices)
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices=branchServices;
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objRemittance = await GetRemittance(id);
                var inResponse = await _transactionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_Remittance, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objRemittance, false, $"Remittance", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<Remittance>> GetRemittances(GetAllRemittanceQuery allRemittanceQuery)
        {
            try
            {
                var couApiResponse = await _transactionApiHelper.PostAsync<ResponseObject<List<Remittance>>>(APICallHelper.GetAllRemittanceRequests, allRemittanceQuery);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<Remittance>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<Remittance>> SearchRemittance(GetAllRemittanceWildQuery allRemittanceWildQuery)
        {
            try
            {
                var couApiResponse = await _transactionApiHelper.PostAsync<ResponseObject<List<Remittance>>>(APICallHelper.GetRemittancesByQUeryParameters, allRemittanceWildQuery);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<Remittance>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<RemittanceChargeDto> GetRemittanceCharge(GetRemittanceChargeQuery remittanceChargeQuery)
        {
            try
            {
                var couApiResponse = await _transactionApiHelper.PostAsync<ResponseObject<RemittanceChargeDto>>(APICallHelper.GetRemittanceCharge, remittanceChargeQuery);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new RemittanceChargeDto { FeeName=couApiResponse .Message};
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<Remittance> GetRemittance(string id)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<Remittance>>(string.Format(APICallHelper.Get_Update_Delete_Remittance, id));
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
        public async Task<ExecutionMessages> Create(AddRemittanceCommand model)
        {
            try
            {//International_Remittance or Local_Remittance
                var branch = await _branchServices.GetBranch(model.SourceBranchId);
                model.SourceBranchCode=branch.BranchCode;
                model.SourceBranchName=branch.Name;
                model.ExternalReference=model.ExternalReference!=null ? model.ExternalReference : "N/A";
                //model.TransferSource=model.TransferType=="Local"? "Local_Remittance" : "International_Remittance";
                // Make an API call to create an individual profile
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<Remittance>>(APICallHelper.CreateRemittanceRequest, model);
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
        public async Task<ExecutionMessages> GenerateOTPRemittance(GenerateRemittanceOTPCommand model)
        {
            try
            {
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<TempOTPDto>>(APICallHelper.GenerateOTPRemittance, model);
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
        public async Task<ExecutionMessages> Validate(ValidationOfRemittanceCommand model)
        {
            try
            {
                var response = await _transactionApiHelper.PutAsync<ServiceResponse<OperationFee>>(string.Format(APICallHelper.RemittanceRequestValidation, model.Id), model);
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

    }

}
