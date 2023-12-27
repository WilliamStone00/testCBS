using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
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
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;

namespace CBS.BusinessService.Loan.Config
{
    public class LoanProductServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;
        private readonly ApiCallerHelper _BankConfigApiHelper;

        public LoanProductServices()
        {
            _BankConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objLoanProduct = await GetLoanProduct(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_LoanProduct, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objLoanProduct.productName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objLoanProduct, false, $"{objLoanProduct.productName}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        
        public async Task<IEnumerable<LoanProduct>> GetLoanProducts()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanProduct>>>(APICallHelper.GetAllLoanProduct);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<LoanProduct>();
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
                var couApiResponse = await _BankConfigApiHelper.GetAsync<ResponseObject<List<Currency>>>(APICallHelper.GetAllCurrency);
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
        ///api/v1/LoanProductConfigurationAgregates
        public async Task<LoanProduct> GetLoanProduct(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<LoanProduct>>(string.Format(APICallHelper.Get_Update_Delete_LoanProduct, id));
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
        public async Task<LoanProductConfigurationAgregates> GetAgreggates()
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<LoanProductConfigurationAgregates>>(APICallHelper.GetLoanProductConfigurationAgregates);
                if (cusResponseObject.IsSuccess)
                {
                    if (cusResponseObject!=null)
                    {
                        return cusResponseObject.ApiResponseData.Data;
                    }

                }
                return new LoanProductConfigurationAgregates();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ExecutionMessages> Create(LoanProduct model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<LoanProduct>>(APICallHelper.CreateLoanProduct, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.productName}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.productName, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(LoanProduct model)
        {
            try
            {

                var LoanProduct = await GetLoanProduct(model.loanProductId);
                if (LoanProduct != null)
                {
                    LoanProduct.productName = model.productName;
                    LoanProduct.scheduleTypeId = model.scheduleTypeId;
                    LoanProduct.productCode = model.productCode;
                    LoanProduct.installmentTypeId = model.installmentTypeId;
                    LoanProduct.numberOfInstallmentMin = model.numberOfInstallmentMin;
                    LoanProduct.numberOfInstallmentMax = model.numberOfInstallmentMax;
                    LoanProduct.currencyId = model.currencyId;
                    LoanProduct.numberOfGracePeriodMin = model.numberOfGracePeriodMin;
                    LoanProduct.numberOfGracePeriodMax = model.numberOfGracePeriodMax;
                    LoanProduct.gracePeriodPercentageMin = model.gracePeriodPercentageMin;
                    LoanProduct.gracePeriodPercentageMax = model.gracePeriodPercentageMax;
                    LoanProduct.loanAmountMin = model.loanAmountMin;
                    LoanProduct.loanAmountMax = model.loanAmountMax;
                    //LoanProduct.interestRateType = model.interestRateType;
                    LoanProduct.interestRateMin = model.interestRateMin;
                    LoanProduct.interestRateMax = model.interestRateMax;
                    LoanProduct.guaranteePackId = model.guaranteePackId;
                    LoanProduct.documentPackId = model.documentPackId;
                    LoanProduct.minPercentageGuarantee = model.minPercentageGuarantee;
                    LoanProduct.minPercentageCollateral = model.minPercentageCollateral;
                    LoanProduct.creditInsuranceMin = model.creditInsuranceMin;
                    LoanProduct.creditInsuranceMax = model.creditInsuranceMax;
                    LoanProduct.feeOlbMin = model.feeOlbMin;
                    LoanProduct.feeOlbMax = model.feeOlbMax;
                    LoanProduct.feeOverduePrincipalMin = model.feeOverduePrincipalMin;
                    LoanProduct.feeOverduePrincipalMax = model.feeOverduePrincipalMax;
                    LoanProduct.feeOverdueInterestMax = model.feeOverdueInterestMax;
                    LoanProduct.feeOverdueInterestMin = model.feeOverdueInterestMin;
                    LoanProduct.fundingLineId = model.fundingLineId;
                    LoanProduct.taxId = model.taxId;
                    LoanProduct.customerProfiles = model.customerProfiles;
                    LoanProduct.feeOlbAccountingRuleId = model.feeOlbAccountingRuleId;
                    LoanProduct.feeOverduePrincipalAccountingRuleId = model.feeOverduePrincipalAccountingRuleId;
                    LoanProduct.feeOverdueInterestAccountingRuleId = model.feeOverdueInterestAccountingRuleId;
                    LoanProduct.activeStatus = model.activeStatus;
                    LoanProduct.hasTopUp = model.hasTopUp;
                    LoanProduct.topUpAmountMax = model.topUpAmountMax; 
                    LoanProduct.topUpAmountMin = model.topUpAmountMin;
                    LoanProduct.topUpAmountAccountingRuleId = model.topUpAmountAccountingRuleId;
                    //LoanProduct.earlyPartialRepaymentFeeType = model.earlyPartialRepaymentFeeType; 
                    LoanProduct.earlyPartialRepaymentFeeRate = model.earlyPartialRepaymentFeeRate;
                    LoanProduct.earlyPartialRepaymentFeeAccountingRuleId = model.earlyPartialRepaymentFeeAccountingRuleId;
                    LoanProduct.earlyTotalRepaymentFeeRate = model.earlyTotalRepaymentFeeRate; 
                    LoanProduct.earlyTotalRepaymentFeeRateType = model.earlyTotalRepaymentFeeRateType;
                    LoanProduct.earlyTotalRepaymentFeeRateAccountingRuleId = model.earlyTotalRepaymentFeeRateAccountingRuleId;
                    LoanProduct.penaltyIds = model.penaltyIds;
                    LoanProduct.fees = model.fees;
                    LoanProduct.accountingRuleIds = model.accountingRuleIds;
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<LoanProduct>>(string.Format(APICallHelper.Get_Update_Delete_LoanProduct, model.loanProductId), LoanProduct);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.productName}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.productName, MessagesResults.Failed,
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
