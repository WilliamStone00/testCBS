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
        public decimal TotalStandingOrderAmount { get; set; } // New property for Standing Order Amount
    }

}
