using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace CBS.BusinessService.Config
{
    public class AttachedDocumentServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;
        private readonly ApiCallerHelper _customerApiHelper;
        private readonly ApiCallerHelper _identityServerBaseUrl;
        public AttachedDocumentServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
            _customerApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CustomerBaseUrl"].ToString());
            _identityServerBaseUrl = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
            
        }
        public async Task<ExecutionMessages> UploadFiles(DocumentAttachedToLoan attachedToLoan)
        {
            try
            {
                // Check if files are attached
                if (attachedToLoan.AttachedFiles[0] == null)
                {
                    // Handle case where no files are attached
                    return GetExecutionMessages(attachedToLoan, false, "File", MessagesResults.Failed,
                  ExecutionProcessOption.NoFileWasSelected, SystemMessageStatus.Failed.ToString(), null,
              null);
                }
                var additionalParams = new Dictionary<string, string>
                {
                    { "OperationID", attachedToLoan.LoanApplicationId },
                    { "DocumentId", attachedToLoan.DocumentId },
                    { "DocumentType", "Loan" },
                    { "ServiceType", "LoanMicroservice" },
                    { "CallBackBaseUrl",ConfigurationManager.AppSettings["LoanBaseUrl"].ToString()},
                    { "CallBackEndPoint", APICallHelper.AttachedDocumentsRemotelyLoan},
                    { "RemoteFilePath", $"LoanDocuments" },
                };
                var response = await _identityServerBaseUrl.PostFilesAndParamsAsync<DocumentAttachedToLoan>(APICallHelper.AttachedDocuments, additionalParams, attachedToLoan.AttachedFiles);
                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                        response.Message);
                    return ExecutionMessage;
                }
                GetExecutionMessages(attachedToLoan, false, null, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);

            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UploadFiles(CustomerDocumentRequest attachedToLoan)
        {
            try
            {
                var additionalParams = new Dictionary<string, string>
                {
                    {"CustomerID", attachedToLoan.CustomerID },
                    {"IsPhoto ","true"},
                    {"IsSignature","false"}
                };
                var response = await _customerApiHelper.PostFilesAndParamsAsync<DocumentUploadResponse>(APICallHelper.UploadCustomerDocument, additionalParams, attachedToLoan.AttachedFiles);
                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null,
                        null);
                    return ExecutionMessage;
                }
                GetExecutionMessages(attachedToLoan, false, null, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null,
                    null);

            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objDocumentAttachedToLoan = await GetDocumentAttachedToLoan(id);
                var response = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Delete_DocumentAttachedToLoan, id), id));
                if (response.IsSuccess)
                {

                    GetExecutionMessages(response, true, $"{objDocumentAttachedToLoan.FileName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objDocumentAttachedToLoan, false, $"{objDocumentAttachedToLoan.FileName}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<IEnumerable<DocumentAttachedToLoan>> GetDocumentAttachedToLoans()
        {
            try
            {
                var response = await _loanConfigApiHelper.GetAsync<ResponseObject<List<DocumentAttachedToLoan>>>(APICallHelper.GetAllAttachedments);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<DocumentAttachedToLoan>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<DocumentAttachedToLoan>> GetDocumentAttachedToLoanByLoanID(string loanApplicationID)
        {
            try
            {
                var response = await _loanConfigApiHelper.GetAsync<ResponseObject<List<DocumentAttachedToLoan>>>(string.Format(APICallHelper.Get_Delete_DocumentAttachedToLoan, loanApplicationID));
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<DocumentAttachedToLoan>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<DocumentAttachedToLoan> GetDocumentAttachedToLoan(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<DocumentAttachedToLoan>>(string.Format(APICallHelper.Get_Delete_DocumentAttachedToLoan, id));
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
