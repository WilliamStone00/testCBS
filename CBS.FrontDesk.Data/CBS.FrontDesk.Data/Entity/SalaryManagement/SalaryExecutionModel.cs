using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;

namespace CBS.FrontDesk.Data.Entity.SalaryManagement
{

    public class GetAllSalaryExtractQuery
    {
        public string FileUploadId { get; set; }
        public string BranchId { get; set; }  // Optional filter
    }
    public class ExecuteSalaryCarrier
    {
        public UploadAnalysedSalaryCommand UploadAnalysedSalaryCommand { get; set; }
        public ExecuteSalaryCommand ExecuteSalaryCommand { get; set; }
        public SalaryProcessingDto SalaryProcessingDto { get; set; }

        public List<SalaryExtractDto> SalaryExtractes { get; set; }
        public List<SalaryPaymentDto> SalaryPaymentExtractes { get; set; }
        public FileUploadDto FileUpload { get; set; }
        public List<FileUploadDto> FileUploads { get; set; }
        public DashboardViewModel DashboardViewModel { get; set; }
        public List<StringValues> StringValues { get; set; }
        public ExecuteSalaryCarrier()
        {
            UploadAnalysedSalaryCommand=new UploadAnalysedSalaryCommand();
            ExecuteSalaryCommand=new ExecuteSalaryCommand();
            SalaryProcessingDto=new SalaryProcessingDto();
            SalaryExtractes=new List<SalaryExtractDto>();
            FileUpload=new FileUploadDto();
            FileUploads=new List<FileUploadDto>();
            DashboardViewModel=new DashboardViewModel();
            StringValues=new List<StringValues>();
        }
    }
    public class UploadAnalysedSalaryCommand
    {
        [Required(ErrorMessage = "File is required.")]
        public HttpPostedFileBase File { get; set; }
        public string UploadFileId { get; set; }
    }

    public sealed class ExecuteSalaryCommand
    {
        [Required(ErrorMessage = "FileUploadId is required.")]
        public string FileUploadId { get; set; }

        [Required(ErrorMessage = "Execution reason is required.")]
        [MinLength(10, ErrorMessage = "Execution reason must be at least 10 characters.")]
        [MaxLength(1000, ErrorMessage = "Execution reason is too long.")]
        public string ExecutionReason { get; set; }

        [RequiredTrue(ErrorMessage = "You must acknowledge that this operation is irreversible.")]
        public bool AckIrreversible { get; set; }

        [RequiredTrue(ErrorMessage = "You must confirm this is the exact file you just analysed.")]
        public bool AckFileConfirmed { get; set; }

        [Required(ErrorMessage = "You must type EXECUTE to confirm.")]
        [RegularExpression(@"^EXECUTE$", ErrorMessage = "Type EXECUTE to confirm.")]
        public string ConfirmPhrase { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredTrueAttribute : ValidationAttribute
    {
        public RequiredTrueAttribute() : base("The {0} field must be accepted.") { }
        public override bool IsValid(object value) => value is bool b && b;
    }



    public class SalaryPaymentDto
    {
        // Basic information
        public string Id { get; set; }
        public decimal NetSalary { get; set; }
        public decimal Saving { get; set; }
        public decimal Deposit { get; set; }
        public decimal Shares { get; set; }
        public decimal Charges { get; set; }
        public bool Status { get; set; }
        public decimal PreferenceShares { get; set; }
        public bool IsOnldLoan { get; set; }
        public decimal Salary { get; set; }
        public decimal RemainingSalary { get; set; }
        public string LoanType { get; set; }
        public decimal StandingOrderAmount { get; set; }
        public string StandingOrderStatement { get; set; }

        // Branch information
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }

        // File and reference information
        public string FileUploadId { get; set; }
        public string FileUploadIdReferenceId { get; set; }
        public string Matricule { get; set; }
        public string MemberReference { get; set; }
        public string MemberName { get; set; }

        // User information
        public string UploadedBy { get; set; }
        public string ExecutedBy { get; set; }
        public string SalaryAnalysisResultId { get; set; }

        // Dates
        public DateTime ExtrationDate { get; set; }
        public DateTime ExecutionDate { get; set; }
        public DateTime AccountingDate { get; set; }
        public DateTime CreatedDate { get; set; }

        // Loan 1 - Main Loan
        public string Loan1_Main_Loan_Id { get; set; }
        public string Loan1_Main_Loan_Type { get; set; }
        public string Loan1_Main_Loan_ProductId { get; set; }
        public string Loan1_Main_Loan_ProductName { get; set; }
        public decimal Loan1_Main_Loan_Capital { get; set; }
        public decimal Loan1_Main_Loan_Interest { get; set; }
        public decimal Loan1_Main_Loan_VAT { get; set; }
        public decimal Loan1_Main_Loan_TotalRepayment { get; set; }

        // Loan 2 - Exceptional Loan
        public string Loan2_Exceptional_Loan_Id { get; set; }
        public string Loan2_Exceptional_Loan_Type { get; set; }
        public string Loan2_Exceptional_Loan_ProductId { get; set; }
        public string Loan2_Exceptional_Loan_ProductName { get; set; }
        public decimal Loan2_Exceptional_Loan_Capital { get; set; }
        public decimal Loan2_Exceptional_Loan_Interest { get; set; }
        public decimal Loan2_Exceptional_Loan_VAT { get; set; }
        public decimal Loan2_Exceptional_Loan_TotalRepayment { get; set; }

        // Loan 3 - Elected Staff
        public string Loan3_Elected_Staff_Id { get; set; }
        public string Loan3_Elected_Staff_Type { get; set; }
        public string Loan3_Elected_Staff_ProductId { get; set; }
        public string Loan3_Elected_Staff_ProductName { get; set; }
        public decimal Loan3_Elected_Staff_Capital { get; set; }
        public decimal Loan3_Elected_Staff_Interest { get; set; }
        public decimal Loan3_Elected_Staff_VAT { get; set; }
        public decimal Loan3_Elected_Staff_TotalRepayment { get; set; }

        // Loan 4 - Special Loan With Overdraft
        public string Loan4_Special_Loan_With_Overdraft_Id { get; set; }
        public string Loan4_Special_Loan_With_Overdraft_Type { get; set; }
        public string Loan4_Special_Loan_With_Overdraft_ProductId { get; set; }
        public string Loan4_Special_Loan_With_Overdraft_ProductName { get; set; }
        public decimal Loan4_Special_Loan_With_Overdraft_Capital { get; set; }
        public decimal Loan4_Special_Loan_With_Overdraft_Interest { get; set; }
        public decimal Loan4_Special_Loan_With_Overdraft_VAT { get; set; }
        public decimal Loan4_Special_Loan_With_Overdraft_TotalRepayment { get; set; }

        // Loan 8 - Micro Loan
        public string Loan8_Micro_Loan_Id { get; set; }
        public string Loan8_Micro_Loan_Type { get; set; }
        public string Loan8_Micro_Loan_ProductId { get; set; }
        public string Loan8_Micro_Loan_ProductName { get; set; }
        public decimal Loan8_Micro_Loan_Capital { get; set; }
        public decimal Loan8_Micro_Loan_Interest { get; set; }
        public decimal Loan8_Micro_Loan_VAT { get; set; }
        public decimal Loan8_Micro_Loan_TotalRepayment { get; set; }

        // Special Savings
        public string SpSavingId { get; set; }
        public string SpSavingProductId { get; set; }
        public string SpSavingProductName { get; set; }
        public decimal SpSavingCapital { get; set; }
        public decimal SpSavingInterest { get; set; }
        public decimal SpSavingVAT { get; set; }
        public decimal SpSavingTotalRepayment { get; set; }
        public string SpSavingType { get; set; }

        // File upload information (nested object)
        public FileUpload FileUpload { get; set; }

        // Loan usage flags
        public object SalaryAnalysisResult { get; set; }
        public bool Loan1_Main_Loan_Id_Used { get; set; }
        public bool Loan2_Exceptional_Loan_Id_Used { get; set; }
        public bool Loan3_Elected_Staff_Id_Used { get; set; }
        public bool Loan4_Special_Loan_With_Overdraft_Id_Used { get; set; }
        public bool Loan8_Micro_Loan_Id_Used { get; set; }
        public bool SpSavingId_Used { get; set; }

        // Payment information
        public bool Ispaid { get; set; }
        public string PaymentComment { get; set; }

        // Non-member and temp code information
        public string NonMemberId { get; set; }
        public string NonMemberReference { get; set; }
        public string LastTempPayCodeId { get; set; }
        public DateTime LastTempPayCodeExpiresAt { get; set; }
        public string LastTempPayCodeStatus { get; set; }





        public decimal TotalLoanRepayment =>
        Loan1_Main_Loan_TotalRepayment +
        Loan2_Exceptional_Loan_TotalRepayment +
        Loan3_Elected_Staff_TotalRepayment +
        Loan4_Special_Loan_With_Overdraft_TotalRepayment +
        Loan8_Micro_Loan_TotalRepayment;

        /// <summary>
        /// Total Loan Capital (capital 1 + capital 2 + capital 3 + capital 4 + capital 8)
        /// </summary>
        public decimal LoanCapital =>
            Loan1_Main_Loan_Capital +
            Loan2_Exceptional_Loan_Capital +
            Loan3_Elected_Staff_Capital +
            Loan4_Special_Loan_With_Overdraft_Capital +
            Loan8_Micro_Loan_Capital;

        /// <summary>
        /// Total Loan Interest (interest 1 + interest 2 + interest 3 + interest 4 + interest 8)
        /// </summary>
        public decimal LoanInterest =>
            Loan1_Main_Loan_Interest +
            Loan2_Exceptional_Loan_Interest +
            Loan3_Elected_Staff_Interest +
            Loan4_Special_Loan_With_Overdraft_Interest +
            Loan8_Micro_Loan_Interest;

        /// <summary>
        /// Total VAT (vat 1 + vat 2 + vat 3 + vat 4 + vat 8)
        /// </summary>
        public decimal VAT =>
            Loan1_Main_Loan_VAT +
            Loan2_Exceptional_Loan_VAT +
            Loan3_Elected_Staff_VAT +
            Loan4_Special_Loan_With_Overdraft_VAT +
            Loan8_Micro_Loan_VAT;
    }
    public class SalaryExtractDto
    {
        public string Id { get; set; }
        public decimal NetSalary { get; set; }
        public decimal Saving { get; set; }
        public decimal Deposit { get; set; }
        public decimal Shares { get; set; }
        public decimal Charges { get; set; }
        public decimal LoanCapital { get; set; }
        public decimal VAT { get; set; }
        public decimal TotalLoanRepayment { get; set; }
        public decimal LoanInterest { get; set; }
        public string LoanType { get; set; }
        public bool Status { get; set; }
        public decimal PreferenceShares { get; set; }
        public decimal StandingOrderAmount { get; set; }
        public string StandingOrderStatement { get; set; }
        public decimal Salary { get; set; }
        public decimal RemainingSalary { get; set; }
        public string LoanId { get; set; }
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
    }
    public class SalaryProcessingDto
    {
        public decimal TotalDeposit { get; set; }
        public decimal TotalSaving { get; set; }
        public decimal TotalLoanCapital { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalShares { get; set; }
        public int NumberOfDeposit { get; set; }
        public int NumberOfSaving { get; set; }
        public int NumberOfShare { get; set; }
        public int NumberOfLoanRepayment { get; set; }
        public decimal NetSalryUploaded { get; set; }
    }
    public class DashboardViewModel
    {
        public int TotalMembers { get; set; }
        public decimal TotalNetSalary { get; set; }
        public decimal TotalLoanRepayment { get; set; }
        public decimal TotalSavings { get; set; }
        public decimal TotalCharges { get; set; }
        public decimal TotalSalary { get; set; }
        public decimal TotalRemainingSalary { get; set; }
        public decimal TotalPreferenceShares { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal TotalShares { get; set; }
        public decimal TotalLoanInterest { get; set; }
        public decimal TotalStandingOrderAmount { get; set; }
        public decimal TotalVAT { get; set; }// New property for Standing Order Amount
        public decimal TotalCapital { get; set; }// New property for Standing Order Amount
    }

}
