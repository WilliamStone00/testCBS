using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.EndOfYearClosure;
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

namespace CBS.BusinessService.AccountingV2.EndOfYearClosure
{
    public class YearEndChecklistDefinitionService : BaseService
    {


        private readonly ApiCallerHelper _apiCallerHelper;

        public YearEndChecklistDefinitionService()
        {

            _apiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());

        }
        public async Task<CustomDataTable2> GetYearEndChecklistDefinitionDataTableAsync(YearEndChecklistStatusQuery query)
        {
            try
            {
                // If not head office and branchId is null, set it
                if (!IsHeadOffice() && string.IsNullOrEmpty(query.BranchId))
                {
                    query.BranchId = GetBranchID();
                }

                query.Options.sortColumnName = "";
                query.Options.sortColumnDirection = "";

                var journalHeaders = (
                   JsonConvert.SerializeObject(query));

                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable2>>(
                    APICallHelper.GetCheckListDefinitionDataTable, query);

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
                System.Diagnostics.Debug.WriteLine($"API Error (Journal Header): {ex.Message}");
                throw new Exception($"Journal header service unavailable: {ex.Message}", ex);
            }
        }


        public async Task<EndOfYearConfig> GetEndofYearConfig(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return new EndOfYearConfig();

                var apiResponse = await _apiCallerHelper.GetAsync<ResponseObject<EndOfYearConfig>>(string.Format(APICallHelper.GetEndofYearConfigById, id));

                if (apiResponse == null || apiResponse.ApiResponseData == null)
                    return new EndOfYearConfig();

                return apiResponse.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Optional: log error for diagnostics
                System.Diagnostics.Debug.WriteLine($"[GetData] Error fetching Accounting Year: {ex.Message}");
                return new EndOfYearConfig();
            }
        }


        public async Task<ExecutionMessages> Create(EndOfYearConfig model)
        {
            try
            {


                var response = await _apiCallerHelper.PostAsync<ServiceResponse<EndOfYearConfig>>(APICallHelper.SaveEndofYearConfig, model);

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

        public async Task<ExecutionMessages> UpdateAsync(EndOfYearConfig model)
        {
            try
            {



                var url = string.Format(APICallHelper.UpdateEndofYearConfig, model.Id);

                // Send model to API via PUT
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<EndOfYearConfig>>(url, model);

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





    }
}
