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
    public class BudgetCategoryServices : BaseService
    {
        private readonly ApiCallerHelper _ConfigApiHelper;

        public BudgetCategoryServices()
        {
            _ConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objSavingProduct = await GetBudgetCategory(id);
                var inResponse = await _ConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_SavingProduct, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objSavingProduct.CategoryName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objSavingProduct, false, $"{objSavingProduct.CategoryName}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> Update(BudgetCategory model)
        {
            try
            {

                var Account = await GetBudgetCategory(model.Id);
                if (model != null)
                {

                    if (Account != null)
                    {



                        var response = await _ConfigApiHelper.PutAsync<ServiceResponse<FrontDesk.Data.Entity.BudgetCategory>>(string.Format(APICallHelper.Get_Update_Delete_BudgetCategory, Account.Id), Account);
                        if (response.IsSuccess)
                        {
                            // Successful creation
                            GetExecutionMessages(response, true, $"{model.Id}", MessagesResults.Success,
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
                        var response = new ApiResponse<ServiceResponse<BudgetCategory>>();
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
        public async Task<IEnumerable<BudgetCategory>> GetBudgetCategorys()
        {
            try
            {
                var couApiResponse = await _ConfigApiHelper.GetAsync<ResponseObject<List<BudgetCategory>>>(APICallHelper.Get_All_BudgetCategory);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<BudgetCategory>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
         public async Task<BudgetCategory> GetBudgetCategory(string id)
        {
            try
            {
                var cusResponseObject = await _ConfigApiHelper.GetAsync<ResponseObject<BudgetCategory>>(string.Format(APICallHelper.Get_Update_Delete_BudgetCategory, id));
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
        public async Task<ExecutionMessages> Create(BudgetCategory model)
        {
            try
            {
          
                // Make an API call to create an individual profile

                var response = await _ConfigApiHelper.PostAsync<ServiceResponse<BudgetCategory>>(APICallHelper.Create_BudgetCategory, model.CreateBudgetCategory());
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.CategoryName}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.CategoryName, MessagesResults.Failed,
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
    
        public async Task<ExecutionMessages> LockBudget(string Id)
        {
            try
            {
                var BudgetDto = await GetBudgetCategory(Id);
                if (BudgetDto != null)
                {
                  

                    var response = await _ConfigApiHelper.GetAsync<ServiceResponse<BudgetCategory>>(string.Format(APICallHelper.LockBudget_Budget, Id));
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{BudgetDto.CategoryName}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(BudgetDto, false, BudgetDto.CategoryName, MessagesResults.Failed,
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
