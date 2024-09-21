using CBS.API.Helper;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting
{

    public class CorrespondingMappingServices : BaseApiServices
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

       

        public CorrespondingMappingServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
        
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var model = await GetCorrespondingMapping(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_CorrespondingMapping, id), id));
                if (inResponse.IsSuccess)
                {
                    GetExecutionMessages(inResponse, true, $"{model.ChartOfAccountId} -{model.Id} ", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
           

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(model, false, $"{model.ChartOfAccountId} -{model.Id} ", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<CorrespondingMappingDto>> GetAllCorrespondingMappingDto()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<CorrespondingMappingDto>>>(APICallHelper.GetAllCorrespondingMapping);
                if (couApiResponse.IsSuccess)
                {

                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<CorrespondingMappingDto>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }
                }
                return new List<CorrespondingMappingDto>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<CorrespondingMappingDto> GetCorrespondingMapping(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<CorrespondingMappingDto>>(string.Format(APICallHelper.Get_Update_Delete_CorrespondingMapping, id));
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
     
        public async Task<ExecutionMessages> Update(CorrespondingMapping model)
        {
            try
            {

                var AccountCategory = await GetCorrespondingMapping(model.Id);
                if (AccountCategory != null)
                {
                   
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<TrialBalanceReference>>(string.Format(APICallHelper.Get_Update_Delete_CorrespondingMapping, model.Id), model);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.AccountNumber} -{model.Id} ", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{model.AccountNumber} -{model.Id} ", MessagesResults.Failed,
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
 

        public async Task<List<CorrespondingMappingDto>> GetAllCorrespondingMappingByDocumentReference(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<List<CorrespondingMappingDto>>>(string.Format(APICallHelper.GetCorrespondingmappingByDocumentRefenceId, id));
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
        public async Task<List<CorrespondingMappingExceptionDto>> GetAllCorrespondingMappingExceptionByDocumentReference(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<List<CorrespondingMappingExceptionDto>>>(string.Format(APICallHelper.GetCorrespondingmappingByDocumentRefenceId, id));
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

        public async Task<ExecutionMessages> DeleteException(string id)
        {
            try
            {
                var model = await GetCorrespondingMappingException(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_CorrespondingMapping, id), id));
                if (inResponse.IsSuccess)
                {
                    GetExecutionMessages(inResponse, true, $"{model.ChartOfAccountId} -{model.Id} ", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);


                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(model, false, $"{model.ChartOfAccountId} -{model.Id} ", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<CorrespondingMappingDto>> GetAllCorrespondingMappingExceptionDto()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<CorrespondingMappingDto>>>(APICallHelper.GetAllCorrespondingMapping);
                if (couApiResponse.IsSuccess)
                {

                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<CorrespondingMappingDto>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }
                }
                return new List<CorrespondingMappingDto>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<CorrespondingMappingDto> GetCorrespondingMappingException(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<CorrespondingMappingDto>>(string.Format(APICallHelper.Get_Update_Delete_CorrespondingMapping, id));
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

        public async Task<ExecutionMessages> UpdateException(CorrespondingMapping model)
        {
            try
            {

                var AccountCategory = await GetCorrespondingMappingException(model.Id);
                if (AccountCategory != null)
                {

                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<TrialBalanceReference>>(string.Format(APICallHelper.Get_Update_Delete_CorrespondingMapping, model.Id), model);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.AccountNumber} -{model.Id} ", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{model.AccountNumber} -{model.Id} ", MessagesResults.Failed,
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


        public async Task<List<CorrespondingMappingDto>> GetAllCorrespondingMappingException(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<List<CorrespondingMappingDto>>>(string.Format(APICallHelper.GetCorrespondingmappingByDocumentRefenceId, id));
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
    }
}
