using BusinessServices;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Session;
using CBS.FrontDesk.Data.Entity.Accounting_V2.API.IPSReporting;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.IPS.IPSReporting
{
    public class IPSReportBuilderService : BaseService
    {
        private readonly IPSReportService _reportService;
        private readonly CashDeskServices _cashDeskServicesService;

        public IPSReportBuilderService(IPSReportService reportService, CashDeskServices cashDeskServices)
        {
            _reportService = reportService;
            _cashDeskServicesService = cashDeskServices;
        }

     

        public async Task<Branch> GetBranchInfo()
        {
            Branch branch = _cashDeskServicesService.RetrieveBranchFromSession();
            return branch;
        }

        public async Task<Bank> GetBankInfo()
        {
            Branch branch = _cashDeskServicesService.RetrieveBranchFromSession();
            return branch?.Bank;
        }

              


        public async Task<List<IPSflatobject>> BuildInsurancePremiumRows(InsurancePremiumDto filter)
        {
            var branch = await GetBranchInfo();
            var bank = await GetBankInfo();

            // Fetch data from API
            var premiumData = await _reportService.GetCombinedInsurancePremiums(filter);

            var rows = new List<IPSflatobject>();

            if (premiumData == null)
                return rows;

            // Generate logo
            var logoPath = branch != null
                ? PaymentReceiptMapping.GenerateAndSaveBankLogoImage(bank?.LogoUrl, branch.Name)
                : "";

            // Create main report row
            var row = new IPSflatobject
            {
                // Bank Information
                BankName = GetBankName(),
                BankCode = GetBankCode(),

                // Branch Information
                BranchName = GetBankName(),
                BranchCode = GetBranchCode(),
                BranchPOBox = branch?.Address ?? "-",
                BranchTell = branch?.Telephone ?? "-",
               

                // Credit Union Information
                NameOfCreditUnion = bank?.Name ?? "-",
                Address = branch?.Address ?? "-",

                // Report Period
                ContractNumber = 0, // Will be populated if available
                ReportForTheMonthOf = filter.EndDate ?? DateTime.Now,

                // Loan Protection (LP) Section - Front Side (Column 1)
                TotalAmountOfOutstandingLoans = premiumData.LpTotalAmountOfOutstandingLoans,
                TotalFromReverseSide = premiumData.LpTotalFromReverseSide,
                InsurableLoans = premiumData.LpInsurableLoans,
                LPPremiumDue = premiumData.LpPremiumDue,

                // Life Savings (LS) Section - Front Side (Column 2)
                TotalNumberOfMembers = premiumData.LsTotalNumberOfMembers,
                TotalSharesandSavings = premiumData.LsTotalSharesAndSavings,
                TotatFromReverseSideLifeS = premiumData.LsTotalFromReverseSide,

                // LP Deductions - Reverse Side (Column 1)
                LSPDRateTimeLine4 = premiumData.LsPremiumDue, // Excess Loan Balance
                LSPDLine5leftcolumn = premiumData.LpLoansOverAgeThreshold, // Loans Over Age Threshold
                TPDLine5plusLine6 = premiumData.LpLoansToOrganizations, // Loans to Organizations
                LBIEOFLP = premiumData.LpExcessLoanBalance, // Other LP Deductions
                LBOM = premiumData.LpTotalDeductions, // Total LP Deductions
                LTCOAU = premiumData.LpInsurableLoans, // Insurable Loans (after deductions)
                OLPD = premiumData.LpPremiumDue, // LP Premium Due
                TLD = premiumData.LpTotalAmountOfOutstandingLoans, // Total Loans (for reference)

                // LS Deductions - Reverse Side (Column 2)
                LSBIEOFLS = premiumData.LsExcessBalance, // Excess Savings Balance
                SSOCOU = premiumData.LsTotalOtherDeductions, // Other LS Deductions
                OLSD = premiumData.LsTotalDeductions, // Total LS Deductions
                TLSD = premiumData.LsTotalSharesAndSavings, // Total Shares & Savings (for reference)

                // Footer Information
                Logo = logoPath,
                PrintedBy = GetUserFullName(),
                PrintedOn = DateTime.Now,
                TotalNumberOfOutstandingLoans = premiumData.LpTotalNumberOfOutstandingLoans.ToString(),
                InsurableSharesAndSaving = premiumData.LsInsurableSharesAndSavings.ToString()
            };

            rows.Add(row);

            return rows;
        }
    }
}