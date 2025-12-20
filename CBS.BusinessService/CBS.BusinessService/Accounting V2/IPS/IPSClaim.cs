using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.BranchAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.IPS;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.IPS
{
    public class IPSClaimService : BaseService
    {
        private readonly ApiCallerHelper _apiHelper;
        private readonly ApiCallerHelper _apiCallerHelper;
        private readonly IndividualProfileServices _IndividualProfileServices;
        private readonly ApiCallerHelper _identityServerBaseUrl;

        public IPSClaimService(IndividualProfileServices individualProfileServices)
        {
            _identityServerBaseUrl = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
            _IndividualProfileServices = individualProfileServices;
            string baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The 'AccountingBaseUrl' appSetting is missing or empty in Web.config.");
            }
            _apiHelper = new ApiCallerHelper(baseUrl);
            //change the base url to the actual base url
            string baseUrl2 = ConfigurationManager.AppSettings["TransactionBaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The baseUrl is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl2);
        }

        public async Task<ExecutionMessages> CreateClaimAsync(IPSClaimCreate model)
        {
            try
            {
                var response = await _apiHelper.PostAsync<ServiceResponse<IPSClaim>>(
                    APICallHelper.CreateIPSClaim, model);

                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    var claim = response.ApiResponseData.Data;

                    // 🔑 PASS CLAIM OBJECT INTO EXECUTION MESSAGE
                    GetExecutionMessages(
                        claim,
                        true,
                        "IPS Claim",
                        MessagesResults.Success,
                        ExecutionProcessOption.InsertObject,
                        SystemMessageStatus.Success.ToString(),
                        null,
                        response.ApiResponseData.Message ?? "Claim created successfully"
                    );
                }
                else
                {
                    GetExecutionMessages(
                        null,
                        false,
                        "IPS Claim",
                        MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject,
                        SystemMessageStatus.Failed.ToString(),
                        null,
                        response.ApiResponseData?.Message ?? response.Message ?? "Failed to create claim"
                    );
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(
                    null,
                    false,
                    "IPS Claim",
                    MessagesResults.Error,
                    ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Error.ToString(),
                    ex,
                    ex.Message
                );
            }

            return ExecutionMessage;
        }


        public async Task<IPSClaim> GetClaimByIdAsync(string claimId)
        {
            try
            {
                string url = string.Format(APICallHelper.GetIPSClaimById, claimId);
                var response = await _apiHelper.GetAsync<ResponseObject<IPSClaim>>(url);
                return response?.ApiResponseData?.Data;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<CustomDataTable> GetClaimsDataTableAsync(IPSClaimQuery query)
        {
            try
            {
                var response = await _apiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.IPSClaimDataTable, query);

                if (!response.IsSuccess || response.ApiResponseData == null)
                {
                    throw new Exception($"API call failed: {response?.Message}");
                }

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IPS Claim API Error: {ex.Message}");
                throw new Exception($"IPS claim service unavailable: {ex.Message}", ex);
            }
        }

        public async Task<List<IPSClaim>> GetAllClaimsAsync()
        {
            var query = new IPSClaimQuery();
            var dataTable = await GetClaimsDataTableAsync(query);

            if (dataTable?.data != null)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(dataTable.data);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<IPSClaim>>(json);
            }

            return new List<IPSClaim>();
        }

        public async Task<ExecutionMessages> ApproveClaimAsync(IPSClaimApprove model)
        {
            try
            {
                string url = string.Format(APICallHelper.ApproveIPSClaim, model.ClaimId);
                var response = await _apiHelper.PostAsync<ServiceResponse<IPSClaim>>(url, model);

                if (response.IsSuccess && response.ApiResponseData != null)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, $"Claim: {model.ClaimId}",
                        MessagesResults.Success, ExecutionProcessOption.UpdateUpject,
                        SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData.Message ?? "Claim approved successfully");
                }
                else
                {
                    GetExecutionMessages(model, false, $"Claim: {model.ClaimId}", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                        null, response.ApiResponseData?.Message ?? response.Message ?? "Failed to approve claim");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, $"Claim: {model.ClaimId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(),
                    ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> RejectClaimAsync(IPSClaimReject model)
        {
            try
            {
                string url = string.Format(APICallHelper.RejectIPSClaim, model.ClaimId);
                var response = await _apiHelper.PostAsync<ServiceResponse<IPSClaim>>(url, model);

                if (response.IsSuccess && response.ApiResponseData != null)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, $"Claim: {model.ClaimId}",
                        MessagesResults.Success, ExecutionProcessOption.UpdateUpject,
                        SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData.Message ?? "Claim rejected successfully");
                }
                else
                {
                    GetExecutionMessages(model, false, $"Claim: {model.ClaimId}", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                        null, response.ApiResponseData?.Message ?? response.Message ?? "Failed to reject claim");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, $"Claim: {model.ClaimId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(),
                    ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> PostClaimAsync(PremiumClaimCashIn model)
        {
            try
            {
             
                string url = string.Format(APICallHelper.PostIPSClaim, model.ClaimId);
                var response = await _apiHelper.PostAsync<ServiceResponse<object>>(url, model);

                if (response.IsSuccess && response.ApiResponseData != null)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, $"Claim: {model.ClaimId}",
                        MessagesResults.Success, ExecutionProcessOption.UpdateUpject,
                        SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData.Message ?? "Claim posted successfully");
                }
                else
                {
                    GetExecutionMessages(model, false, $"Claim: {model.ClaimId}", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                        null, response.ApiResponseData?.Message ?? response.Message ?? "Failed to post claim");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, $"Claim: {model.ClaimId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(),
                    ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UploadDocumentAsync(string claimId, IPSClaimDocument document)
        {
            try
            {
                string url = string.Format(APICallHelper.UploadIPSClaimDocument, claimId);
                var response = await _apiHelper.PostAsync<ServiceResponse<bool>>(url, document);

                if (response.IsSuccess && response.ApiResponseData?.Data == true)
                {
                    GetExecutionMessages(document, true, $"Document for Claim: {claimId}",
                        MessagesResults.Success, ExecutionProcessOption.UpdateUpject,
                        SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData.Message ?? "Document uploaded successfully");
                }
                else
                {
                    GetExecutionMessages(document, false, $"Document for Claim: {claimId}",
                        MessagesResults.Failed, ExecutionProcessOption.UpdateUpject,
                        SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message ?? "Failed to upload document");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(document, false, $"Document for Claim: {claimId}",
                    MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> DeleteClaimAsync(string claimId)
        {
            try
            {
                string url = string.Format(APICallHelper.DeleteIPSClaim, claimId);
                var response = await _apiHelper.DeleteAsync<ServiceResponse<bool>>(url);

                if (response.IsSuccess && response.ApiResponseData?.Data == true)
                {
                    GetExecutionMessages(null, true, $"Claim: {claimId}",
                        MessagesResults.Success, ExecutionProcessOption.DeleteObject,
                        SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData.Message ?? "Claim deleted successfully");
                }
                else
                {
                    GetExecutionMessages(null, false, $"Claim: {claimId}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(),
                        null, response.ApiResponseData?.Message ?? response.Message ?? "Failed to delete claim");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"Claim: {claimId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(),
                    ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<CustomerLightDto>> GetMemberByBranch(string branchId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Decide branchId first so the query sent to server is correct.
                if (IsHeadOffice())
                {
                    if (string.IsNullOrWhiteSpace(branchId))
                        branchId = null; // ensure API understands this convention
                                         // Head Office may keep a specific branchId if passed
                }
                else
                {
                    branchId = GetBranchID() ?? throw new InvalidOperationException("Current user's branch ID is not available.");
                }

                var query = new GetCustomersForDataTableQuery
                {
                    BranchId = branchId,
                    Options = new DataTableOptions() { pageSize = 30000, length = 30000 },
                };

                // Reuse existing method which calls the API datatable endpoint
                var dataTable = await _IndividualProfileServices.GetDataTableAsync(query, "MyMembers");

                // Convert datatable.data to strongly-typed list safely
                List<CustomerLightDto> branchList = new List<CustomerLightDto>();

                if (dataTable?.data is JToken token)
                {
                    branchList = token.ToObject<List<CustomerLightDto>>() ?? new List<CustomerLightDto>();
                }
                else if (dataTable?.data != null)
                {
                    // Fallback if data is plain object
                    branchList = JsonConvert.DeserializeObject<List<CustomerLightDto>>(JsonConvert.SerializeObject(dataTable.data))
                                 ?? new List<CustomerLightDto>();
                }

                var formatted = branchList
                    .Select(a => new CustomerLightDto
                    {
                        CustomerId = a.CustomerId,
                        FirstName = $"[{a.CustomerType}] - [{a.FirstName} - {a.LastName}]".Trim()
                    })
                    .OrderBy(a => a.CustomerId) // nicer UX for dropdown; change if you prefer Id
                    .ToList();

                return formatted;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<ExecutionMessages> UploadFiles(IPSClaimDocument documentRequest)
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


                var url = $"/api/v1/IPSClaim/{documentRequest.Id}/document";
                var additionalParams = new Dictionary<string, string>
                {
                    { "OperationID", documentRequest.Id},
                    { "DocumentId", "N/A" },
                    { "DocumentType", documentRequest.DocumentType },
                    { "ServiceType", "AccountingVII" },
                    { "CallBackBaseUrl",ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString()},
                    { "CallBackEndPoint", url },
                    { "RemoteFilePath", $"IPSImages/{documentRequest.DocumentType}" },
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

        public async Task<CustomerMetaDataResponse> GetinfoAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);
                string formattedUrl = string.Format(APICallHelper.Clientdata, encodedId);

                var response = await _apiCallerHelper.GetAsync<ServiceResponse<CustomerMetaDataResponse>>(formattedUrl);

                if (response?.ApiResponseData?.Data != null && response.IsSuccess)
                {
                    return response.ApiResponseData?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

