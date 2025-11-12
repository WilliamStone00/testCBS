using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.AccountingYear;
using CBS.FrontDesk.Data.Entity.AccountingV2.CashReconciliation;
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
                var apiResponse = await _accountingYearapiCallerHelper
                    .GetAsync<ResponseObject<List<accountingyear>>>(
                        APICallHelper.GetAllAccoutingYear
                    );

                if (apiResponse == null || apiResponse.ApiResponseData == null)
                    return new List<accountingyear>();

                return apiResponse.ApiResponseData.Data ?? new List<accountingyear>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetAllAsync] Error fetching all ConfigurationManualEntries: {ex.Message}");
                return new List<accountingyear>();
            }
        }



        public async Task<List<accountingyear>> GetAllAsyncs()
        {
            try
            {
                // Simulate async call delay (optional)
                await Task.Delay(200);

                // Return mock data
                var mockData = new List<accountingyear>
            {
                new accountingyear { Id = "AY-2023", BranchId = "BR001", Year = 2023, Status = "CLOS" },
                new accountingyear { Id = "AY-2024", BranchId = "BR001", Year = 2024, Status = "OPEN" },
                new accountingyear { Id = "AY-2025", BranchId = "BR002", Year = 2025, Status = "LOCK" }
            };

                return mockData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetAllAsync Mock] Error: {ex.Message}");
                return new List<accountingyear>();
            }
        }

        public async Task<accountingyear> GetData(string id)
        {
            try
            {
                await Task.Delay(100);

                // Reuse mock list and return one record
                var all = await GetAllAsyncs();
                return all.FirstOrDefault(x => x.Id == id) ?? new accountingyear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetData Mock] Error: {ex.Message}");
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



    }
}
