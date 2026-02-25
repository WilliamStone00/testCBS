using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.IPS
{

    public class IPSClaim
    {
        public string Id { get; set; } = string.Empty;
        public string MemberId { get; set; } = string.Empty;
        public string ClaimType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string IdempotencyKey { get; set; } = string.Empty;

        public DateTime? CurrentDate { get; set; }
        public string EventCategory { get; set; } = string.Empty;

        public string MemberLastName { get; set; } = string.Empty;
        public string MemberFirstName { get; set; } = string.Empty;
        public string MemberMiddleName { get; set; }
        public string Sex { get; set; } = string.Empty;

        public string AccountNumber { get; set; }
        public string Address { get; set; } = string.Empty;

        public string UsualDutiesOfLivelihood { get; set; } = string.Empty;
        public DateTime? DateLastFullyPerformedSuchDuties { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public int? Age { get; set; }

        public DateTime? DateOfDeathOrDisability { get; set; }
        public string Cause { get; set; } = string.Empty;

        public string EmployerName { get; set; } = string.Empty;
        public string EmployerAddress { get; set; } = string.Empty;
        public string ReasonForStopping { get; set; } = string.Empty;

        public string BeneficiaryName { get; set; } = string.Empty;
        public string BeneficiaryAddress { get; set; } = string.Empty;
        public string BeneficiaryContact { get; set; } = string.Empty;

        public bool? IsAccountSubjectToPayrollDeduction { get; set; }
        public DateTime? PayrollLastPostingDate { get; set; }

        public bool? IsAccidentRelated { get; set; }
        public DateTime? AccidentDate { get; set; }

        public decimal? SavingsBalanceAtEvent { get; set; }

        public DateTime? DateLastWorked { get; set; }
        public DateTime? DateOfDeath { get; set; }

        public decimal? ClaimedAmount { get; set; }

        public DateTime? LoanGrantedDate { get; set; }
        public decimal? LoanGrantedAmount { get; set; }

        public DateTime? DateOfLastPrincipalOrInterestPayment { get; set; }
        public decimal? LoanBalanceAtEvent { get; set; }
        public decimal? LoanInterestClaimed { get; set; }
        public decimal? TotalPrincipalAndInterestDue { get; set; }
        public decimal? ApprovedAmount { get; set; }

        public string BranchId { get; set; } = string.Empty;
        public string BranchName { get; set; }
        public string BankName { get; set; }

        public string Remarks { get; set; } = string.Empty;

        public string PostingReference { get; set; }
        public DateTime? PostedAt { get; set; }

        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }

        public bool IsDeleted { get; set; }

        public string SourceBranchId { get; set; }
        public string BankId { get; set; }
        public string BranchCode { get; set; }
        public string PrintedBy { get; set; }

        public List<IpsClaimDocument> Documents { get; set; } = new List<IpsClaimDocument>();
    }

    public class IpsClaimDocument
        {
            public string Id { get; set; } = string.Empty;
            public string CustomerId { get; set; } = string.Empty;
            public string ClaimId { get; set; } = string.Empty;

            public string UrlPath { get; set; } = string.Empty;
            public string DocumentName { get; set; } = string.Empty;
            public string Extension { get; set; } = string.Empty;
            public string BaseUrl { get; set; } = string.Empty;
            public string DocumentType { get; set; } = string.Empty;

            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
        }
    


    public class IPSClaimCreate
    {
        public string Id { get; set; }
        public string MemberId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }

        public string LegalForm { get; set; }
        public string CustomerType { get; set; }
        public string IdNumber { get; set; }
        public string Email { get; set; }
        public string Occupation { get; set; }

        public DateTime DateOfEvent { get; set; }
        public string ClaimType { get; set; }
        public string EventCategory { get; set; }
        public string Cause { get; set; }

        public decimal ClaimedAmount { get; set; }
        public string BranchId { get; set; }

        public string UsualDutiesOfLivelihood { get; set; }
        public DateTime DateLastFullyPerformedSuchDuties { get; set; }
        public string ReasonForStopping { get; set; }

        public string BeneficiaryAddress { get; set; }

        public bool IsAccountSubjectToPayrollDeduction { get; set; }
        public DateTime PayrollLastPostingDate { get; set; }

        public bool IsAccidentRelated { get; set; }
        public DateTime AccidentDate { get; set; }

        public DateTime LoanGrantedDate { get; set; }
        public decimal LoanGrantedAmount { get; set; }

        public DateTime DateOfLastPrincipalOrInterestPayment { get; set; }
        public decimal LoanInterestClaimed { get; set; }

        public string Remarks { get; set; }

        public string BeneficiaryName { get; set; }
        public string BeneficiaryContact { get; set; }

        public bool IsMemberOfAGroup { get; set; }
        public bool IsBelongToGroup { get; set; }

        public string EmployerName { get; set; }
        public string EmployerTelephone { get; set; }
        public string EmployerAddress { get; set; }

        public string Gender { get; set; }

        public string Phone { get; set; }
        public string Address { get; set; }
    }


    public class IPSClaimApprove
    {
        public string ClaimId { get; set; }
        public decimal ApprovedAmount { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovalComment { get; set; }
    }


    public class IPSClaimReject
    {
        public string ClaimId { get; set; }
        public string Reason { get; set; }
        public string RejectedBy { get; set; }
    }
          
    
        public class PremiumClaimCashIn
        {
            public string ClaimId { get; set; } = string.Empty;
            public string CustomerId { get; set; } = string.Empty;
            public string Note { get; set; }
            public decimal TotalAmount { get; set; }

            public List<MemberBalanceDestinationEntry> MemberBalanceDestinationEntries { get; set; } = new List<MemberBalanceDestinationEntry>();
            public List<SourceAccountEntryListing> SourceAccountEntryListing { get; set; } = new List<SourceAccountEntryListing>();
            public List<DestinationBranchEntryListing> DestinationBranchEntryListing { get; set; } = new List<DestinationBranchEntryListing>();
        }

        public class MemberBalanceDestinationEntry
    {
        public string AccountId { get; set; } = string.Empty;
        public string DestinationGLAccountId { get; set; } 

        public string AccountType { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string Description { get; set; }
        }

        public class SourceAccountEntryListing
        {
            public string SourceBranchId { get; set; } = string.Empty;
            public string SourceGLAccountId { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string Description { get; set; }
        }

        public class DestinationBranchEntryListing
        {
            public string DestinationBranchGLAccountId { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string Description { get; set; }
        }
    
       


    public class IPSClaimDocument
    {
        public List<HttpPostedFileBase> AttachedFiles { get; set; }
        public string Id { get; set; }
        public string UrlPath { get; set; }
        public string DocumentName { get; set; }
        public string Extension { get; set; }
        public string BaseUrl { get; set; }
        public string DocumentType { get; set; } // "DeathCertificate", "IdFront", "IdBack", "PassBook"
    }

    public enum IPSClaimType { LifeSavings = 1, LoanProtection = 2 }

    public class IPSClaimQuery
    {
        public DataTableOptions Options { get; set; }
        public IPSClaimQuery() { Options = new DataTableOptions(); }

        public string MemberId { get; set; }
        public string BranchId { get; set; }
        public string Status { get; set; }
        public string ClaimType { get; set; }

        public DateTime? EventFrom { get; set; }
        public DateTime? EventTo { get; set; }

        public DateTime? CreatedFromUtc { get; set; }
        public DateTime? CreatedToUtc { get; set; }

        public decimal? MinApprovedAmount { get; set; }
        public decimal? MaxApprovedAmount { get; set; }

        public bool? IsPosted { get; set; }
        public bool IncludeDeleted { get; set; } = true;

        public string SourceGLAccountId { get; set; } = string.Empty;
    }

    public class IPSClaimExportRequest
    {
        public string ExportFormat { get; set; } = "excel";
        public string ReportType { get; set; } = "current";
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string FileName { get; set; } = "IPS_Claims_Report";
        public bool IncludeSummary { get; set; } = true;
        public List<IPSClaimExportRecord> Data { get; set; }
    }

    public class IPSClaimExportRecord
    {
        public string Id { get; set; }
        public string ClaimNumber { get; set; }
        public string MemberId { get; set; }
        public string MemberName { get; set; }
        public string ClaimType { get; set; }
        public string BranchName { get; set; }
        public decimal? ClaimedAmount { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public string BeneficiaryName { get; set; }
        public string BeneficiaryContact { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? EventDate { get; set; }
        public string CreatedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string RejectedBy { get; set; }
        public string PostedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? PostedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool? IsPosted { get; set; }
        public string Notes { get; set; }
    }

    public class IPSClaimExportOptions
    {
        public string FileName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool IncludeSummary { get; set; } = true;
    }


    public class CustomerMetaDataResponse
    {
        public CustomerDto customer { get; set; }
        public List<AccountDto> accounts { get; set; }
        public List<LoanDto> loans { get; set; }

        public int accountnum { get; set; }
        public int loannum { get; set; }

        public decimal totalAccountBalance { get; set; }
        public decimal totalLoanBalance { get; set; }
        public decimal netClaimableAmount { get; set; }
        public string statusMessage { get; set; }
        public bool success { get; set; }
    }


    public class CustomerDto
    {
        public string customerId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string occupation { get; set; }
        public string address { get; set; }
        public string idNumber { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public bool active { get; set; }
        public string branchId { get; set; }
        public string legalForm { get; set; }
        public string customerType { get; set; }
        public string gender { get; set; }
        public string employerName { get; set; }
        public string employerTelephone { get; set; }
        public string employerAddress { get; set; }
        public string maritalStatus { get; set; }
        public int numberOfKids { get; set; }
        public decimal income { get; set; }
        public DateTime? dateOfBirth { get; set; }
        public string placeOfBirth { get; set; }

        // Calculated
        public string fullName => $"{firstName} {lastName}".Trim();
    }


    public class AccountDto
    {
        public string id { get; set; }
        public string accountNumber { get; set; }
        public decimal balance { get; set; }
        public string status { get; set; }
        public string customerId { get; set; }
        public string accountName { get; set; }
        public string accountType { get; set; }
        public decimal blockedAmount { get; set; }
        public string blockedId { get; set; }
        public string profileType { get; set; }
        public DateTime? dateBlocked { get; set; }
        public string reasonOfBlocked { get; set; }
        public DateTime? dateOfLastOperation { get; set; }

        // Calculated
        public decimal availableBalance => balance - blockedAmount;
        public bool isBlocked => blockedAmount > 0;
    }


    public class LoanDto
    {
        public string id { get; set; }
        public string loanApplicationId { get; set; }
        public decimal principal { get; set; }
        public decimal balance { get; set; }
        public decimal interestRate { get; set; }
        public string loanStatus { get; set; }
        public DateTime? disbursementDate { get; set; }
        public DateTime? maturityDate { get; set; }
        public string customerId { get; set; }
        public string loanManager { get; set; }
        public bool isDeliquentLoan { get; set; }
        public bool isCurrentLoan { get; set; }
        public int loanDuration { get; set; }
    }

}