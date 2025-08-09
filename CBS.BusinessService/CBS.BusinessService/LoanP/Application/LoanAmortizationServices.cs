using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.BusinessService
{
  
    public class LoanAmortizationServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public LoanAmortizationServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());

        }


        public async Task<IEnumerable<LoanAmortization>> GetLoanAmortizationByLoanID(string loanid)
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanAmortization>>>(string.Format(APICallHelper.GetAllLoanAmortizationByLoanIdQuery, loanid));
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<LoanAmortization>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<LoanAmortization> GetLoanAmortization(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<LoanAmortization>>(string.Format(APICallHelper.GetLoanAmortizationByID, id));
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
        public async Task<ExecutionMessages> GenerateLoanAmortizationSchedule(LoanParameters loanParameter)
        {
            try
            {
                // Assuming loanParameter is an instance of LoanParameters class

                // Use unary expression to assign RepaymentStartDate based on StrRepaymentStartDate
                loanParameter.RepaymentStartDate = string.IsNullOrEmpty(loanParameter.StrRepaymentStartDate)
                    ? DateTime.Now.AddMonths(1)
                    : GetDateTime(loanParameter.StrRepaymentStartDate);
                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ResponseObject<List<LoanAmortization>>>(APICallHelper.GenerateAmortizationSchedule, loanParameter);
                if (response.IsSuccess)
                {
                    HttpContext.Current.Session["loan_schedule"] = response.ApiResponseData.Data;
                    // Successful creation
                    GetExecutionMessages(response, true, $"Loan schedule", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(response, false, "Loan application", MessagesResults.Failed,
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

    }

}
