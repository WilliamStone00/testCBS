
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.EMMA;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounts
{

    public class GeneralDailyDashboardServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;
        private readonly ApiCallerHelper _customerApiHelper;
        private readonly ApiCallerHelper _accountingConfigApiHelper;
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public GeneralDailyDashboardServices()
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _customerApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CustomerBaseUrl"].ToString());
            _accountingConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
        }


        //CustomerDashboardStatistics
        //AccountDashboardStatistics

        public async Task<LoanMainDashboard> GetLoanDashboardAdmin()
        {
            try
            {
                var dailyDashboardQuery = new DashboardQueryParameter { BranchId = "N/A", QueryParameter = "all" };


                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<LoanMainDashboard>>(APICallHelper.GetLoanDashboardQuery, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return new LoanMainDashboard();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<LoanMainDashboard> GetLoanDashboard()
        {
            try
            {
                var dailyDashboardQuery = new DashboardQueryParameter { BranchId = GetBranchID(), QueryParameter = "bybranch" };


                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<LoanMainDashboard>>(APICallHelper.GetLoanDashboardQuery, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return new LoanMainDashboard();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<LoanMainDashboard> GetLoanDashboard(string branchid, string queryParameter)
        {
            try
            {
                var dailyDashboardQuery = new DashboardQueryParameter { BranchId = branchid, QueryParameter = queryParameter };


                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<LoanMainDashboard>>(APICallHelper.GetLoanDashboardQuery, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return new LoanMainDashboard();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }



        public async Task<List<AccountingDashboardStatistics>> GetAccountingDashboardAdmin()
        {
            try
            {
                var dailyDashboardQuery = new DashboardQueryParameter { BranchId = "N/A", QueryParameter = "all" };


                var response = await _accountingConfigApiHelper.PostAsync<ServiceResponse<List<AccountingDashboardStatistics>>>(APICallHelper.GetAllAccountingDashboard, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<AccountingDashboardStatistics>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<AccountingDashboardStatistics>> GetAccountingDashboard()
        {
            try
            {
                var dailyDashboardQuery = new DashboardQueryParameter { BranchId = GetBranchID(), QueryParameter = "bybranch" };


                var response = await _accountingConfigApiHelper.PostAsync<ServiceResponse<List<AccountingDashboardStatistics>>>(APICallHelper.GetAllAccountingDashboard, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<AccountingDashboardStatistics>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<AccountingDashboardStatistics>> GetAccountingDashboard(string branchid, string queryParameter)
        {
            try
            {
                var dailyDashboardQuery = new DashboardQueryParameter { BranchId = branchid, QueryParameter = queryParameter };


                var response = await _accountingConfigApiHelper.PostAsync<ServiceResponse<List<AccountingDashboardStatistics>>>(APICallHelper.GetAllAccountingDashboard, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<AccountingDashboardStatistics>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

        public async Task<CustomerDashboardStatistics> GetCustomerDashboardAdmin()
        {
            try
            {
                var dailyDashboardQuery = new DashboardQueryParameter { BranchId = "N/A", QueryParameter = "all" };


                var response = await _customerApiHelper.PostAsync<ServiceResponse<CustomerDashboardStatistics>>(APICallHelper.GetAllMembersDashboard, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return new CustomerDashboardStatistics();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

        public async Task<CustomerDashboardStatistics> GetCustomerDashboard()
        {
            try
            {
                var dailyDashboardQuery = new DashboardQueryParameter { BranchId = GetBranchID(), QueryParameter = "bybranch" };


                var response = await _customerApiHelper.PostAsync<ServiceResponse<CustomerDashboardStatistics>>(APICallHelper.GetAllMembersDashboard, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return new CustomerDashboardStatistics();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<CustomerDashboardStatistics> GetCustomerDashboard(string branchid, string queryParameter)
        {
            try
            {
                var dailyDashboardQuery = new DashboardQueryParameter { BranchId = branchid, QueryParameter = queryParameter };


                var response = await _customerApiHelper.PostAsync<ServiceResponse<CustomerDashboardStatistics>>(APICallHelper.GetAllMembersDashboard, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return new CustomerDashboardStatistics();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }


        public async Task<AccountDashboardStatistics> GetAccountsDashboardAdmin()
        {
            try
            {
                var dailyDashboardQuery = new DashboardQueryParameter { BranchId = "N/A", QueryParameter = "all" };


                var response = await _transactionApiHelper.PostAsync<ServiceResponse<AccountDashboardStatistics>>(APICallHelper.GetAllAccountsDashboard, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return new AccountDashboardStatistics();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<AccountDashboardStatistics> GetAccountsDashboard()
        {
            try
            {
                var dailyDashboardQuery = new DashboardQueryParameter { BranchId = GetBranchID(), QueryParameter = "bybranch" };


                var response = await _transactionApiHelper.PostAsync<ServiceResponse<AccountDashboardStatistics>>(APICallHelper.GetAllAccountsDashboard, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return new AccountDashboardStatistics();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<AccountDashboardStatistics> GetAccountsDashboard(string branchid, string queryParameter)
        {
            try
            {
                var dailyDashboardQuery = new DashboardQueryParameter { BranchId = branchid, QueryParameter = queryParameter };


                var response = await _transactionApiHelper.PostAsync<ServiceResponse<AccountDashboardStatistics>>(APICallHelper.GetAllAccountsDashboard, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return new AccountDashboardStatistics();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

        public MainAccountingDashboardStatistics GenerateDashboardAccountingStatistics(List<AccountingDashboardStatistics> statistics)
        {
            var mainDashboardStatistics = new MainAccountingDashboardStatistics();

            if (statistics == null || statistics.Count == 0)
                return mainDashboardStatistics;
            // Assuming all entries have the same Branch info, we set Branch info based on the first entry
            mainDashboardStatistics.BranchName = statistics[0].BranchName;
            mainDashboardStatistics.BranchId = statistics[0].BranchId;
            mainDashboardStatistics.BranchCode = statistics[0].BranchCode;

            foreach (var stat in statistics)
            {
                // Safely parse the DashboardAccountType to handle invalid or missing values
                //if (Enum.TryParse(stat.DashboardAccountType, out DashboardAccountingType accountType))
                if (Enum.TryParse(stat.DashboardAccountType, out DashboardAccountingType accountType))
                {
                    switch (accountType)
                    {
                        case DashboardAccountingType.CashInHand:
                            mainDashboardStatistics.CashInHandBalance += stat.Balance;
                            break;

                        case DashboardAccountingType.CashInBank:
                            mainDashboardStatistics.CashInBankBalance += stat.Balance;
                            break;

                        case DashboardAccountingType.PreferenceShare:
                            mainDashboardStatistics.PreferenceShareBalance += stat.Balance;
                            break;

                        case DashboardAccountingType.OrdinaryShares:
                            mainDashboardStatistics.OrdinarySharesBalance += stat.Balance;
                            break;

                        case DashboardAccountingType.Deposit:
                            mainDashboardStatistics.DepositBalance += stat.Balance;
                            break;

                        case DashboardAccountingType.Savings:
                            mainDashboardStatistics.SavingsBalance += stat.Balance;
                            break;

                        case DashboardAccountingType.Gav:
                            mainDashboardStatistics.GavBalance += stat.Balance;
                            break;

                        case DashboardAccountingType.DailyCollections:
                            mainDashboardStatistics.DailyCollectionsBalance += stat.Balance;
                            break;

                        case DashboardAccountingType.MTNMobileMoney:
                            mainDashboardStatistics.MTNMobileMoneyBalance += stat.Balance;
                            break;
                        case DashboardAccountingType.MTNMobileMoneyMaster:
                            mainDashboardStatistics.MTNMobileMoneyMasterBalance += stat.Balance;
                            break;

                        case DashboardAccountingType.OrangeMoney:
                            mainDashboardStatistics.OrangeMoneyBalance += stat.Balance;
                            break;
                        case DashboardAccountingType.OrangeMoneyMaster:
                            mainDashboardStatistics.OrangeMoneyMasterBalance += stat.Balance;
                            break;

                        case DashboardAccountingType.TotalExpense:
                            mainDashboardStatistics.TotalExpenseBalance += stat.Balance;
                            break;
                        case DashboardAccountingType.TotalLiquidity:
                            mainDashboardStatistics.TotalLiquidity += stat.Balance;
                            break;

                        case DashboardAccountingType.TotalIncome:
                            mainDashboardStatistics.TotalIncomeBalance += stat.Balance;
                            break;

                            // Add additional cases if there are other account types to handle
                    }
                }
                else
                {
                    // Optional: Log or handle the case where DashboardAccountType could not be parsed
                    Console.WriteLine($"Warning: Unknown DashboardAccountType '{stat.DashboardAccountType}' encountered.");
                }
            }

            // Calculate the total of Preference and Ordinary Shares
            mainDashboardStatistics.TotalSharesBalance =mainDashboardStatistics.PreferenceShareBalance + mainDashboardStatistics.OrdinarySharesBalance;

            return mainDashboardStatistics;
        }

        public MainDashboardOrdinaryAccounts ConvertToMainDashboardOrdinaryAccounts(AccountDashboardStatistics accountStatistics)
        {

            if (accountStatistics==null)
            {
                var data = new MainDashboardOrdinaryAccounts();
                return data;
            }

            var dashboard = new MainDashboardOrdinaryAccounts
            {
                TotalAccounts = accountStatistics.TotalNumberOfAccounts,
                TotalActieAccounts = accountStatistics.TotalNumberOfActiveAccounts,
                TotalInactiveAccounts = accountStatistics.TotalNumberOfInActiveAccounts,
                TotalVolumeOfBlockedAccounts = accountStatistics.TotalBlockedAmount,
                BranchName = accountStatistics.BranchId, // Assuming BranchId maps to a name in the system
                TotalBranches = accountStatistics.TotalBranches, // This value can be adjusted if more than one branch is involved
                TotalMembers = accountStatistics.TotalMembers, // Assuming this data is available in accountStatistics
                TotalBalance = accountStatistics.TotalBalance, // Assuming this data is available in accountStatistics
                TotalBalanceWithoutBlocked = accountStatistics.TotalBalanceWithoutBlocked, // Assuming this data is available
                TotalBlockedAmount = accountStatistics.TotalBlockedAmount, // Assuming this data is available
            };

            // Populate counts and volumes based on account types
            foreach (var account in accountStatistics.StatisticPerAccounts)
            {
                // Convert the account type string to the AccountType enum
                if (Enum.TryParse(account.AccountType, out AccountTypeEnumerations accountType))
                {
                    switch (accountType)
                    {
                        case AccountTypeEnumerations.PreferenceShare:
                            dashboard.TotalPreferenceShares += account.NumberOfAccounts;
                            dashboard.TotalVolumeOfPreferenceShares += account.Balance;
                            break;

                        case AccountTypeEnumerations.MemberShare:
                            dashboard.TotalOrdinaryShares += account.NumberOfAccounts;
                            dashboard.TotalVolumeOfOrdinaryShares += account.Balance;
                            break;

                        case AccountTypeEnumerations.Saving:
                            dashboard.TotalSavings += account.NumberOfAccounts;
                            dashboard.TotalVolumeOfSavings += account.Balance;
                            break;

                        case AccountTypeEnumerations.Deposit:
                            dashboard.TotalDeposits += account.NumberOfAccounts;
                            dashboard.TotalVolumeOfDeposits += account.Balance;
                            break;

                        case AccountTypeEnumerations.DailyCollection:
                            dashboard.TotalDailyCollections += account.NumberOfAccounts;
                            dashboard.TotalVolumeOfDailyCollections += account.Balance;
                            break;

                        case AccountTypeEnumerations.Gav:
                            dashboard.TotalGav += account.NumberOfAccounts;
                            dashboard.TotalVolumeOfGav += account.Balance;
                            break;

                        // Add cases for other account types as needed
                        case AccountTypeEnumerations.Loan:
                        case AccountTypeEnumerations.Atm:
                        case AccountTypeEnumerations.Membership:
                        case AccountTypeEnumerations.MobileMoneyMTN:
                        case AccountTypeEnumerations.MobileMoneyORANGE:
                        case AccountTypeEnumerations.Teller:
                        case AccountTypeEnumerations.MomocashCollectionMTN:
                        case AccountTypeEnumerations.MomocashCollectionOrange:
                            // Handle these account types as needed
                            break;
                    }
                }
                else
                {
                    // Handle the case where the AccountType string could not be parsed into an enum
                    // For example, log an error or handle the invalid value
                }
            }

            return dashboard;
        }

        public MainDashboardMembers ConvertToMainDashboardMembers(CustomerDashboardStatistics customerStatistics)
        {
            if (customerStatistics==null)
            {
                return new MainDashboardMembers();
            }
            var dashboard = new MainDashboardMembers
            {
                TotalPhysicalMembers = customerStatistics.TotalNumberOfPhysical,
                TotalMoralMembers = customerStatistics.TotalNumberOfMoral,
                TotalBranches = customerStatistics.TotalBranches,
                TotalMembers = customerStatistics.TotalMembers
            };
            return dashboard;
        }


        //public async Task<CustomerDashboardStatistics> GetCustomerDashboard(string branchid, string queryParameter)
        //{
        //    try
        //    {
        //        var dailyDashboardQuery = new DashboardQueryParameter { BranchId = branchid, QueryParameter = queryParameter };


        //        var response = await _transactionApiHelper.PostAsync<ServiceResponse<CustomerDashboardStatistics>>(APICallHelper.GetAllMembersDashboard, dailyDashboardQuery);
        //        if (response.IsSuccess)
        //        {
        //            return response.ApiResponseData.Data;
        //        }
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log and handle exception
        //        throw ex;
        //    }
        //}



        public async Task<GeneralDailyDashboardDto> GetDailyDashboard()
        {
            try
            {
                var dailyDashboardQuery = new GetDailyDashboardQuery { BranchId = GetBranchID(), DateFrom = CurrentDate, DateTo = CurrentDate };


                var response = await _transactionApiHelper.PostAsync<ServiceResponse<GeneralDailyDashboardDto>>(APICallHelper.GetGeneralDailyDashboardByBranch, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return new GeneralDailyDashboardDto();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<GeneralDailyDashboardDto> GetDailyDashboard(string datefrom, string dateto, string branchid)
        {
            try
            {
                var dailyDashboardQuery = new GetDailyDashboardQuery { BranchId = branchid, DateFrom = GetDateTime(datefrom), DateTo = GetDateTime(dateto) };
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<GeneralDailyDashboardDto>>(APICallHelper.GetGeneralDailyDashboardByBranch, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return new GeneralDailyDashboardDto();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

        public async Task<GeneralDailyDashboardDto> GetDailyDashboardHeadOffice()
        {
            try
            {
                var dailyDashboardQuery = new GetDailyDashboardQuery { DateFrom = CurrentDate, DateTo = CurrentDate };
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<GeneralDailyDashboardDto>>(APICallHelper.GetAllGeneralDailySummaryDashboard, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return new GeneralDailyDashboardDto();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<GeneralDailyDashboardDto>> GetDailyDashboards(string datefrom, string dateto)
        {
            try
            {
                var dailyDashboardQuery = new GetDailyDashboardQuery { DateFrom = GetDateTime(datefrom), DateTo = GetDateTime(dateto) };
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<List<GeneralDailyDashboardDto>>>(APICallHelper.GetAllGeneralDailyDashboard, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
    }

}
