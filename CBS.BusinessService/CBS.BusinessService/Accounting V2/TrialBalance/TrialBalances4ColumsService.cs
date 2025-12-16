using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.FlatBaseE;
using CBS.FrontDesk.Data.Entity.Accounting_V2.TrialBalance;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.MockData;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Service.Accounting_V2.TrialBalance;
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
        private readonly BranchServices _branchServices;

        public TrialBalances4ColumnService(BranchServices branchServices)
        {
            // Hardcoded base URL (intentionally allowed)
            string baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
            _trialBalance6ColumnsMock = new TrialBalance6ColumnsMock();
            _branchServices = branchServices;
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



        public async Task<List<TrialBalanceFourColumnsFlatItems>>
          BuildTrialBalanceDataset(AccountingV2ReportsFilter model)
        {
            // 1) Load trial balance
            var response = await GetTrialBalancesAsync4columns(model);
            if (response == null || !response.Any())
                return new List<TrialBalanceFourColumnsFlatItems>();

            // 2) Resolve branch
            var currentBranchId = GetBranchID();
            var branchIdToLoad = model.Consolidated || string.IsNullOrWhiteSpace(model.BranchId)
                ? currentBranchId
                : model.BranchId;

            var branch = await _branchServices.GetBranch(branchIdToLoad);
            if (branch == null)
                return new List<TrialBalanceFourColumnsFlatItems>();

            // 3) Metadata
            var now = DateTime.Now;
            var username = GetUserFullName();

            var modeLabel = model.SourceMode == "Temp"
                ? "( TEMPORAL REPORT )"
                : $"( {model.SourceMode?.ToUpperInvariant()} REPORT )";

            var consolidationLabel = model.Consolidated
                ? "CONSOLIDATED"
                : "BRANCH LEVEL";

            var header = new BankHeaderInformation
            {
                BankId = branch.Bank?.Id,
                BankBankCode = branch.Bank?.BankCode,
                BankName = branch.Bank?.Name,
                BankTelephone = branch.Bank?.Telephone,
                BankEmail = branch.Bank?.Email,
                BankAddress = branch.Bank?.Address,
                BankLogoUrl = branch.Bank?.LogoUrl,
                BankMotto = branch.Bank?.Motto,
                BankRegistrationNumber = branch.Bank?.RegistrationNumber,
                BankImmatriculationNumber = branch.Bank?.ImmatriculationNumber,
                BankPBox = branch.Bank?.PBox ?? "",

                BranchId = branch.Id,
                BranchCode = branch.BranchCode,
                BranchName = branch.Name,
                BranchTelephone = branch.Telephone,
                BranchEmail = branch.Email,
                BranchAddress = branch.Address,
                BranchLogoUrl = branch.LogoUrl,
                BranchCapital = branch.Capital,
                BranchRegistrationNumber = branch.RegistrationNumber,
                BranchImmatriculationNumber = branch.ImmatriculationNumber,
                BranchPBox = branch.PBox ?? ""
            };

            // 4) Map dataset
            var result = response.Select(x =>
            {
                var item = new TrialBalanceFourColumnsFlatItems
                {
                    AccountNumber = x.AccountNumber,
                    AccountName = x.AccountName,
                    Debit = x.Debit,
                    Credit = x.Credit,

                    BeginningBalance = x.BeginningBalance,
                    EndingBalance = x.EndingBalance,
                    OpeningDebit = x.BeginningBalance,

                    TotalBeginningNet = x.TotalBeginningNet,
                    TotalEndingNet = x.TotalEndingNet,
                    TotalCredit = x.TotalCredit,
                    TotalDebit = x.TotalDebit,
                    TotalMovementNet = x.TotalMovementNet,
                    TotalBeginningSide = x.TotalBeginningSide,
                    TotalEndingSide = x.TotalEndingSide,

                    BeginningBookingDirection = x.BeginningSide,
                    EndingBookingDirection = x.EndingSide,

                    Username = username,
                    From = model.From,
                    To = model.To,
                    Mode = modeLabel,
                    ConsolidationStatus = consolidationLabel,

                    Date = now.Date,
                    DayTime = now,
                    Time = now.TimeOfDay,
                    Year = now.Year.ToString(),
                    BankAddress = branch.Bank?.Address
                };

                ApplyHeader(item, header);
                return item;
            }).ToList();

            return result;
        }





        private void ApplyHeader(TrialBalanceFourColumnsFlatItems item,
             BankHeaderInformation header)
        {
            item.BankId = header.BankId;
            item.BankBankCode = header.BankBankCode;
            item.BankName = header.BankName;
            item.BankTelephone = header.BankTelephone;
            item.BankEmail = header.BankEmail;
            item.BankAddress = header.BankAddress;
            item.BankLogoUrl = header.BankLogoUrl;
            item.BankMotto = header.BankMotto;
            item.BankRegistrationNumber = header.BankRegistrationNumber;
            item.BankImmatriculationNumber = header.BankImmatriculationNumber;
            item.BankPBox = header.BankPBox;

            item.BranchId = header.BranchId;
            item.BranchCode = header.BranchCode;
            item.BranchName = header.BranchName;
            item.BranchTelephone = header.BranchTelephone;
            item.BranchEmail = header.BranchEmail;
            item.BranchAddress = header.BranchAddress;
            item.BranchLogoUrl = header.BranchLogoUrl;
            item.BranchCapital = header.BranchCapital;
            item.BranchRegistrationNumber = header.BranchRegistrationNumber;
            item.BranchImmatriculationNumber = header.BranchImmatriculationNumber;
            item.BranchPBox = header.BranchPBox;
        }




        public async Task<GenericReportResponseV2Dto> GetTrialMockInformation()
        {
            
            var results  =    TrialBalance6ColumnsMock.GetMockData();
            return results;
        }
    }
}
