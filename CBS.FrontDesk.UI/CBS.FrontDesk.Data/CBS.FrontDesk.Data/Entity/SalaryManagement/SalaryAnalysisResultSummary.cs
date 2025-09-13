using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace CBS.FrontDesk.Data.Entity.SalaryManagement
{
    public class SalaryAnalysisResultSummary
    {
        public string Id { get; set; }
        public string TotalMembers { get; set; }
        public decimal TotalNetSalary { get; set; }
        public decimal TotalLoanCapital { get; set; }
        public decimal TotalLoanInterest { get; set; }
        public decimal TotalVAT { get; set; }
        public decimal TotalLoanRepayment { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal TotalSavings { get; set; }
        public decimal TotalCharges { get; set; }
        public decimal TotalShares { get; set; }
        public decimal TotalRemainingSalary { get; set; }
        public int TotalBranches { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string FileUploadId { get; set; }
        public string ExecutedBy { get; set; }
        public DateTime Date { get; set; }
        public FileUploadDto FileUpload { get; set; }
        public ICollection<SalaryAnalysisResultDetailNew> salaryAnalysisResultDetails { get; set; }

    }
    public class SalaryAnalysisResultDetailNew
    {
        public string Id { get; set; }
        public string Matricule { get; set; }
        public string CustomerId { get; set; }
        public string MemberName { get; set; }
        public decimal NetSalary { get; set; }

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

        public string SpSavingId { get; set; }
        public string LoanType { get; set; }
        public string SpSavingProductId { get; set; }
        public string SpSavingProductName { get; set; }
        public decimal SpSavingCapital { get; set; }
        public decimal SpSavingInterest { get; set; }
        public decimal SpSavingVAT { get; set; }
        public decimal SpSavingTotalRepayment { get; set; }

        public decimal Deposit { get; set; }

        public decimal Salary { get; set; }

        public decimal Savings { get; set; }
        public decimal Charges { get; set; }
        public decimal Shares { get; set; }
        public decimal PreferenceShares { get; set; }
        public decimal RemainingSalary { get; set; }

        public bool IsOnldLoan { get; set; }
        public decimal StandingOrderAmount { get; set; }
        public string StandingOrderStatement { get; set; }
        public string Status { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string BranchId { get; set; }
        public string SalaryAnalysisResultId { get; set; }
        public SalaryAnalysisResultSummary SalaryAnalysisResult { get; set; }

    }

    public class SalaryAnalysisResultDetail
    {
        public string Id { get; set; }
        public string Matricule { get; set; }
        public string CustomerId { get; set; }
        public string MemberName { get; set; }
        public decimal NetSalary { get; set; }
        public decimal LoanCapital { get; set; }
        public decimal LoanInterest { get; set; }
        public decimal VAT { get; set; }
        public decimal TotalLoanRepayment { get; set; }
        public decimal Deposit { get; set; }
        public decimal Savings { get; set; }
        public decimal Charges { get; set; }
        public decimal Shares { get; set; }
        public decimal RemainingSalary { get; set; }
        public string Status { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string BranchId { get; set; }
        public string SalaryAnalysisResultId { get; set; }
        public decimal PreferenceShares { get; set; }
        public string LoanId { get; set; }
        public string LoanType { get; set; }
        public decimal StandingOrderAmount { get; set; }
        public string StandingOrderStatement { get; set; }
        public string LoanProductId { get; set; }
        public string LoanProductName { get; set; }
        public bool IsOnldLoan { get; set; }
        public SalaryAnalysisResultSummary SalaryAnalysisResult { get; set; }
    }
    public class SalaryAnalysisCommand
    {
        public string FileUploadId { get; set; }
    }
}
