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

namespace CBS.BusinessService.Accounting
{

    public class AccountCategoryServices : BaseApiServices
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public string BranchId { get; private set; }
        public string BankId { get; private set; }
        public string OrganizationId { get; private set; }

        public AccountCategoryServices()
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
                var objAccountCategory = await GetAccountCategory(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_AccountCategory, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objAccountCategory.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objAccountCategory, false, $"{objAccountCategory.Name}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<FrontDesk.Data.Entity.Accounting.AccountCategory>> GetAccountCategory()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<FrontDesk.Data.Entity.Accounting.AccountCategory>>>(APICallHelper.GetAllAccountCategory);
                if (couApiResponse.IsSuccess)
                {

                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<FrontDesk.Data.Entity.Accounting.AccountCategory>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }
                }
                return new List<FrontDesk.Data.Entity.Accounting.AccountCategory>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<FrontDesk.Data.Entity.Accounting.AccountCategory> GetAccountCategory(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<FrontDesk.Data.Entity.Accounting.AccountCategory>>(string.Format(APICallHelper.Get_Update_Delete_AccountCategory, id));
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

        public async Task<ExecutionMessages> Update(FrontDesk.Data.Entity.Accounting.AccountCategory model)
        {
            try
            {

                var AccountCategory = await GetAccountCategory(model.Id);
                if (AccountCategory != null)
                {
                    AccountCategory.Name = model.Name;

                    AccountCategory.Id = model.Id;
                    AccountCategory.Description = model.Description;

                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<FrontDesk.Data.Entity.Accounting.AccountCategory>>(string.Format(APICallHelper.Get_Update_Delete_AccountCategory, model.Id), AccountCategory);
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

        public async Task<ExecutionMessages> Create(FrontDesk.Data.Entity.Accounting.AccountCategory model)
        {
            try
            {

                // Make an API call to create an individual profile

                //model.BankId = this.BankId;
                //model.BranchId= this .BranchId;
                //model.OrganizationId= this .OrganizationId;
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<FrontDesk.Data.Entity.Accounting.AccountCategory>>(APICallHelper.CreateAccountCategory, model);
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

    }
}
