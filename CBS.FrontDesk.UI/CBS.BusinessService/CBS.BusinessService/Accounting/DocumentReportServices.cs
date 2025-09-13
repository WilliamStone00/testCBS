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


    public class DocumentReportServices : BaseApiServices
    {
        private readonly ApiCallerHelper _AccountingConfigApiHelper;



        public DocumentReportServices()
        {
            _AccountingConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());

        }
        public async Task<ExecutionMessages> Create(Document model)
        {
            try
            {

                var response = await _AccountingConfigApiHelper.PostAsync<ServiceResponse<DocumentReferenceCodeDto>>(APICallHelper.CreateDocument, model);
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
        public async Task<ExecutionMessages> Update(Document model)
        {
            try
            {

                var AccountCategory = await GetDocument(model.id);
                if (AccountCategory != null)
                {

                    var response = await _AccountingConfigApiHelper.PutAsync<ServiceResponse<Document>>(string.Format(APICallHelper.Get_Update_Delete_Document, model.id), model);
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

        public async Task<Document> GetDocument(string id)
        {
            try
            {
                var cusResponseObject = await _AccountingConfigApiHelper.GetAsync<ResponseObject<Document>>(string.Format(APICallHelper.Get_Update_Delete_Document, id));
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
                var model = await GetDocument(id);
                var inResponse = await _AccountingConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_Document, id), id));
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
        public async Task<IEnumerable<Document>> GetAllDocument()
        {
            try
            {
                var couApiResponse = await _AccountingConfigApiHelper.GetAsync<ResponseObject<List<Document>>>(APICallHelper.Get_Update_Delete_Document);
                if (couApiResponse.IsSuccess)
                {

                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<Document>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }
                }
                return new List<Document>();
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
                var couApiResponse = await _AccountingConfigApiHelper.GetAsync<ResponseObject<List<DocumentType>>>(string.Format(APICallHelper.Get_DocumentType_By_DocumentReference, Id));
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
