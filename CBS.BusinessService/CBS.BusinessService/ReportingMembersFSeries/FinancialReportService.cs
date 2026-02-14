using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.ReportMembersFSeries;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.ReportingMembersFSeries
{

  public class FinancialReportService : BaseService
        {
            private readonly ApiCallerHelper _transactionApiHelper;
            private readonly ApiCallerHelper _loanApiHelper;
            private readonly ApiCallerHelper _accountApiHelper;
          //  private readonly ApiCallerHelper _memberApiHelper;

            public FinancialReportService()
            {
                // Initialize API helpers with respective base URLs
                string transactionUrl = ConfigurationManager.AppSettings["TransactionBaseUrl"];
                string loanUrl = ConfigurationManager.AppSettings["LoanBaseUrl"];
              //  string accountUrl = ConfigurationManager.AppSettings["AccountServiceBaseUrl"];

                _transactionApiHelper = new ApiCallerHelper(transactionUrl);
                _loanApiHelper = new ApiCallerHelper(loanUrl);
            // _accountApiHelper = new ApiCallerHelper(accountUrl);
            //  _memberApiHelper = new ApiCallerHelper(accountUrl);
        }

        public async Task<AccountStatementResponseDto>
     GetCustomerTransactionsByAccountNumber(FinancialReportFilter parameters)
        {
            try
            {
                var request = new FinancialReportRequest
                {
                    Filter = parameters
                };

                string endpoint = APICallHelper.GetTransactionHistoryByAccountNumber3;

                var response = await _transactionApiHelper
                    .PostAsync<ResponseObject<FinancialReportResponseDto>>(endpoint, request);

                // =========================
                // SUCCESS
                // =========================
                if (response != null && response.IsSuccess)
                {
                    GetExecutionMessages(
                        parameters,
                        true,
                        parameters.MemberReference,
                        MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages,
                        SystemMessageStatus.Success.ToString(),
                        null,
                        response.ApiResponseData?.Message
                    );

                    // ✅ RETURN THE WHOLE ACCOUNT STATEMENT (SUMMARY + TRANSACTIONS)
                    return response.ApiResponseData?.Data?.AccountStatement
                           ?? new AccountStatementResponseDto();
                }

                // =========================
                // API FAILURE (Handled)
                // =========================
                GetExecutionMessages(
                    parameters,
                    false,
                    parameters.MemberReference,
                    MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages,
                    SystemMessageStatus.Failed.ToString(),
                    null,
                    response?.ApiResponseData?.Message ?? response?.Message
                );

                return new AccountStatementResponseDto();
            }
            catch (Exception ex)
            {
                // =========================
                // EXCEPTION
                // =========================
                GetExecutionMessages(
                    parameters,
                    false,
                    parameters.MemberReference,
                    MessagesResults.Error,
                    ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Error.ToString(),
                    ex,
                    ex.Message
                );

                return new AccountStatementResponseDto();
            }
        }

      
        public async Task<MemberSituationBackendResponse> GetMemberSituationRaw(FinancialReportFilter parameters)
        {

            var request = new FinancialReportRequest
            {
                Filter = parameters
            };

            string endpoint = APICallHelper.GetTransactionHistoryByAccountNumber3;

            var response = await _transactionApiHelper
                .PostAsync<ResponseObject<MemberSituationBackendResponse>>(endpoint, request);         

            return response.ApiResponseData.Data;
        }


        public async Task<LoanRepaymentData> GetLoanRepaymentData(string loanId, DateTime fromDate, DateTime toDate)
            {
                try
                {
                    // Get loan details
                    var loanResponse = await _loanApiHelper.GetAsync<ResponseObject<LoanDetails>>(
                        string.Format(APICallHelper.GetLoanDetails, loanId));

                    // Get repayment schedule
                    var scheduleResponse = await _loanApiHelper.GetAsync<ResponseObject<List<RepaymentSchedule>>>(
                        string.Format(APICallHelper.GetLoanRepaymentSchedule, loanId));

                    // Get actual repayments
                    var repaymentsResponse = await _loanApiHelper.GetAsync<ResponseObject<List<LoanRepayment>>>(
                        string.Format(APICallHelper.GetLoanRepayments, loanId, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd")));

                    return new LoanRepaymentData
                    {
                        LoanDetails = loanResponse.ApiResponseData.Data,
                        RepaymentSchedule = scheduleResponse.ApiResponseData.Data,
                        ActualRepayments = repaymentsResponse.ApiResponseData.Data,
                        ReportPeriod = new ReportParameters { DateFrom = fromDate, DateTo = toDate }
                    };
                }
                catch (Exception ex)
                {
                    // Log exception
                    throw;
                }
            }

        public async Task<ReportData> GetLoanSituationData(FinancialReportFilter parameters)
        {
            try
            {
                var request = new FinancialReportRequest
                {
                    Filter = parameters
                };

                string endpoint = APICallHelper.GetTransactionHistoryByAccountNumber3; // Your Loan History endpoint

                var response = await _transactionApiHelper
                    .PostAsync<ResponseObject<ReportData>>(endpoint, request);

                // =========================
                // SUCCESS
                // =========================
                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    GetExecutionMessages(
                        parameters,
                        true,
                        parameters.MemberReference,
                        MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages,
                        SystemMessageStatus.Success.ToString(),
                        null,
                        response.ApiResponseData.Message
                    );

                    // ✅ RETURN REPORT DATA DIRECTLY
                    return response.ApiResponseData.Data;
                }

                // =========================
                // API FAILURE (Handled)
                // =========================
                GetExecutionMessages(
                    parameters,
                    false,
                    parameters.MemberReference,
                    MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages,
                    SystemMessageStatus.Failed.ToString(),
                    null,
                    response?.ApiResponseData?.Message ?? response?.Message
                );

                return null;
            }
            catch (Exception ex)
            {
                // =========================
                // EXCEPTION
                // =========================
                GetExecutionMessages(
                    parameters,
                    false,
                    parameters.MemberReference,
                    MessagesResults.Error,
                    ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Error.ToString(),
                    ex,
                    ex.Message
                );

                return null;
            }
        }


        public async Task<MemberFinancialReport> GetAccountSituationData(FinancialReportFilter parameters)
        {
            try
            {
                var request = new FinancialReportRequest
                {
                    Filter = parameters
                };

                string endpoint = APICallHelper.GetTransactionHistoryByAccountNumber3;

                var response = await _transactionApiHelper.PostAsync<ResponseObject<MemberFinancialReport>>(endpoint, request);

                // =========================
                // SUCCESS
                // =========================
                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    GetExecutionMessages(
                        parameters,
                        true,
                        parameters.MemberReference,
                        MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages,
                        SystemMessageStatus.Success.ToString(),
                        null,
                        response.ApiResponseData.Message
                    );

                    // ✅ RETURN FULL REPORT DATA
                    return response.ApiResponseData.Data;
                }

                // =========================
                // API FAILURE (Handled)
                // =========================
                GetExecutionMessages(
                    parameters,
                    false,
                    parameters.MemberReference,
                    MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages,
                    SystemMessageStatus.Failed.ToString(),
                    null,
                    response?.ApiResponseData?.Message ?? response?.Message
                );

                return null;
            }
            catch (Exception ex)
            {
                // =========================
                // EXCEPTION
                // =========================
                GetExecutionMessages(
                    parameters,
                    false,
                    parameters.MemberReference,
                    MessagesResults.Error,
                    ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Error.ToString(),
                    ex,
                    ex.Message
                );

                return null;
            }
        }

        /// <summary>
        /// Gets interest report data for a specific date range and optionally for a specific customer
        /// </summary>
        public async Task<InterestReportRow> GetInterestReportData(DateTime fromDate, DateTime toDate, string memberReference = null)
        {
            try
            {
                var parameters = new FinancialReportFilter
                {
                    DateFrom = fromDate,
                    DateTo = toDate,
                    MemberReference = memberReference,
                    ReportType = (int)FinancialReportType.Interest
                };

                var request = new FinancialReportRequest
                {
                    Filter = parameters
                };

                string endpoint = APICallHelper.GetInterestReportData; // You'll need to define this constant

                var response = await _transactionApiHelper.PostAsync<ResponseObject<InterestReportRow>>(endpoint, request);

                // =========================
                // SUCCESS
                // =========================
                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    GetExecutionMessages(
                        parameters,
                        true,
                        parameters.MemberReference,
                        MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages,
                        SystemMessageStatus.Success.ToString(),
                        null,
                        response.ApiResponseData.Message
                    );

                    return response.ApiResponseData.Data;
                }

                // =========================
                // API FAILURE (Handled)
                // =========================
                GetExecutionMessages(
                    parameters,
                    false,
                    parameters.MemberReference,
                    MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages,
                    SystemMessageStatus.Failed.ToString(),
                    null,
                    response?.ApiResponseData?.Message ?? response?.Message
                );

                // Return empty data structure instead of null for testing/development
                return new InterestReportRow
                {
                    InterestDetails = new List<InterestDetail>(),
                    Summary = new InterestSummary()
                };
            }
            catch (Exception ex)
            {
                // =========================
                // EXCEPTION
                // =========================
                GetExecutionMessages(
                    new FinancialReportFilter { MemberReference = memberReference },
                    false,
                    memberReference,
                    MessagesResults.Error,
                    ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Error.ToString(),
                    ex,
                    ex.Message
                );

                // Return empty data structure instead of null for testing/development
                return new InterestReportRow
                {
                    InterestDetails = new List<InterestDetail>(),
                    Summary = new InterestSummary()
                };
            }
        }

        /// <summary>
        /// Gets VAT report data for a specific date range and optionally for a specific customer
        /// </summary>
        public async Task<VatReportRow> GetVatReportData(DateTime fromDate, DateTime toDate, string memberReference = null)
        {
            try
            {
                var parameters = new FinancialReportFilter
                {
                    DateFrom = fromDate,
                    DateTo = toDate,
                    MemberReference = memberReference,
                    ReportType = (int)FinancialReportType.VAT
                };

                var request = new FinancialReportRequest
                {
                    Filter = parameters
                };

                string endpoint = APICallHelper.GetVatReportData; // You'll need to define this constant

                var response = await _transactionApiHelper.PostAsync<ResponseObject<VatReportRow>>(endpoint, request);

                // =========================
                // SUCCESS
                // =========================
                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    GetExecutionMessages(
                        parameters,
                        true,
                        parameters.MemberReference,
                        MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages,
                        SystemMessageStatus.Success.ToString(),
                        null,
                        response.ApiResponseData.Message
                    );

                    return response.ApiResponseData.Data;
                }

                // =========================
                // API FAILURE (Handled)
                // =========================
                GetExecutionMessages(
                    parameters,
                    false,
                    parameters.MemberReference,
                    MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages,
                    SystemMessageStatus.Failed.ToString(),
                    null,
                    response?.ApiResponseData?.Message ?? response?.Message
                );

                // Return empty data structure instead of null for testing/development
                return new VatReportRow
                {
                    VatDetails = new List<VatDetail>(),
                    Summary = new VatSummary()
                };
            }
            catch (Exception ex)
            {
                // =========================
                // EXCEPTION
                // =========================
                GetExecutionMessages(
                    new FinancialReportFilter { MemberReference = memberReference },
                    false,
                    memberReference,
                    MessagesResults.Error,
                    ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Error.ToString(),
                    ex,
                    ex.Message
                );

                // Return empty data structure instead of null for testing/development
                return new VatReportRow
                {
                    VatDetails = new List<VatDetail>(),
                    Summary = new VatSummary()
                };
            }
        }

        /// <summary>
        /// Gets penalty report data for a specific date range and optionally for a specific customer
        /// </summary>
        public async Task<PenaltyReportRow> GetPenaltyReportData(DateTime fromDate, DateTime toDate, string memberReference = null)
        {
            try
            {
                var parameters = new FinancialReportFilter
                {
                    DateFrom = fromDate,
                    DateTo = toDate,
                    MemberReference = memberReference,
                    ReportType = (int)FinancialReportType.Penalty
                };

                var request = new FinancialReportRequest
                {
                    Filter = parameters
                };

                string endpoint = APICallHelper.GetPenaltyReportData; // You'll need to define this constant

                var response = await _transactionApiHelper.PostAsync<ResponseObject<PenaltyReportRow>>(endpoint, request);

                // =========================
                // SUCCESS
                // =========================
                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    GetExecutionMessages(
                        parameters,
                        true,
                        parameters.MemberReference,
                        MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages,
                        SystemMessageStatus.Success.ToString(),
                        null,
                        response.ApiResponseData.Message
                    );

                    return response.ApiResponseData.Data;
                }

                // =========================
                // API FAILURE (Handled)
                // =========================
                GetExecutionMessages(
                    parameters,
                    false,
                    parameters.MemberReference,
                    MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages,
                    SystemMessageStatus.Failed.ToString(),
                    null,
                    response?.ApiResponseData?.Message ?? response?.Message
                );

                // Return empty data structure instead of null for testing/development
                return new PenaltyReportRow
                {
                    PenaltyDetails = new List<PenaltyDetail>(),
                    Summary = new PenaltySummary()
                };
            }
            catch (Exception ex)
            {
                // =========================
                // EXCEPTION
                // =========================
                GetExecutionMessages(
                    new FinancialReportFilter { MemberReference = memberReference },
                    false,
                    memberReference,
                    MessagesResults.Error,
                    ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Error.ToString(),
                    ex,
                    ex.Message
                );

                // Return empty data structure instead of null for testing/development
                return new PenaltyReportRow
                {
                    PenaltyDetails = new List<PenaltyDetail>(),
                    Summary = new PenaltySummary()
                };
            }
        }

    }      
}


