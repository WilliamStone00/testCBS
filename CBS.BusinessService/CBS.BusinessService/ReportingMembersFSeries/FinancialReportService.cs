using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.ReportMembersFSeries;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.Spreadsheet;
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

        public async Task<List<TransactionRaw>> GetCustomerTransactionsByAccountNumber(ReportParameters parameters)
        {
            try
            {
                string endpoint = string.Format(APICallHelper.GetTransactionHistoryByAccountNumber, parameters.AccountTypeId);

                var response = await _transactionApiHelper.GetAsync<ResponseObject<List<TransactionRaw>>>(endpoint);
                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log exception
                throw;
            }
        }

       

        //public async Task<ExecutionMessages> GetCustomerTransactionsByAccountNumber(ReportParameters parameters)
        //{
        //    try
        //    {
        //        string endpoint = string.Format(APICallHelper.GetTransactionHistoryByAccountNumber, parameters);
        //        var response = await _transactionApiHelper.GetAsync<ResponseObject<List<TransactionHistory>>>(endpoint);

        //        if (response.IsSuccess)
        //        {           
        //            GetExecutionMessages(response.ApiResponseData.Data, true, null, MessagesResults.Success,
        //            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
        //        }
        //        else
        //        {
        //            GetExecutionMessages(null, false, null, MessagesResults.Failed,
        //                ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        GetExecutionMessages(null, false, null, MessagesResults.Error,
        //            ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
        //    }
        //    return ExecutionMessage;
        //}

        public async Task<MemberSituationData> GetMemberSituationData(string accountTypeId, string loanId, DateTime fromDate, DateTime toDate)
            {
                try
                {
                    // Get member accounts
                    var accountsResponse = await _accountApiHelper.GetAsync<ResponseObject<List<MemberAccount>>>(
                        string.Format(APICallHelper.GetMemberAccounts, accountTypeId));

                    // Get member loans
                    var loansResponse = await _loanApiHelper.GetAsync<ResponseObject<List<MemberLoan>>>(
                        string.Format(APICallHelper.GetMemberLoans, loanId));

                    // Get transaction summary
                    var transactionSummary = await GetTransactionSummary(accountTypeId, fromDate, toDate);

                    return new MemberSituationData
                    {
                        MemberAccounts = accountsResponse.ApiResponseData.Data,
                        MemberLoans = loansResponse.ApiResponseData.Data,
                        TransactionSummary = transactionSummary,
                        ReportPeriod = new ReportParameters { DateFrom = fromDate, DateTo = toDate }
                    };
                }
                catch (Exception ex)
                {
                    // Log exception
                    throw;
                }
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

            public async Task<LoanSituationData> GetLoanSituationData(DateTime fromDate, DateTime toDate)
            {
                try
                {
                    // Get all loans
                    var loansResponse = await _loanApiHelper.GetAsync<ResponseObject<List<LoanPortfolioItem>>>(
                        string.Format(APICallHelper.GetLoanPortfolio, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd")));

                    // Get portfolio summary
                    var summaryResponse = await _loanApiHelper.GetAsync<ResponseObject<LoanPortfolioSummary>>(
                        APICallHelper.GetLoanPortfolioSummary);

                    // Get delinquency data
                    var delinquencyResponse = await _loanApiHelper.GetAsync<ResponseObject<LoanDelinquencyData>>(
                        APICallHelper.GetLoanDelinquencyData);

                    return new LoanSituationData
                    {
                        LoanPortfolio = loansResponse.ApiResponseData.Data,
                        PortfolioSummary = summaryResponse.ApiResponseData.Data,
                        DelinquencyData = delinquencyResponse.ApiResponseData.Data,
                        ReportPeriod = new ReportParameters { DateFrom = fromDate, DateTo = toDate }
                    };
                }
                catch (Exception ex)
                {
                    // Log exception
                    throw;
                }
            }
            public async Task<LoanSituationData> GetAccountSituationData(string Account,DateTime fromDate, DateTime toDate)
            {
                try
                {
                    // Get all loans
                    var loansResponse = await _loanApiHelper.GetAsync<ResponseObject<List<LoanPortfolioItem>>>(
                        string.Format(APICallHelper.GetLoanPortfolio, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd")));

                    // Get portfolio summary
                    var summaryResponse = await _loanApiHelper.GetAsync<ResponseObject<LoanPortfolioSummary>>(
                        APICallHelper.GetLoanPortfolioSummary);

                    // Get delinquency data
                    var delinquencyResponse = await _loanApiHelper.GetAsync<ResponseObject<LoanDelinquencyData>>(
                        APICallHelper.GetLoanDelinquencyData);

                    return new LoanSituationData
                    {
                        LoanPortfolio = loansResponse.ApiResponseData.Data,
                        PortfolioSummary = summaryResponse.ApiResponseData.Data,
                        DelinquencyData = delinquencyResponse.ApiResponseData.Data,
                        ReportPeriod = new ReportParameters { DateFrom = fromDate, DateTo = toDate }
                    };
                }
                catch (Exception ex)
                {
                    // Log exception
                    throw;
                }
            }

            private async Task<TransactionSummary> GetTransactionSummary(string accountTypeId, DateTime? fromDate, DateTime? toDate)
            {
                // Implementation for transaction summary
                return await Task.FromResult(new TransactionSummary());
            }
  }      
}


