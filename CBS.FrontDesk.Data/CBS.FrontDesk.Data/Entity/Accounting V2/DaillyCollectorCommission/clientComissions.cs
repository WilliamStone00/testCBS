using CBS.FrontDesk.Data.Entity.DataTable;
using Newtonsoft.Json;
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
        public string Name { get; set; }
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
        public bool Active { get; set; }
        public DateTime? CreateDate { get; set; }
        public string AccountConfirmationNumber { get; set; } = string.Empty;
        public string CustomerTypeName { get; set; }
        public string AgeGroup { get; set; }
        public bool UsingMobileApp { get; set; }
        public string Address { get; set; }
        public string Cni { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public string PlaceOfBirth { get; set; }
        public string UserId { get; set; }
        public bool IsLinkToUser { get; set; }
    }

    public class CollectorComissionResponse
    {
        //post
        public int Year { get; set; }

        //payment
        public string BranchCommisionGLId { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountingDate { get; set; } = string.Empty;
        public decimal Incentives { get; set; }
        public decimal CollectorTotalCommision { get; set; }
        public decimal DailyCollectorShare { get; set; }
        public decimal Total { get; set; }
        public string DailyCollectorAccount { get; set; } = string.Empty;
        public decimal BranchShare { get; set; }
        public string PaymentGl { get; set; }

        public string TotalMembersWithActivity { get; set; }
        public string TotalValueCollected { get; set; }
        public string TotalFeeCharged { get; set; }
        public string TotalAmountToDistribute { get; set; }


        public string CollectorId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string CollectorName { get; set; } = string.Empty;
        public string BranchId { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public List<MemberStat> MemberStats { get; set; } = new List<MemberStat>();
        public List<StakeholderShare> SharedAmounts { get; set; } = new List<StakeholderShare>();
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
        public string PaymentStatus { get; set; } = string.Empty;
        public string AccountNumber { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public decimal TotalActivityAmount { get; set; }
        public decimal FeeCharged { get; set; }
        public decimal ActualBalance { get; set; }
        public DateTime? LastTransactionDate { get; set; }

        // Add a string property to handle the date string from JSON
        [JsonIgnore]
        public string LastTransactionDateString
        {
            get => LastTransactionDate?.ToString("yyyy-MM-ddTHH:mm:sszzz");
            set
            {
                if (!string.IsNullOrEmpty(value) && DateTime.TryParse(value, out DateTime date))
                {
                    LastTransactionDate = date;
                }
            }
        }
    }

    public class Payment
    {

        public string DailyCollectorId { get; set; }
        public string AccountNumber { get; set; }
        public string BranchCommisionGLId { get; set; }
        public decimal IncentiveAmount { get; set; }
        public decimal CollectorTotalCommision { get; set; }
        public DateTime? AccountingDate { get; set; }
        public string BranchId { get; set; }
        public decimal TotalAmountToShare { get; set; }
        public List<SharedAmount> SharedAmounts { get; set; } = new List<SharedAmount>();
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class SharedAmount
    {
        public string StakeHolderId { get; set; }
        public string Stakeholder { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
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
        public string CollectorId { get; set; }
        public string MemberReference { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinPaidAmount { get; set; }
        public decimal? MaxPaidAmount { get; set; }
        public string ReferenceNumber { get; set; }
        public string ProcessedByUserId { get; set; }
        public string Currency { get; set; }
        public bool IsExport { get; set; }

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
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class ExportTableRequest
    {
        public List<DataTableResponse> Data { get; set; }
        public int TotalRecords { get; set; }
        public ExportOptions ExportOptions { get; set; }
        public dynamic Filters { get; set; }
    }

    public class DataTableResponse
    {
        public string Id { get; set; }
        public string CollectorId { get; set; }
        public string CollectorName { get; set; }
        public string CollectorPhoneNumber { get; set; }
        public string CollectorAccountNumber { get; set; }
        public string MemberReference { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string ReferenceNumber { get; set; }
        public decimal CollectorShareAmount { get; set; }
        public decimal IncentiveAmount { get; set; }
        public decimal TotalPaidAmountToCollector { get; set; }
        public decimal TotalCommissionShared { get; set; }
        public decimal AmountPaid { get; set; }
        public string Currency { get; set; }
        public DateTimeOffset DatePaid { get; set; }
        public string Description { get; set; }
        public string PaymentSource { get; set; }
        public string ProcessedBy { get; set; }
        public string ProcessedByUserId { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset? DeletedDate { get; set; }
        public string DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}
