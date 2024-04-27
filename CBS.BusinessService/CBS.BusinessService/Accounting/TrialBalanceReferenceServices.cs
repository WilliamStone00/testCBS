using CBS.API.Helper;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting
{

    public class TrialBalanceReferenceServices : BaseApiServices
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

       

        public TrialBalanceReferenceServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
        
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var model = await GetTrialBalanceReference(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_TrialBalanceReference, id), id));
                if (inResponse.IsSuccess)
                {
                    GetExecutionMessages(inResponse, true, $"{model.ChartOfAccountId} -{model.StatementModelId} ", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
           

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(model, false, $"{model.ChartOfAccountId} -{model.StatementModelId} ", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<TrialBalanceReference>> GetAllTrialBalanceReference()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<TrialBalanceReference>>>(APICallHelper.GetAllTrialBalanceReference);
                if (couApiResponse.IsSuccess)
                {

                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<TrialBalanceReference>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }
                }
                return new List<TrialBalanceReference>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<TrialBalanceReference> GetTrialBalanceReference(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<TrialBalanceReference>>(string.Format(APICallHelper.Get_Update_Delete_TrialBalanceReference, id));
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

        public async Task<ExecutionMessages> Update(TrialBalanceReference model)
        {
            try
            {

                var AccountCategory = await GetTrialBalanceReference(model.Id);
                if (AccountCategory != null)
                {
                   
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<TrialBalanceReference>>(string.Format(APICallHelper.Get_Update_Delete_TrialBalanceReference, model.Id), model);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.ChartOfAccountId} -{model.StatementModelId} ", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{model.ChartOfAccountId} -{model.StatementModelId} ", MessagesResults.Failed,
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

        public async Task<ExecutionMessages> Create(TrialBalanceReference model)
        {
            try
            {

                model.StatementModelId = model.StatementModelId;
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<TrialBalanceReference>>(APICallHelper.CreateTrialBalanceReference, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.ChartOfAccountId} -{model.StatementModelId} ", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.ChartOfAccountId} -{model.StatementModelId} ", MessagesResults.Failed,
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


        public async Task<List<TrialBalanceReference>> GetTrialBalanceReferenceCategory(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<List<TrialBalanceReference>>>(string.Format(APICallHelper.Get_TrialBalanceReference, id));
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
    }
}
