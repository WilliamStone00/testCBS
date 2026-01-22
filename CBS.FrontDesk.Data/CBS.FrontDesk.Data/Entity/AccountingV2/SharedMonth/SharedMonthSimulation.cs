using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.SharedMonth
{
    public class SharedMonthSimulation
    {
        public string BranchId1 { get; set; }
        public string BranchId { get; set; }
        public int Year { get; set; }
        public string IntrestType { get; set; }
        
        public decimal InterestRate { get; set; }
        public string Month { get; set; }
        //public string StartPeriodKey { get; set; }
        //public string EndPeriodKey { get; set; }
        //public string ProductId { get; set; }

    }

    public class ShareMonthPsiReportLineDto
    {
        public string MemberReference { get; set; }
        public string MemberName { get; set; }
        public decimal Balance { get; set; }      // opening balance at StartPeriodKey
        public decimal Interest { get; set; }     // computed interest
        public decimal Gross { get; set; }        // here = Interest (before tax)
        public decimal Adjustment { get; set; }   // sum of adjustment-like tx in period
        public decimal Tax { get; set; }

        
        
    }

    public class CreateSimulations
    {
        public string BranchId { get; set; }
        public int Year { get; set; }
        public string IntrestDistributionType { get; set; }
        public string StartPeriodKey { get; set; }  // e.g. "1 Jan"
        public string EndPeriodKey { get; set; }    // e.g. "1 Feb"
        public string ProductId { get; set; }
        public decimal? RateOverride { get; set; }
        public List<ShareMonthPsiReportLineDto> ShareMonthPsiReportLineDto { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string SimulatedByUserId { get; set; }

        public DateTime Date { get; set; }
        public string SimulatedByUserName { get; set; }
        public string BranchName { get; set; }
    }

    public class SharedMonthSimulationQuery
    {
        public DataTableOptions Options { get; set; }
        public SharedMonthSimulationQuery() { Options = new DataTableOptions(); }

        public string Name { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string BranchId { get; set; }
        public decimal InterestRate { get; set; }
        public string Month { get; set; }



    }

    public class InstrestCalculation
    {
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string BranchId { get; set; } = null;
        public bool IsForNetwork { get; set; }
    }

    public class SimulationFilterRequest
    {
        public string Name { get; set; } = null;
        public string SimulatedByUserId { get; set; } = null;
        public string BranchId { get; set; } = null;

        public DateTime? StartUtc { get; set; }
        public DateTime? EndUtc { get; set; }

        public int PageNumber { get; set; } = 0;
        public int PageSize { get; set; } = 0;

        public string SortBy { get; set; }
        public string SortDir { get; set; } = "asc";
    }

    public class JobRequestItem
    {
        public string Id { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Code { get; set; }

        public bool IsForNetwork { get; set; }

        public string BranchName { get; set; }

        public string Status { get; set; }

        public DateTime? CreatedAt { get; set; }

        public int TryCount { get; set; }

        public string LastError { get; set; }

        public DateTime? LastTriedAt { get; set; }
    }

    public class JobRequestListResponse
    {
        public List<JobRequestItem> Items { get; set; } 
    }

    public class SharedMonthQuery
    {
        public DataTableOptions Options { get; set; }
        public SharedMonthQuery() { Options = new DataTableOptions(); }

        public string BranchId { get; set; } = null;
        public string Month { get; set; }
        public string IntrestType { get; set; }
       

    }

    public class SharedMonthDownloadDto
    {
        public string Id { get; set; }              // maps to "_id"
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string DownloadPath { get; set; }
        public string FullPath { get; set; }
        public string FileType { get; set; }
        public string ReportType { get; set; }
        public string BranchName { get; set; }
        public string Username { get; set; }
        public string Size { get; set; }
    }


}