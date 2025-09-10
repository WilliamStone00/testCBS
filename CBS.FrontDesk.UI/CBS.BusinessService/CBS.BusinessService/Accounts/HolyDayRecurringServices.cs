
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.EMMA;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounts
{
   
    public class HolyDayRecurringServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;

        public HolyDayRecurringServices()
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());

        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objHolyDayRecurring = await GetHolyDayRecurring(id);
                var inResponse = await _transactionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_HolyDayRecurring, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objHolyDayRecurring.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objHolyDayRecurring, false, $"{objHolyDayRecurring.Name}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<HolyDayRecurring>> GetHolyDayRecurrings()
        {
            try
            {
                var model = new GetAllMobileMoneyCashTopupQuery { BranchId = GetBranchID(), ByBranch = true };
                if (IsHeadOffice())
                {
                    model.ByBranch = false;
                    model.BranchId = "N/A";
                }

                var couApiResponse = await _transactionApiHelper.PostAsync<ServiceResponse<List<HolyDayRecurring>>>(APICallHelper.GetAllHolyDayRecurring, model);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<HolyDayRecurring>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<HolyDayRecurring> GetHolyDayRecurring(string id)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<HolyDayRecurring>>(string.Format(APICallHelper.Get_Update_Delete_HolyDayRecurring, id));
                if (cusResponseObject.IsSuccess)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ExecutionMessages> Create(HolyDayRecurring model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<HolyDayRecurring>>(APICallHelper.CreateHolyDayRecurring, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
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
        public async Task<ExecutionMessages> Update(HolyDayRecurring model)
        {
            try
            {
                var HolyDayRecurring = await GetHolyDayRecurring(model.Id);
                if (HolyDayRecurring != null)
                {
                    HolyDayRecurring.HolidayType = model.HolidayType;
                    HolyDayRecurring.RecurrencePattern = model.RecurrencePattern;
                    HolyDayRecurring.Description = model.Description;
                    HolyDayRecurring.BranchId = model.BranchId;
                    HolyDayRecurring.RecurringDay = model.RecurringDay;
                    HolyDayRecurring.Month = model.Month;
                    HolyDayRecurring.DayOfMonth = model.DayOfMonth;
                    HolyDayRecurring.DayOfMonth = model.DayOfMonth;
                    HolyDayRecurring.IsGlobal = model.IsGlobal;
                    HolyDayRecurring.Name = model.Name;
                    var response = await _transactionApiHelper.PutAsync<ServiceResponse<HolyDayRecurring>>(string.Format(APICallHelper.Get_Update_Delete_HolyDayRecurring, model.Id), HolyDayRecurring);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
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
