using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.Config;

namespace CBS.BusinessService.Loan.Config
{
    public class FundingLineServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;
        private readonly ApiCallerHelper _bankApiHelper;

        public FundingLineServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
            _bankApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objFundingLine = await GetFundingLine(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_FundingLine, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objFundingLine.name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objFundingLine, false, $"{objFundingLine.name}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        
        public async Task<IEnumerable<FundingLine>> GetFundingLines()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<FundingLine>>>(APICallHelper.GetAllFundingLine);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<FundingLine>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<Currency>> GetCurrencies()
        {
            try
            {
                var couApiResponse = await _bankApiHelper.GetAsync<ResponseObject<List<Currency>>>(APICallHelper.GetAllCurrency);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<Currency>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<FundingLine> GetFundingLine(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<FundingLine>>(string.Format(APICallHelper.Get_Update_Delete_FundingLine, id));
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
        public async Task<ExecutionMessages> Create(FundingLine model)
        {
            try
            {

                 model.BankId = GetBankID();
                 model.BranchId = GetBranchID();
                 model.OrganizationId = GetOrganizationID();
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<FundingLine>>(APICallHelper.CreateFundingLine, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.name}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.name, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(FundingLine model)
        {
            try
            {

                var FundingLine = await GetFundingLine(model.id);
                if (FundingLine != null)
                {
                    FundingLine.name = model.name;
                    FundingLine.beginDate = model.beginDate;
                    FundingLine.endDate = model.endDate;
                    FundingLine.amount = model.amount;
                    FundingLine.Description = model.Description;
                    FundingLine.currencyId = model.currencyId;
                    FundingLine.BankId = model.BankId;
                    FundingLine.BranchId = model.BranchId;
                    FundingLine.OrganizationId = model.OrganizationId;
                    FundingLine.AccountingRuleId = model.AccountingRuleId;

                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<FundingLine>>(string.Format(APICallHelper.Get_Update_Delete_FundingLine, model.id), FundingLine);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.name}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.name, MessagesResults.Failed,
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
