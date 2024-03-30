using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.BudgetManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.BudgetManagement
{
    public class BudgetServices : BaseService
    {
        private readonly ApiCallerHelper _ConfigApiHelper;

        public BudgetServices()
        {
            _ConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objSavingProduct = await GetBudget(id);
                var inResponse = await _ConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_Budget, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objSavingProduct.BudgetPeriodId}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objSavingProduct, false, $"{objSavingProduct.BudgetPeriodId}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<Budget>> GetAllBudgets()
        {
            try
            {
                var couApiResponse = await _ConfigApiHelper.GetAsync<ResponseObject<List<Budget>>>(APICallHelper.Get_All_Budget);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<Budget>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<OrganizationalUnit>> GetAllOrganizationalUnits()
        {
            try
            {
                var couApiResponse = await _ConfigApiHelper.GetAsync<ResponseObject<List<OrganizationalUnit>>>(APICallHelper.Get_All_OrganizationalUnit);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<OrganizationalUnit>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<BudgetPeriod>> GetAllBudgetPeriods()
        {
            try
            {
                var couApiResponse = await _ConfigApiHelper.GetAsync<ResponseObject<List<BudgetPeriod>>>(APICallHelper.Get_All_BudgetPeriod);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<BudgetPeriod>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<Budget> GetBudget(string id)
        {
            try
            {
                var cusResponseObject = await _ConfigApiHelper.GetAsync<ResponseObject<Budget>>(string.Format(APICallHelper.Get_Update_Delete_Budget, id));
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
        public async Task<ExecutionMessages> Create(Budget model)
        {
            try
            {
          
                // Make an API call to create an individual profile

                var response = await _ConfigApiHelper.PostAsync<ServiceResponse<Budget>>(APICallHelper.Create_Budget, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.BudgetPeriodId}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.BudgetPeriodId, MessagesResults.Failed,
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

        public async Task<ExecutionMessages> Update(Budget model)
        {
            try
            {

                var Account = await GetBudget(model.Id);
                if (model != null)
                {

                    if (Account != null)
                    {

                     

                        var response = await _ConfigApiHelper.PutAsync<ServiceResponse<FrontDesk.Data.Entity.Budget>>(string.Format(APICallHelper.Get_Update_Delete_Budget, Account.Id), Account);
                        if (response.IsSuccess)
                        {
                            // Successful creation
                            GetExecutionMessages(response, true, $"{model.Id }", MessagesResults.Success,
                                ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                            return ExecutionMessage;
                        }
                        else
                        {
                            // Failed creation
                            GetExecutionMessages(model, false, $"{model.Id}", MessagesResults.Failed,
                                ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                        }
                    }
                    else
                    {
                        var response = new ApiResponse<ServiceResponse<Budget>>();
                        response.Message = $"{model.Id} cannot be updated because it doesn't exist in the system kindly create";
                        GetExecutionMessages(model, false, $"{model.Id} cannot be updated because it doesn't exist in the system kindly create", MessagesResults.Failed,
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


        public async Task<ExecutionMessages> LockBudget(string Id)
        {
            try
            {
                var BudgetDto = await GetBudget(Id);
                if (BudgetDto != null)
                {
                  

                    var response = await _ConfigApiHelper.GetAsync<ServiceResponse<Budget>>(string.Format(APICallHelper.LockBudget_Budget, Id));
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{BudgetDto.BudgetPeriodId}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(BudgetDto, false, BudgetDto.BudgetPeriodId, MessagesResults.Failed,
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
