using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.AccountingYear;
using CBS.FrontDesk.Data.Entity.AccountingV2.CashReconciliation;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.AccountingYear
{
    public class AccountingYearService : BaseService
    {

        private readonly ApiCallerHelper _accountingYearapiCallerHelper;


        public AccountingYearService()
        {

            _accountingYearapiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());

        }


        public async Task<List<accountingyear>> GetAllAsync()
        {
            try
            {
                var apiResponse = await _accountingYearapiCallerHelper.GetAsync<ResponseObject<List<accountingyear>>>(APICallHelper.GetAllAccoutingYear);

                if (apiResponse == null || apiResponse.ApiResponseData == null)
                    return new List<accountingyear>();

                return apiResponse.ApiResponseData.Data ?? new List<accountingyear>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetAllAsync] Error fetching all Accounting Year: {ex.Message}");
                return new List<accountingyear>();
            }
        }
        public async Task<List<CreateYear>> GetAllYearAsync()
        {
            try
            {
                var apiResponse = await _accountingYearapiCallerHelper.GetAsync<ResponseObject<List<CreateYear>>>(APICallHelper.GetAllYear);

                if (apiResponse == null || apiResponse.ApiResponseData == null)
                    return new List<CreateYear>();

                return apiResponse.ApiResponseData.Data ?? new List<CreateYear>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetAllAsync] Error fetching all  Year: {ex.Message}");
                return new List<CreateYear>();
            }
        }



        public async Task<accountingyear> GetData(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return new accountingyear();

                var apiResponse = await _accountingYearapiCallerHelper.GetAsync<ResponseObject<accountingyear>>(string.Format(APICallHelper.GetAccountyearById,id));

                if (apiResponse == null || apiResponse.ApiResponseData == null)
                    return new accountingyear();

                return apiResponse.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Optional: log error for diagnostics
                System.Diagnostics.Debug.WriteLine($"[GetData] Error fetching Accounting Year: {ex.Message}");
                return new accountingyear();
            }
        }

        public async Task<ExecutionMessages> Create(accountingyear model)
        {
            try
            {


                var response = await _accountingYearapiCallerHelper.PostAsync<ServiceResponse<accountingyear>>(APICallHelper.SaveAccountingYear, model);

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

        public async Task<ExecutionMessages> UpdateAccoutingYearAsync(accountingyear model)
        {
            try
            {
               

                
                var url = string.Format( APICallHelper.UpdateAccountingYear, model.Id);

                // Send model to API via PUT
                var response = await _accountingYearapiCallerHelper.PutAsync<ServiceResponse<accountingyear>>(url, model);

                if (response.IsSuccess)
                {
                    // Success execution message
                    GetExecutionMessages(
                        response?.ApiResponseData?.Data, true,null,
                        MessagesResults.Success, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),null,
                        response.ApiResponseData?.Message
                    );
                }
                else
                {
                    // Failure execution message
                    GetExecutionMessages(model, false, null,
                        MessagesResults.Failed,ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message
                    );
                }
            }
            catch (Exception ex)
            {
                // Exception execution message
                GetExecutionMessages( model, false, null, MessagesResults.Error, 
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message
                );
            }

            return ExecutionMessage; // Return accumulated execution result
        }
        public async Task<CustomDataTable2> GetAccountingYearDaTableAsync(AccoutingyearQuery query)
        {
            try
            {



                query.Options.sortColumnName = "";
                query.Options.sortColumnDirection = "";

                var response = await _accountingYearapiCallerHelper.PostAsync<ResponseObject<CustomDataTable2>>(
                    APICallHelper.GetAccoutingYearDataTable, query);

                if (!response.IsSuccess)
                    throw new Exception($"API call failed: {response.Message}");

                if (response.ApiResponseData == null)
                    throw new Exception("API returned null data");

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Error (Accounting Year): {ex.Message}");
                throw new Exception($"Accounting Year service unavailable: {ex.Message}", ex);
            }
        }

        public async Task<ExecutionMessages> AddAsync(CreateYear model)
        {
            try
            {


                var response = await _accountingYearapiCallerHelper.PostAsync<ServiceResponse<CreateYear>>(APICallHelper.AddYear, model);

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


        public async Task<ExecutionMessages> UpdateAsync(CreateYear model)
        {
            try
            {



                var url = string.Format(APICallHelper.UpdateYear, model.Id);

                // Send model to API via PUT
                var response = await _accountingYearapiCallerHelper.PutAsync<ServiceResponse<CreateYear>>(url, model);

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


        public async Task<CustomDataTable2> GetDaTableAsync(CreateYearQuery query)
        {
            try
            {



                query.Options.sortColumnName = "";
                query.Options.sortColumnDirection = "";

                var response = await _accountingYearapiCallerHelper.PostAsync<ResponseObject<CustomDataTable2>>(
                    APICallHelper.GetDataTable, query);

                if (!response.IsSuccess)
                    throw new Exception($"API call failed: {response.Message}");

                if (response.ApiResponseData == null)
                    throw new Exception("API returned null data");

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Error (Accounting Year): {ex.Message}");
                throw new Exception($"Accounting Year service unavailable: {ex.Message}", ex);
            }
        }


        public async Task<CreateYear> GetyearData(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return new CreateYear();

                var apiResponse = await _accountingYearapiCallerHelper.GetAsync<ResponseObject<CreateYear>>(string.Format(APICallHelper.GetyearById, id));

                if (apiResponse == null || apiResponse.ApiResponseData == null)
                    return new CreateYear();

                return apiResponse.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Optional: log error for diagnostics
                System.Diagnostics.Debug.WriteLine($"[GetData] Error fetching Accounting Year: {ex.Message}");
                return new CreateYear();
            }
        }
    }
}
