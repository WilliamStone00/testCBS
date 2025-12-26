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
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.Owin.Logging;
using CBS.FrontDesk.Data.Entity.LoanConf.PCMFStructure;

namespace CBS.BusinessService.Config
{
    public class LoanProductServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;
        private readonly ApiCallerHelper _BankConfigApiHelper;
        private readonly LoanProductCategoryServices _loanProductCategoryServices;
        private readonly LoanTermServices _loanTermServices;

        public LoanProductServices(LoanProductCategoryServices loanProductCategoryServices = null, LoanTermServices loanTermServices = null)
        {
            _BankConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
            _loanProductCategoryServices = loanProductCategoryServices;
            _loanTermServices = loanTermServices;
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
        // ✅ mapper for edit -> form command
        public CreateLoanProductCommand MapToCreateCommand(PCMFLoanProduct p)
        {
            return new CreateLoanProductCommand
            {
                Id = p.Id,
                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                Description = p.Description,
                LoanTermId = p.LoanTermId,
                LoanTargetId = p.LoanTargetId,
                PcmfLoanPurposeId = p.PcmfLoanPurposeId,
                ActiveStatus = p.ActiveStatus,
                LoanFacility = p.LoanFacility, 
                ChartOfAccountIdForInterestReceived = p.AccountingMapping.ChartOfAccountIdForInterestReceived,
                ChartOfAccountIdForPenalty = p.AccountingMapping.ChartOfAccountIdForPenalty,
                ChartOfAccountIdForPrincipalAmount = p.AccountingMapping.ChartOfAccountIdForPrincipalAmount,
                ChartOfAccountIdForTax = p.AccountingMapping.ChartOfAccountIdForTax,
                ChartOfAccountIdForTaxibleInterestReceived = p.AccountingMapping.ChartOfAccountIdForTaxibleInterestReceived,
                ChartOfAccountIdForTaxiblePrincipalAmount = p.AccountingMapping.ChartOfAccountIdForTaxiblePrincipalAmount
            };
        }
        public async Task<IEnumerable<LoanProduct>> GetLoanProducts()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanProduct>>>(APICallHelper.GetAllLoanProductLighterVersion);
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

        public async Task<IEnumerable<PCMFLoanProduct>> PCMFGetLoanProducts()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<PCMFLoanProduct>>>(APICallHelper.GetAllLoanProductLighterVersion);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<PCMFLoanProduct>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<IEnumerable<StringValues>> GetStringValuesAsync()
        {
            try
            {
                var loanProducts = await GetLoanProducts();

                var results = (from a in loanProducts
                               let badge = GetLoanTypeBadge(a.LoanTypeCategory)
                               select new StringValues
                               {
                                   Text = $"{badge} [{a.ProductCode}] {a.ProductName} " +
                                          $"| 📦C: {a.LoanProductCategory?.Name ?? "N/A"} " +
                                          $"| 🕒T: {a.LoanTerm?.Name ?? "N/A"} " +
                                          $"| 💰A {a.LoanMinimumAmount:N0} XAF–{a.LoanMaximumAmount:N0} XAF " +
                                          $"| ⏳D {a.LoanTerm?.MinInMonth}-{a.LoanTerm?.MaxInMonth} {a.LoanDurationPeriod} " +
                                          $"| ⏳I {a.MinimumInterestRate}-{a.MaximumInterestRate}" +
                                          $"{(a.IsMortgage ? "| 🏠 Mortgage" : "")} ",
                                   Value = a.Id
                               }).ToList();

                return results;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        // 🧩 Helper to get LoanType badge
        private string GetLoanTypeBadge(string type)
        {
            switch (type)
            {
                case "LineOfCredit":
                    return "[LOC]";
                case "Overdraft":
                    return "[ODT]";
                case "SSF":
                    return "[SSF]";
                case "Main_Loan":
                    return "[ML]";
                default:
                    return "[N/A]";
            }
        }

        public async Task<IEnumerable<StringValues>> GetLoanProductsDropDown()
        {
            try
            {
                //var couApiResponse = await _smsConfigApiHelper.GetAsync<ResponseObject<List<LoanProduct>>>(APICallHelper.GetAllLoanProduct);
                //if (couApiResponse.IsSuccess)
                //{
                //    var data = couApiResponse.ApiResponseData.Data.Select(x => new SelectedItemsApprovedUploads { Text = $"{x.ProductName}, [Min: {x.LoanMinimumAmount.ToString("#,##0.0")} : Max: {x.LoanMaximumAmount.ToString("#,##0.0")}]", Value = x.Id }).ToList();
                //    return data;
                //}
                return new List<StringValues>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<SelectList> GetLoanProductsDropDown(string targetType, string loanTermId, string loanCategoryid, bool isSSF)
        {
            try
            {
                // Fetch data from API
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanProduct>>>(APICallHelper.GetAllLoanProductLighterVersion);

                // Check if the response is successful and contains data
                if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData?.Data != null)
                {
                    // Filter and map the data to the list of SelectListItem
                    var values = couApiResponse.ApiResponseData.Data
                        .Where(x => x.TargetType == targetType && x.ActiveStatus && x.LoanTermId == loanTermId && x.LoanProductCategoryId == loanCategoryid && x.IsProductWithSavingFacilities == isSSF)
                        .Select(x => new SelectListItem
                        {
                            Text = $"{x.ProductCode} {x.ProductName}, [Min: {x.LoanMinimumAmount.ToString("#,##0")} - Max: {x.LoanMaximumAmount.ToString("#,##0")}], [BTN: {x.LoanTerm.MinInMonth} - {x.LoanTerm.MaxInMonth} Month(s)]",
                            Value = x.Id
                        })
                        .ToList();

                    // Default selected value (adjust as per your needs)
                    var defaultSelectedValue = "default-value";

                    // Return the SelectList
                    return new SelectList(values, "Value", "Text", defaultSelectedValue);
                }

                // Return an empty SelectList with a default "No options available" option
                return new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Text = "No options available", Value = string.Empty }
                }, "Value", "Text");
            }
            catch (Exception ex)
            {
                // Log the exception
                // _logger.LogError(ex, "An error occurred while getting the loan products dropdown.");

                // Handle the exception accordingly
                throw; // Re-throw the exception after logging
            }
        }

        public async Task<SelectList> GetTargetsConfiguredForProductByTermOrDuration(string loanTermId, string LoanProductCategoryId, bool isSSF)
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanProduct>>>(APICallHelper.GetAllLoanProductLighterVersion);

                // Check if the response is successful and contains data
                if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData?.Data != null)
                {
                    // Filter and map the data to the list of SelectListItem
                    var values = couApiResponse.ApiResponseData.Data
                        .Where(x => x.ActiveStatus && x.LoanTermId == loanTermId && x.LoanProductCategoryId == LoanProductCategoryId && x.IsProductWithSavingFacilities == isSSF)
                        .Select(x => new SelectListItem
                        {
                            Text = $"{x.TargetType}",
                            Value = x.TargetType
                        })
                        .GroupBy(x => x.Value) // Group by Value to ensure distinct entries
                        .Select(group => group.First()) // Select the first item from each group
                        .ToList();

                    // Default selected value (adjust as per your needs)
                    var defaultSelectedValue = "default-value";

                    // Return the SelectList
                    return new SelectList(values, "Value", "Text", defaultSelectedValue);
                }


                // Return an empty SelectList with a default "No options available" option
                return new SelectList(new List<SelectListItem>
        {
            new SelectListItem { Text = "No options available", Value = string.Empty }
        }, "Value", "Text");
            }
            catch (Exception ex)
            {
                // Log the exception
                // _logger.LogError(ex, "An error occurred while getting the loan products dropdown.");

                // Handle the exception accordingly
                throw; // Re-throw the exception after logging
            }
        }
        //GetProductTermOrDurationFromConfiguredProduct
        public async Task<List<LoanProductCategory>> GetProductCategoryFromConfiguredProduct()
        {
            try
            {
                // Fetch loan products using the API helper
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanProduct>>>(APICallHelper.GetAllLoanProductLighterVersion);

                // Return an empty list if the API response is unsuccessful or data is null
                if (!(couApiResponse?.IsSuccess == true && couApiResponse.ApiResponseData?.Data != null))
                {
                    return new List<LoanProductCategory>();
                }

                // Extract loan products
                var loanProducts = couApiResponse.ApiResponseData.Data;

                // Fetch loan product categories
                var productsCats = await _loanProductCategoryServices.GetLoanProductCategorys();

                // Perform join to filter categories based on loan products
                var productCategories = productsCats
                    .Where(pc => loanProducts.Any(lp => lp.LoanProductCategoryId == pc.Id && lp.ActiveStatus))
                    .ToList();

                return productCategories;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes

                // Rethrow the exception to propagate it further
                throw;
            }
        }

        public async Task<List<LoanTerm>> GetProductTermOrDurationFromConfiguredProduct()
        {
            try
            {
                // Fetch loan products using the API helper
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanProduct>>>(APICallHelper.GetAllLoanProductLighterVersion);

                // Return an empty list if the API response is unsuccessful or data is null
                if (!(couApiResponse?.IsSuccess == true && couApiResponse.ApiResponseData?.Data != null))
                {
                    return new List<LoanTerm>();
                }

                // Extract loan products
                var loanProducts = couApiResponse.ApiResponseData.Data;

                // Fetch loan product termes
                var loanTerms = await _loanTermServices.GetLoanTerms();

                // Perform join to filter categories based on loan products
                var terms = loanTerms
                    .Where(pc => loanProducts.Any(lp => lp.LoanTermId == pc.Id))
                    .ToList();

                return terms;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes

                // Rethrow the exception to propagate it further
                throw;
            }
        }

        public async Task<IEnumerable<StringValues>> GetFees()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<Fee>>>(APICallHelper.GetAllFee);
                if (couApiResponse.IsSuccess)
                {
                    var data = couApiResponse.ApiResponseData.Data.Select(x => new StringValues { Text = $"{x.Name}", Value = x.Id }).ToList();
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
                    var data = cusResponseObject.ApiResponseData.Data;

                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

        public async Task<PCMFLoanProduct> PcmfGetLoanProduct(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<PCMFLoanProduct>>(string.Format(APICallHelper.Get_Update_Delete_LoanProduct, id));
                if (cusResponseObject.IsSuccess)
                {
                    var data = cusResponseObject.ApiResponseData.Data;

                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        private static PCMFLoanProduct MapToPcmfLoanProduct(LoanProductFullDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new PCMFLoanProduct
            {
                // --------------------
                // Core product
                // --------------------
                Id = dto.Id,
                ProductCode = dto.ProductCode,
                ProductName = dto.ProductName,
                Description = dto.Description,
                ActiveStatus = dto.ActiveStatus,
                LoanTypeCategory = dto.LoanTypeCategory,

                // --------------------
                // PCMF classification (derived)
                // --------------------
                PcmfSection = dto.PcmfSection,
                PcmfGroupCode = dto.PcmfGroupCode,
                PcmfPopulation = dto.PcmfPopulation,
                PcmfBaseCode = dto.PcmfBaseCode,

                // --------------------
                // Foreign ids
                // --------------------
                LoanTermId = dto.LoanTermId,
                LoanTargetId = dto.LoanTargetId,
                PcmfLoanPurposeId = dto.PcmfLoanPurposeId,
                AccountingProfileId = dto.AccountingProfileId,

                // --------------------
                // Optional convenience fields
                // --------------------
                //RepaymentCycles = dto.RepaymentCycles ?? new List<string>()
            };
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
        public async Task<SelectList> GetLoanProductRepayments(string id = null, string path = null)
        {
            try
            {
                if (path == "loanapplicationtype")
                {
                    var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<Loan>>>(string.Format(APICallHelper.GetAllMembersCurrentLoans, id));
                    return ProcessApiResponseResponse(couApiResponse.ApiResponseData.Data);

                }
                else
                {
                    var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<LoanProduct>>(string.Format(APICallHelper.Get_Update_Delete_LoanProduct, id));
                    return ProcessApiResponseResponse(couApiResponse.ApiResponseData.Data, path);

                }

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }


        private SelectList ProcessApiResponseResponse(List<Loan> loans)
        {
            var values = loans.Select(a => new StringValues
            {
                Text = $"[Amount: {a.LoanAmount}]-[Balance: {a.Balance}]",
                Value = a.Id
            });
            var defaultSelectedValue = "default-value";
            return new SelectList(values.ToList(), "Value", "Text", defaultSelectedValue);

        }

        private SelectList ProcessApiResponseResponse(LoanProduct product, string path)
        {
            var values = product.LoanProductRepaymentCycles.Select(a => new StringValues
            {
                Text = $"{a.RepaymentCycle}",
                Value = a.Id
            });
            var defaultSelectedValue = "default-value";
            return new SelectList(values.ToList(), "Value", "Text", defaultSelectedValue);

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
        public async Task<ExecutionMessages> UpdateAccountingProfileSimple(LoanProductAccountingProfile model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<bool>>(APICallHelper.LoanProductAccountingMapping, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
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
        public async Task<PcmfLoanProductUiCatalogResponse> GetPcmfLoanProductUiCatalogQuery(GetPcmfLoanProductUiCatalogQuery resource)
        {
            try
            {
                var queryString = ToQueryString(resource);
                var fullUrl = $"{APICallHelper.GetPCMFCatalog}?{queryString}";

                var api = await _loanConfigApiHelper
                    .GetAsync<ResponseObject<PcmfLoanProductUiCatalogResponse>>(fullUrl);

                if (api != null && api.IsSuccess && api.ApiResponseData?.Data != null)
                    return api.ApiResponseData.Data;

                return new PcmfLoanProductUiCatalogResponse();
            }
            catch
            {
                // TODO: log exception
                return new PcmfLoanProductUiCatalogResponse();
            }
        }

        public async Task<ExecutionMessages> UpdatePcmfLoanProductSimple(CreateLoanProductCommand model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<bool>>(APICallHelper.LoanProductUpdateCore, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.ProductName}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
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

        public async Task<ExecutionMessages> CreatePcmfLoanProductSimple(CreateLoanProductCommand model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.CreateLoanProduct, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.ProductName}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
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

        public async Task<ExecutionMessages> Create(AddLoanProductCommand model)
        {
            try
            {
                model.IsProductWithSavingFacilities = model.LoanTypeCategory == "SSF" ? true : false;

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


        public UpdateLoanProductCommand ProductMappingToUpdateObject(LoanProduct product, string ServiceOption, string UpdateOption)
        {
            try
            {
                var updateCommand = new UpdateLoanProductCommand
                {
                    Id = product.Id,
                    ProductCode = product.ProductCode,
                    LoanProductCategoryId = product.LoanProductCategoryId,
                    LoanProductCategory = product.LoanProductCategory ?? new LoanProductCategory { Name = "N/A" },
                    LoanProductId = product.Id,
                    Co_obligorMustHaveFundToGuranteeLoan = product.Co_obligorMustHaveFundToGuranteeLoan,
                    NumberOfDaysToStopInterestCalculation = product.NumberOfDaysToStopInterestCalculation,
                    MinimumPercentageCoverageOfShortee = product.MinimumPercentageCoverageOfShortee,
                    MinimumPercentageRefundBeforeRefinancing = product.MinimumPercentageRefundBeforeRefinancing,
                    ShorteeMustHaveFundToGuranteeLoan = product.ShorteeMustHaveFundToGuranteeLoan,
                    StopInterestCalculationAtLoanMaturityDate = product.StopInterestCalculationAtLoanMaturityDate,
                    TargetType = product.TargetType,
                    LoanTermId = product.LoanTermId,
                    InterestMustBePaidUpFront = product.InterestMustBePaidUpFront,
                    IsPaidFeeBeforeProcessing = product.IsPaidFeeBeforeProcessing,
                    LoanTerm = product.LoanTerm,
                    LoanMaximumAmount = product.LoanMaximumAmount,
                    LoanTypeCategory = product.LoanTypeCategory,
                    ProductName = product.ProductName,
                    LoanInterestPeriod = product.LoanInterestPeriod,
                    MinimumInterestRate = product.MinimumInterestRate,
                    MaximumInterestRate = product.MaximumInterestRate,
                    ChartOfAccountIdForFee = product.ChartOfAccountIdForFee,
                    PenaltyId = product.PenaltyId,
                    LoanDurationPeriod = product.LoanDurationPeriod,
                    MinimumDurationPeriod = product.MinimumDurationPeriod,
                    MaximumDurationPeriod = product.MaximumDurationPeriod,
                    RequiresGuarantor = product.RequiresGuarantor,
                    IsInterestWaiverApplied = product.IsInterestWaiverApplied,
                    IsProductWithSavingFacilities = product.IsProductWithSavingFacilities,
                    MinimumInterestWaiver = product.MinimumInterestWaiver,
                    MaximumInterestWaiver = product.MaximumInterestWaiver,
                    IsChargesApplied = product.IsChargesApplied,
                    MinimumChargesToAppliedInPercentage = product.MinimumChargesToAppliedInPercentage,
                    MaximumChargesToAppliedPercentage = product.MaximumChargesToAppliedPercentage,
                    DefaultChargeToAppliedPercentage = product.DefaultChargeToAppliedPercentage,
                    MinimumChargesStartDayAfterLoanDueDate = product.MinimumChargesStartDayAfterLoanDueDate,
                    MaximumChargesStartDayAfterLoanDueDate = product.MaximumChargesStartDayAfterLoanDueDate,
                    MinimumDownPaymentPercentage = product.MinimumDownPaymentPercentage,
                    DefaulChargesStartDayAfterLoanDueDate = product.DefaulChargesStartDayAfterLoanDueDate,
                    ChargesAreAppliedToInterestOrBalance = product.ChargesAreAppliedToInterestOrBalance,
                    ChargesStopAfterHowManyDaysFromStart = product.ChargesStopAfterHowManyDaysFromStart,
                    StartGeneratingInterestAfterDisbustment = product.StartGeneratingInterestAfterDisbustment,
                    MinimumNumberOfRepayment = product.MinimumNumberOfRepayment,
                    Description = product.Description,
                    LoanMinimumAmount = product.LoanMinimumAmount,
                    MinimumCollateralPercentage = product.MinimumCollateralPercentage,
                    IsRequiredShareAccount = product.IsRequiredShareAccount,
                    IsRequiredSalaryccount = product.IsRequiredSalaryccount,
                    IsRequiredSavingAccount = product.IsRequiredSavingAccount,
                    IsRequresRegisteredPublicAuthority = product.IsRequresRegisteredPublicAuthority,
                    IsRequredIrrivocableSalaryTransfer = product.IsRequredIrrivocableSalaryTransfer,
                    IsRequiredCollateral = product.IsRequiredCollateral,
                    BlockSalaryAccount = product.BlockSalaryAccount,
                    BlockShareAccount = product.BlockShareAccount,
                    BlockSavingAccount = product.BlockSavingAccount,
                    BlockGurantorAccount = product.BlockGurantorAccount,
                    MinimumSavingAccountBalanceRateForTheRequestAmount = product.MinimumSavingAccountBalanceRateForTheRequestAmount,
                    MinimumSalaryAccountBalanceRateForTheRequestAmount = product.MinimumSalaryAccountBalanceRateForTheRequestAmount,
                    MinimumShareAccountBalanceForTheRequestAmount = product.MinimumShareAccountBalanceForTheRequestAmount,
                    ActiveStatus = product.ActiveStatus,
                    HasTopUp = product.HasTopUp,
                    ChartOfAccountIdForPrincipalAmount = product.ChartOfAccountIdForPrincipalAmount,
                    ChartOfAccountIdForInterestReceived = product.ChartOfAccountIdForInterestReceived,
                    ChartOfAccountIdForPenalty = product.ChartOfAccountIdForPenalty,
                    ChartOfAccountIdForProvisionMoreThanOneYear = product.ChartOfAccountIdForProvisionMoreThanOneYear,
                    ChartOfAccountIdForTax = product.ChartOfAccountIdForTax,
                    ChartOfAccountIdForLoanTransition = product.ChartOfAccountIdForLoanTransition,
                    ChartOfAccountIdForWriteOffPrincipal = product.ChartOfAccountIdForWriteOffPrincipal,
                    ChartOfAccountIdForProvisionMoreThanTwoYear = product.ChartOfAccountIdForProvisionMoreThanTwoYear,
                    ChartOfAccountIdForProvisionMoreThanThreeYear = product.ChartOfAccountIdForProvisionMoreThanThreeYear,
                    ChartOfAccountIdForProvisionMoreThanFourYear = product.ChartOfAccountIdForProvisionMoreThanFourYear,
                    ChartOfAccountIdForTaxiblePrincipalAmount = product.ChartOfAccountIdForTaxiblePrincipalAmount,
                    ChartOfAccountIdForTaxibleInterestReceived = product.ChartOfAccountIdForTaxibleInterestReceived,
                    RepaymentCycles = product.RepaymentCycles,
                    ServiceOption = ServiceOption,
                    UpdateOption = UpdateOption,
                    CapitalOrder = product.CapitalOrder,
                    InterestOrder = product.InterestOrder,
                    FineOrder = product.FineOrder,
                    InterestRate = product.InterestRate,
                    FineRate = product.FineRate,
                    CapitalRate = product.CapitalRate,
                    LoanDeliquencyPeriod = "N/A",
                    RepaymentTypeName = "N/A",
                    IsMortgage = product.IsMortgage,
                    RequireInsurance = product.RequireInsurance,
                    RequireTitleRegistration = product.RequireTitleRegistration,
                    GracePeriodMonths = product.GracePeriodMonths,
                    MaxLoanToValueRatio = product.MaxLoanToValueRatio,
                    MinCollateralCoveragePercent = product.MinCollateralCoveragePercent,
                    EnablePhasedDisbursement = product.EnablePhasedDisbursement,
                    AllowThirdPartyOwnership = product.AllowThirdPartyOwnership
                };

                return updateCommand;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                // _logger.LogError(ex, "An error occurred while mapping LoanProduct to UpdateLoanProductCommand.");
                throw ex;
            }
        }

        //public async Task<ExecutionMessages> Update(UpdateLoanProductCommand model)
        //{
        //    try
        //    {

        //        var LoanProduct = await GetLoanProduct(model.Id);
        //        LoanProduct.UpdateOption = "N/A";
        //        if (LoanProduct != null)
        //        {
        //            if (model.ServiceOption == "product")
        //            {
        //                LoanProduct.ProductCode = model.ProductCode;
        //                LoanProduct.ProductName = model.ProductName;
        //                LoanProduct.ActiveStatus = model.ActiveStatus;
        //                LoanProduct.Description = model.Description;
        //                LoanProduct.TargetType = model.TargetType;
        //                LoanProduct.LoanProductCategoryId = model.LoanProductCategoryId;
        //                LoanProduct.LoanTermId = model.LoanTermId;
        //                LoanProduct.LoanTermId = model.LoanTermId;
        //                LoanProduct.IsProductWithSavingFacilities = model.LoanTypeCategory == "SSF" ? true : false;
        //                LoanProduct.LoanTypeCategory = model.LoanTypeCategory;
        //                LoanProduct.IsMortgage = model.IsMortgage;

        //                //
        //            }
        //            else if (model.ServiceOption == "mortgage")
        //            {
        //                LoanProduct.IsMortgage = model.IsMortgage;
        //                LoanProduct.RequireInsurance = model.RequireInsurance;
        //                LoanProduct.RequireTitleRegistration = model.RequireTitleRegistration;
        //                LoanProduct.GracePeriodMonths = model.GracePeriodMonths;
        //                LoanProduct.MaxLoanToValueRatio = model.MaxLoanToValueRatio;
        //                LoanProduct.MinCollateralCoveragePercent = model.MinCollateralCoveragePercent;
        //                LoanProduct.EnablePhasedDisbursement = model.EnablePhasedDisbursement;
        //                LoanProduct.AllowThirdPartyOwnership = model.AllowThirdPartyOwnership;
        //            }

        //            else if (model.ServiceOption == "gurantee")
        //            {
        //                LoanProduct.RequiresGuarantor = model.RequiresGuarantor;
        //                LoanProduct.MinimumCollateralPercentage = model.MinimumCollateralPercentage;
        //                LoanProduct.IsRequiredCollateral = model.IsRequiredCollateral;
        //                LoanProduct.IsRequiredSalaryccount = model.IsRequiredSalaryccount;
        //                LoanProduct.IsRequiredSavingAccount = model.IsRequiredSavingAccount;
        //                LoanProduct.IsRequiredShareAccount = model.IsRequiredShareAccount;
        //                LoanProduct.IsRequredIrrivocableSalaryTransfer = model.IsRequredIrrivocableSalaryTransfer;
        //                LoanProduct.IsRequresRegisteredPublicAuthority = model.IsRequresRegisteredPublicAuthority;
        //                LoanProduct.MinimumShareAccountBalanceForTheRequestAmount = model.MinimumShareAccountBalanceForTheRequestAmount;
        //                LoanProduct.MinimumSalaryAccountBalanceRateForTheRequestAmount = model.MinimumSalaryAccountBalanceRateForTheRequestAmount;
        //                LoanProduct.BlockGurantorAccount = model.BlockGurantorAccount;
        //                LoanProduct.BlockSalaryAccount = model.BlockSalaryAccount;
        //                LoanProduct.BlockSavingAccount = model.BlockSavingAccount;
        //                LoanProduct.BlockShareAccount = model.BlockShareAccount;
        //                LoanProduct.MinimumSavingAccountBalanceRateForTheRequestAmount =
        //                    model.MinimumSavingAccountBalanceRateForTheRequestAmount;
        //                LoanProduct.Co_obligorMustHaveFundToGuranteeLoan = model.Co_obligorMustHaveFundToGuranteeLoan;
        //                LoanProduct.ShorteeMustHaveFundToGuranteeLoan = model.ShorteeMustHaveFundToGuranteeLoan;
        //                LoanProduct.MinimumPercentageCoverageOfShortee = model.MinimumPercentageCoverageOfShortee;


        //            }
        //            else if (model.ServiceOption == "loan_range")
        //            {
        //                LoanProduct.LoanMinimumAmount = model.LoanMinimumAmount;
        //                LoanProduct.MinimumDownPaymentPercentage = model.MinimumDownPaymentPercentage;
        //                LoanProduct.LoanMaximumAmount = model.LoanMaximumAmount;
        //                //LoanProduct.TargetType = model.TargetType;
        //                LoanProduct.IsPaidFeeBeforeProcessing = model.IsPaidFeeBeforeProcessing;

        //            }
        //            else if (model.ServiceOption == "topup")
        //            {
        //                LoanProduct.HasTopUp = model.HasTopUp;
        //                LoanProduct.MinimumPercentageRefundBeforeRefinancing = model.MinimumPercentageRefundBeforeRefinancing;
        //            }

        //            else if (model.ServiceOption == "interest")
        //            {
        //                LoanProduct.IsInterestWaiverApplied = model.IsInterestWaiverApplied;
        //                LoanProduct.MinimumInterestWaiver = model.MinimumInterestWaiver;
        //                LoanProduct.MaximumInterestWaiver = model.MaximumInterestWaiver;
        //                LoanProduct.LoanInterestPeriod = model.LoanInterestPeriod;
        //                LoanProduct.MinimumInterestRate = model.MinimumInterestRate;
        //                LoanProduct.MaximumInterestRate = model.MaximumInterestRate;
        //                LoanProduct.StartGeneratingInterestAfterDisbustment = model.StartGeneratingInterestAfterDisbustment;
        //                LoanProduct.InterestMustBePaidUpFront = model.InterestMustBePaidUpFront;
        //                LoanProduct.StopInterestCalculationAtLoanMaturityDate = model.StopInterestCalculationAtLoanMaturityDate;
        //                LoanProduct.NumberOfDaysToStopInterestCalculation = model.NumberOfDaysToStopInterestCalculation;
        //            }
        //            else if (model.ServiceOption == "duration")
        //            {
        //                LoanProduct.LoanDurationPeriod = model.LoanDurationPeriod;
        //                LoanProduct.MinimumDurationPeriod = model.MinimumDurationPeriod;
        //                LoanProduct.MaximumDurationPeriod = model.MaximumDurationPeriod;

        //            }
        //            else if (model.ServiceOption == "repayment")
        //            {
        //                LoanProduct.RepaymentCycles = model.RepaymentCycles;
        //                LoanProduct.CapitalOrder = model.CapitalOrder;
        //                LoanProduct.InterestOrder = model.InterestOrder;
        //                LoanProduct.FineOrder = model.FineOrder;
        //                LoanProduct.InterestRate = model.InterestRate;
        //                LoanProduct.FineRate = model.FineRate;
        //                LoanProduct.CapitalRate = model.CapitalRate;
        //            }
        //            else if (model.ServiceOption == "fee")
        //            {

        //                //LoanProduct.IsEarlyPartialRepaymentFeeRate = model.IsEarlyPartialRepaymentFeeRate;
        //                //LoanProduct.EarlyPartialRepaymentFee = model.EarlyPartialRepaymentFee;
        //                //LoanProduct.IsEarlyTotalRepaymentFeeRate = model.IsEarlyTotalRepaymentFeeRate;
        //                //LoanProduct.EarlyTotalRepaymentFee = model.EarlyTotalRepaymentFee;
        //                //LoanProduct.MinimumProcessingFeeRate = model.MinimumProcessingFeeRate;
        //                //LoanProduct.DefaultProcessingFeeRate = model.DefaultProcessingFeeRate;
        //                //LoanProduct.MaximumProcessingFeeRate = model.MaximumProcessingFeeRate;
        //                //LoanProduct.MinimumInspectionFeeRate = model.MinimumInspectionFeeRate;
        //                //LoanProduct.MaximumInspectionFeeRate = model.MaximumInspectionFeeRate;
        //                //LoanProduct.DefaultInspectionFeeRate = model.DefaultInspectionFeeRate;
        //            }
        //            else if (model.ServiceOption == "advancedsettings")
        //            {
        //                //LoanProduct.FirstRepaymentAmount = model.FirstRepaymentAmount;
        //                //LoanProduct.HowShoudInterestBeCahrgedInLoanSchedule = model.HowShoudInterestBeCahrgedInLoanSchedule;
        //                //LoanProduct.HowShoudPrincipalBeCahrgedInLoanSchedule = model.HowShoudPrincipalBeCahrgedInLoanSchedule;
        //                //LoanProduct.CalculateInterestOnEachRepaymentOnProRatabase = model.CalculateInterestOnEachRepaymentOnProRatabase;
        //                //LoanProduct.LoanScheduleDescription = model.LoanScheduleDescription;

        //            }
        //            else if (model.ServiceOption == "accounting")
        //            {
        //                LoanProduct.ChartOfAccountIdForPrincipalAmount = model.ChartOfAccountIdForPrincipalAmount;
        //                LoanProduct.ChartOfAccountIdForInterestReceived = model.ChartOfAccountIdForInterestReceived;
        //                LoanProduct.ChartOfAccountIdForPenalty = model.ChartOfAccountIdForPenalty;
        //                LoanProduct.ChartOfAccountIdForTax = model.ChartOfAccountIdForTax;
        //                LoanProduct.ChartOfAccountIdForLoanTransition = model.ChartOfAccountIdForLoanTransition;
        //                LoanProduct.ChartOfAccountIdForWriteOffPrincipal = model.ChartOfAccountIdForWriteOffPrincipal;
        //                LoanProduct.ChartOfAccountIdForProvisionMoreThanOneYear = model.ChartOfAccountIdForProvisionMoreThanOneYear;
        //                LoanProduct.ChartOfAccountIdForProvisionMoreThanTwoYear = model.ChartOfAccountIdForProvisionMoreThanTwoYear;
        //                LoanProduct.ChartOfAccountIdForProvisionMoreThanThreeYear = model.ChartOfAccountIdForProvisionMoreThanThreeYear;
        //                LoanProduct.ChartOfAccountIdForProvisionMoreThanFourYear = model.ChartOfAccountIdForProvisionMoreThanFourYear;
        //                LoanProduct.ChartOfAccountIdForTaxiblePrincipalAmount = model.ChartOfAccountIdForTaxiblePrincipalAmount;
        //                LoanProduct.ChartOfAccountIdForTaxibleInterestReceived = model.ChartOfAccountIdForTaxibleInterestReceived;
        //                LoanProduct.UpdateOption = "assign_account_chart";
        //                LoanProduct.ChartOfAccountIdForFee = model.ChartOfAccountIdForFee;

        //            }
        //            //ChartOfAccountIdForFee
        //            else if (model.ServiceOption == "charges")
        //            {
        //                LoanProduct.IsChargesApplied = model.IsChargesApplied;
        //                LoanProduct.PenaltyId = model.PenaltyId;
        //                //LoanProduct.MinimumChargesToAppliedInPercentage = model.MinimumChargesToAppliedInPercentage;
        //                //LoanProduct.MaximumChargesToAppliedPercentage = model.MaximumChargesToAppliedPercentage;
        //                //LoanProduct.ChargesAreAppliedToInterestOrBalance = model.ChargesAreAppliedToInterestOrBalance;
        //                //LoanProduct.ChargesStopAfterHowManyDaysFromStart = model.ChargesStopAfterHowManyDaysFromStart;
        //                //LoanProduct.DefaultChargeToAppliedPercentage = model.DefaultChargeToAppliedPercentage;
        //                //LoanProduct.MinimumChargesStartDayAfterLoanDueDate = model.MinimumChargesStartDayAfterLoanDueDate;
        //                //LoanProduct.MaximumChargesStartDayAfterLoanDueDate = model.MaximumChargesStartDayAfterLoanDueDate;
        //                //LoanProduct.DefaulChargesStartDayAfterLoanDueDate = model.DefaulChargesStartDayAfterLoanDueDate;

        //            }
        //            var dataobject = ProductMappingToUpdateObject(LoanProduct, model.ServiceOption, LoanProduct.UpdateOption);
        //            var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<LoanProduct>>(string.Format(APICallHelper.Get_Update_Delete_LoanProduct, model.Id), dataobject);
        //            if (response.IsSuccess)
        //            {
        //                // Successful creation
        //                GetExecutionMessages(response, true, $"{model.ServiceOption.ToUpper()}", MessagesResults.Success,
        //                    ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
        //                return ExecutionMessage;
        //            }
        //            else
        //            {
        //                // Failed creation
        //                GetExecutionMessages(model, false, model.ServiceOption.ToUpper(), MessagesResults.Failed,
        //                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        // Log and handle exception
        //        GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
        //            SystemMessageStatus.Failed.ToString(), ex);
        //    }
        //    return ExecutionMessage;
        //}


        public async Task<ExecutionMessages> Update(LoanProductPolicy policy, string serviceOption, List<string> repaymentCycles = null, LoanProductAccountingProfile accounting = null)
        {
            try
            {
                // 1. FETCH existing data first (The "Monolithic" approach)
                var fullLoanProduct = await GetLoanProduct2(policy.LoanProductId);
                var UpdatePolicy = fullLoanProduct.Policy;
                UpdatePolicy.LoanProductId = fullLoanProduct.Id;
                if (fullLoanProduct == null)
                {
                    GetExecutionMessages(policy, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Loan Product not found in database.");
                    return ExecutionMessage;
                }

                // 2. MAP the incoming UI data onto the Fetched Object
                // We update specific sections of 'fullLoanProduct' based on the serviceOption
                //  fullLoanProduct.UpdateOption = "N/A"; // Legacy flag from your snippet

                switch (serviceOption)
                {

                    case "mortgage":
                        // Assuming 'Policy' is a property on PCMFLoanProduct
                        UpdatePolicy.IsMortgage = policy.IsMortgage;
                        UpdatePolicy.RequireInsurance = policy.RequireInsurance;
                        UpdatePolicy.RequireTitleRegistration = policy.RequireTitleRegistration;
                        UpdatePolicy.GracePeriodMonths = policy.GracePeriodMonths;
                        UpdatePolicy.MaxLoanToValueRatio = policy.MaxLoanToValueRatio;
                        UpdatePolicy.MinCollateralCoveragePercent = policy.MinCollateralCoveragePercent;
                        UpdatePolicy.EnablePhasedDisbursement = policy.EnablePhasedDisbursement;
                        UpdatePolicy.AllowThirdPartyOwnership = policy.AllowThirdPartyOwnership;
                        break;

                    case "gurantee":
                        UpdatePolicy.RequiresGuarantor = policy.RequiresGuarantor;
                        UpdatePolicy.MinimumCollateralPercentage = policy.MinimumCollateralPercentage;
                        UpdatePolicy.IsRequiredCollateral = policy.IsRequiredCollateral;
                        UpdatePolicy.IsRequiredSalaryccount = policy.IsRequiredSalaryccount;
                        UpdatePolicy.IsRequiredSavingAccount = policy.IsRequiredSavingAccount;
                        UpdatePolicy.IsRequiredShareAccount = policy.IsRequiredShareAccount;
                        UpdatePolicy.IsRequredIrrivocableSalaryTransfer = policy.IsRequredIrrivocableSalaryTransfer;
                        UpdatePolicy.IsRequresRegisteredPublicAuthority = policy.IsRequresRegisteredPublicAuthority;
                        UpdatePolicy.MinimumShareAccountBalanceForTheRequestAmount = policy.MinimumShareAccountBalanceForTheRequestAmount;
                        UpdatePolicy.MinimumSalaryAccountBalanceRateForTheRequestAmount = policy.MinimumSalaryAccountBalanceRateForTheRequestAmount;
                        UpdatePolicy.MinimumSavingAccountBalanceRateForTheRequestAmount = policy.MinimumSavingAccountBalanceRateForTheRequestAmount;
                        UpdatePolicy.BlockSalaryAccount = policy.BlockSalaryAccount;
                        UpdatePolicy.BlockSavingAccount = policy.BlockSavingAccount;
                        UpdatePolicy.BlockShareAccount = policy.BlockShareAccount;
                        // fullLoanProduct.Policy.BlockGurantorAccount = policy.BlockGurantorAccount; // Uncomment if property exists in DTO
                        UpdatePolicy.Co_obligorMustHaveFundToGuranteeLoan = policy.Co_obligorMustHaveFundToGuranteeLoan;
                        UpdatePolicy.ShorteeMustHaveFundToGuranteeLoan = policy.ShorteeMustHaveFundToGuranteeLoan;
                        UpdatePolicy.MinimumPercentageCoverageOfShortee = policy.MinimumPercentageCoverageOfShortee;
                        break;

                    case "loan_range":
                        UpdatePolicy.LoanMinimumAmount = policy.LoanMinimumAmount;
                        UpdatePolicy.MinimumDownPaymentPercentage = policy.MinimumDownPaymentPercentage;
                        UpdatePolicy.LoanMaximumAmount = policy.LoanMaximumAmount;
                        UpdatePolicy.IsPaidFeeBeforeProcessing = policy.IsPaidFeeBeforeProcessing;

                        break;

                    case "topup":
                        UpdatePolicy.HasTopUp = policy.HasTopUp;
                        UpdatePolicy.MinimumPercentageRefundBeforeRefinancing = policy.MinimumPercentageRefundBeforeRefinancing;
                        break;

                    case "interest":
                        UpdatePolicy.IsInterestWaiverApplied = policy.IsInterestWaiverApplied;
                        UpdatePolicy.MinimumInterestWaiver = policy.MinimumInterestWaiver;
                        UpdatePolicy.MaximumInterestWaiver = policy.MaximumInterestWaiver;
                        UpdatePolicy.LoanInterestPeriod = policy.LoanInterestPeriod;
                        UpdatePolicy.MinimumInterestRate = policy.MinimumInterestRate;
                        UpdatePolicy.MaximumInterestRate = policy.MaximumInterestRate;
                        UpdatePolicy.StartGeneratingInterestAfterDisbustment = policy.StartGeneratingInterestAfterDisbustment;
                        UpdatePolicy.InterestMustBePaidUpFront = policy.InterestMustBePaidUpFront;
                        UpdatePolicy.StopInterestCalculationAtLoanMaturityDate = policy.StopInterestCalculationAtLoanMaturityDate;
                        UpdatePolicy.NumberOfDaysToStopInterestCalculation = policy.NumberOfDaysToStopInterestCalculation;
                        break;

                    case "duration":
                        UpdatePolicy.LoanDurationPeriod = policy.LoanDurationPeriod;
                        UpdatePolicy.MinimumDurationPeriod = policy.MinimumDurationPeriod;
                        UpdatePolicy.MaximumDurationPeriod = policy.MaximumDurationPeriod;
                        break;

                    case "repayment":
                        fullLoanProduct.RepaymentCycles = repaymentCycles; // Directly assign list
                        UpdatePolicy.CapitalOrder = policy.CapitalOrder;
                        UpdatePolicy.InterestOrder = policy.InterestOrder;
                        UpdatePolicy.FineOrder = policy.FineOrder;
                        UpdatePolicy.InterestRate = policy.InterestRate;
                        UpdatePolicy.FineRate = policy.FineRate;
                        UpdatePolicy.CapitalRate = policy.CapitalRate;
                        break;

                    case "charges":
                        UpdatePolicy.IsChargesApplied = policy.IsChargesApplied;
                        UpdatePolicy.PenaltyId = policy.PenaltyId;
                        break;
                }

                // 3. SEND the single consolidated object to the Single Endpoint
                // We ignore the individual 'policy' payload and send 'fullLoanProduct'
                var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<bool>>(APICallHelper.LoanProductUpsertPolicy, UpdatePolicy);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, $"{serviceOption.ToUpper()}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    GetExecutionMessages(policy, false, serviceOption.ToUpper(), MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }

        // ----------------------------------------------------------------------------------
        // HELPER METHOD: To fetch the product by ID (Essential for this approach)
        // ----------------------------------------------------------------------------------
        private async Task<PCMFLoanProduct> GetLoanProduct2(string id)
        {
            try
            {
                var response = await _loanConfigApiHelper.GetAsync<ServiceResponse<PCMFLoanProduct>>(string.Format(APICallHelper.Get_Update_Delete_LoanProduct, id));
                if (response.IsSuccess)
                {
                    var data = response.ApiResponseData.Data;

                    return data;
                }
                return null;
            }
            catch (Exception)
            {
                // Fail silently here, main method handles null check
            }
            return null;
        }

    }


}
