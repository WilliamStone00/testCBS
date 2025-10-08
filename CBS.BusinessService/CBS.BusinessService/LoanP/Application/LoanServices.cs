using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using System.Web.Mvc;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.ReportDataSetDto.LoanPortFolioDataSet;
using CBS.BusinessService.LoanportFolioFlattener;
using CBS.NLoan.Data.Dto.DataSetLoanPortfolio;
using CBS.FrontDesk.Data.ReportDataSetDto.LoanDeliquentAnalysis;
using Microsoft.Owin.Logging;
using CBS.FrontDesk.Data.Entity.DataSetLoanPortfolio;

namespace CBS.BusinessService
{

    public class LoanServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;
        private readonly BranchServices _branchServices;
        private readonly IndividualProfileServices _individualProfileServices;

        public LoanServices(BranchServices branchServices = null, IndividualProfileServices individualProfileServices = null)
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
            _branchServices = branchServices;
            _individualProfileServices = individualProfileServices;
        }

        public async Task<CustomDataTable> GetDataTable(DataTableOptions dataTableOptions, string searchCriterial, bool isByBranch = true)
        {
            var loanResource = new LoanResource
            {
                OrderBy = "CustomerId",
                PageSize = dataTableOptions.pageSize,
                Skip = dataTableOptions.skip,
                IsByBranch = isByBranch,
                SearchQuery = searchCriterial == "" ? "all" : searchCriterial,
            };
            var loans = await GetLoans(loanResource);
            // Handle potential null values safely
            var paginationMetadata = loans?.FirstOrDefault()?.PaginationMetadata ?? new PaginationMetadata
            {
                TotalCount = 0
            };

            Func<Task<List<DataTableLoan>>> getDataFunc = async () => (await GetDataTable(loans.ToList()));
            var dataTable = await GenerateDataTable(dataTableOptions, getDataFunc, paginationMetadata.TotalCount);
            return dataTable;

        }

        public async Task<CustomDataTable> GenerateDataTable(DataTableOptions dataTableOptions, Func<Task<List<DataTableLoan>>> getDataFunc, int totalRecords)
        {
            List<DataTableLoan> data = (await getDataFunc()).ToList();
            dataTableOptions.recordsTotal = totalRecords;
            //var filteredData = DatatableHelper.FilterData(data, dataTableOptions);
            // Construct CustomDataTable using pagination metadata
            var dataTable = new CustomDataTable(
                Convert.ToInt32(dataTableOptions.draw),
                totalRecords,
                dataTableOptions.recordsFiltered,
                data,
                dataTableOptions
            );

            return dataTable;
        }


        public async Task<IEnumerable<Loan>> GetLoans(LoanResource resource)
        {
            try
            {

                var queryString = ToQueryString(resource);
                var fullUrl = $"{APICallHelper.GelLoansSearchByAnyCriterialQuery}?{queryString}";
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<Loan>>>(fullUrl);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<Loan>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<List<DataTableLoan>> GetDataTable(List<Loan> loans)
        {

            var loanDtos = loans.Select(loan => new DataTableLoan
            {
                DisbursementDate = loan.DisbursementDate.ToString("yyyy-MM-dd, hh:mm:ss"),
                MaturityDate = loan.MaturityDate.ToString("yyyy-MM-dd"),
                Principal = loan.LoanAmount,
                InterestRate = loan.InterestRate,
                AccrualInterest = loan.AccrualInterest,
                Fee = loan.Fee,
                Tax = loan.Tax,
                Fines=loan.Penalty,
                Penalty = loan.Penalty,
                DueAmount = loan.DueAmount,
                Paid = loan.Paid,
                Balance = loan.Balance,
                LastPayment = loan.LastPayment,
                LoanStatus = loan.LoanStatus,
                IsCurrentLoan = loan.IsCurrentLoan,
                Id = loan.Id,
                CustomerId = loan.CustomerId
            }).ToList();
            return loanDtos;


        }

        public async Task<CustomDataTable> GetDataTableAsync(GetLoansDataTableQuery loansDataTableQuery)
        {
           
            loansDataTableQuery.DataTableOptions.sortColumnName = "LoanDate";
            if (!IsHeadOffice())
            {
                loansDataTableQuery.BranchId=GetBranchID();
            }
            // Make API call to fetch the DataTable result
            var couApiResponse = await _loanConfigApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.LoaDataTablePaggination,
                loansDataTableQuery
            );

            // Return response if successful
            if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
            {
                return couApiResponse.ApiResponseData.Data;
            }

            // Return an empty DataTable if the request fails
            return new CustomDataTable(
                draw: Convert.ToInt32(loansDataTableQuery.DataTableOptions.draw),
                recordsTotal: 0,
                recordsFiltered: 0,
                data: new List<object>(), // No data
                dataTableOptions: loansDataTableQuery.DataTableOptions
            );
        }

        public List<Loan> MapLoansWithBranchDetails(List<Branch> branches, List<Loan> loans)
        {
            var mappedLoans = new List<Loan>();

            foreach (var loan in loans)
            {
                var matchedBranch = branches.FirstOrDefault(b => b.BranchCode == loan.BranchCode);

                var mappedLoan = new Loan
                {
                    Id = loan.Id,
                    LoanApplicationId = loan.LoanApplicationId,
                    Principal = loan.Principal,
                    LoanAmount = loan.LoanAmount,
                    InterestForcasted = loan.InterestForcasted,
                    InterestRate = loan.InterestRate,
                    LastPayment = loan.LastPayment,
                    Paid = loan.Paid,
                    Balance = loan.Balance,
                    DueAmount = loan.DueAmount,
                    AccrualInterest = loan.AccrualInterest,
                    LastCalculatedInterest = loan.LastCalculatedInterest,
                    AccrualInterestPaid = loan.AccrualInterestPaid,
                    TotalPrincipalPaid = loan.TotalPrincipalPaid,
                    Tax = loan.Tax,
                    TaxPaid = loan.TaxPaid,
                    FeePaid = loan.FeePaid,
                    Fee = loan.Fee,
                    Penalty = loan.Penalty,
                    PenaltyPaid = loan.PenaltyPaid,
                    DisbursementDate = loan.DisbursementDate,
                    FirstInstallmentDate = loan.FirstInstallmentDate,
                    NextInstallmentDate = loan.NextInstallmentDate,
                    LoanDate = loan.LoanDate,
                    IsLoanDisbursted = loan.IsLoanDisbursted,
                    DisbursmentStatus = loan.DisbursmentStatus,
                    LastInterestCalculatedDate = loan.LastInterestCalculatedDate,
                    LastRefundDate = loan.LastRefundDate,
                    LastEventData = loan.LastEventData,
                    CustomerId = loan.CustomerId,
                    LoanManager = loan.LoanManager,
                    LoanStatus = loan.LoanStatus,
                    IsRestructured = loan.IsRestructured,
                    NewLoanId = loan.NewLoanId,
                    IsWriteOffLoan = loan.IsWriteOffLoan,
                    IsDeliquentLoan = loan.IsDeliquentLoan,
                    IsCurrentLoan = loan.IsCurrentLoan,
                    MaturityDate = loan.MaturityDate,
                    OrganizationId = loan.OrganizationId,
                    BranchId = loan.BranchId,
                    BankId = loan.BankId,
                    LoanType = loan.LoanType,
                    BranchCode = loan.BranchCode,
                    LoanId = loan.LoanId,
                    CustomerName = loan.CustomerName,
                    LoanDuration = loan.LoanDuration,
                    LoanApplication = loan.LoanApplication,
                    LoanJourneyStatus = loan.LoanJourneyStatus,
                    VatRate = loan.VatRate,
                    LoanTarget = loan.LoanTarget,
                    LoanCategory = loan.LoanCategory,
                    AccountNumber = loan.AccountNumber,
                    IsUpload = loan.IsUpload,
                    RequestedAmount = loan.RequestedAmount,
                    RestructuredBalance = loan.RestructuredBalance,
                    OldLoanPayment = loan.OldLoanPayment,
                    DeliquentInterest = loan.DeliquentInterest,
                    AdvancedPaymentDays = loan.AdvancedPaymentDays,
                    DeliquentDays = loan.DeliquentDays,
                    AdvancedPaymentAmount = loan.AdvancedPaymentAmount,
                    DeliquentAmount = loan.DeliquentAmount,
                    LoanStructuringStatus = loan.LoanStructuringStatus,
                    LoanStructuringDate = loan.LoanStructuringDate,
                    OldCapital = loan.OldCapital,
                    OldInterest = loan.OldInterest,
                    OldVAT = loan.OldVAT,
                    OldPenalty = loan.OldPenalty,
                    OldBalance = loan.OldBalance,
                    OldDueAmount = loan.OldDueAmount,
                    DeliquentStatus = loan.DeliquentStatus,
                    StopInterestCalculation = loan.StopInterestCalculation,
                    StoppedBy = loan.StoppedBy,
                    DateInterestWastStoped = loan.DateInterestWastStoped,
                    LastDeliquecyProcessedDate = loan.LastDeliquecyProcessedDate,
                    BranchName = matchedBranch != null ? matchedBranch.Name : null,
                    NumberOfInstallments = loan.NumberOfInstallments,
                    RepaymentCycle = loan.RepaymentCycle,
                    LoanDurarion = loan.LoanDurarion,
                    IndividualCustomer = loan.IndividualCustomer,
                    PaginationMetadata = loan.PaginationMetadata,
                    FileDownloadInfos = loan.FileDownloadInfos,
                    InitiateLoanDownloadCommand = loan.InitiateLoanDownloadCommand,
                    Refunds = loan.Refunds,
                    LoanAmortizations = loan.LoanAmortizations,
                    DisburstedLoans = loan.DisburstedLoans,
                    DailyInterestCalculations = loan.DailyInterestCalculations,
                    IsCompleted = loan.IsCompleted,
                    MigrationDate = loan.MigrationDate,
                    InterestMustBePaidUpFront = loan.InterestMustBePaidUpFront,
                    InterestAmountUpfront = loan.InterestAmountUpfront,
                    LoanDeliquencyConfigurationId = loan.LoanDeliquencyConfigurationId,
                    Savings = loan.Savings,
                    OShares = loan.OShares,
                    PShares = loan.PShares,
                    Deposit = loan.Deposit,
                    Salary = loan.Salary,
                    Shortee = loan.Shortee,
                    Co_Obligor = loan.Co_Obligor,
                    Co_OperationGurantor = loan.Co_OperationGurantor,
                    OtherGuaranteeFund = loan.OtherGuaranteeFund,
                    TotalFundGuranteed = loan.TotalFundGuranteed,
                    PercentageOfLiquidityCoverage = loan.PercentageOfLiquidityCoverage,
                    PercentageOfCollateralCoverage = loan.PercentageOfCollateralCoverage,
                    PercentageOfOverAllCoverage = loan.PercentageOfOverAllCoverage,
                    LoanDeliquencyConfiguration = loan.LoanDeliquencyConfiguration,
                    LoanDeliquencyConfigurationName = loan.LoanDeliquencyConfigurationName
                };

                mappedLoans.Add(mappedLoan);
            }

            return mappedLoans;
        }

        public async Task<ExecutionMessages> ApprovePendingDisbursement(AddLoanDisbumentCommand model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<LoanApplication>>(APICallHelper.Disbursed, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Disbursement", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, "Disbursement", MessagesResults.Failed,
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
        public async Task<IEnumerable<Loan>> GetPendingDisbursementLoans()
        {
            try
            {
                var getAllLoanQuery = new GetAllLoanQuery { BranchId = GetBranchID(), IsByBranch=true, QueryParam="Pending" };
                var couApiResponse = await _loanConfigApiHelper.PostAsync<ResponseObject<List<Loan>>>(APICallHelper.GetLoans, getAllLoanQuery);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<Loan>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<FileDownloadInfo>> GetAllFileDownloadInfoLoan()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<FileDownloadInfo>>>(APICallHelper.GetAllBulkDownloadInfosLoan);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<FileDownloadInfo>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<FileDownloadInfo>> GetAllFileDownloadInfoLoanPerUser()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<FileDownloadInfo>>>(string.Format(APICallHelper.GetAllBulkDownloadInfosLoanPerUser, GetUserID()));
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<FileDownloadInfo>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<FileDownloadDto> DownloadFile(string fileId)
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<FileDownloadDto>>(string.Format(APICallHelper.DownloadLoanFile, fileId));

                if (couApiResponse.IsSuccess)
                {
                    // FileDownloadDto should contain file data and metadata
                    return couApiResponse.ApiResponseData.Data;
                }
                return new FileDownloadDto { ErrorMessage = couApiResponse.Message };

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.BulkDownloadDeleteAndGetLoan, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(inResponse, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<FileDownloadInfo> GetFileDownloadInfoLoan(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<FileDownloadInfo>>(string.Format(APICallHelper.BulkDownloadDeleteAndGetLoan, id));
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
        public async Task<ExecutionMessages> InitiateBulkDownloadLoansBranch(InitiateLoanDownloadCommand model)
        {
            try
            {
                model.IsByBranch = true;
                model.StartDate = GetDateTime(model.StrStartDate);
                model.EndDate = GetDateTime(model.StrEndDate);
                model.EndDate = GetDateTime(model.StrEndDate);
                model.UserId = GetUserID();
                model.FullName = GetUserFullName();
                model.BranchName = "N/A";
                model.BranchId = model.BranchId;
                if (model.IsMigratedLoan)
                {
                    model.QueryParameter = "MigratedLoans";
                }
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<FileDownloadInfo>>(APICallHelper.InitiateBulkDownloadLoans, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Bulk Download", MessagesResults.Success,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, "Bulk Download", MessagesResults.Failed,
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
        public async Task<IEnumerable<Loan>> GetLoanByCustomerID(GetAllLoanByCustomerIdQuery loanByCustomerIdQuery)
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.PostAsync<ResponseObject<List<Loan>>>(APICallHelper.GetAllLoanByCustomerId, loanByCustomerIdQuery);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<Loan>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<List<Loan>> GetAllMembersCurrents(string customerid)
        {
            try
            {
                var getAllLoanByCustomerIdQuery = new GetAllLoanByCustomerIdQuery { CustomerId = customerid, QueryParameter = "Open" };
                var couApiResponse = await _loanConfigApiHelper.PostAsync<ResponseObject<List<Loan>>>(APICallHelper.GetAllLoanByCustomerId, getAllLoanByCustomerIdQuery);

                if (couApiResponse.ApiResponseData!=null)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<Loan>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public SelectList GetAllMembersCurrentsDropdown(List<Loan> loans)
        {
            try
            {
                return ProcessApiResponseResponse(loans);

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
                Text = $"[REF: {a.Id} ][Date: {a.LoanDate.ToString("dd/MM/yyyy")}][Amount: {CurrencyFormatter(a.LoanAmount)}][A.Int: {CurrencyFormatter(a.AccrualInterest)}][Balance: {CurrencyFormatter(a.Balance)}][D.Amt: {CurrencyFormatter(a.DueAmount)}]",
                Value = a.Id
            });
            var defaultSelectedValue = "default-value";
            return new SelectList(values.ToList(), "Value", "Text", defaultSelectedValue);

        }

        public async Task<Loan> GetLoan(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<Loan>>(string.Format(APICallHelper.GetLoan, id));
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
        public LoanData MapLoan(Loan loan)
        {
            if (loan == null)
                return null;

            var loanData = new LoanData
            {
                // 🔢 Core financials
                Id = loan.Id,
                Principal = loan.Balance,
                AccrualInterest = loan.AccrualInterest,
                Tax = loan.Tax,
                Penalty = loan.Penalty,
                DueAmount = loan.DueAmount,
                VatRate = loan.VatRate,
                InterestRate = loan.InterestRate,

                // 📌 Identifiers
                LoanProductId = loan.LoanApplication?.LoanProductId,
                LoanProductName = loan.LoanApplication?.LoanProduct?.ProductName,

                // 👤 Member
                MemberId = loan.CustomerId,
                MemberName = loan.CustomerName,

                // 🏦 Loan config
                LoanType = loan.LoanType,
                LoanTermName = loan.LoanApplication?.LoanProduct?.LoanTerm?.Name,
                LoanTarget = loan.LoanTarget,
                LoanPurpose = loan.LoanApplication?.LoanPurpose?.purposeName,
                RepaymentPeriod = loan.LoanApplication?.RepaymentCircle,
                NumberOfInstallments = loan.NumberOfInstallments,
                RepaymentFrequency = loan.RepaymentCycle,

                // 📅 Dates
                DisbursementDate = loan.DisbursementDate,
                MaturityDate = loan.MaturityDate,
                LastRepaymentDate = loan.LastRefundDate,

                // 📊 Status
                LoanStatus = loan.LoanStatus?.ToString(),
                LoanCategory = loan.LoanCategory?.ToString(),
                DisbursementChannel = loan.DisbursmentStatus,
                InterestCalculationMethod = loan.LoanApplication.InterestMethod,

                ProductCategoryId = loan.LoanApplication.LoanProduct.LoanProductCategoryId,
                ProductCategoryName = loan.LoanApplication.LoanProduct.LoanProductCategory.Name,
            };

            return loanData;
        }

        public async Task<Loan> GetLoanWithCustomerAndBranch(string customerId)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<Loan>>(string.Format(APICallHelper.GetLoan, customerId));
                if (cusResponseObject.IsSuccess && cusResponseObject.ApiResponseData != null)
                {
                    var loan = cusResponseObject.ApiResponseData.Data;
                    var individualCustomerProfile = await _individualProfileServices.GetCustomerLight(loan.CustomerId);
                    loan.IndividualCustomer = individualCustomerProfile;
                    return loan;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }

        }
        public List<LoanDeliquentDto> OrderDelinquentLoans(List<LoanDeliquentDto> loans)
        {
            if (loans == null || !loans.Any())
                return new List<LoanDeliquentDto>();

            return loans
                .OrderBy(l => l.LoanType != "Main_Loan")  // Main_Loan first
                .ThenBy(l => l.DaysFrom)                  // Then by DaysFrom (PAR structure)
                .ThenBy(l => l.CustomerId)                // Finally by CustomerId
                .ToList();
        }
        public List<LoanDeliquentDto> OrderAndSummarizeDelinquentLoans(List<LoanDeliquentDto> loans)
        {
            if (loans == null || !loans.Any())
                return new List<LoanDeliquentDto>();

            var summary = new LoanDeliquentDto();
            decimal totalSavings = 0;
            foreach (var loan in loans)
            {
                totalSavings+=loan.SavingBalance;
                bool isCurrent = loan.DeliquentStatus?.Trim().Equals("Current", StringComparison.OrdinalIgnoreCase) == true;

                if (isCurrent)
                {
                    summary.TotalCurrentLoanCount++;
                    summary.TotalCurrentCapital += loan.LoanAmount;
                    summary.TotalCurrentBalance += loan.Balance;
                    summary.TotalCurrentInterest += loan.InterestForcasted;
                }
                else
                {
                    summary.TotalDelinquentLoanCount++;
                    summary.TotalDelinquentCapital += loan.LoanAmount;
                    summary.TotalDelinquentBalance += loan.Balance;
                    summary.TotalDelinquentInterest += loan.InterestForcasted;
                }
            }

            // Compute derived metrics
            decimal totalCapital = summary.TotalCapital;
            decimal totalDelinquentCapital = summary.TotalDelinquentCapital;

            //summary.PortfolioInsight = totalCapital switch
            //{
            //    > 0 when summary.DefaultRate < 5 => "✅ Portfolio is healthy.",
            //    > 0 when summary.DefaultRate < 15 => "⚠️ Moderate default risk.",
            //    > 0 => "❌ High default risk. Immediate action needed.",
            //    _ => "No loan data to analyze."
            //};

            // Label the summary row
            summary.CustomerName = "SUMMARY (OUTSTANDING)";
            summary.CustomerId = "TOTAL";
            summary.DeliquentStatus = "Summary";

            // Order and return the list
            var ordered = loans
                .OrderBy(l => l.LoanType != "Main_Loan")
                .ThenBy(l => l.DaysFrom)
                .ThenBy(l => l.CustomerId)
                .ToList();

            ordered.Add(summary);
            return ordered;
        }

        public async Task<LoanDelinquencyReportResultRPT> GetLoanPortfolioAnalysisAsync(GenerateLoanPortfolioReportCommand reportCommand)
        {
            //GenerateLoanPortfolioReportCommand
            try
            {
                bool isSingleBranch = false;

                if (!IsHeadOffice())
                {
                    isSingleBranch=true;
                    reportCommand.BranchId=GetBranchID();
                }
                else
                {
                    if (reportCommand.BranchId==null)
                    {
                        reportCommand.BranchId="All";
                    }
                }

                var queryString = ToQueryString(reportCommand);
                var fullUrl = $"{APICallHelper.GetLoanPortFolio}?{queryString}";
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<LoanDelinquencyReportResult>>(fullUrl);

                if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                {
                    var loan = couApiResponse.ApiResponseData.Data;
                    var branches = await _branchServices.GetBranches();
                    if (IsHeadOffice())
                    {
                        if (reportCommand.BranchId=="All")
                        {
                            reportCommand.BranchId=GetBranchID();
                        }
                    }

                    var mappingobject = LoanDelinquencyFlattener.FlattenAll(loan, branches.FirstOrDefault(x => x.Id==reportCommand.BranchId));
                    return mappingobject;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }

        }
        public async Task<LoanDelinquencyReportDto> GetLoanDelinquencyReportAsync(GenerateLoanPortfolioReportCommand reportCommand)
        {
            try
            {
                bool isSingleBranch = false;

                // If not head office, use user's branch
                if (!IsHeadOffice())
                {
                    isSingleBranch = true;
                    reportCommand.BranchId = GetBranchID();
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(reportCommand.BranchId))
                    {
                        reportCommand.BranchId = "All";
                    }
                }

                // Build API call
                var queryString = ToQueryString(reportCommand);
                var fullUrl = $"{APICallHelper.GetLoanPortFolioAlpha}?{queryString}";

                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<LoanDelinquencyReportDto>>(fullUrl);

                if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                {
                    var report = couApiResponse.ApiResponseData.Data;
                    var branches = await _branchServices.GetBranches();

                    // If "All", fallback to default branch to fetch bank metadata
                    if (IsHeadOffice() && reportCommand.BranchId == "All")
                    {
                        reportCommand.BranchId = GetBranchID();
                    }

                    var branch = branches.FirstOrDefault(x => x.Id == reportCommand.BranchId);

                    // Enrich the report with metadata
                    var enrichedReport = LoanDelinquencyFlattener.FlattenAll(report, branch);

                    // Add flattened rows (optional, used in Crystal or API)
                    enrichedReport.FlattenedRows = LoanDelinquencyFlattener.FlattenToFlatRows(enrichedReport);

                    return enrichedReport;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public LoanPortfolioAnalysis MapToLoanPortfolioAnalysis(LoanPortfolioAnalysis reportDto, Branch branch)
        {
            var headOffice = branch.Bank;


            var analysis = new LoanPortfolioAnalysis
            {
                // Branch Info
                Logo = branch.LogoUrl ?? headOffice.LogoUrl,
                BranchName = branch.Name,
                BranchCode = branch.BranchCode,
                BranchAddress = branch.Address,
                BranchTelephone = branch.Telephone,

                // Head Office Info
                HeadOfficeName = headOffice.Name,
                HeadOfficeAddress = headOffice.Address,
                HeadOfficeTelephone = headOffice.Telephone,
                HeadOfficeEmail = headOffice.Email,
                HeadOfficeWebSite = headOffice.WebSite,
                HeadOfficeInitial = headOffice.BankInitial,
                HeadOfficeCode = headOffice.BankCode,

                // Portfolio Overview
                TotalLoans = reportDto.PortfolioOverview.TotalLoans,
                TotalPrincipal = reportDto.PortfolioOverview.TotalPrincipal,
                TotalInterest = reportDto.PortfolioOverview.TotalInterest,
                TotalForecastedInterest = reportDto.PortfolioOverview.TotalForecastedInterest,
                TotalBalance = reportDto.PortfolioOverview.TotalBalance,

                CurrentLoanCount = reportDto.PortfolioOverview.CurrentLoanCount,
                CurrentPrincipal = reportDto.PortfolioOverview.CurrentPrincipal,
                CurrentBalance = reportDto.PortfolioOverview.CurrentBalance,

                DelinquentLoanCount = reportDto.PortfolioOverview.DelinquentLoanCount,
                DelinquentPrincipal = reportDto.PortfolioOverview.DelinquentPrincipal,
                DelinquentInterest = reportDto.PortfolioOverview.DelinquentInterest,
                DelinquentBalance = reportDto.PortfolioOverview.DelinquentBalance,

                PortfolioAtRiskPercentage_Overview = reportDto.PortfolioOverview.PortfolioAtRiskPercentage,
                DefaultRate = reportDto.PortfolioOverview.DefaultRate,

                // Current Loan Summary
                CurrentTotalLoans = reportDto.CurrentLoanSummary.TotalLoans,
                CurrentTotalPrincipal = reportDto.CurrentLoanSummary.TotalPrincipal,
                CurrentTotalInterest = reportDto.CurrentLoanSummary.TotalInterest,
                CurrentTotalForecastedInterest = reportDto.CurrentLoanSummary.TotalForecastedInterest,
                CurrentTotalOutstandingPrincipal = reportDto.CurrentLoanSummary.TotalOutstandingPrincipal,
                CurrentTotalBalance = reportDto.CurrentLoanSummary.TotalBalance,
                CurrentPercentageBalance = reportDto.CurrentLoanSummary.PercentageBalance,
                CurrentPercentageOfPortfolioPrincipal = reportDto.CurrentLoanSummary.PercentageOfPortfolioPrincipal,
                CurrentPercentageOfPortfolioInterest = reportDto.CurrentLoanSummary.PercentageOfPortfolioInterest,
                CurrentPercentageOfPortfolioForecastedInterest = reportDto.CurrentLoanSummary.PercentageOfPortfolioForecastedInterest,

                // Delinquent Loan Summary
                DelinquentTotalPrincipal = reportDto.DeliquentLoanSummary.TotalPrincipal,
                DelinquentTotalInterest = reportDto.DeliquentLoanSummary.TotalInterest,
                DelinquentTotalLoans = reportDto.DeliquentLoanSummary.TotalLoans,
                TotalDelinquentLoans = reportDto.DeliquentLoanSummary.TotalDelinquentLoans,
                DelinquentTotalOutstandingPrincipal = reportDto.DeliquentLoanSummary.TotalOutstandingPrincipal,
                TotalDelinquentPrincipal = reportDto.DeliquentLoanSummary.TotalDelinquentPrincipal,
                TotalDelinquentInterest = reportDto.DeliquentLoanSummary.TotalDelinquentInterest,
                DelinquentTotalBalance = reportDto.DeliquentLoanSummary.TotalBalance,
                DelinquentPercentageBalance = reportDto.DeliquentLoanSummary.PercentageBalance,
                DelinquentPercentageDelinquentPrincipal = reportDto.DeliquentLoanSummary.PercentageDelinquentPrincipal,
                DelinquentPercentageDelinquentInterest = reportDto.DeliquentLoanSummary.PercentageDelinquentInterest,
                DelinquentPercentageArrears = reportDto.DeliquentLoanSummary.PercentageArrears,
                DelinquentTotalForecastedInterest = reportDto.DeliquentLoanSummary.TotalForecastedInterest,
                DelinquentPercentageOfPortfolioPrincipal = reportDto.DeliquentLoanSummary.PercentageOfPortfolioPrincipal,
                DelinquentPercentageOfPortfolioInterest = reportDto.DeliquentLoanSummary.PercentageOfPortfolioInterest,
                DelinquentPercentageOfPortfolioForecastedInterest = reportDto.DeliquentLoanSummary.PercentageOfPortfolioForecastedInterest,

                // Collections
                AgingAnalysis = reportDto.AgingAnalysis,
                GenderAgingAnalysis = reportDto.GenderAgingAnalysis,
                GroupDelinquency = reportDto.GroupDelinquency,
                IndividualDelinquency = reportDto.IndividualDelinquency,
                LoanTypeDelinquency = reportDto.LoanTypeDelinquency,
                MemberAgeDelinquency = reportDto.MemberAgeDelinquency,
                LoanPortfolios = reportDto.LoanPortfolios,
                LoanTargetGenderAnalysis = reportDto.LoanTargetGenderAnalysis,
                LoanProductTypeTargetGenderAnalysis = reportDto.LoanProductTypeTargetGenderAnalysis,
                LoanTermProductTargetGenderAnalysis = reportDto.LoanTermProductTargetGenderAnalysis,
                LoanCategoryTermProductTargetGenderAnalysis = reportDto.LoanCategoryTermProductTargetGenderAnalysis,

                // Raw nested for reference
                PortfolioOverview = reportDto.PortfolioOverview,
                CurrentLoanSummary = reportDto.CurrentLoanSummary,
                DeliquentLoanSummary = reportDto.DeliquentLoanSummary
            };

            return analysis;
        }

        public List<LoanPortfolioDto> QueryLoans(List<LoanPortfolioDto> loanPortfolioDtos, string queryParam)
        {
            if (string.IsNullOrWhiteSpace(queryParam))
                return loanPortfolioDtos.OrderBy(x => x.LoanDate).ToList();

            queryParam = queryParam.ToLowerInvariant();

            switch (queryParam)
            {
                case "loanbytypes":
                    return loanPortfolioDtos
                        .Where(x => !string.IsNullOrEmpty(x.LoanType))
                        .OrderBy(x => x.LoanDate)
                        .ToList();
                case "loanbytarget":
                    return loanPortfolioDtos
                        .Where(x => !string.IsNullOrEmpty(x.LoanTarget))
                        .OrderBy(x => x.LoanDate)
                        .ToList();
                case "loanbystatus":
                    return loanPortfolioDtos
                        .Where(x => !string.IsNullOrEmpty(x.LoanStatus))
                        .OrderBy(x => x.LoanDate)
                        .ToList();
                case "loanbyproduct":
                    return loanPortfolioDtos
                        .Where(x => !string.IsNullOrEmpty(x.ProductType))
                        .OrderBy(x => x.LoanDate)
                        .ToList();
                case "loanbycategory":
                    return loanPortfolioDtos
                        .Where(x => !string.IsNullOrEmpty(x.LoanCategory))
                        .OrderBy(x => x.LoanDate)
                        .ToList();
                case "loanbygender":
                    return loanPortfolioDtos
                        .Where(x => !string.IsNullOrEmpty(x.Gender))
                        .OrderBy(x => x.LoanDate)
                        .ToList();
                case "loanbydelinquencyconfig":
                    return loanPortfolioDtos
                        .Where(x => !string.IsNullOrEmpty(x.DelinquencyConfigName))
                        .OrderBy(x => x.LoanDate)
                        .ToList();
                case "loanbylegalform":
                    return loanPortfolioDtos
                        .Where(x => !string.IsNullOrEmpty(x.LegalForm))
                        .OrderBy(x => x.LoanDate)
                        .ToList();
                default:
                    return loanPortfolioDtos.OrderBy(x => x.LoanDate).ToList();
            }
        }
        public List<string> GetLoanDropdownQueryOptions(string queryParam)
        {
            if (string.IsNullOrWhiteSpace(queryParam))
                return new List<string>();

            queryParam = queryParam.ToLowerInvariant();

            switch (queryParam)
            {
                case "loanbygender":
                    return new List<string> { "Male", "Female", "Group" };

                case "loanbytypes":
                    return new List<string> { "Short Term", "Medium Term", "Long Term", "Emergency Loan", "Business Loan" };

                case "loanbytarget":
                    return new List<string> { "Salary Earners", "Business Owners", "Farmers", "Traders", "Others" };

                case "loanbystatus":
                    return new List<string> { "Active", "Closed", "In Default", "Pending" };

                case "loanbyproduct":
                    return new List<string> { "Micro Loan", "Housing Loan", "Education Loan", "Agricultural Loan" };

                case "loanbycategory":
                    return new List<string> { "Granted This Month", "Outstanding", "Overdue", "Recovered" };

                case "loanbydelinquencyconfig":
                    return new List<string> { "0–30 Days", "31–60 Days", "61–90 Days", "90+ Days" };

                case "loanbylegalform":
                    return new List<string> { "Individual", "Group", "Corporation" };

                default:
                    return new List<string>();
            }
        }

    }

}
