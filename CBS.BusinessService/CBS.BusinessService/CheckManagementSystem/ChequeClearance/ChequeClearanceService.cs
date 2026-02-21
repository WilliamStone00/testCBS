using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Clearance.ClearanceRequest;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.FeeConfiguration;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.ChequeClearance
{
    public class ChequeClearanceService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;
        private readonly ApiCallerHelper _identityServerBaseUrl;

        public ChequeClearanceService()
        {
            string baseUrl = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ConfigurationErrorsException("The 'CheckbookServiceBaseUrl' appSetting is missing or empty in Web.config.");
            _identityServerBaseUrl = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());

            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<CustomDataTable> ClearanceDataTableAsync(ClearanceQuery query)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.ClearanceDataTable, query);

                // ⚠️ CRITICAL: If API call fails or returns unsuccessful, THROW exception
                if (!response.IsSuccess)
                {
                    throw new Exception($"API call failed: {response.Message}");
                }

                if (response.ApiResponseData == null)
                {
                    throw new Exception("API returned null data");
                }

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log the original exception
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");

                // Re-throw to trigger fallback
                throw new Exception($"Cheque book service unavailable: {ex.Message}", ex);
            }
        }





        public async Task<ExecutionMessages> CreateAsync(OptionRequest model)
        {
            if(model.ExternalChequeNumber != null)
            {
                model.IsExternalCheck = true;
            }
            try
            {
                var response = await _apiCallerHelper
                    .PostAsync<ServiceResponse<ClearanceResponce>>(
                        APICallHelper.ChequeClearanceRequest,  // ✅ Correct endpoint
                        model);

                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    GetExecutionMessages(
                        response.ApiResponseData.Data,
                        true,
                        response.ApiResponseData.Data.Id,
                        MessagesResults.Success,
                        ExecutionProcessOption.InsertObject,
                        SystemMessageStatus.Success.ToString(),
                        null,
                        response.ApiResponseData.Message
                    );
                }
                else
                {
                    GetExecutionMessages(
                        model,
                        false,
                        model?.Id ?? "ChequeClearance",
                        MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject,
                        SystemMessageStatus.Failed.ToString(),
                        null,
                        response?.ApiResponseData?.Message ?? response?.Message
                    );
                }

            }
            catch (Exception ex)
            {
                GetExecutionMessages(
                    model,
                    false,
                    model?.Id ?? "ChequeClearance",
                    MessagesResults.Error,
                    ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Error.ToString(),
                    ex,
                    ex.Message
                );
            }

            return ExecutionMessage;
        }






        public async Task<OptionRequest> ClearanceDetails(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Clearance ID cannot be null or empty.", nameof(id));
                // ✅ Make API call
                var response = await _apiCallerHelper.GetAsync<ResponseObject<OptionRequest>>(string.Format(APICallHelper.GetClearanceDetails, id));

                // ✅ Validate response
                if (!response.IsSuccess)
                    throw new Exception($"API call failed: {response.Message}");


                var entry = response.ApiResponseData?.Data;
                if (entry == null)
                    throw new Exception("Clearance  Details not found.");

                return entry;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetJournalEntryByIdAsync] Error: {ex.Message}");
                throw;
            }
        }



        public async Task<ExecutionMessages> UpdateAsync(OptionRequest model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.Id))
                {
                    GetExecutionMessages(model, false, model?.Id , MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, "Invalid model or Id.");
                    return ExecutionMessage;
                }

                string url = string.Format(APICallHelper.UpdateFeeConfig, model.Id);
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<OptionRequest>>(url, model);
                if (response != null && (response.IsSuccess))
                {
                    GetExecutionMessages(response.ApiResponseData?.Data, true, model?.Id, MessagesResults.Success, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model?.Id, MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response?.ApiResponseData?.Message ?? response?.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model?.Id, MessagesResults.Error, ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UploadFiles(ClearanceRequestImage documentRequest)
        {
            try
            {
                // Check if files are attached
                if (documentRequest.AttachedFiles[0] == null)
                {
                    // Handle case where no files are attached
                    return GetExecutionMessages(documentRequest, false, "File", MessagesResults.Failed,
                  ExecutionProcessOption.NoFileWasSelected, SystemMessageStatus.Failed.ToString(), null,
              null);
                }
                documentRequest.Id = "CLR574384137304312";


                var url = $"/api/v1/clearance/{documentRequest.Id}/document";
                var additionalParams = new Dictionary<string, string>
                {
                    { "OperationID", documentRequest.Id},
                    { "DocumentId", "N/A" },
                    { "DocumentType", documentRequest.DocumentType },
                    { "ServiceType", "ChequeManagement" },
                    { "CallBackBaseUrl",ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"].ToString()},
                    { "CallBackEndPoint", url },
                    { "RemoteFilePath", $"ChequeImages/{documentRequest.DocumentType}" },
                };
                var response = await _identityServerBaseUrl.PostFilesAndParamsAsync<DocumentUploadResponse>(APICallHelper.AttachedDocuments, additionalParams, documentRequest.AttachedFiles);
                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                        response.Message);
                    return ExecutionMessage;
                }
                GetExecutionMessages(documentRequest, false, null, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);

            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }

    }
}
