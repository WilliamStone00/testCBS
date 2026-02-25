using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.API.IPSReporting;

using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.IPS.IPSReporting
{
    public class IPSReportService : BaseService
    {
        private readonly ApiCallerHelper _ipsApiHelper;

        public IPSReportService()
        {
            string ipsBaseUrl = ConfigurationManager.AppSettings["TransactionBaseUrl"];
            if (string.IsNullOrEmpty(ipsBaseUrl))
            {
                throw new ConfigurationErrorsException("The 'TransactionBaseUrl' appSetting is missing or empty in Web.config.");
            }
            _ipsApiHelper = new ApiCallerHelper(ipsBaseUrl);
        }

        public async Task<InsurancePremiumData> GetCombinedInsurancePremiums(InsurancePremiumDto filter)
        {

         



            try
            {
                string formattedUrl = string.Format(APICallHelper.ipspremium);

                var response = await _ipsApiHelper
                    .PostAsync<ResponseObject<InsurancePremiumData>>(formattedUrl, filter);

                // =========================
                // SUCCESS
                // =========================
                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    GetExecutionMessages(
                        filter,
                        true,
                        filter.BranchId,
                        MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages,
                        SystemMessageStatus.Success.ToString(),
                        null,
                        response.ApiResponseData.Message
                    );

                    return response.ApiResponseData.Data;
                }

                // =========================
                // API FAILURE
                // =========================
                GetExecutionMessages(
                    filter,
                    false,
                    filter.BranchId,
                    MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages,
                    SystemMessageStatus.Failed.ToString(),
                    null,
                    response?.ApiResponseData?.Message ?? response?.Message
                );

                return new InsurancePremiumData();
            }
            catch (Exception ex)
            {
                // =========================
                // EXCEPTION
                // =========================
                GetExecutionMessages(
                    filter,
                    false,
                    filter.BranchId,
                    MessagesResults.Error,
                    ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Error.ToString(),
                    ex,
                    ex.Message
                );

                return new InsurancePremiumData();
            }
        }
    }
}