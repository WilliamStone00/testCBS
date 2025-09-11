
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.CashCeilingManagement;
using CBS.FrontDesk.Data.Entity.CashChangeManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.VaultManagement;
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
   
    public class CashChangeHistoryServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;

        public CashChangeHistoryServices()
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());

        }

      
      
        public async Task<IEnumerable<CashChangeHistory>> GetDenominationHistories()
        {
            try
            {
                GetCashChangeHistoryQuery getCashChangeHistory = new GetCashChangeHistoryQuery();
                if (!IsHeadOffice())
                {
                    getCashChangeHistory.BranchId=GetBranchID();
                }

                var queryString = ToQueryString(getCashChangeHistory);
                var fullUrl = $"{APICallHelper.GetDenominationHistories}?{queryString}";
                var response = await _transactionApiHelper.GetAsync<ResponseObject<List<CashChangeHistory>>>(fullUrl);
                var data = new List<CashChangeHistory>();
                if (response.ApiResponseData != null)
                {
                    data = response.ApiResponseData.Data;
                }
                return data;

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<CashChangeHistory>> GetDenominationHistories(GetCashChangeHistoryQuery getCashChangeHistory)
        {
            try
            {

                var queryString = ToQueryString(getCashChangeHistory);
                var fullUrl = $"{APICallHelper.GetDenominationHistories}?{queryString}";
                var response = await _transactionApiHelper.GetAsync<ResponseObject<List<CashChangeHistory>>>(fullUrl);
                var data = new List<CashChangeHistory>();
                if (response.ApiResponseData != null)
                {
                    data = response.ApiResponseData.Data;
                }
                return data;

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }


        public async Task<CashChangeHistory> GetDenominationHistoryById(string id)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<CashChangeHistory>>(string.Format(APICallHelper.GetDenominationHistoryById, id));
                if (cusResponseObject.IsSuccess)
                {
                    var data= cusResponseObject.ApiResponseData.Data;
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
        public async Task<ExecutionMessages> Create(CashChangeHistory model)
        {
            try
            {
                // Determine the API endpoint based on the action
                string apiEndpoint;
                switch (model.Action)
                {
                    case "vault":
                        apiEndpoint = APICallHelper.DenominationCashChangeVault;
                        break;
                    case "sub_teller":
                        apiEndpoint = APICallHelper.DenominationCashChangeSubTeller;
                        break;
                    case "primary_teller":
                        apiEndpoint = APICallHelper.DenominationCashChangePrimaryTeller;
                        break;
                    default:
                        // Handle unsupported actions
                        GetExecutionMessages(null, false, null, MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Invalid action.");
                        return ExecutionMessage;
                }

                // Call the API
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<CashChangeHistory>>(apiEndpoint, model.CashChangeCommand);

                // Process the response
                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exceptions
                GetExecutionMessages(null, false, null, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
            }

            return ExecutionMessage;
        }

    }

}
