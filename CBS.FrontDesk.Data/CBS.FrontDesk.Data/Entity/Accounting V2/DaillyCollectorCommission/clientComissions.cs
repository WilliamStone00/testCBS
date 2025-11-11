using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorCommission
{
    public class ClientComissionCommand
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string CollectorId { get; set; }
        public string OperationType { get; set; }
        public string BranchId { get; set; }

    }
    public class Customer
    {
        public string Id { get; set; }
        public string  Name { get; set; }
        public string VillageOfOrigin { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Matricule { get; set; }
        public string MobileLoginId { get; set; }
        public string CustomerType { get; set; } = string.Empty;
        public string LegalForm { get; set; } = string.Empty;
        public string MembershipApprovalStatus { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string BranchId { get; set; } = string.Empty;
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string BankId { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public bool? Active { get; set; }
        public DateTime? CreateDate { get; set; }
        public string AccountConfirmationNumber { get; set; } = string.Empty;
        public string CustomerTypeName { get; set; }
        public string AgeGroup { get; set; }
        public bool? UsingMobileApp { get; set; }
        public string Address { get; set; }
        public string Cni { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public string PlaceOfBirth { get; set; }
        public string UserId { get; set; }
        public bool? IsLinkToUser { get; set; }
    }

    public class CollectorComissionResponse
    {
        //post
        public int Year { get; set; }
        //public int Month { get; set; }
        //public string CollectorId { get; set; }
        //public string OperationType { get; set; }
        //public string BranchId { get; set; }    

        //payment
        public string AccountingDate { get; set; } = string.Empty;
        public string Incentives { get; set; } = string.Empty;
        public string DaillyCollectorShare { get; set; }
        public string Total { get; set; }
        public string DaillyCollectorAccount { get; set; } = string.Empty;
        public double BranchShare { get; set; }
        public double PaymentGl { get; set; }

        public string TotalMembersWithActivity { get; set; }
        public string TotalValueCollected { get; set; }
        public string TotalFeeCharged { get; set; }
        public string TotalAmountToDistribute { get; set; }

        
        public string CollectorId { get; set; } = string.Empty;
        public string CollectorName { get; set; } = string.Empty;
        public string BranchId { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public List<MemberStat> MemberStats { get; set; } = new List<MemberStat>();
        public List<StakeholderShare> SharedAmounts = new List<StakeholderShare>();
    }

    public class StakeholderShare
    {
        public string StakeHolderId { get; set; }
        public string Stakeholder { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
    }

    public class MemberStat
    {
        public string MemberId { get; set; } = string.Empty;
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public decimal TotalActivityAmount { get; set; }
        public decimal FeeCharged { get; set; }
        public decimal ActualBalance { get; set; }
        public DateTime LastTransactionDate { get; set; }
    }

    public class Payment
    {
        public string Incentives { get; set; } = string.Empty;
        public string DaillyCollectorShare { get; set; }
        public string Total { get; set; }
        public string DaillyCollectorAccount { get; set; } = string.Empty;
        public double BranchShare { get; set; }
        public double PaymentGl { get; set; }
     }

    public class customerQuery
    {
        public DataTableOptions Options { get; set; }
        public customerQuery() { Options = new DataTableOptions(); }
              
            public string MembershipApprovalStatus { get; set; } = string.Empty;
            public string Gender { get; set; } = string.Empty;
            public string MaritalStatus { get; set; } = string.Empty;
            public string WorkingStatus { get; set; } = string.Empty;
            public string CustomerType { get; set; } = string.Empty;
            public string AgeCategoryStatus { get; set; } = string.Empty;
            public string LegalForm { get; set; } = string.Empty;
            public string CustomerId { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string BranchId { get; set; } = string.Empty;
            public DateTime? DateOfBirthFrom { get; set; }
            public DateTime? DateOfBirthTo { get; set; }
            public DateTime? CreatedFrom { get; set; }
            public DateTime? CreatedTo { get; set; }
            public bool ShowAll { get; set; } = true;
        }
    

    public class commisionQuery
    {
        public DataTableOptions DataTableOptions { get; set; }
        public commisionQuery() { DataTableOptions = new DataTableOptions(); }

        public string BranchId { get; set; }
       
    }

    public class DataTableResponse
    {
       public string BranchId { get; set; }
    }

    public class ExportCommissionRequest
    {
        public CollectorComissionResponse CommissionData { get; set; }
        public ExportOptions ExportOptions { get; set; }
    }

    public class ExportOptions
    {
        public string Format { get; set; }
        public bool IncludeSummary { get; set; }
        public string FileName { get; set; }
        public string ReportType { get; set; }
    }

}
