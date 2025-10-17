using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AffiliateAccount;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.AffiliateAccounts
{
    public class AffiliateAccountService : BaseService
    {

          private readonly ApiCallerHelper _apiCallerHelper;

            public AffiliateAccountService()
            {
                //change the base url to the actual base url
                string baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new ConfigurationErrorsException("The 'AccountingV2BaseUrl' appSetting is missing or empty in Web.config.");
                }
                _apiCallerHelper = new ApiCallerHelper(baseUrl);
            }

            public async Task<IEnumerable<AffiliateAccountDto>> GetAsync()
            {
                try
                {
                    // CORRECTED: The helper returns an ApiResponse which contains the ServiceResponse
                    var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<AffiliateAccountDto>>>(APICallHelper.GetAllAffiliateAccount);

                    // CORRECTED: Access the final payload via .ApiResponseData.Data
                    if (response.IsSuccess && response.ApiResponseData?.Data != null)
                    {
                        return response.ApiResponseData.Data;
                    }
                    return new List<AffiliateAccountDto>();
                }
                catch (Exception ex)
                {
                    // In a real scenario, log 'ex'
                    throw;
            }
        }

        public async Task<CustomDataTable> GetcategoryDataTableAsync(AffiliateAccountQuery query)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.AffiliateAccountdatatable, query);

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

        //tree structure 
        public async Task<Affiliateresponse> GetByIdAsync(string id)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(id))
                        throw new ArgumentException("id is required", nameof(id));

                    var encodedId = Uri.EscapeDataString(id);
                    string formattedUrl = string.Format(APICallHelper.GetAffiliateAccountById, encodedId);
                    // formattedUrl => "/api/v1/get-checkbook-category/123" (no colon)

                    var response = await _apiCallerHelper.GetAsync<ServiceResponse<Affiliateresponse>>(formattedUrl);

                    //string formattedUrl = string.Format(APICallHelper.GetChequeBookCategoryById, id);
                    //var response = await _apiCallerHelper.GetAsync<ServiceResponse<CategoryConfig>>(formattedUrl);

                    // CORRECTED: Access the final payload via .ApiResponseData.Data
                    if (response.IsSuccess)
                    {
                        return response.ApiResponseData?.Data;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    // In a real scenario, log 'ex'
                    throw;
                }
            }

        // real endpoint version
        public async Task<IEnumerable<Affiliateresponse>> GetAffiliatesFromEndpointAsync()
        {
            try
            {
                // Call API
                var response = await _apiCallerHelper.GetAsync<ResponseObject<List<Affiliateresponse>>>(APICallHelper.GetAllAccountDropAffiliate);
                var affiliates = response?.ApiResponseData?.Data ?? new List<Affiliateresponse>();

                // Only active ones (mirrors mock's GetAsync which returns only IsActive)
                affiliates = affiliates.Where(a => a.IsActive).ToList();

                if (!IsHeadOffice())
                {
                    // Filter only the affiliate that matches current branch (if applicable)
                    string currentBranchId = GetBranchID();
                    affiliates = affiliates.Where(a => a.Id == currentBranchId).ToList();
                }
                else
                {
                    // Add "All" option at the top for Head Office users
                    var defaultAffiliate = new Affiliateresponse
                    {
                        Id = "All",
                        Name = "All Affiliates",
                        Code = "ALL",
                        IsActive = true
                    };
                    // ensure we don't duplicate if API already returns such entry
                    if (!affiliates.Any(x => string.Equals(x.Id, defaultAffiliate.Id, StringComparison.OrdinalIgnoreCase)))
                    {
                        affiliates.Insert(0, defaultAffiliate);
                    }
                }

                // Format name for display and order by Code
                return affiliates
                    .Select(a =>
                    {
                        a.Name = $"[{a.Code}] - {a.Name} {(a.IsHeadOffice ? "(Head Office)" : string.Empty)}".Trim();
                        return a;
                    })
                    .OrderBy(a => a.Code)
                    .ToList();
            }
            catch (Exception)
            {
                // Consider logging: _logger.LogError(ex, "GetAffiliatesFromEndpointAsync failed");
                throw;
            }
        }



        public async Task<ExecutionMessages> CreateAsync(AddAffiliateAccountCommand model)
            {
                try
                {
                    var response = await _apiCallerHelper.PostAsync<ServiceResponse<AffiliateCommand>>(APICallHelper.CreateAffiliateAccount, model);

                    // CORRECTED: Pass the ServiceResponse object to GetExecutionMessages
                    if (response.IsSuccess)
                    {
                        GetExecutionMessages(response.ApiResponseData.Data, true, model.NameEn, MessagesResults.Success,
                            ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                    }
                    else
                    {
                        GetExecutionMessages(model, false, model.NameEn, MessagesResults.Failed,
                            ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                    }
                }
                catch (Exception ex)
                {
                    GetExecutionMessages(model, false, model.NameEn, MessagesResults.Error,
                        ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                }
                return ExecutionMessage;
            }

            public async Task<ExecutionMessages> UpdateAsync(AddAffiliateAccountCommand model)
            {
                try
                {
                    var catid = model.Id;
                    string formattedUrl = string.Format(APICallHelper.UpdateAffiliateAccount, catid);
                    var response = await _apiCallerHelper.PutAsync<ServiceResponse<AffiliateCommand>>(formattedUrl, model);

                    if (response.IsSuccess)
                    {
                        GetExecutionMessages(response.ApiResponseData.Data, true, model.NameEn, MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                    }
                    else
                    {
                        GetExecutionMessages(model, false, model.NameEn, MessagesResults.Failed,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                    }
                }
                catch (Exception ex)
                {
                    GetExecutionMessages(model, false, model.NameEn, MessagesResults.Error,
                        ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                }
                return ExecutionMessage;
            }

            public async Task<ExecutionMessages> DeleteAsync(string categoryId)
            {
                try
                {
                    string formattedUrl = string.Format(APICallHelper.DeactivateAffiliateAccount, categoryId);
                    var response = await _apiCallerHelper.DeleteAsync<ServiceResponse<bool>>(formattedUrl);

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

        }
    }



