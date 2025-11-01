using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.ReportData
{
    public sealed class JournalQuery
    {
        /// <summary>Inclusive period start (local date).</summary>
        [Required] public DateTime DateFrom { get; set; }

        /// <summary>Inclusive period end (local date).</summary>
        [Required] public DateTime DateTo { get; set; }

        /// <summary>When Consolidated = false, filter by a single branch.</summary>
        public string BranchId { get; set; }

        /// <summary>Consolidated across branches.</summary>
        public bool Consolidated { get; set; }

        /// <summary>
        /// Optional subset of branches to include when Consolidated = true.
        /// If empty/null and Consolidated = true, includes all branches.
        /// </summary>
        public IReadOnlyList<string> SelectedBranchIds { get; set; }

        /// <summary>Exclude internal liaison (DueTo/DueFrom) movements.</summary>
        public bool ExcludeLiaisonInternal { get; set; } = true;

        /// <summary>“Auto” picks Temp or RealTime/Reconciled based on Accounting Day.</summary>
        public DataSourceMode SourceMode { get; set; } = DataSourceMode.Auto;

        /// <summary>Two-letter code: “en” or “fr”.</summary>
        public string Language { get; set; } = "en";

        /// <summary>Optional filter: only these account numbers.</summary>
        public IReadOnlyList<string> AccountNumbers { get; set; }
        public PagingOptions Paging { get; set; }
        public SortOptions Sorting { get; set; }
    }

    public sealed class StatementQuery
    {
        /// <summary>Inclusive period start (local date).</summary>
        [Required] public DateTime From { get; set; }

        /// <summary>Inclusive period end (local date).</summary>
        [Required] public DateTime To { get; set; }

        /// <summary>When Consolidated = false, filter by a single branch.</summary>
        public string BranchId { get; set; }

        /// <summary>Consolidated across branches.</summary>
        public bool Consolidated { get; set; }

        /// <summary>
        /// Optional subset of branches to include when Consolidated = true.
        /// If empty/null and Consolidated = true, includes all branches.
        /// </summary>
        public IReadOnlyList<string> SelectedBranchIds { get; set; }

        /// <summary>Exclude internal liaison (DueTo/DueFrom) movements.</summary>
        public bool ExcludeLiaisonInternal { get; set; } = true;

        /// <summary>“Auto” picks Temp or RealTime/Reconciled based on Accounting Day.</summary>
        public DataSourceMode SourceMode { get; set; } = DataSourceMode.Auto;

        /// <summary>Two-letter code: “en” or “fr”.</summary>
        public string Language { get; set; } = "en";

        /// <summary>Single account (mutually exclusive with AccountNumbers in controller logic).</summary>
        public string AccountNumber { get; set; }

        /// <summary>Multiple accounts (mutually exclusive with AccountNumber in controller logic).</summary>
        public IReadOnlyList<string> AccountNumbers { get; set; }
        public PagingOptions Paging { get; set; }
        public SortOptions Sorting { get; set; }
    }

    public sealed class TrialBalanceQuery
    {
        /// <summary>Inclusive period start (local date).</summary>
        [Required] public DateTime From { get; set; }

        /// <summary>Inclusive period end (local date).</summary>
        [Required] public DateTime To { get; set; }

        /// <summary>When Consolidated = false, filter by a single branch.</summary>
        public string BranchId { get; set; }

        /// <summary>Consolidated across branches.</summary>
        public bool Consolidated { get; set; }

        /// <summary>
        /// Optional subset of branches to include when Consolidated = true.
        /// If empty/null and Consolidated = true, includes all branches.
        /// </summary>
        public IReadOnlyList<string> SelectedBranchIds { get; set; }

        /// <summary>Exclude internal liaison (DueTo/DueFrom) movements.</summary>
        public bool ExcludeLiaisonInternal { get; set; } = true;

        /// <summary>“Auto” picks Temp or RealTime/Reconciled based on Accounting Day.</summary>
        public DataSourceMode SourceMode { get; set; } = DataSourceMode.Auto;

        /// <summary>Two-letter code: “en” or “fr”.</summary>
        public string Language { get; set; } = "en";
        public PagingOptions Paging { get; set; }
        public SortOptions Sorting { get; set; }
    }

    public sealed class FinancialQuery
    {
        /// <summary>Inclusive period start (local date).</summary>
        [Required] public DateTime From { get; set; }

        /// <summary>Inclusive period end (local date).</summary>
        [Required] public DateTime To { get; set; }

        /// <summary>When Consolidated = false, filter by a single branch.</summary>
        public  string BranchId { get; set; }

        /// <summary>Consolidated across branches.</summary>
        public bool Consolidated { get; set; }

        /// <summary>
        /// Optional subset of branches to include when Consolidated = true.
        /// If empty/null and Consolidated = true, includes all branches.
        /// </summary>
        public IReadOnlyList<string> SelectedBranchIds { get; set; }

        /// <summary>Exclude internal liaison (DueTo/DueFrom) movements.</summary>
        public bool ExcludeLiaisonInternal { get; set; } = true;

        /// <summary>“Auto” picks Temp or RealTime/Reconciled based on Accounting Day.</summary>
        public DataSourceMode SourceMode { get; set; } = DataSourceMode.Auto;

        /// <summary>Two-letter code: “en” or “fr”.</summary>
        public string Language { get; set; } = "en";
        public PagingOptions Paging { get; set; }
        public SortOptions Sorting { get; set; }
    }
    public sealed class ReportsFilterVm
    {
        // Dates map to ReportFilter.DateFrom/DateTo
        [Display(Name = "From")] public DateTime? DateFrom { get; set; }
        [Display(Name = "To")] public DateTime? DateTo { get; set; }


        // Scope
        public string BranchId { get; set; }
        public bool Consolidated { get; set; }
        public List<string> SelectedBranchIds { get; set; }


        // Flags
        public bool ExcludeLiaisonInternal { get; set; } = true;
        public DataSourceMode SourceMode { get; set; } = DataSourceMode.Auto;
        public string Language { get; set; } = "en"; // "en" or "fr"


        // Accounts (statement/journal specific)
        public string AccountNumber { get; set; }
        public List<string> AccountNumbers { get; set; }


        // DataTables helpers
        public string SearchText { get; set; }


        public SelectList Branches { get; set; } = new SelectList(Enumerable.Empty<object>(), "Id", "Name");
        public SelectList LanguageList { get; set; } = new SelectList(
            new[] { new { K = "en", V = "English" }, new { K = "fr", V = "Français" } }, "K", "V");
        public SelectList SourceModes { get; set; } =
            new SelectList(Enum.GetNames(typeof(DataSourceMode)));
    }
    public enum DataSourceMode
    {
        Temp = 1,          // if you still expose temp during the day
        Reconciled = 2,    // use ReconciledLedgerLine
        Auto = 3,           // let the service decide based on AccountingDay
        RealTime = 4   // <-- NEW: dbo + temp union
    }

public static class DataTableMapping
    {
       
        public static PagingOptions ToPaging(this DataTableOptions o)
        {
            if (o is null) return new PagingOptions { Page = 1, PageSize = 50 };

            var size = o.length <= 0 ? 50 : o.length;
            // put a sensible cap to protect server
            size = Math.Min(size, 1000);

            var start = o.start < 0 ? 0 : o.start;
            var page = size == 0 ? 1 : (start / size) + 1;

            return new PagingOptions
            {
                Page = page,
                PageSize = size
            };
        }

        public static SortOptions ToSort(this DataTableOptions o, string defaultColumn = "date", string defaultDir = "asc")
        {
            if (o is null) return new SortOptions { SortBy = defaultColumn, SortDir = defaultDir };

            var col = string.IsNullOrWhiteSpace(o.sortColumnName) ? defaultColumn : o.sortColumnName;

          

            var dir = string.Equals(o.sortColumnDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

            return new SortOptions
            {
                SortBy = col,
                SortDir = dir
            };
        }

        public static string SearchText(this DataTableOptions o)
            => o?.searchValue ?? string.Empty;
    }

    public sealed class PagingOptions
    {
        // 1-based page index
        public int Page { get; set; } = 1;
        // Max safeguard; service/repo can impose a stricter cap if needed
        public int PageSize { get; set; } = 200;
    }

    public sealed class SortOptions
    {
        /// <summary>Column name to sort by. Allowed columns are enforced/sanitized upstream.</summary>
        public string SortBy { get; set; } = "date";   // e.g., "date","reference","accountNumber"
        public string SortDir { get; set; } = "asc";   // "asc" | "desc"
    }
    /// <summary>
    /// One entrypoint for "print this report".
    /// Optional fields are used by certain kinds (e.g., AccountNumber for statements).
    /// </summary>
    public sealed class PrintReportRequest
    {
        public ReportKind Kind { get; set; }
        public ReportFilter Filter { get; set; } = default;

        // Optional selectors
        public string AccountNumber { get; set; }       // single account (kept for compatibility)
        public IReadOnlyList<string> AccountNumbers { get; set; } // ← NEW multi-account selection
        public string FinancialReportCode { get; set; } // for Income/Expenditure/BalanceSheet/FinancialStatement
        public string TitleOverride { get; set; }       // custom title for print
    }

    public sealed class ReportFilter
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        // Branch selection:
        public string BranchId { get; set; }     // when Consolidated == false
        public bool Consolidated { get; set; }    // true => many branches

        /// <summary>
        /// When Consolidated == true and this list is not null/empty,
        /// restrict consolidation to ONLY these branches.
        /// </summary>
        public IReadOnlyList<string> SelectedBranchIds { get; set; }  // ← NEW

        public bool ExcludeLiaisonInternal { get; set; } = true;
        public DataSourceMode SourceMode { get; set; } = DataSourceMode.Reconciled;

        /// <summary>Two-letter code: "en" or "fr".</summary>
        public string Language { get; set; } = "en";
        // already added previously:
        public PagingOptions Paging { get; set; }
        public SortOptions Sorting { get; set; }

        // NEW: free-text search for DataTables
        public string SearchText { get; set; }
    }
    public sealed class TrialBalance4ColRowDto
    {
        public string AccountNumber { get; set; } = "";
        public string AccountName { get; set; } = "";
        public decimal Opening { get; set; }
        public decimal Period { get; set; }
        public decimal Closing { get; set; }
    }
    public sealed class TrialBalanceRowDto
    {
        public string AccountNumber { get; set; } = "";
        public string AccountName { get; set; } = "";
        public decimal OpeningDR { get; set; }
        public decimal OpeningCR { get; set; }
        public decimal PeriodDR { get; set; }
        public decimal PeriodCR { get; set; }
        public decimal ClosingDR { get; set; }
        public decimal ClosingCR { get; set; }
    }
    /// <summary>
    /// One entrypoint for "print this report".
    /// Optional fields are used by certain kinds (e.g., AccountNumber for statements).
    /// </summary>
    public enum ReportKind
    {
        Journal,
        AccountStatement,
        TrialBalance6,
        TrialBalance4,
        IncomeStatement,
        ExpenditureStatement,
        BalanceSheet,
        FinancialStatement
    }
    public sealed class GenerateReportCommand
    {
        public ReportKind Kind { get; set; }

        /// <summary>Required when Kind is FinancialStatement / BalanceSheet / IncomeStatement / ExpenditureStatement.</summary>
        public string FinancialReportCode { get; set; }

        /// <summary>Filters incl. dates, branch scope, selected branches, source mode, language, etc.</summary>
        public ReportFilter Filter { get; set; } = default;

        /// <summary>Optional custom title for the output envelope.</summary>
        public string TitleOverride { get; set; }

        /// <summary>Optional list for Journal/Statement multi-account output.</summary>
        public IReadOnlyList<string> AccountNumbers { get; set; }

        /// <summary>Optional single account (Statement fallback if AccountNumbers not supplied).</summary>
        public string AccountNumber { get; set; }
    }
    //public sealed class DataTableOptions { public int draw { get; set; } public int start { get; set; } public int length { get; set; } public string searchValue { get; set; } public string? sortColumnName { get; set; } public string? sortColumnDirection { get; set; } }
    public sealed class DataTableQuery<TFilter> { public TFilter Filters { get; set; } = default; public DataTableOptions Options { get; set; }}
    public sealed class ReportRequest<TFilter> { public TFilter Filters { get; set; } = default; }
    public sealed class ExportRequest { public string Type { get; set; } = ""; public string Format { get; set; } = "csv"; public ReportsFilterVm Filters { get; set; }}
}
