using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.BusinessService.Config
{
    public class LoanPurposeServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public LoanPurposeServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objLoanPurpose = await GetLoanPurpose(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_LoanPurpose, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objLoanPurpose.purposeName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objLoanPurpose, false, $"{objLoanPurpose.purposeName}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<LoanPurpose>> GetLoanPurposes()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanPurpose>>>(APICallHelper.GetAllLoanPurpose);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<LoanPurpose>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<List<StringValues>> GetAllLoanPurpose()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanPurpose>>>(APICallHelper.GetAllLoanPurpose);
                if (couApiResponse != null)
                {
                    var values = couApiResponse.ApiResponseData.Data.Select(a => new StringValues
                    {
                        Text = $"{a.purposeName}",
                        Value = a.id
                    }).ToList();
                    return values;
                }
                return new List<StringValues>();

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<List<StringValues>> GetAllLoanPurpose(string categoryid)
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanPurpose>>>(APICallHelper.GetAllLoanPurpose);
                if (couApiResponse != null)
                {
                    var values = couApiResponse.ApiResponseData.Data.Where(x=>x.PcmfLoanPurposeId==categoryid).Select(a => new StringValues
                    {
                        Text = $"{a.purposeName}",
                        Value = a.id
                    }).ToList();
                    return values;
                }
                return new List<StringValues>();

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<LoanPurpose> GetLoanPurpose(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<LoanPurpose>>(string.Format(APICallHelper.Get_Update_Delete_LoanPurpose, id));
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
        public async Task<ExecutionMessages> Create(LoanPurpose model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<LoanPurpose>>(APICallHelper.CreateLoanPurpose, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.purposeName}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.purposeName, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(LoanPurpose model)
        {
            try
            {

                var LoanPurpose = await GetLoanPurpose(model.id);
                if (LoanPurpose != null)
                {
                    LoanPurpose.purposeName = model.purposeName;
                    LoanPurpose.PcmfLoanPurposeId = model.PcmfLoanPurposeId;
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<LoanPurpose>>(string.Format(APICallHelper.Get_Update_Delete_LoanPurpose, model.id), LoanPurpose);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.purposeName}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.purposeName, MessagesResults.Failed,
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
