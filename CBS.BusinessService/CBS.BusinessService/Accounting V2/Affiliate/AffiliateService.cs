using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AffiliateAccount;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.Affiliate
{
    public class AffiliateService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;

        public AffiliateService()
        {
            //change the base url to the actual base url
            string baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The 'AccountingV2BaseUrl' appSetting is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<IEnumerable<Affiliateresponse>> GetAsync()
        {
            try
            {
                // CORRECTED: The helper returns an ApiResponse which contains the ServiceResponse
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<Affiliateresponse>>>(APICallHelper.GetAllAffiliate);

                // CORRECTED: Access the final payload via .ApiResponseData.Data
                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<Affiliateresponse>();
            }
            catch (Exception ex)
            {
                // In a real scenario, log 'ex'
                throw;
            }
        }

        public async Task<CustomDataTable> GetcategoryDataTableAsync(AffiliateQuery query)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.Affiliatedatatable, query);

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

        public async Task<Affiliateresponse> GetByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);
                string formattedUrl = string.Format(APICallHelper.GetAffiliateById, encodedId);

                var response = await _apiCallerHelper.GetAsync<ServiceResponse<Affiliateresponse>>(formattedUrl);

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

        public async Task<ExecutionMessages> CreateAsync(AffiliateCommand model)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<AffiliateCommand>>(APICallHelper.CreateAffiliate, model);

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

        public async Task<ExecutionMessages> UpdateAsync(AffiliateCommand model)
        {
            try
            {
                var catid = model.Id;
                string formattedUrl = string.Format(APICallHelper.UpdateAffiliate, catid);
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<AffiliateCommand>>(formattedUrl, model);

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

        public async Task<ExecutionMessages> DeleteAsync(string categoryId)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.DeactivateAffiliate, categoryId);
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

        public async Task<IEnumerable<Affiliateresponse>> GetAffiliatesAsync()
        {
            try
            {              
                var isActive = true;
                var includeDeleted = false;
                string formattedUrl = string.Format(APICallHelper.GetAffiliatedropId, isActive, includeDeleted);

                var response = await _apiCallerHelper.GetAsync<ResponseObject<List<Affiliateresponse>>>(formattedUrl);
                var affiliates = response?.ApiResponseData?.Data ?? new List<Affiliateresponse>();

                if (!IsHeadOffice())
                {
                    string currentBranchId = GetBranchID();
                    affiliates = affiliates.Where(a => a.Id == currentBranchId).ToList();
                }
                else
                {
                    var defaultAffiliate = new Affiliateresponse
                    {
                        Id = "All",
                        Name = "All Affiliates",
                        Code = "ALL",
                        IsActive = true
                    };
                    affiliates.Insert(0, defaultAffiliate);
                }

                // Optional: format name for display and order by Code
                return affiliates
                    .Select(a =>
                    {
                        a.Name = $"[{a.Code}] - {a.Name} {(a.IsHeadOffice ? "(Head Office)" : string.Empty)}";
                        return a;
                    })
                    .OrderBy(a => a.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
               throw;
            }
        }

    }
}

