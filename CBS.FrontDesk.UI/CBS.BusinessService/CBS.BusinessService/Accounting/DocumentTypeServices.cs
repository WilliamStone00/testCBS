using CBS.API.Helper;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity;

namespace CBS.BusinessService.Accounting
{
    public class DocumentTypeServices : BaseApiServices
    {
        private readonly ApiCallerHelper _AccountingConfigApiHelper;



        public DocumentTypeServices()
        {
            _AccountingConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());

        }

        public async Task<ExecutionMessages> Create(DocumentType model)
        {
            try
            {

                var response = await _AccountingConfigApiHelper.PostAsync<ServiceResponse<DocumentType>>(APICallHelper.Create_DocumentType, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.name} has been created successfully ", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.name}   Creation Failed", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(DocumentType model)
        {
            try
            {

                var AccountCategory = await GetDocumentType(model.id);
                if (AccountCategory != null)
                {

                    var response = await _AccountingConfigApiHelper.PutAsync<ServiceResponse<DocumentType>>(string.Format(APICallHelper.Get_Update_Delete_DocumentType, model.id), model);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.name} has been updated successfully", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{model.name} has been updated failed", MessagesResults.Failed,
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

        public async Task<DocumentType> GetDocumentType(string id)
        {
            try
            {
                var cusResponseObject = await _AccountingConfigApiHelper.GetAsync<ResponseObject<DocumentType>>(string.Format(APICallHelper.Get_Update_Delete_DocumentType, id));
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
        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var model = await GetDocumentType(id);
                var inResponse = await _AccountingConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_DocumentType, id), id));
                if (inResponse.IsSuccess)
                {
                    GetExecutionMessages(inResponse, true, $" {model.name} has been successfully deleted", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);


                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(model, false, $"{model.name} failed to be deleted ", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<IEnumerable<DocumentType>> GetAllDocumentType()
        {
            try
            {
                var couApiResponse = await _AccountingConfigApiHelper.GetAsync<ResponseObject<List<DocumentType>>>(APICallHelper.Get_Update_Delete_DocumentType);
                if (couApiResponse.IsSuccess)
                {

                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<DocumentType>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }
                }
                return new List<DocumentType>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<DocumentType>> GetAllReportTypeBydocumentId(string Id)
        {
            try
            {
                var couApiResponse = await _AccountingConfigApiHelper.GetAsync<ResponseObject<List<DocumentType>>>(string.Format(APICallHelper.Get_Update_Delete_DocumentType, Id));
                if (couApiResponse.IsSuccess)
                {

                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<DocumentType>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }
                }
                return new List<DocumentType>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

    }
}
