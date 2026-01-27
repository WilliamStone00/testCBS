using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing.Report
{
    public class ChequeBookReportRow
    {
        // ================= HEADER =================
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string PrintedBy { get; set; }
        public DateTime PrintedOn { get; set; }
        public string ReportTitle { get; set; }

        // ================= SUMMARY =================
        public int TotalBranches { get; set; }
        public int TotalChequeBooksIssued { get; set; }
        public int TotalAccountsLinked { get; set; }
        public int TotalLeaves { get; set; }
        public int TotalLeavesUsed { get; set; }
        public decimal TotalBalance { get; set; }
        public int ApprovedChequeBooks { get; set; }
        public int UnclearedChequeBooks { get; set; }
        public int PendingChequeBooks { get; set; }
        public DateTime ReportPeriodFrom { get; set; }
        public DateTime ReportPeriodTo { get; set; }
        public string CreatedDate { get; set; }
        public string Time { get; set; }
        // ================= DETAIL =================
        public int RowNumber { get; set; }
        public string CustomerName { get; set; }
        public string AccountNumber { get; set; }
        public string CategoryName { get; set; }
        public int NumberOfLeaves { get; set; }
        public string StatusDescription { get; set; }
        public string Year { get; set; }
    }

}
