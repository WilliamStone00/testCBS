using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.AccountingReportsD
{
    // This class represents the response structure returned by the Balance Sheet (Financial Statement) API.
    public class FinancialStatementResponse
    {
        public string Kind { get; set; }
        public string Title { get; set; }
        public DateTime GeneratedAt { get; set; }
        public FilterEcho FilterEcho { get; set; }
        public Payloads Payload { get; set; }
        public string DatasetName { get; set; }
        public string Language { get; set; }
    }

    public class FilterEcho
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? AccountingYear { get; set; }
        public string BranchId { get; set; }
        public bool Consolidated { get; set; }
        public List<string> SelectedBranchIds { get; set; }
        public bool ExcludeLiaisonInternal { get; set; }
        public string SourceMode { get; set; }
        public string Language { get; set; }
        public object Paging { get; set; }
        public object Sorting { get; set; }
        public string SearchText { get; set; }
        public bool IncludeZeroGlAccount { get; set; }
    }

    public class Payloads
    {
        public DateTime AsAt { get; set; }
        public int CurrentYear { get; set; }
        public int PreviousYear { get; set; }
        public string Currency { get; set; }
        
        public List<BalanceItem> Assets { get; set; }
        public List<BalanceItem> LiabilitiesAndEquity { get; set; }
        public decimal TotalAssetsNetCurrentYear { get; set; }
        public decimal TotalAssetsNetLastYear { get; set; }
        public decimal TotalLiabEquityNetCurrentYear { get; set; }
        public decimal TotalLiabEquityNetLastYear { get; set; }
        public decimal? ResultPendingApprovalCurrentYear { get; set; }
        public decimal? ResultPendingApprovalLastYear { get; set; }


        /// <summary>
        // income and expense statement
        /// </summary>
        /// 

        public List<BalanceItem> rows { get; set; }
        public decimal? TotalExpensesCurrentYear { get; set; }
        public decimal? TotalExpensesLastYear { get; set; }
        public decimal? TotalIncomeLastYear { get; set; }
        public decimal? TotalIncomeCurrentYear { get; set; }

      
    }

    public class BalanceItem
    {
        public string RefCode { get; set; }
        public string Caption { get; set; }
        public string Section { get; set; }
        public string Side { get; set; }
        public decimal CurrentYearAmount { get; set; } = 0;
        public decimal LastYearAmount { get; set; } = 0;

        
        public int SortOrder { get; set; }
        public decimal Gross { get; set; }
        public decimal AmortizationProvision { get; set; }
        public decimal NetCurrentYear { get; set; }
        public decimal NetLastYear { get; set; }
        public bool IsSubtotal { get; set; }
        public string Note { get; set; }
    }






}
