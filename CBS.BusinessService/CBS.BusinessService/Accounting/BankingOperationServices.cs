using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting
{
    public class BankingOperationServices:BaseService
    {
        private readonly ApiCallerHelper _ConfigApiHelper;

        public string BranchId { get;  set; }
        public string BankId { get;  set; }
        

        public BankingOperationServices()
        {
            _ConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
            this.BranchId = GetBranchID();
            this.BankId = GetBankID();
  
        }
 
        public async Task<ChartOfAccount> GetChartOfAccountById(string id)
        {
            try
            {
                var cusResponseObject = await _ConfigApiHelper.GetAsync<ResponseObject<FrontDesk.Data.Entity.Accounting.ChartOfAccount>>(string.Format(APICallHelper.Get_Update_Delete_ChartOfAccount, id));
                if (cusResponseObject.IsSuccess)
                {
                    if (cusResponseObject.ApiResponseData == null)
                    {

                    }
                    else
                    {
                        return cusResponseObject.ApiResponseData.Data;
                    }

                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ExecutionMessages> PostManualAccountingEntry(ManualAccountingEntry model)
        {
            try
            {

                // Make an API call to create an individual profile

                var debiter = await GetChartOfAccountByAccountNumber(model.SourceAccountId);

                if (debiter == null)
                {
                    // Successful creation
                    GetExecutionMessages(null, true, $"The Credit account was not found: Please kindly contact administrator", MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, "");
                    return ExecutionMessage;
                }
                var crediter = await GetChartOfAccountByAccountNumber(model.destinationAccountId);
                if (crediter == null)
                {
                    // Successful creation
                    GetExecutionMessages(null, true, $"The Debit account was not found: Please kindly contact administrator", MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, "");
                    return ExecutionMessage;
                }
                var response = await _ConfigApiHelper.PostAsync<ServiceResponse<ManualAccountingEntry>>(APICallHelper.ManualEntriePosting, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"XAF {model.Amount} has been debited from {debiter.AccountNumber + "-" + debiter.LabelEn} to {crediter.AccountNumber + "-" + crediter.LabelEn}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"XAF {model.Amount} has been debited from {debiter.AccountNumber + "-" + debiter.LabelEn} to {crediter.AccountNumber + "-" + crediter.LabelEn}", MessagesResults.Failed,
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
        public async Task<ChartOfAccount> GetChartOfAccountByAccountNumber(string accountNumber)
        {
            try
            {
                var cusResponseObject = await _ConfigApiHelper.GetAsync<ResponseObject<FrontDesk.Data.Entity.Accounting.ChartOfAccount>>(string.Format(APICallHelper.Get_ChartOfAccount_By_AccountNumber, accountNumber));
                if (cusResponseObject.IsSuccess)
                {
                    if (cusResponseObject.ApiResponseData == null)
                    {
                        throw new Exception("Accounts were not found");
                    }
                    else
                    {
                        return cusResponseObject.ApiResponseData.Data;
                    }

                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

    }
}
