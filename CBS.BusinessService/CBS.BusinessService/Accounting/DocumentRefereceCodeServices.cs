using CBS.API.Helper;
using CBS.FrontDesk.Data;
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

    public class DocumentRefereceCodeServices : BaseApiServices
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;



        public DocumentRefereceCodeServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());

        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var model = await GetDocumentReferenceCode(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Delete_DocumentReference, id), id));
                if (inResponse.IsSuccess)
                {
                    GetExecutionMessages(inResponse, true, $"{model.ReferenceCode} -{model.Description} has been successfully deleted", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);


                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(model, false, $"{model.ReferenceCode} -{model.Description} failed to be deleted ", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<IEnumerable<Document>> GetAllReport()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<Document>>>(APICallHelper.Get_Update_Delete_Document);
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
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<DocumentType>>>(string.Format(APICallHelper.Get_DocumentType_By_DocumentReference,Id));
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
        public async Task<IEnumerable<DocumentReferenceCodeDto>> GetAllDocumentReferenceCodeModel()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<DocumentReferenceCodeDto>>>(APICallHelper.GetAllDocumentReferenceCode);
                if (couApiResponse.IsSuccess)
                {

                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<DocumentReferenceCodeDto>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }
                }
                return new List<DocumentReferenceCodeDto>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<DocumentReferenceCodeDto> GetDocumentReferenceCodeModel(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<DocumentReferenceCodeDto>>(string.Format(APICallHelper.GetDocumentReferenceCode, id));
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

        public async Task<ExecutionMessages> Update(DocumentReferenceCode model)
        {
            try
            {

                var AccountCategory = await GetDocumentReferenceCode(model.Id);
                if (AccountCategory != null)
                {

                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<DocumentReferenceCodeDto>>(string.Format(APICallHelper.UpdateDocumentReferenceCode, model.Id), DocumentPasser(model));
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.ReferenceCode} -{model.Description} has been updated successfully", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{model.ReferenceCode} -{model.Description} Failed to be updated ", MessagesResults.Failed,
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

        public async Task<ExecutionMessages> CreateCode(DocReferenceCode model)
        {
            try
            {

                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<DocumentReferenceCodeDto>>(APICallHelper.CreateDocumentReferenceCode, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.ReferenceCode} -{model.Description} has been created successfully ", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.ReferenceCode} -{model.Description} Creation Failed", MessagesResults.Failed,
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




        public async Task<ExecutionMessages> Create(DocumentReferenceCode model)
        {
            try
            {

             var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<DocumentReferenceCodeDto>>(APICallHelper.CreateDocumentReferenceCode,DocumentPasser( model));
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.ReferenceCode} -{model.Description} has been created successfully ", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.ReferenceCode} -{model.Description} Creation Failed", MessagesResults.Failed,
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

        private DocumentReferenceCode DocumentPasser(DocumentReferenceCode model)
        {
            var TransformModel = model;
            TransformModel.ProvCorrespondingAccount = model.ProvCorrespondingAccount==null ? new List<string> { "NONE" } : model.ProvCorrespondingAccount;
            TransformModel.ProvCorrespondingExceptionAccount = model.ProvCorrespondingExceptionAccount == null ? new List<string> {"NONE" } : model.ProvCorrespondingExceptionAccount;
            TransformModel.GrossCorrespondingAccount = model.GrossCorrespondingAccount == null ? new List<string> { "NONE" } : model.GrossCorrespondingAccount;
            TransformModel.GrossCorrespondingExceptionAccount = model.GrossCorrespondingExceptionAccount == null ? new List<string> { "NONE" } : model.GrossCorrespondingExceptionAccount;

            return TransformModel;
        }

        public async Task<DocumentReferenceCodeDto> GetDocumentReferenceCode(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<DocumentReferenceCodeDto>>(string.Format(APICallHelper.GetDocumentReferenceCode, id));
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
