using CBS.API.Helper;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper.Helper;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.FrontDesk.Service;

namespace CBS.BusinessService.Accounting
{
    public class ChartOfAccountServices:BaseApiServices
    {
        private readonly ApiCallerHelper _ConfigApiHelper;

        public string BranchId { get; private set; }
        public string BankId { get; private set; }
        public string OrganizationId { get; private set; }

        public ChartOfAccountServices()
        {
            _ConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
            this.BranchId = GetBranchID();
            this.BankId = GetBankID();
            this.OrganizationId = GetOrganizationID();
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objOperationEvent = await GetChartOfAccountByAccountNumber(id);
                var inResponse = await _ConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Delete_ChartOfAccount, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objOperationEvent.AccountNumber + " " + objOperationEvent.LabelEn}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objOperationEvent, false, $"{objOperationEvent.AccountNumber + " " + objOperationEvent.LabelEn}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<IEnumerable<FrontDesk.Data.Entity.Accounting.AccountTreeNode>> GetAllChartOfAccountTreeNodes()
        {
            try
            {
                var couApiResponse = await _ConfigApiHelper.GetAsync<ResponseObject<List<FrontDesk.Data.Entity.Accounting.AccountTreeNode>>>(APICallHelper.GetAllJsTreeNode);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<FrontDesk.Data.Entity.Accounting.AccountTreeNode>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<FrontDesk.Data.Entity.Accounting.ChartOfAccount>> GetAllChartOfAccounts()
        {
            try
            {
                var couApiResponse = await _ConfigApiHelper.GetAsync<ResponseObject<List<FrontDesk.Data.Entity.Accounting.ChartOfAccount>>>(APICallHelper.GetAllChartOfAccount);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<FrontDesk.Data.Entity.Accounting.ChartOfAccount>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<FrontDesk.Data.Entity.Accounting.ChartOfAccount> GetChartOfAccountById(string id)
        {
            try
            {
                var cusResponseObject = await _ConfigApiHelper.GetAsync<ResponseObject<FrontDesk.Data.Entity.Accounting.ChartOfAccount>>(string.Format(APICallHelper.Get_Update_Delete_ChartOfAccount, id));
                if (cusResponseObject.IsSuccess)
                {
                    if (cusResponseObject.ApiResponseData==null)
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
        public async Task<ExecutionMessages> Create(FrontDesk.Data.Entity.Accounting.ChartOfAccountDto model)
        {
            try
            {

                // Make an API call to create an individual profile

             
                var response = await _ConfigApiHelper.PostAsync<ServiceResponse<FrontDesk.Data.Entity.Accounting.ChartOfAccount>>(APICallHelper.CreateChartOfAccount, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.AccountNumber+" "+model.LabelEn}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.AccountNumber + " " + model.LabelEn}", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(FrontDesk.Data.Entity.Accounting.ChartOfAccount model)
        {
            try
            {

                var Account = await GetChartOfAccountByAccountNumber (model.AccountNumber);
                if (model != null)
                {
 
                    Account.AccountNumber = model.AccountNumber;
                    Account.LabelEn = model.LabelEn;
                    Account.LabelFr = model.LabelFr;
                    Account.IsBalanceAccount = model.IsBalanceAccount;


                    var response = await _ConfigApiHelper.PutAsync<ServiceResponse<FrontDesk.Data.Entity.Accounting.ChartOfAccount>>(string.Format(APICallHelper.Get_Update_Delete_ChartOfAccount, Account.Id), Account);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.AccountNumber + " " + model.LabelEn}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{model.AccountNumber + " " + model.LabelEn}", MessagesResults.Failed,
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

        public async Task<FrontDesk.Data.Entity.Accounting.ChartOfAccount> GetChartOfAccountByAccountNumber(string accountNumber)
        {
            try
            {
                var cusResponseObject = await _ConfigApiHelper.GetAsync<ResponseObject<FrontDesk.Data.Entity.Accounting.ChartOfAccount>>(string.Format(APICallHelper.Get_ChartOfAccount_By_AccountNumber, accountNumber));
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
    }
}
