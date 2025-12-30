using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem
{
    public class CategoryConfigService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;

        public CategoryConfigService()
        {
            string baseUrl = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The 'CheckbookServiceBaseUrl' appSetting is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<IEnumerable<CategoryConfig>> GetCategoriesAsync()
        {
            try
            {
                // CORRECTED: The helper returns an ApiResponse which contains the ServiceResponse
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<CategoryConfig>>>(APICallHelper.GetAllChequeBookCategories);

                // CORRECTED: Access the final payload via .ApiResponseData.Data
                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<CategoryConfig>();
            }
            catch (Exception ex)
            {
                // In a real scenario, log 'ex'
                throw;
            }
        }

        public async Task<CustomDataTable> GetcategoryDataTableAsync(CategoryConfigQuery query)
        {
            try
            {
                if (!IsHeadOffice())
                {
                    query.BranchId = GetBranchID();
                }

                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.categorydatatable, query);

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

        public async Task<CategoryConfig> GetCategoryByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);
                string formattedUrl = string.Format(APICallHelper.GetChequeBookCategoryById, encodedId);
                // formattedUrl => "/api/v1/get-checkbook-category/123" (no colon)

                var response = await _apiCallerHelper.GetAsync<ServiceResponse<CategoryConfig>>(formattedUrl);

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

       

        public async Task<IEnumerable<CategoryConfig>> GetCategories()
        {
            try
            {
                var response = await _apiCallerHelper.GetAsync<ResponseObject<List<CategoryConfig>>>(APICallHelper.GetAllChequeBookCategories);
                var categories = response?.ApiResponseData?.Data ?? new List<CategoryConfig>();

                // If you have a different permission check for categories, replace IsHeadOffice() accordingly
                if (!IsHeadOffice())
                {
                    // Example: limit categories based on user's branch or permission.
                    string currentBranchId = GetBranchID();
                    // adjust filter logic if categories aren't branch-scoped
                    categories = categories.Where(c => c.branchID == currentBranchId).ToList();
                }
                else
                {
                    // Add "All" option as the default for head-office users
                    var defaultCategory = new CategoryConfig
                    {
                        id = "All",
                        name = "All Categories"
                    };
                    categories.Insert(0, defaultCategory);
                }

                // Format display Name and order by Code (adjust property names if needed)
                return categories
                     .Select(category =>
                     {
                         category.name = $"[{category.id}]-[{category.name}]-[{category.basePrice}]-[{category.numberofCheckBooks}]";
                         return category;
                     })
                     .OrderBy(category => category.id)
                     .ToList();
            }
            catch (Exception)
            {
                // log if you have a logger, then rethrow or return empty list
                // _logger.LogError(ex, "Failed getting categories");
                throw;
            }
        }

       
        public async Task<ExecutionMessages> CreateCategoryAsync(CategoryConfig model)
        {
            try
            {
                model.bankId = GetBankID();
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<CategoryConfig>>(APICallHelper.CreateChequeBookCategory, model);

                // CORRECTED: Pass the ServiceResponse object to GetExecutionMessages
                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.name, MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model.name, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.name, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateCategoryAsync(CategoryConfig model)
        {
            try
            {
                var catid = model.id;
                model.bankId = GetBankID();
                string formattedUrl = string.Format(APICallHelper.UpdateChequeBookCategory, catid);
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<CategoryConfig>>(formattedUrl, model);

                if (response.IsSuccess)
                {
                        GetExecutionMessages(response.ApiResponseData.Data, true, model.name, MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model.name, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.name, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> DeactivateCategoryAsync(string categoryId)
        {
            try
            {

                string formattedUrl = string.Format(APICallHelper.DeactivateChequeBookCategory, categoryId);
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



        public async Task<CustomDataTable> GetDataTableAsync(GetAllUsersDataTableQuery getAllUsersDataTableQuery)
        {
            if (!IsHeadOffice())
            {
                getAllUsersDataTableQuery.BranchId = GetBranchID();
            }
            var couApiResponse = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.LoadDataTablePagginationForUsers,
                getAllUsersDataTableQuery
            );

            // Return response if successful
            if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
            {
                return couApiResponse.ApiResponseData.Data;
            }

            // Return an empty DataTable if the request fails
            return new CustomDataTable(
                draw: Convert.ToInt32(getAllUsersDataTableQuery.DataTableOptions.draw),
                recordsTotal: 0,
                recordsFiltered: 0,
                data: new List<object>(), // No data
                dataTableOptions: getAllUsersDataTableQuery.DataTableOptions
            );
        }      

    }
}
