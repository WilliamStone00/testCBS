using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
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

        public async Task<CategoryConfig> GetCategoryByIdAsync(string categoryId)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.GetChequeBookCategoryById, categoryId);
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<CategoryConfig>>(formattedUrl);

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

        public async Task<ExecutionMessages> CreateCategoryAsync(CategoryConfig model)
        {
            try
            {
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
                var catid = model.Id;
                string formattedUrl = string.Format(APICallHelper.UpdateChequeBookCategory,catid);
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<CategoryConfig>>(formattedUrl, model);

                // CORRECTED: Pass the ServiceResponse object to GetExecutionMessages
                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.name, MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, "Update was Succesful");
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
                if (string.IsNullOrEmpty(APICallHelper.DeactivateChequeBookCategory))
                    throw new InvalidOperationException("DeactivateChequeBookCategory URL is not configured.");

                string formattedUrl = string.Format(APICallHelper.DeactivateChequeBookCategory, categoryId);

                if (_apiCallerHelper == null)
                    throw new InvalidOperationException("_apiCallerHelper is not initialized.");

                var response = await _apiCallerHelper.DeleteAsync<ServiceResponse<bool>>(formattedUrl);

                if (response == null)
                    throw new InvalidOperationException("API returned null response.");

                //if (response.IsSuccess)
                //{
                //    GetExecutionMessages(null, true, $"Category ID: {categoryId}", MessagesResults.Success,
                //        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null,
                //        response.ApiResponseData?.Message ?? "Category deleted successfully.");
                //}
                //else
                //{
                //    GetExecutionMessages(null, false, $"Category ID: {categoryId}", MessagesResults.Failed,
                //        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null,
                //        response.ApiResponseData?.Message ?? response.Message ?? "Unknown error.");
                //}

            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"Category ID: {categoryId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage ?? new ExecutionMessages
            {
                MessageString = "No execution message was created.",
                MessageStatus = MessagesResults.Error.ToString()
            };
        }


        public async Task<CustomDataTable> GetDataTableAsync(GetAllUsersDataTableQuery getAllUsersDataTableQuery)
        {
            if (!IsHeadOffice())
            {
                getAllUsersDataTableQuery.BranchId = GetBranchID();
            }
            // Make API call to fetch the DataTable result
            // nb changed the _apiCallerHelper to the actual base url
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

        public List<CategoryConfig> MapToUserDownloadDtos(IEnumerable<CategoryConfig> users)
        {
            return users.Select(MapToUserDownloadDto).ToList();
        }
        public CategoryConfig MapToUserDownloadDto(CategoryConfig cat)
        {
            return new CategoryConfig
            {
                name = cat.name,
                basePrice = cat.basePrice,
                numberOfPages = cat.numberOfPages,
                issuanceLimitPerCustomerType = cat.issuanceLimitPerCustomerType,
                validityPeriodInMonths = cat.validityPeriodInMonths,
                isActive = cat.isActive,
            };
        }

        //public async Task<ExecutionMessages> DeleteServiceAsync(string serviceId)
        //{
        //    try
        //    {
        //        var url = string.Format(APICallHelper.DeleteInterestServiceUrl, serviceId);
        //        var response = await _apiCallerHelper.DeleteAsync<ResponseObject<bool>>(url);
        //        if (response.IsSuccess)
        //        {
        //            // FIX: Matched the GetExecutionMessages signature.
        //            GetExecutionMessages(null, true, "Service", MessagesResults.Success,
        //                ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, response.Message);
        //        }
        //        else
        //        {
        //            GetExecutionMessages(null, false, "Service", MessagesResults.Failed,
        //                ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, response.Message);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        GetExecutionMessages(null, false, "Service Deletion", MessagesResults.Error,
        //            ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex);
        //    }
        //    return ExecutionMessage;
        //}
    }
}
