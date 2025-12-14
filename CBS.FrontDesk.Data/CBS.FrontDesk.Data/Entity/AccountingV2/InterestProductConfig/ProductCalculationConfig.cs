using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.InterestProductConfig
{
    public class ProductCalculationConfig
    {
        public string Id { get; set; }
        public string AccountingYearId { get; set; }
        public string ProductId { get; set; }
        public string BranchId { get; set; }
        public bool IsEnabled { get; set; }
        public string Frequency { get; set; }
        public int CalculationDayOfMonth { get; set; }
        public string CalculationDayOfWeek { get; set; }
        public int FirstCalculationMonth { get; set; }
        public string ComputationMode { get; set; }
        public string AverageDaysCsv { get; set; }
        public decimal ShareMonthFactor { get; set; }
        public decimal CustomDenominator { get; set; }
        public bool IsShareMonthProduct { get; set; }
        public string ShareMonthMode { get; set; }
        public string ShareMonthAverageDaysCsv { get; set; }
        public decimal ShareMonthFactorOverride { get; set; }
        public string RateSource { get; set; }
        public decimal FixedRate { get; set; }
        public string RateProfileCode { get; set; }
        public bool ApplyRti { get; set; }
        public decimal RtiRateOverride { get; set; }
        public bool ApplyTprcm { get; set; }
        public decimal TprcmRateOverride { get; set; }
        public decimal TaxableThresholdOverride { get; set; }
        public string PostingScope { get; set; }
        public string ExpenseGlId { get; set; }
        public string MemberCreditGlGroup { get; set; }
        public string TaxGlId { get; set; }
        public string LiaisonHoGlId { get; set; }
        public string LiaisonBranchGlId { get; set; }
        public decimal MinBalanceForInterest { get; set; }
        public decimal MaxBalanceForInterest { get; set; }
    }



    public class InterestProductConfigQuery
    {
        public DataTableOptions Options { get; set; }
        public InterestProductConfigQuery() { Options = new DataTableOptions(); }

        public string AccountingYearId { get; set; }
        public string ProductId { get; set; }
        public string BranchId { get; set; }
        public bool IsEnabled { get; set; }

    }
}
