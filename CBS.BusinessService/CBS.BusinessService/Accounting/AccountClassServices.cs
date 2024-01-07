using CBS.API.Helper;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.Accounting;

namespace CBS.BusinessService.Accounting
{
 
    public class AccountClassServices : BaseApiServices
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public string BranchId { get; private set; }
        public string BankId { get; private set; }
        public string OrganizationId { get; private set; }

        public AccountClassServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
            this.BranchId = GetBranchID();
            this.BankId = GetBankID();
            this.OrganizationId = GetOrganizationID();
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objAccountClass = await GetAccountClass(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_AccountClass, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objAccountClass.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objAccountClass, false, $"{objAccountClass.Name}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<FrontDesk.Data.Entity.Accounting.AccountClass>> GetAccountClass()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<FrontDesk.Data.Entity.Accounting.AccountClass>>>(APICallHelper.GetAllAccountClass);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<FrontDesk.Data.Entity.Accounting.AccountClass>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<FrontDesk.Data.Entity.Accounting.AccountClass> GetAccountClass(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<FrontDesk.Data.Entity.Accounting.AccountClass>>(string.Format(APICallHelper.Get_Update_Delete_AccountClass, id));
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
        public async Task<ExecutionMessages> Create(FrontDesk.Data.Entity.Accounting.AccountClass model)
        {
            try
            {

                // Make an API call to create an individual profile

                //model.BankId = this.BankId;
                //model.BranchId= this .BranchId;
                //model.OrganizationId= this .OrganizationId;
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<FrontDesk.Data.Entity.Accounting.AccountClass>>(APICallHelper.CreateAccountClass, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(FrontDesk.Data.Entity.Accounting.AccountClass model)
        {
            try
            {

                var AccountClass = await GetAccountClass(model.Id);
                if (AccountClass != null)
                {
                    AccountClass.Name = model.Name;
                    //AccountClass.BankId = model.BankId;
                    //AccountClass.BranchId = model.BranchId;
                    //AccountClass.OrganizationId = model.OrganizationId;

                    AccountClass.Id = model.Id;
                    AccountClass.Description = model.Description;

                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<FrontDesk.Data.Entity.Accounting.AccountClass>>(string.Format(APICallHelper.Get_Update_Delete_AccountClass, model.Id), AccountClass);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, (string)model.Name, MessagesResults.Failed,
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
