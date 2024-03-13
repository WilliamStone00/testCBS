using BusinessServices;
using CBS.API.Helper;
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

namespace CBS.BusinessService.Config
{
    public class LoanGuarantorServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public LoanGuarantorServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objLoanGuarantor = await GetLoanGuarantor(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_LoanGuarantor, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objLoanGuarantor.guarantorName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objLoanGuarantor, false, $"{objLoanGuarantor.guarantorName}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        
        public async Task<IEnumerable<LoanGuarantor>> GetLoanGuarantors()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanGuarantor>>>(APICallHelper.GetAllLoanGuarantor);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<LoanGuarantor>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<LoanGuarantor> GetLoanGuarantor(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<LoanGuarantor>>(string.Format(APICallHelper.Get_Update_Delete_LoanGuarantor, id));
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
        public async Task<ExecutionMessages> Create(LoanGuarantor model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<LoanGuarantor>>(APICallHelper.CreateLoanGuarantor, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.guarantorName}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.guarantorName, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(LoanGuarantor model)
        {
            try
            {

                var LoanGuarantor = await GetLoanGuarantor(model.id);
                if (LoanGuarantor != null)
                {
                    LoanGuarantor.guarantorName = model.guarantorName;
                    LoanGuarantor.loanApplicationId = model.loanApplicationId;
                    LoanGuarantor.idCardNumber = model.idCardNumber;
                    LoanGuarantor.expireDate = model.expireDate;
                    LoanGuarantor.issueDate = model.issueDate;
                    LoanGuarantor.relationship = model.relationship;
                    LoanGuarantor.address = model.address;
                    LoanGuarantor.phoneNumber = model.phoneNumber;
                    LoanGuarantor.isMemberOfMicrofinance = model.isMemberOfMicrofinance;
                    LoanGuarantor.bankAccountNumber = model.bankAccountNumber;
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<LoanGuarantor>>(string.Format(APICallHelper.Get_Update_Delete_LoanGuarantor, model.id), LoanGuarantor);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.guarantorName}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.guarantorName, MessagesResults.Failed,
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
