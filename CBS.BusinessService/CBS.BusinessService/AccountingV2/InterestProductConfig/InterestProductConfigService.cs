using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.EndOfYearClosure;
using CBS.FrontDesk.Data.Entity.AccountingV2.InterestProductConfig;
using CBS.FrontDesk.Data.Entity.BulkOperation;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.InterestProductConfig
{
    public class InterestProductConfigService : BaseService
    {

        private readonly ApiCallerHelper _apiCallerHelper;
        private readonly ApiCallerHelper _ProductapiCallerHelper;




        public InterestProductConfigService()
        {

            _apiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());
            _ProductapiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());

        }


        public async Task<ExecutionMessages> Create(ProductCalculationConfig model)
        {
            try
            {
                model.ProductType = "Savings";
                //model.ProductName = "Loan";
                //model.AccountingYearId = "00000000000000";
                //model.Command = "Create";
                if (model.BranchId == null)
                {
                    model.BranchId = "Global"; // Assuming BranchId is a string. If int?, handle differently.
                }
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<ProductCalculationConfig>>(APICallHelper.SaveInterestProductConfig, model);

                if (response.ApiResponseData != null)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.Message);
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

        public async Task<ExecutionMessages> UpdateAsync(ProductCalculationConfig model)
        {
            try
            {

                var url = string.Format(APICallHelper.UpdateInterestProductConfig, model.Id);

                // Send model to API via PUT
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<ProductCalculationConfig>>(url, model);

                if (response.IsSuccess)
                {
                    // Success execution message
                    GetExecutionMessages(
                        response?.ApiResponseData?.Data, true, null,
                        MessagesResults.Success, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData?.Message
                    );
                }
                else
                {
                    // Failure execution message
                    GetExecutionMessages(model, false, null,
                        MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message
                    );
                }
            }
            catch (Exception ex)
            {
                // Exception execution message
                GetExecutionMessages(model, false, null, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message
                );
            }

            return ExecutionMessage; // Return accumulated execution result
        }






        public async Task<CustomDataTable2> GetDataTableAsync(InterestProductConfigQuery query)
        {
            try
            {
                // If not head office and branchId is null, set it
                //if (!IsHeadOffice() && string.IsNullOrEmpty(query.BranchId))
                //{
                //    query.BranchId = GetBranchID();
                //}
                query.Options.pageSize = 10;
                query.Options.sortColumnName = "";
                query.Options.sortColumnDirection = "";

                var journalHeaders = (
                   JsonConvert.SerializeObject(query));

                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable2>>(
                    APICallHelper.GetInterestProductConfigDataTable, query);

                // If API call fails or response unsuccessful
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
                System.Diagnostics.Debug.WriteLine($" {ex.Message}");
                throw new Exception($" {ex.Message}", ex);
            }
        }

        public async Task<List<ProductInfo>> GetProductAsync()
        {
            try
            {
                // ✅ Make API call
                var response = await _ProductapiCallerHelper
                    .GetAsync<ResponseObject<List<ProductInfo>>>(APICallHelper.GetAllProducts);

                // ✅ Validate response
                if (!response.IsSuccess)
                    throw new Exception($" {response.Message}");

                var data = response.ApiResponseData?.Data;

                if (data == null || data.Count == 0)
                    throw new Exception("null ");

                return data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" {ex.Message}");
                throw;
            }
        }

        public async Task<ProductCalculationConfig> GetinterestProductConfig(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return new ProductCalculationConfig();

                var url = $"{APICallHelper.GetByIdInterestProductConfig}?id={id}";

                var apiResponse =
                    await _apiCallerHelper.GetAsync<ResponseObject<ProductCalculationConfig>>(url);

                if (apiResponse == null || apiResponse.ApiResponseData == null)
                    return new ProductCalculationConfig();

                return apiResponse.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Optional: log error for diagnostics
                System.Diagnostics.Debug.WriteLine($"[GetData] {ex.Message}");
                return new ProductCalculationConfig();
            }
        }

        



    }
}
