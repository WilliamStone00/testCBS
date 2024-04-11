using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounts
{
    public class SavingProductServices : BaseService
    {
        private readonly ApiCallerHelper _savingConfigApiHelper;

        public SavingProductServices()
        {
            _savingConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objSavingProduct = await GetSavingProduct(id);
                var inResponse = await _savingConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_SavingProduct, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objSavingProduct.name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objSavingProduct, false, $"{objSavingProduct.name}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<SavingProduct>> GetSavingProducts()
        {
            try
            {
                var couApiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<List<SavingProduct>>>(APICallHelper.GetAllSavingProducts);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<SavingProduct>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<SavingConfigurationAggregates> GetSavingConfigurationAggregates()
        {
            try
            {
                var couApiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<SavingConfigurationAggregates>>(APICallHelper.GetAllConfigurationEnums);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new SavingConfigurationAggregates();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<SavingProduct> GetSavingProduct(string id)
        {
            try
            {
                var cusResponseObject = await _savingConfigApiHelper.GetAsync<ResponseObject<SavingProduct>>(string.Format(APICallHelper.Get_Update_Delete_SavingProduct, id));
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
        public async Task<ExecutionMessages> Create(SavingProduct model)
        {
            try
            {
                //model.bankId = GetBankID();
                // Make an API call to create an individual profile
      
                var response = await _savingConfigApiHelper.PostAsync<ServiceResponse<SavingProduct>>(APICallHelper.CreateSavingProduct, model);
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
        public async Task<ExecutionMessages> Update(SavingProduct model)
        {
            try
            {
                var SavingProduct = await GetSavingProduct(model.id);
                if (SavingProduct != null)
                {
                    SavingProduct.name = model.name;
                    SavingProduct.code = model.code;
                    SavingProduct.interestAccrualFrequency = model.interestAccrualFrequency;
                    SavingProduct.isCapitalizeInterest = model.isCapitalizeInterest;
                    SavingProduct.postingFrequency = model.postingFrequency;
                    SavingProduct.currencyId = model.currencyId;
                    SavingProduct.maxAmount = model.maxAmount;
                    SavingProduct.minAmount = model.minAmount;
                    SavingProduct.activeStatus = model.activeStatus;
                    SavingProduct.isTermProduct = model.isTermProduct;
                    SavingProduct.isUsedForTellerProvisioning = model.isUsedForTellerProvisioning;
                    SavingProduct.description = model.description;
                    SavingProduct.AccountType = model.AccountType;
                    SavingProduct.UpdateOption = "N/A";
                    var response = await _savingConfigApiHelper.PutAsync<ServiceResponse<SavingProduct>>(string.Format(APICallHelper.Get_Update_Delete_SavingProduct, model.id), SavingProduct);
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
        public async Task<ExecutionMessages> UpdateProductAccountMapping(SavingProduct model)
        {
            try
            {
                var SavingProduct = await GetSavingProduct(model.id);
                if (SavingProduct != null)
                {
                    SavingProduct.UpdateOption = "assign_account_chart";
                    SavingProduct.ChartOfAccountIdPricipalAccount = model.ChartOfAccountIdPricipalAccount;
                    SavingProduct.ChartOfAccountIdInterestAccount = model.ChartOfAccountIdInterestAccount;
                    SavingProduct.ChartOfAccountIdInterestExpenseAccount = model.ChartOfAccountIdInterestExpenseAccount;
                    SavingProduct.ChartOfAccountIdSavingFee = model.ChartOfAccountIdSavingFee;
                    SavingProduct.ChartOfAccountIdWithrawalFee = model.ChartOfAccountIdWithrawalFee;
                    SavingProduct.ChartOfAccountIdTransferFee = model.ChartOfAccountIdTransferFee;
                    SavingProduct.ChartOfAccountIdCommissionAccount = model.ChartOfAccountIdCommissionAccount;
                    SavingProduct.ChartOfAccountIdLiassonAccount = model.ChartOfAccountIdLiassonAccount;
                    var response = await _savingConfigApiHelper.PutAsync<ServiceResponse<SavingProduct>>(string.Format(APICallHelper.Get_Update_Delete_SavingProduct, model.id), SavingProduct);
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
