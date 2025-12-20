using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.BranchAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.FileUpload;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using Hangfire.Storage.Monitoring;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.BusinessService.Accounting_V2.FilesUpload
{
    public class FileUploadService : BaseService
    {
        private readonly ApiCallerHelper _apiHelper;
        private readonly ApiCallerHelper _customerApiHelper;
        private readonly ApiCallerHelper _ExtractedDetails;
        private readonly BranchServices _branchServices;
        public FileUploadService(BranchServices branchServices)
        {
            var baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            _apiHelper = new ApiCallerHelper(baseUrl);
            var cusbaseurl = ConfigurationManager.AppSettings["CustomerBaseUrl"];
            _branchServices = branchServices;
        }

        public async Task<CustomDataTable2> AccountwaitingDataTableAsync(AccountwaitingCorrespondanceQuery query)
        {
            try
            {
                query.Options.lang = GetUserLanguage();
                var response = await _apiHelper.PostAsync<ResponseObject<CustomDataTable2>>(
                    APICallHelper.awaitingcorrespondancedatatable, query);

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

        //***************************************** Mock Data table ********************************
        // Mock implementation for testing the Account Awaiting Correspondance DataTable
        public async Task<CustomDataTable> AccountwaitingMockDataTableAsync(AccountwaitingCorrespondanceQuery query)
        {
            // small artificial latency to mimic real API
            await Task.Delay(10);

            try
            {
                var now = DateTime.UtcNow;

                // --- In-memory sample data (adjust fields to match your real Accountwaiting DTO) ---
                var all = new List<Accountwaiting>
        {
            new Accountwaiting {
                Id = "AW-1001",
                CreatedDate = now.AddDays(-1),
                Name = "Alpha Co.",
                Class = "ASSET",
                Reason = "Missing signature",
                PostingAllowed = true,
                Scope = "Affiliate",
                IsDeleted = false
            },
            new Accountwaiting {
                Id = "AW-1002",
                CreatedDate = now.AddDays(-2),
                Name = "Beta Ltd",
                Class = "LIABILITY",
                Reason = "Incorrect account number",
                PostingAllowed = false,
                Scope = "TryBallance",
                IsDeleted = false
            },
            new Accountwaiting {
                Id = "AW-1003",
                CreatedDate = now.AddDays(-10),
                Name = "Gamma Enterprises",
                Class = "EQUITY",
                Reason = "Awaiting documentation",
                PostingAllowed = true,
                Scope = "Affiliate",
                IsDeleted = true
            },
            new Accountwaiting {
                Id = "AW-1004",
                CreatedDate = now.AddDays(-5),
                Name = "Delta Partners",
                Class = "ASSET",
                Reason = "Mismatch in currency",
                PostingAllowed = true,
                Scope = "TryBallance",
                IsDeleted = false
            },
            new Accountwaiting {
                Id = "AW-1005",
                CreatedDate = now.AddDays(-3),
                Name = "Epsilon Services",
                Class = "EXPENSE",
                Reason = "Missing approval",
                PostingAllowed = false,
                Scope = "Affiliate",
                IsDeleted = false
            },
            // add more items if you want more pages
        };

                // Work on an IQueryable for convenient filtering / paging
                var items = all.AsQueryable();

                //// --- Apply filters from AccountwaitingCorrespondanceQuery (defensive) ---
                if (query != null)
                {
                    // Example string filters (Scope, Code, Class, Language) - adjust property names as needed
                    if (!string.IsNullOrWhiteSpace(query.Scope))
                    {
                        var s = query.Scope.Trim();
                        items = items.Where(x => (x.Scope ?? string.Empty).IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
                    }

                    if (!string.IsNullOrWhiteSpace(query.Code))
                    {
                        var s = query.Code.Trim();
                        items = items.Where(x => (x.Id ?? string.Empty).IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0
                                              || (x.Name ?? string.Empty).IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
                    }

                    if (!string.IsNullOrWhiteSpace(query.Class))
                    {
                        var s = query.Class.Trim();
                        items = items.Where(x => (x.Class ?? string.Empty).IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
                    }

                    if (query.CreatedFromUtc.HasValue)
                    {
                        var from = query.CreatedFromUtc.Value;
                        items = items.Where(x => (DateTime)x.CreatedDate >= from);
                    }

                    if (query.CreatedToUtc.HasValue)
                    {
                        var to = query.CreatedToUtc.Value;
                        items = items.Where(x => (DateTime)x.CreatedDate <= to);
                    }

                   

                    if (!string.IsNullOrWhiteSpace(query.Language))
                    {
                        // no-op in mock (unless you want to simulate different names by language)
                    }
                }
                else
                {
                    // default behavior when query is null: exclude deleted
                    items = items.Where(x => x.IsDeleted == false);
                }

                //// --- Global search coming from DataTable options (if provided) ---
                var opts = query?.Options;
                var globalSearch = opts?.searchValue?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(globalSearch))
                {
                    var s = globalSearch.ToLowerInvariant();
                    items = items.Where(x =>
                        ((x.Id ?? string.Empty).ToString().ToLowerInvariant().Contains(s))
                        || ((x.Name ?? string.Empty).ToLowerInvariant().Contains(s))
                        || ((x.Class ?? string.Empty).ToLowerInvariant().Contains(s))
                        || ((x.Reason ?? string.Empty).ToLowerInvariant().Contains(s))
                        || ((x.Scope ?? string.Empty).ToLowerInvariant().Contains(s))
                    );
                }

                // --- Sorting: default CreatedDate desc ---
                items = items.OrderByDescending(x => (DateTime)x.CreatedDate);

                // --- Paging (DataTables style) ---
                var total = all.Count;
                var filteredCount = items.Count();

                var start = opts?.start ?? 0;
                var length = opts?.length ?? 10;
                if (start < 0) start = 0;
                if (length <= 0) length = 10;

                var page = items.Skip(start).Take(length).ToList();

                // Build and return CustomDataTable result
                var result = new CustomDataTable
                {
                    draw = Convert.ToInt32(opts?.draw ?? "1"),
                    recordsTotal = total,
                    recordsFiltered = filteredCount,
                    data = page // the controller will deserialize this to List<Accountwaiting>
                };

                return result;
            }
            catch (Exception ex)
            {
                // Bubble up to caller - controller will handle and return a DataTables-safe error response
                throw new Exception($"Mock AccountwaitingDataTableAsync failed: {ex.Message}", ex);
            }
        }

        //***************************************** END OF Mock Data table ********************************

        public async Task<CustomDataTable2> CorrespondenceRequestDataTableAsync(CorrespondanceRequestQuery query)
        {
            try
            {
                var str = JsonConvert.SerializeObject(query);
                query.Options.lang = GetUserLanguage();
                var response = await _apiHelper.PostAsync<ResponseObject<CustomDataTable2>>(
                    APICallHelper.Correspondancedatatable, query);

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
                throw new Exception($"Accounting Service Unavailable: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse<ServiceResponse<AffiliateFileUploadResponse>>> AffiliateUpload(FileUpload affiliateUpload)
        {
            try
            {
                if (affiliateUpload.file == null)
                {
                    return new ApiResponse<ServiceResponse<AffiliateFileUploadResponse>>
                    {
                        IsSuccess = false,
                        ApiResponseData = null,
                        Message = "❌ No file provided."
                    };
                }
            
                var endpoint = string.Format(APICallHelper.AffiliateUpload, Uri.EscapeDataString(affiliateUpload.affiliateId));
                var result = await _apiHelper.UploadBulkCashPaymentFileAsync<ServiceResponse<AffiliateFileUploadResponse>>(affiliateUpload.file, endpoint);
                return result;
            }
            catch (Exception ex)
            {
                return new ApiResponse<ServiceResponse<AffiliateFileUploadResponse>>
                {
                    IsSuccess = false,
                    ApiResponseData = null,
                    Message = $"❌ Error while uploading file: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<ServiceResponse<BranchFileUploadResponse>>> BranchUpload(FileUpload BranchUpload)
        {
            try
            {
                if (BranchUpload.file == null)
                {
                    return new ApiResponse<ServiceResponse<BranchFileUploadResponse>>
                    {
                        IsSuccess = false,
                        ApiResponseData = null,
                        Message = "❌ No file provided."
                    };
                }

                var endpoint = $"{APICallHelper.BranchUpload}";
                var result = await _apiHelper.UploadAccountingV2FileAsync<ServiceResponse<BranchFileUploadResponse>>(BranchUpload.file, BranchUpload.branchId, endpoint);

                return result;
            }
            catch (Exception ex)
            {
                return new ApiResponse<ServiceResponse<BranchFileUploadResponse>>
                {
                    IsSuccess = false,
                    ApiResponseData = null,
                    Message = $"❌ Error while uploading file: {ex.Message}"
                };
            }
        }


        public async Task<List<Accountwaiting>> GetAllAsync()
        {
            try
            {
                var url = APICallHelper.GetAllawaitingcorrespondance;
                var response = await _apiHelper.GetAsync<ResponseObject<List<Accountwaiting>>>(url);
                return response?.ApiResponseData?.Data ?? new List<Accountwaiting>();
            }
            catch (Exception ex)
            {
                // Log ex
                return new List<Accountwaiting>();
            }
        }

        /// <summary>
        /// Gets the full details of a single file. Used for the read-only Details page/preview.
        /// </summary>
        public async Task<Accountwaiting> GetByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id)) return null;
                var endpoint = string.Format(APICallHelper.GetawaitingcorrespondanceById, Uri.EscapeDataString(id));
                var response = await _apiHelper.GetAsync<ResponseObject<Accountwaiting>>(endpoint);
                var awt= response?.ApiResponseData?.Data;

                awt.BranchName = await GetBranchNameAsync(awt?.BranchId);

                return awt;
            }
            catch (Exception ex)
            {
                // Log ex
                return null;
            }
        }

        public async Task<string> GetBranchNameAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            var getAllBranches= await _branchServices.GetBranches();

            return (getAllBranches.Where(x => x.Id == id).FirstOrDefault())?.Name;
        }

        public async Task<Accountwaiting> GetAccountAwaitingCorrespondence(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id)) return null;
                var endpoint = string.Format(APICallHelper.GetawaitingcorrespondanceById, Uri.EscapeDataString(id));
                var response = await _apiHelper.GetAsync<ResponseObject<Accountwaiting>>(endpoint);
                return response?.ApiResponseData?.Data;
            }
            catch (Exception ex)
            {
                // Log ex
                return null;
            }
        }

        public async Task<ExecutionMessages> CreateAsync(Accountwaiting model)
        {
            try
            {

                var response = await _apiHelper.PostAsync<ServiceResponse<Accountwaiting>>(APICallHelper.createawaitingcorrespondanceRequest, model);

                // CORRECTED: Pass the ServiceResponse object to GetExecutionMessages
                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.Name, MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.Name, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> CreateCorrespondanceRequestAsync(AddCorrespondenceRequestModel model)
        {
            try
            {
                var response = await _apiHelper.PostAsync<ServiceResponse<Accountwaiting>>(string.Format(APICallHelper.CreateCorrespondanceRequest,GetUserLanguage()), model);

                // CORRECTED: Pass the ServiceResponse object to GetExecutionMessages
                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.Type, MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model.Type, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.Type, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateAsync(Accountwaiting model)
        {
            try
            {
                string Id = model.Id;
                string formattedUrl = string.Format(APICallHelper.Updateawaitingcorrespondance, Id);
                var response = await _apiHelper.PutAsync<ServiceResponse<CategoryConfig>>(formattedUrl, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.Name, MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.Name, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> DeactivateAsync(string categoryId)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.Deactivateawaitingcorrespondance, categoryId);
                var response = await _apiHelper.DeleteAsync<ServiceResponse<bool>>(formattedUrl);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, $"Category ID: {categoryId}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData?.Message ?? "Category deactivated successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, $"Category ID: {categoryId}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message ?? "Failed to deactivate category.");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"Category ID: {categoryId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> ApproveOrRejectCorrespondenceRequest(CorrespondenceApproveAndRejection model)
        {
            try
            {

               if (model.Type == "Approval")
                {
                    return await ApproveCorresponceRequestAsync(model);
                }

                if (model.Type == "Rejection")
                {
                    model.RejectionReason = model.Note;
                    return await RejectCorrespondenceRequestAsync(model);
                }


                GetExecutionMessages(model, false, "Correspondence Action", MessagesResults.Error,
                    ExecutionProcessOption.UnknownError, "Error", null, "Action not found");

            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, "Correspondence Action", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, "Error", ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> ApproveCorresponceRequestAsync(CorrespondenceApproveAndRejection model)
        {
            try
            {

                string url;
                model.Language = GetLanguage();

                url = string.Format( APICallHelper.ApproveCorrespondence,model.Id, model.Language);
                // Expect a detailed object back from the API (adjust generic type to your response DTO)
                var response = await _apiHelper.PutAsync<ServiceResponse<CorrespondenceRequestDto>>(url, model);

                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    // success: attach returned details as the Data of ExecutionMessages
                    GetExecutionMessages(response.ApiResponseData.Data, true, "Correspondence Action", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, "Success", null, "Action completed successfully.");
                }
                else
                {
                    // failure: bubble message
                    var message = response?.Message ?? "Action failed";
                    GetExecutionMessages(model, false, "Correspondence Action", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, "Failed", null, message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, "Correspondence Action", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, "Error", ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> RejectCorrespondenceRequestAsync(CorrespondenceApproveAndRejection model)
        {
            try
            {
                //var payload = model.Id;
                model.Language = GetLanguage();
                var url = string.Format(APICallHelper.RejectCorrespondence, model.Id, model.Language);

                var response = await _apiHelper.PutAsync<ServiceResponse<CorrespondenceRequestDto>>(url, model);

                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, "Reject Correspondence", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, "Success", null, "Correspondence rejected successfully.");
                }
                else
                {
                    var message = response?.Message ?? "Reject operation failed";
                    GetExecutionMessages(model, false, "Reject Correspondence", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, "Failed", null, message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, "Reject Correspondence", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, "Error", ex, ex.Message);
            }

            return ExecutionMessage;
        }

        // Correspondance DETAILS 
        public async Task<CorrespondenceRequestDto> GetCorrespondanceRequestByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);
                string formattedUrl = string.Format(APICallHelper.GetCorrespondanceRequest, encodedId,GetUserLanguage());

                var response = await _apiHelper.GetAsync<ServiceResponse<CorrespondenceRequestDto>>(formattedUrl);

                if (response.IsSuccess)
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

