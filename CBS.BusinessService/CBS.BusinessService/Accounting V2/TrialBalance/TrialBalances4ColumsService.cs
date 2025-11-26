using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.FlatBaseE;
using CBS.FrontDesk.Data.Entity.Accounting_V2.TrialBalance;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.MockData;
using CBS.FrontDesk.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.TrialBalance
{
    public class TrialBalances4ColumnService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;
        private readonly TrialBalance6ColumnsMock _trialBalance6ColumnsMock;

        public TrialBalances4ColumnService()
        {
            // Hardcoded base URL (intentionally allowed)
            string baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
            _trialBalance6ColumnsMock = new TrialBalance6ColumnsMock();
        }

        /// <summary>
        /// Fetch trial balances using provided filter (6-column format).
        /// </summary>
        public async Task<List<TrialBalanceFourItemDto>> GetTrialBalancesAsync4columns(AccountingV2ReportsFilter filter)
        {
            string jsonFilter = JsonConvert.SerializeObject(filter, Formatting.Indented);
            try
            {
               
                // POST request to the API
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<List<TrialBalanceFourItemDto>>>(
                    APICallHelper.TrialBalance4,
                    filter
                );


                

                if (response?.IsSuccess == true)
                {
                    return response.ApiResponseData.Data ?? new List<TrialBalanceFourItemDto>();
                }
                else
                {
                     new List<TrialBalanceV2Dto>();
                    System.Diagnostics.Debug.WriteLine($"TrialBalanceService.GetTrialBalancesAsync6columns: API returned failure ({response?.Message})");
                }

                return new List<TrialBalanceFourItemDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TrialBalanceService.GetTrialBalancesAsync6columns Error: {ex}");
                // optional: throw new Exception("Failed to fetch trial balances", ex);
               throw ex;
            }
        }


        private BankHeaderInformation BuildHeader(dynamic BranchInformation)
        {
            return new BankHeaderInformation
            {
                BankId = BranchInformation.Bank.Id,
                BankBankCode = BranchInformation.Bank.BankCode,
                BankName = BranchInformation.Bank.Name,
                BankTelephone = BranchInformation.Bank.Telephone,
                BankEmail = BranchInformation.Bank.Email,
                BankAddress = BranchInformation.Bank.Address,
                BankLogoUrl = BranchInformation.Bank.LogoUrl,
                BankMotto = BranchInformation.Bank.Motto,
                BankRegistrationNumber = BranchInformation.Bank.RegistrationNumber,
                BankImmatriculationNumber = BranchInformation.Bank.ImmatriculationNumber,
                BankPBox = BranchInformation.Bank.PBox,

                BranchId = BranchInformation.Id,
                BranchCode = BranchInformation.BranchCode,
                BranchName = BranchInformation.Name,
                BranchTelephone = BranchInformation.Telephone,
                BranchEmail = BranchInformation.Email,
                BranchAddress = BranchInformation.Address,
                BranchLogoUrl = BranchInformation.LogoUrl,
                BranchCapital = BranchInformation.Capital,
                BranchRegistrationNumber = BranchInformation.RegistrationNumber,
                BranchImmatriculationNumber = BranchInformation.ImmatriculationNumber,
                BranchPBox = BranchInformation.PBox
            };
        }

        



        public async Task<GenericReportResponseV2Dto> GetTrialMockInformation()
        {
            
            var results  =    TrialBalance6ColumnsMock.GetMockData();
            return results;
        }
    }
}
