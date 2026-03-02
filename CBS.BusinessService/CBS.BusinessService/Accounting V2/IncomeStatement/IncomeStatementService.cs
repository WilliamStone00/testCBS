using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AccountingReportsD;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.FlatBaseE;

using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.MockData;
using CBS.FrontDesk.Helper;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using ZXing;

namespace CBS.BusinessService.Accounting_V2.IncomeStatement
{
    public class IncomeStatementService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;
        private readonly BranchServices _branchServices;
        public IncomeStatementService(BranchServices branchServices)
        {
            var baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            _apiCallerHelper = new ApiCallerHelper(baseUrl);

            _branchServices = branchServices;


        }

        public async Task<FinancialStatementResponse> GetBalanceSheetAsync(AccountingV2ReportsFilter filter)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<FinancialStatementResponse>>(
                    APICallHelper.incomeStatement,
                    filter
                );

        

                if (response?.IsSuccess == true)
                    return response.ApiResponseData.Data;

                throw new Exception(response?.Message ?? "API returned failure");
            }
            
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetBalanceSheetAsync ERROR: {ex.Message}");
                return null;
            }
        }


        public async Task<List<BalanceSheetFlatItems>> BuildFlatDataset(AccountingV2ReportsFilter model)
        {
            // 1) Load trial balance
            var response = await GetBalanceSheetAsync(model);
            if (response == null)
                return new List<BalanceSheetFlatItems>();

            // 2) Resolve branch context
            var currentBranchId = this.GetBranchID();
            var branchIdToLoad = model.Consolidated || string.IsNullOrWhiteSpace(model.BranchId)
                ? currentBranchId
                : model.BranchId;

            var BranchInformation = await _branchServices.GetBranch(branchIdToLoad);
            if (BranchInformation == null)
                return new List<BalanceSheetFlatItems>();

            // 3) Metadata
            var now = DateTime.Now;
            var username = GetUserFullName();
            var modeLabel = model.SourceMode == "Temp"
                ? "( TEMPORAL REPORT) "
                : $"( {model.SourceMode?.ToUpperInvariant()} REPORT )";

            var consolidationLabel = model.Consolidated
                ? "CONSOLIDATED"
                : "BRANCH LEVEL";

            var logoPath = BranchInformation != null
              ? PaymentReceiptMapping.GenerateAndSaveBankLogoImage(
                  BranchInformation.Bank?.LogoUrl ?? string.Empty,
                  BranchInformation.Name ?? string.Empty)
              : string.Empty;

            var header = new BankHeaderInformation
            {
                BankId = BranchInformation.Bank?.Id,
                BankBankCode = BranchInformation.Bank?.BankCode,
                BankCode = BranchInformation.Bank?.BankCode,
                BankName = BranchInformation.Bank?.Name,
                BankTelephone = BranchInformation.Bank?.Telephone,
                BankEmail = BranchInformation.Bank?.Email,
                BankAddress = BranchInformation.Bank?.Address,
                BankLogoUrl = logoPath,
                //BankLogoUrl = BranchInformation.Bank?.LogoUrl,
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
                BranchPBox = BranchInformation.PBox ?? "",
                
            };

            // 4) Map to TrialBalanceReportItem (Crystal + Excel compatible)
            var result = response.Payload.rows.Select(x =>
            {
                var item = new BalanceSheetFlatItems
                {
                    Title = response.Title,
                    GroupName = "",
                    GroupReference = "",
                    Referernce = x.RefCode,
                    PreviousYear = response.Payload.PreviousYear,
                    CurrentYear = response.Payload.CurrentYear,
                    AccountingDate = response.Payload.AsAt,
                    Reference = x.RefCode,
                    Heading = x.Caption,
                    Side = x.Side,
                    NetN1 = x.NetLastYear,
                    NetN = x.NetCurrentYear,
                    Year = $"2021 - {DateTime.Now.Year}",
                    Note = x.Note,
                    PrintedBy = _branchServices.GetUserFullName(),
                    BankAddress = BranchInformation.Bank.Address,
                   
                    // totals 
                    TotalExpensesLastYear = response.Payload.TotalExpensesLastYear ?? 0,
                    TotalExpensesCurrentYear = response.Payload.TotalExpensesCurrentYear ?? 0,
                    TotalIncomeCurrentYear = response.Payload.TotalIncomeCurrentYear ?? 0,
                    TotalIncomeLastYear = response.Payload.TotalIncomeLastYear ?? 0,
                };

                // Attach full static bank + branch header information
                ApplyHeader(item, header);
                return item;
            }).ToList();

            return result;
        }

        private void ApplyHeader(BalanceSheetFlatItems item, BankHeaderInformation header)
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

    }
}
