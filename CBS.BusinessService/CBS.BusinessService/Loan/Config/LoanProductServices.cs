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
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using System.Web.Mvc;

namespace CBS.BusinessService.Config
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

                    GetExecutionMessages(inResponse, true, $"{objLoanProduct.ProductName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objLoanProduct, false, $"{objLoanProduct.ProductName}", MessagesResults.Failed,
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
        public async Task<IEnumerable<StringValues>> GetLoanProductsDropDown()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanProduct>>>(APICallHelper.GetAllLoanProduct);
                if (couApiResponse.IsSuccess)
                {
                    var data = couApiResponse.ApiResponseData.Data.Select(x => new StringValues { Text = $"{x.ProductName}, Loan Range>> Min: {x.LoanMinimumAmount.ToString("#0,0.0")}, Max: {x.LoanMaximumAmount.ToString("#0,0.0")}", Value = x.Id }).ToList();
                    return data;
                }
                return new List<StringValues>();
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
                    if (cusResponseObject != null)
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
        public async Task<SelectList> GetLoanProductRepayments(string loanProductId = null, string path = null)
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<LoanProduct>>(string.Format(APICallHelper.Get_Update_Delete_LoanProduct, loanProductId));
                return ProcessApiResponseResponse(couApiResponse.ApiResponseData.Data, path);

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        private SelectList ProcessApiResponseResponse(LoanProduct product, string path)
        {
            if (path == "loanrepayment_cycles")
            {
                var values = product.LoanProductRepaymentCycles.Select(a => new StringValues
                {
                    Text = $"{a.RepaymentCycle}",
                    Value = a.Id
                });
                var defaultSelectedValue = "default-value";
                return new SelectList(values.ToList(), "Value", "Text", defaultSelectedValue);

            }
            else
            {
                var values = product.LoanProductRepaymentOrders.Select(a => new StringValues
                {
                    Text = $"{a.RepaymentOrder}",
                    Value = a.Id
                });
                var defaultSelectedValue = "default-value";
                return new SelectList(values.ToList(), "Value", "Text", defaultSelectedValue);

            }
        }
        public async Task<LoanProductEnumAgregates> GetLoanProductEnumAggregates()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<LoanProductEnumAgregates>>(APICallHelper.LoanProductEnumAggregates);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new LoanProductEnumAgregates();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
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
                    GetExecutionMessages(response, true, $"{model.ProductName}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.ProductName, MessagesResults.Failed,
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
                
                var LoanProduct = await GetLoanProduct(model.Id);
                LoanProduct.UpdateOption = "N/A";
                if (LoanProduct != null)
                {
                    if (model.ServiceOption == "product")
                    {
                        LoanProduct.ProductCode = model.ProductCode;
                        LoanProduct.ProductName = model.ProductName;
                        LoanProduct.TaxId = model.TaxId;
                        LoanProduct.ActiveStatus = model.ActiveStatus;
                        LoanProduct.Description = model.Description;
                    }
                    else if (model.ServiceOption == "gurantee")
                    {
                        LoanProduct.RequiresGuarantor = model.RequiresGuarantor;
                        LoanProduct.MinimumCollateralPercentage = model.MinimumCollateralPercentage;
                        LoanProduct.DefaultCollateralPercentage = model.DefaultCollateralPercentage;
                        LoanProduct.MaximumCollateralPercentage = model.MaximumCollateralPercentage;
                        LoanProduct.IsRequiredCollateral = model.IsRequiredCollateral;
                        LoanProduct.IsRequiredSalaryccount = model.IsRequiredSalaryccount;
                        LoanProduct.IsRequiredSavingAccount = model.IsRequiredSavingAccount;
                        LoanProduct.IsRequiredShareAccount = model.IsRequiredShareAccount;
                        LoanProduct.IsRequredIrrivocableSalaryTransfer = model.IsRequredIrrivocableSalaryTransfer;
                        LoanProduct.IsRequresRegisteredPublicAuthority = model.IsRequresRegisteredPublicAuthority;
                        LoanProduct.MinimumShareAccountBalanceForTheRequestAmount = model.MinimumShareAccountBalanceForTheRequestAmount;
                        LoanProduct.MaximumShareAccountBalanceForTheRequestAmount = model.MaximumShareAccountBalanceForTheRequestAmount;
                        LoanProduct.MaximumSavingAccountBalanceRateForTheRequestAmount = model.MaximumSavingAccountBalanceRateForTheRequestAmount;
                        LoanProduct.MinimumSavingAccountBalanceRateForTheRequestAmount = model.MinimumSavingAccountBalanceRateForTheRequestAmount;
                        LoanProduct.MaximumMaximumSalaryAccountBalanceRateForTheRequestAmount = model.MaximumMaximumSalaryAccountBalanceRateForTheRequestAmount;
                        LoanProduct.MinimumSalaryAccountBalanceRateForTheRequestAmount = model.MinimumSalaryAccountBalanceRateForTheRequestAmount;
                        LoanProduct.BlockedGuarantorAccount = model.BlockedGuarantorAccount;
                        LoanProduct.BlockedSalaryAccount = model.BlockedSalaryAccount;
                        LoanProduct.BlockedSavingAccount = model.BlockedSavingAccount;

                    }
                    else if (model.ServiceOption == "loan_range")
                    {
                        LoanProduct.LoanMinimumAmount = model.LoanMinimumAmount;
                        LoanProduct.DefaultLoanAmount = model.DefaultLoanAmount;
                        LoanProduct.LoanMaximumAmount = model.LoanMaximumAmount;

                    }
                    else if (model.ServiceOption == "topup")
                    {
                        LoanProduct.HasTopUp = model.HasTopUp;
                        LoanProduct.MinTopUpLoanAmount = model.MinTopUpLoanAmount;
                        LoanProduct.TopUpAmount = model.TopUpAmount;
                        LoanProduct.MaxTopUpLoanAmount = model.MaxTopUpLoanAmount;


                    }
                    
                    else if (model.ServiceOption == "interest")
                    {
                        LoanProduct.LoanInterestType = model.LoanInterestType;
                        LoanProduct.LoanInterestPeriod = model.LoanInterestPeriod;
                        LoanProduct.MinimumInterestRate = model.MinimumInterestRate;
                        LoanProduct.DefaultInterestRate = model.DefaultInterestRate;
                        LoanProduct.MaximumInterestRate = model.MaximumInterestRate;
                        LoanProduct.StartGeneratingInterestAfterDisbustment = model.StartGeneratingInterestAfterDisbustment;

                    }
                    else if (model.ServiceOption == "duration")
                    {
                        LoanProduct.LoanDurationPeriod = model.LoanDurationPeriod;
                        LoanProduct.MinimumDurationPeriod = model.MinimumDurationPeriod;
                        LoanProduct.DefaultDurationsPeriod = model.DefaultDurationsPeriod;
                        LoanProduct.MaximumDurationPeriod = model.MaximumDurationPeriod;

                    }
                    else if (model.ServiceOption == "repayment")
                    {
                        LoanProduct.RepaymentCycles = model.RepaymentCycles;
                        LoanProduct.MinimumNumberOfRepayment = model.MinimumNumberOfRepayment;
                        LoanProduct.DefaultNumberOfRepayment = model.DefaultNumberOfRepayment;
                        LoanProduct.MaximumNumberOfRepayment = model.MaximumNumberOfRepayment;
                        LoanProduct.RefundOrders = model.RefundOrders;
                        LoanProduct.ServiceOption = model.ServiceOption;


                    }
                    else if (model.ServiceOption == "fee")
                    {
        
                        LoanProduct.IsEarlyPartialRepaymentFeeRate = model.IsEarlyPartialRepaymentFeeRate;
                        LoanProduct.EarlyPartialRepaymentFee = model.EarlyPartialRepaymentFee;
                        LoanProduct.IsEarlyTotalRepaymentFeeRate = model.IsEarlyTotalRepaymentFeeRate;
                        LoanProduct.EarlyTotalRepaymentFee = model.EarlyTotalRepaymentFee;
                        LoanProduct.MinimumProcessingFeeRate = model.MinimumProcessingFeeRate;
                        LoanProduct.DefaultProcessingFeeRate = model.DefaultProcessingFeeRate;
                        LoanProduct.MaximumProcessingFeeRate = model.MaximumProcessingFeeRate;
                        LoanProduct.MinimumInspectionFeeRate = model.MinimumInspectionFeeRate;
                        LoanProduct.MaximumInspectionFeeRate = model.MaximumInspectionFeeRate;
                        LoanProduct.DefaultInspectionFeeRate = model.DefaultInspectionFeeRate;
                    }
                    else if (model.ServiceOption == "advancedsettings")
                    {
                        LoanProduct.FirstRepaymentAmount = model.FirstRepaymentAmount;
                        LoanProduct.HowShoudInterestBeCahrgedInLoanSchedule = model.HowShoudInterestBeCahrgedInLoanSchedule;
                        LoanProduct.HowShoudPrincipalBeCahrgedInLoanSchedule = model.HowShoudPrincipalBeCahrgedInLoanSchedule;
                        LoanProduct.CalculateInterestOnEachRepaymentOnProRatabase = model.CalculateInterestOnEachRepaymentOnProRatabase;
                        LoanProduct.LoanScheduleDescription = model.LoanScheduleDescription;

                    }
                    else if (model.ServiceOption == "accounting")
                    {
                        LoanProduct.ChartOfAccountIdForPrincipalAmount = model.ChartOfAccountIdForPrincipalAmount;
                        LoanProduct.ChartOfAccountIdForAccrualInterest = model.ChartOfAccountIdForAccrualInterest;
                        LoanProduct.ChartOfAccountIdForPenalty = model.ChartOfAccountIdForPenalty;
                        LoanProduct.ChartOfAccountIdForFee = model.ChartOfAccountIdForFee;
                        LoanProduct.ChartOfAccountIdForTax = model.ChartOfAccountIdForTax;
                        LoanProduct.ChartOfAccountIdForLoanTransition = model.ChartOfAccountIdForLoanTransition;
                        LoanProduct.ChartOfAccountIdForWriteOffPrincipal = model.ChartOfAccountIdForWriteOffPrincipal;
                        LoanProduct.ChartOfAccountIdForProvisionOnPrincipal = model.ChartOfAccountIdForProvisionOnPrincipal;
                        LoanProduct.UpdateOption = model.UpdateOption;
                    }

                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<LoanProduct>>(string.Format(APICallHelper.Get_Update_Delete_LoanProduct, model.Id), LoanProduct);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.ServiceOption.ToUpper()}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.ServiceOption.ToUpper(), MessagesResults.Failed,
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
