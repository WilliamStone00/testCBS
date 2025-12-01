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
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using ZXing.OneD;

namespace CBS.BusinessService.Accounting_V2.TrialBalance
{
    public class TrialBalances6ColumnService : BaseService
    {
        
        private readonly ApiCallerHelper _apiCallerHelper;
        private readonly TrialBalance6ColumnsMock _trialBalance6ColumnsMock;
        private readonly BranchServices _branchServices;

        public TrialBalances6ColumnService(BranchServices branchServices)
        {
            // Hardcoded base URL (intentionally allowed)
            string baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
            _branchServices = branchServices;

            _trialBalance6ColumnsMock = new TrialBalance6ColumnsMock();
        }

        /// <summary>
        /// Fetch trial balances using provided filter (6-column format).
        /// </summary>
        public async Task<GenericReportResponseV2Dto> GetTrialBalancesAsync6columns(AccountingV2ReportsFilter filter)
        {
            try
            {
               
                // POST request to the API
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<List<TrialBalanceV2Dto>>>(
                    APICallHelper.TrialBalance6,
                    filter
                );

                var result = new GenericReportResponseV2Dto();

                if (response?.IsSuccess == true)
                {
                    result.Lines = response.ApiResponseData?.Data ?? new List<TrialBalanceV2Dto>();
                }
                else
                {
                    result.Lines = new List<TrialBalanceV2Dto>();



                    System.Diagnostics.Debug.WriteLine($"TrialBalanceService.GetTrialBalancesAsync6columns: API returned failure ({response?.Message})");
                }




                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TrialBalanceService.GetTrialBalancesAsync6columns Error: {ex}");
                // optional: throw new Exception("Failed to fetch trial balances", ex);
                return new GenericReportResponseV2Dto
                {
                    Lines = new List<TrialBalanceV2Dto>()
                };
            }
        }



        public async Task<List<TrialBalanceReportItem>> BuildTrialBalanceDataset(AccountingV2ReportsFilter model)
        {
            // 1) Load trial balance
            var response = await GetTrialBalancesAsync6columns(model);
            if (response?.Lines == null || !response.Lines.Any())
                return new List<TrialBalanceReportItem>();
            
            // 2) Resolve branch context
            var currentBranchId = this.GetBranchID();
            var branchIdToLoad = model.Consolidated || string.IsNullOrWhiteSpace(model.BranchId)
                ? currentBranchId
                : model.BranchId;

            var BranchInformation = await _branchServices.GetBranch(branchIdToLoad);
            if (BranchInformation == null)
                return new List<TrialBalanceReportItem>();

            // 3) Metadata
            var now = DateTime.Now;
            var username = GetUserFullName();
            var modeLabel = model.SourceMode == "Temp"
                ? "( TEMPORAL REPORT) "
                : $"( {model.SourceMode?.ToUpperInvariant()} REPORT )";

            var consolidationLabel = model.Consolidated
                ? "CONSOLIDATED"
                : "BRANCH LEVEL";
            var header = new BankHeaderInformation
            {
                BankId = BranchInformation.Bank?.Id,
                BankBankCode = BranchInformation.Bank?.BankCode,
                BankCode = BranchInformation.Bank?.BankCode,
                BankName = BranchInformation.Bank?.Name,
                BankTelephone = BranchInformation.Bank?.Telephone,
                BankEmail = BranchInformation.Bank?.Email,
                BankAddress = BranchInformation.Bank?.Address,
                BankLogoUrl = BranchInformation.Bank?.LogoUrl,
                BankMotto = BranchInformation.Bank?.Motto,
                BankRegistrationNumber = BranchInformation.Bank?.RegistrationNumber,
                BankImmatriculationNumber = BranchInformation.Bank?.ImmatriculationNumber,
                BankPBox = BranchInformation.Bank?.PBox ?? "",

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
                BranchPBox = BranchInformation.PBox ?? ""
            };

            // 4) Map to TrialBalanceReportItem (Crystal + Excel compatible)
            var result = response.Lines.Select(x =>
            {
                var item = new TrialBalanceReportItem
                {
                    AccountNumber = x.AccountNumber,
                    AccountName = x.AccountName,
                    OpeningDebit = x.OpeningDR,
                    OpeningCredit = x.OpeningCR,
                    MovementDebit = x.PeriodDR,
                    MovementCredit = x.PeriodCR,
                    ClosingDebit = x.ClosingDR,
                    ClosingCredit = x.ClosingCR,

                    TotalOpeningDebit = x.TotalOpeningDR,
                    TotalOpeningCredit = x.TotalOpeningCR,
                    TotalMovementDebit = x.TotalMovementDR,
                    TotalMovementCredit = x.TotalMovementCR,
                    TotalClosingDebit = x.TotalClosingDR,
                    TotalClosingCredit = x.TotalClosingCR,
                    TotalOpeningDifference = x.TotalOpeningDifference,
                    TotalMovementDifference = x.TotalMovementDifference,
                    TotalClosingDifference = x.TotalClosingDifference,

                    Phone = BranchInformation.Telephone,
                    Address = BranchInformation.Address,
                    Username = username,
                    From = model.From,
                    To = model.To,
                    Mode = modeLabel,
                    ConsolidationStatus = consolidationLabel,

                    Date = now.Date,
                    DayTime = now,
                    Time = now.TimeOfDay,
                    Year = now.Year.ToString(),
                    BankAddress = BranchInformation.Bank.Address
                };

                // Attach full static bank + branch header information
                ApplyHeader(item, header);
                return item;
            }).ToList();

            return result;
        }

        private void ApplyHeader(TrialBalanceReportItem item, BankHeaderInformation header)
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
