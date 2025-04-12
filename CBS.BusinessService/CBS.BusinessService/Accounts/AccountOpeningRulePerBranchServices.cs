
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.AccountOpeningRulePerBranchP;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounts
{
   
    public class AccountOpeningRulePerBranchServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;

        public AccountOpeningRulePerBranchServices()
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());

        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var inResponse = await _transactionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Or_Delete_AccountOpeningRule, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true,null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(null, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        //public async Task<IEnumerable<AccountOpeningRulePerBranch>> GetAccountOpeningRulePerBranches(GetAllAccountOpeningRulesQuery openingRulesQuery )
        //{
        //    try
        //    {


        //        if (IsHeadOffice())
        //        {
        //            openingRulesQuery.BranchId = "N/A";
        //        }

        //        var couApiResponse = await _transactionApiHelper.PostAsync<ServiceResponse<List<HolyDay>>>(APICallHelper.GetAllAccountOpeningRule, openingRulesQuery);
        //        if (couApiResponse.IsSuccess)
        //        {
        //            return couApiResponse.ApiResponseData.Data;
        //        }
        //        return new List<HolyDay>();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log and handle exception
        //        throw;
        //    }
        //}
        public async Task<HolyDay> GetHolyDay(string id)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<HolyDay>>(string.Format(APICallHelper.Get_Update_Delete_HolyDay, id));
                if (cusResponseObject.IsSuccess)
                {
                    var data= cusResponseObject.ApiResponseData.Data;
                    data.DateFromStr = data.DateFrom.ToString();
                    data.DateToStr = data.DateTo.ToString();
                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ExecutionMessages> Create(HolyDay model)
        {
            try
            {
                if (model.IsCentralisedConfiguration)
                {
                    model.BranchId = "N/A";
                }
                model.DateFrom = GetDateTime(model.DateFromStr);
                model.DateTo = GetDateTime(model.DateToStr);
                // Make an API call to create an individual profile
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<HolyDay>>(APICallHelper.CreateHolyDay, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.EventName}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.EventName, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(HolyDay model)
        {
            try
            {
                var HolyDay = await GetHolyDay(model.Id);
                if (HolyDay != null)
                {
                    model.DateFrom = GetDateTime(model.DateFromStr);
                    model.DateTo = GetDateTime(model.DateToStr);
                    HolyDay.EventName = model.EventName;
                    HolyDay.DateTo = model.DateTo;
                    HolyDay.Description = model.Description;
                    if (model.IsCentralisedConfiguration)
                    {
                        HolyDay.BranchId = "N/A";
                    }
                    else
                    {
                        HolyDay.BranchId = model.BranchId;
                    }
    
                    HolyDay.DateFrom = model.DateFrom;
                    HolyDay.IsActive = model.IsActive;
                    HolyDay.IsCentralisedConfiguration = model.IsCentralisedConfiguration;
                    var response = await _transactionApiHelper.PutAsync<ServiceResponse<HolyDay>>(string.Format(APICallHelper.Get_Update_Delete_HolyDay, model.Id), HolyDay);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.EventName}", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.EventName, MessagesResults.Failed,
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
