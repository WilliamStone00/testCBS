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
                var inResponse = await _savingConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_SavingProduct, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objSavingProduct.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objSavingProduct, false, null, MessagesResults.Failed,
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
                    GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
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
        public async Task<ExecutionMessages> Update(SavingProduct model)
        {
            try
            {
                var SavingProduct = await GetSavingProduct(model.Id);
                if (SavingProduct != null)
                {
                    SavingProduct.Name = model.Name;
                    SavingProduct.Code = model.Code;
                    SavingProduct.InterestAccrualFrequency = model.InterestAccrualFrequency;
                    SavingProduct.IsCapitalizeInterest = model.IsCapitalizeInterest;
                    SavingProduct.PostingFrequency = model.PostingFrequency;
                    SavingProduct.CurrencyId = model.CurrencyId;
                    SavingProduct.MaxAmount = model.MaxAmount;
                    SavingProduct.MinAmount = model.MinAmount;
                    SavingProduct.WithdrawalFormSavingFormFeeFor3PP = model.WithdrawalFormSavingFormFeeFor3PP;
                    SavingProduct.ActiveStatus = model.ActiveStatus;
                    SavingProduct.IsTermProduct = model.IsTermProduct;
                    SavingProduct.IsUsedForTellerProvisioning = model.IsUsedForTellerProvisioning;
                    SavingProduct.Description = model.Description;
                    SavingProduct.MinimumAccountBalanceMoralPerson = model.MinimumAccountBalanceMoralPerson;
                    SavingProduct.MinimumAccountBalancePhysicalPerson = model.MinimumAccountBalancePhysicalPerson;
                    SavingProduct.AccountType = model.AccountType;
                    SavingProduct.AutoAddToMember = model.AutoAddToMember;
                    SavingProduct.AllowShareing = model.AllowShareing;
                    SavingProduct.AllowInterbranchWithdrawal = model.AllowInterbranchWithdrawal;
                    SavingProduct.IsDepositAllowedDirectlyTothisAccount = model.IsDepositAllowedDirectlyTothisAccount;
                    SavingProduct.IsWithdrawalAllowedDirectlyFromthisAccount = model.IsWithdrawalAllowedDirectlyFromthisAccount;
                    SavingProduct.UpdateOption = "N/A";
                    SavingProduct.CanPeformTransferMobileApp = model.CanPeformTransferMobileApp;
                    SavingProduct.CanPeformCashinMobileApp = model.CanPeformCashinMobileApp;
                    SavingProduct.CanPeformCashOutMobileApp = model.CanPeformCashOutMobileApp;
                    SavingProduct.ActivateSavingWithdrawalNotificationForMobileApp = model.ActivateSavingWithdrawalNotificationForMobileApp;
                    SavingProduct.CanPeformTransfer3PP = model.CanPeformTransfer3PP;
                    SavingProduct.CanPeformCashin3PP = model.CanPeformCashin3PP;
                    SavingProduct.CanPeformCashOut3PP = model.CanPeformCashOut3PP;
                    SavingProduct.DisplayOrder = model.DisplayOrder;
                    SavingProduct.ActivateForMobileApp = model.ActivateForMobileApp;
                    SavingProduct.ProductCategory = model.ProductCategory;
                    SavingProduct.AutoVerifyRemittanceReceiver = model.AutoVerifyRemittanceReceiver;
                    SavingProduct.AutoVerifyRemittanceSender = model.AutoVerifyRemittanceSender;
                    SavingProduct.ActivateFor3PPApp = model.ActivateFor3PPApp;
                    SavingProduct.AllowInterbranchDeposit = model.AllowInterbranchDeposit;
                    SavingProduct.AllowInterbranchTransfter = model.AllowInterbranchTransfter;
                    var response = await _savingConfigApiHelper.PutAsync<ServiceResponse<SavingProduct>>(string.Format(APICallHelper.Get_Update_Delete_SavingProduct, model.Id), SavingProduct);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
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
                var SavingProduct = await GetSavingProduct(model.Id);
                if (SavingProduct != null)
                {
                    SavingProduct.UpdateOption = "assign_account_chart";
                    SavingProduct.ChartOfAccountIdPricipalAccount = model.ChartOfAccountIdPricipalAccount;
                    SavingProduct.ChartOfAccountIdInterestAccount = model.ChartOfAccountIdInterestAccount;
                    SavingProduct.ChartOfAccountIdInterestExpenseAccount = model.ChartOfAccountIdInterestExpenseAccount;
                    SavingProduct.ChartOfAccountIdSavingFee = model.ChartOfAccountIdSavingFee;
                    SavingProduct.ChartOfAccountIdWithrawalFee = model.ChartOfAccountIdWithrawalFee;
                    SavingProduct.ChartOfAccountIdTransferFee = model.ChartOfAccountIdTransferFee;
                    SavingProduct.ChartOfAccountIdCashInCommission = model.ChartOfAccountIdCashInCommission;
                    SavingProduct.ChartOfAccountIdCashOutCommission = model.ChartOfAccountIdCashOutCommission;
                    SavingProduct.ChartOfAccountIdLiassonAccount = model.ChartOfAccountIdLiassonAccount;
                    SavingProduct.ChartOfAccountIdHeadOfficeShareCashInCommission = model.ChartOfAccountIdHeadOfficeShareCashInCommission;
                    SavingProduct.ChartOfAccountIdHeadOfficeShareCashOutCommission = model.ChartOfAccountIdHeadOfficeShareCashOutCommission;
                    SavingProduct.ChartOfAccountIdCamCCULShareCashInCommission = model.ChartOfAccountIdCamCCULShareCashInCommission;
                    SavingProduct.ChartOfAccountIdCamCCULShareCashOutCommission = model.ChartOfAccountIdCamCCULShareCashOutCommission;
                    SavingProduct.ChartOfAccountIdFluxAndPTMShareCashInCommission = model.ChartOfAccountIdFluxAndPTMShareCashInCommission;
                    SavingProduct.ChartOfAccountIdFluxAndPTMShareCashOutCommission = model.ChartOfAccountIdFluxAndPTMShareCashOutCommission;
                    SavingProduct.ChartOfAccountIdCamCCULShareTransferCommission = model.ChartOfAccountIdCamCCULShareTransferCommission;
                    SavingProduct.ChartOfAccountIdFluxAndPTMShareTransferCommission = model.ChartOfAccountIdFluxAndPTMShareTransferCommission;
                    SavingProduct.ChartOfAccountIdHeadOfficeShareTransferCommission = model.ChartOfAccountIdHeadOfficeShareTransferCommission;
                    SavingProduct.ChartOfAccountIdHeadOfficeShareCMoneyTransferCommission = model.ChartOfAccountIdHeadOfficeShareCMoneyTransferCommission;
                    SavingProduct.ChartOfAccountIdFluxAndPTMShareCMoneyTransferCommission = model.ChartOfAccountIdFluxAndPTMShareCMoneyTransferCommission;
                    SavingProduct.ChartOfAccountIdCamCCULShareCMoneyTransferCommission = model.ChartOfAccountIdCamCCULShareCMoneyTransferCommission;
                    SavingProduct.ChartOfAccountIdSourceCMoneyTransferCommission = model.ChartOfAccountIdSourceCMoneyTransferCommission;
                    SavingProduct.ChartOfAccountIdDestinationCMoneyTransferCommission = model.ChartOfAccountIdDestinationCMoneyTransferCommission;
                    var response = await _savingConfigApiHelper.PutAsync<ServiceResponse<SavingProduct>>(string.Format(APICallHelper.Get_Update_Delete_SavingProduct, model.Id), SavingProduct);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> UpdateProductEventMapping(SavingProduct model)
        {
            try
            {
                var SavingProduct = await GetSavingProduct(model.Id);
                if (SavingProduct != null)
                {
                   
                    SavingProduct.UpdateOption = "N/A";
                    SavingProduct.EventCodeAdvanceOfSalaryFormFee = model.EventCodeAdvanceOfSalaryFormFee;
                    SavingProduct.EventCodeMoralPersonWithdrawalFormFee = model.EventCodeMoralPersonWithdrawalFormFee;
                    SavingProduct.EventCodePhysicalPersonWithdrawalFormFee = model.EventCodePhysicalPersonWithdrawalFormFee;
                    SavingProduct.EventCodeWithdrawalFormSavingFormFeeFor3PP = model.EventCodeWithdrawalFormSavingFormFeeFor3PP;
                    var response = await _savingConfigApiHelper.PutAsync<ServiceResponse<SavingProduct>>(string.Format(APICallHelper.Get_Update_Delete_SavingProduct, model.Id), SavingProduct);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> UpdateProductEventMappingSalary(SavingProduct model)
        {
            try
            {
                var SavingProduct = await GetSavingProduct(model.Id);
                if (SavingProduct != null)
                {

                    SavingProduct.UpdateOption = "N/A";
                    SavingProduct.EventCodeAdvanceOfSalaryFormFee = model.EventCodeAdvanceOfSalaryFormFee;
                    var response = await _savingConfigApiHelper.PutAsync<ServiceResponse<SavingProduct>>(string.Format(APICallHelper.Get_Update_Delete_SavingProduct, model.Id), SavingProduct);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
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
