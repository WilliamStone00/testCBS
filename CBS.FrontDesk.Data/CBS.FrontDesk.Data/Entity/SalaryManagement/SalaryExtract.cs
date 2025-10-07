using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.ReportDataSetDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace CBS.FrontDesk.Data.Entity.SalaryManagement
{
    public class TempPayCode
    {
        public string Id { get; set; }

        // Store only the hash; plaintext will be shown once by the handler to the user
        public string CodeHash { get; set; }
        public string Salt { get; set; }

        public string SalaryExtractId { get; set; }

        // "NonMember" | "Member" (future-proof)
        public string BeneficiaryType { get; set; } = "NonMember";
        public string BeneficiaryId { get; set; }
        public string BeneficiaryReference { get; set; }
        public string BeneficiaryName { get; set; }
        public string CNI { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "XAF";

        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }

        public DateTime ExpiresAt { get; set; }
        public string Status { get; set; }

        public int AttemptCount { get; set; }
        public int MaxAttempts { get; set; } = 5;

        public string GeneratedByUserId { get; set; }
        public string GeneratedByUserName { get; set; }
        public string Channel { get; set; } = "BackOffice"; // BackOffice|API|CashDesk

        public DateTime? RedeemedAt { get; set; }
        public string RedeemedByUserId { get; set; }
        public string RedeemedByUserName { get; set; }
    }
    public sealed class GetTempPayCodeByCodeQuery
    {
        public string PlainCode { get; set; }
    }
    public class SalaryExtract
    {
        public string Id { get; set; }
        public decimal NetSalary { get; set; }
        public decimal Saving { get; set; }
        public decimal Deposit { get; set; }
        public decimal Shares { get; set; }
        public decimal Charges { get; set; }
        public decimal Salary { get; set; }
        public bool IsOnldLoan { get; set; }
        public string LoanType { get; set; }
        public decimal StandingOrderAmount { get; set; }
        public string StandingOrderStatement { get; set; }
        public bool Status { get; set; }
        public string FileUploadIdReferenceId { get; set; }
        public decimal PreferenceShares { get; set; }
        public decimal RemainingSalary { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string FileUploadId { get; set; }
        public string Matricule { get; set; }
        public string MemberReference { get; set; }
        public string MemberName { get; set; }
        public string UploadedBy { get; set; }
        public string ExecutedBy { get; set; }
        public string SalaryAnalysisResultId { get; set; }
        public DateTime ExtrationDate { get; set; }
        public DateTime ExecutionDate { get; set; }
        public DateTime AccountingDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Loan1_Main_Loan_Id { get; set; }
        public string Loan1_Main_Loan_Type { get; set; }
        public string Loan1_Main_Loan_ProductId { get; set; }
        public string Loan1_Main_Loan_ProductName { get; set; }
        public decimal Loan1_Main_Loan_Capital { get; set; }
        public decimal Loan1_Main_Loan_Interest { get; set; }
        public decimal Loan1_Main_Loan_VAT { get; set; }
        public decimal Loan1_Main_Loan_TotalRepayment { get; set; }

        public string Loan2_Exceptional_Loan_Id { get; set; }
        public string Loan2_Exceptional_Loan_Type { get; set; }
        public string Loan2_Exceptional_Loan_ProductId { get; set; }
        public string Loan2_Exceptional_Loan_ProductName { get; set; }
        public decimal Loan2_Exceptional_Loan_Capital { get; set; }
        public decimal Loan2_Exceptional_Loan_Interest { get; set; }
        public decimal Loan2_Exceptional_Loan_VAT { get; set; }
        public decimal Loan2_Exceptional_Loan_TotalRepayment { get; set; }

        public string Loan3_Elected_Staff_Id { get; set; }
        public string Loan3_Elected_Staff_Type { get; set; }
        public string Loan3_Elected_Staff_ProductId { get; set; }
        public string Loan3_Elected_Staff_ProductName { get; set; }
        public decimal Loan3_Elected_Staff_Capital { get; set; }
        public decimal Loan3_Elected_Staff_Interest { get; set; }
        public decimal Loan3_Elected_Staff_VAT { get; set; }
        public decimal Loan3_Elected_Staff_TotalRepayment { get; set; }

        public string Loan4_Special_Loan_With_Overdraft_Id { get; set; }
        public string Loan4_Special_Loan_With_Overdraft_Type { get; set; }
        public string Loan4_Special_Loan_With_Overdraft_ProductId { get; set; }
        public string Loan4_Special_Loan_With_Overdraft_ProductName { get; set; }
        public decimal Loan4_Special_Loan_With_Overdraft_Capital { get; set; }
        public decimal Loan4_Special_Loan_With_Overdraft_Interest { get; set; }
        public decimal Loan4_Special_Loan_With_Overdraft_VAT { get; set; }
        public decimal Loan4_Special_Loan_With_Overdraft_TotalRepayment { get; set; }

        public string Loan8_Micro_Loan_Id { get; set; }
        public string Loan8_Micro_Loan_Type { get; set; }
        public string Loan8_Micro_Loan_ProductId { get; set; }
        public string Loan8_Micro_Loan_ProductName { get; set; }
        public decimal Loan8_Micro_Loan_Capital { get; set; }
        public decimal Loan8_Micro_Loan_Interest { get; set; }
        public decimal Loan8_Micro_Loan_VAT { get; set; }
        public decimal Loan8_Micro_Loan_TotalRepayment { get; set; }

        // --- Usage flags ---
        public bool Loan1_Main_Loan_Id_Used { get; set; }
        public bool Loan2_Exceptional_Loan_Id_Used { get; set; }
        public bool Loan3_Elected_Staff_Id_Used { get; set; }
        public bool Loan4_Special_Loan_With_Overdraft_Id_Used { get; set; }
        public bool Loan8_Micro_Loan_Id_Used { get; set; }

        public bool SpSavingId_Used { get; set; }

        public string SpSavingId { get; set; }
        public string SpSavingProductId { get; set; }
        public string SpSavingProductName { get; set; }
        public decimal SpSavingCapital { get; set; }
        public decimal SpSavingInterest { get; set; }
        public decimal SpSavingVAT { get; set; }
        public decimal SpSavingTotalRepayment { get; set; }

        public bool Ispaid { get; set; }
        public string PaymentComment { get; set; }
        public virtual FileUpload FileUpload { get; set; }
        public string NonMemberId { get; set; }
        public string NonMemberReference { get; set; }
        public string LastTempPayCodeId { get; set; }
        public DateTime LastTempPayCodeExpiresAt { get; set; } = DateTime.MinValue;
        public string LastTempPayCodeStatus { get; set; }
        public string SpSavingType { get; set; }
        public RevokeTempPayCodesByIdList RevokeTempPayCodesByIdList { get; set; } = new RevokeTempPayCodesByIdList();
        public RegisterNonMemberAndGenerateTempCode RegisterNonMemberAndGenerateTempCode { get; set; } = new RegisterNonMemberAndGenerateTempCode();
    }
    public class TempPayCodeResultDto
    {
        public string TempPayCodeId { get; set; } = default;
        public string TempCode { get; set; } = default;     // plaintext (once)
        public DateTime ExpiresAt { get; set; }
        public string SalaryExtractId { get; set; } = default;
        public string NonMemberReference { get; set; }
        public decimal Amount { get; set; }
        public string BeneficiaryType { get; set; }
    }
    /// <summary>
    /// Summarizes the outcome of a bulk TempPayCode revocation operation.
    /// </summary>
    public class BulkTempPayCodeActionResultDto
    {
        /// <summary>Total number of TempPayCode IDs requested in the operation.</summary>
        public int TotalRequested { get; set; }

        /// <summary>Number of codes successfully revoked.</summary>
        public int Revoked { get; set; }

        /// <summary>Number of codes skipped because they were already redeemed.</summary>
        public int SkippedRedeemed { get; set; }

        /// <summary>Number of codes skipped because they belonged to another branch.</summary>
        public int SkippedWrongBranch { get; set; }

        /// <summary>Number of codes not found in the database.</summary>
        public int NotFound { get; set; }

        /// <summary>IDs of codes that were successfully revoked.</summary>
        public List<string> RevokedIds { get; set; } 

        /// <summary>IDs of codes skipped because they were already redeemed.</summary>
        public List<string> SkippedRedeemedIds { get; set; }

        /// <summary>IDs of codes skipped because they belonged to another branch.</summary>
        public List<string> SkippedWrongBranchIds { get; set; }

        /// <summary>IDs of codes that were not found in the database.</summary>
        public List<string> NotFoundIds { get; set; }

        /// <summary>Detailed summary message describing the outcome of the operation.</summary>
        public string Message { get; set; }
    }
    public class RevokeTempPayCodesByIdList
    {
        /// <summary>Branch context to enforce branch-level ownership.</summary>
        public string BranchId { get; set; } = default;

        /// <summary>List of TempPayCode Ids to revoke.</summary>
        public List<string> TempPayCodeIds { get; set; }

        /// <summary>Optional reason for audit trail.</summary>
        public string Reason { get; set; }

    }
    public class RedeemTempPayCodeCommand
    {
        public string TempPayCodeId { get; set; }
        // NEW: cash denominations for the cash-out
        public CurrencyNotesRequest CurrencyNotes { get; set; } =new CurrencyNotesRequest();
        public string PlainCode { get; internal set; }
        public decimal Amount { get; set; }
        public string Naration { get; set; }
        public string CNI { get; set; }
        public string Telephone { get; set; }
        public string NoneMeberName { get; set; }
        public string SourceChartOfAccountId { get; set; }
        public string CustomerId { get; set; }
        public string SourceType { get; set; }
        public string TransactionType { get; set; }
    }
    public class GenerateTempPayCodeForKnownBeneficiary
    {
        public string SalaryExtractId { get; set; } = default;
        /// <summary>CNI | NonMemberReference | MemberReference | Name</summary>
        public string BeneficiaryLookup { get; set; } = default;
        /// <summary>Optional; default = now + 48h (UTC)</summary>
        public DateTime? ExpiresAt { get; set; }
    }
    public class GetProcessedSalaryDataTableQuery
    {
        public DataTableOptions DataTableOptions { get; set; }

        public string BranchId { get; set; }
        public string UploadedBy { get; set; }
        public string ExecutedBy { get; set; }

        // 🔎 NEW: filters for TempPayCode presence
        public bool? WithActiveCodeOnly { get; set; }     // TempPayCode Active & not expired
        public bool? WithoutActiveCodeOnly { get; set; }  // no active code

        // 🔎 NEW: show only non-members (extracted but not registered members)
        public bool? NonMembersOnly { get; set; }         // NonMemberReference != null OR MemberReference == null

        public string MemberReference { get; set; }
        public string Matricule { get; set; }
        public string MemberName { get; set; }
        public bool? Status { get; set; }                 // true = Salary Paid, false = Pending
        public DateTime? StartDate { get; set; }          // inclusive
        public DateTime? EndDate { get; set; }            // inclusive
    }
    public class RegisterNonMemberAndGenerateTempCode
    {
        public string SalaryExtractId { get; set; } = default;
        public NoneMemberProfileCreateionRequest Kyc { get; set; } = default;
        /// <summary>Optional expiration; if null, defaults to now + 48h (UTC).</summary>
        public DateTime? ExpiresAt { get; set; }
    }
    public class NoneMemberProfileCreateionRequest
    {
        public string FirstName { get; set; } = default;
        public string LastName { get; set; } = default;
        public string CNI { get; set; } = default;
        public string IssuedDate { get; set; } = default;
        public string ExpiryDate { get; set; } = default;
        public string PlaceOfIssued { get; set; } = default;
        public string PlaceOfBirth { get; set; } = default;
        public string Phone { get; set; } = default;
        public DateTime? DOB { get; set; }
        public string Address { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string SubDivisionId { get; set; }
        public string CountryId { get; set; }
        public string RegionId { get; set; }
        public string TownId { get; set; }
        public string DivisionId { get; set; }
        public string Language { get; set; }
        public string BankId { get; set; }
        public string CustomerCode { get; set; }

    }
}
