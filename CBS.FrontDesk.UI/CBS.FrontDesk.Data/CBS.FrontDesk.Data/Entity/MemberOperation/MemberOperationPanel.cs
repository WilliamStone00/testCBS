using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.LoanCommitee;
using CBS.FrontDesk.Data.Entity.LoanConf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.MemberOperation
{
    public class MemberOperationPanel
    {
        public AddLoanDisbumentCommand AddLoanDisbumentCommand { get; set; }
        public LoanApplication LoanApplication { get; set; }
        public List<LoanApplicationFee> LoanApplicationFees { get; set; }
        public LoanApplicationFee LoanApplicationFee { get; set; }
        public LoanGuarantor LoanGuarantor { get; set; }
        public List<LoanGuarantor> LoanGuarantors { get; set; }
        public LoanCommentry LoanCommentry { get; set; }
        public List<LoanCommentry> LoanCommentries { get; set; }
        public DocumentAttachedToLoan DocumentAttachedToLoan { get; set; }
        public List<DocumentAttachedToLoan> DocumentAttachedToLoans { get; set; }
        public LoanApplicationCollateral LoanCollatera { get; set; }
        public List<LoanApplicationCollateral> LoanCollateras { get; set; }
        public UpdateLoanApplicationStatusCommand UpdateLoanApplicationStatus { get; set; }
        public List<LoanApplication> LoanApplications { get; set; }
        public List<IndividualProfile> Customers { get; set; }
        public AddOTPNotificationCommand AddOTPNotificationCommand { get; set; }
        public IndividualProfile Customer { get; set; }
        public Account Account { get; set; }
        public List<LoanCommiteeValidationHistory> LoanCommiteeValidationHistories { get; set; }
        public LoanCommiteeValidationHistory LoanCommiteeValidationHistory { get; set; }
        public LoanParameters LoanParameter { get; set; }
        public LoanAmortization LoanAmortization { get; set; }
        public List<LoanAmortization> LoanAmortizations { get; set; }
        public RefundDetail RefundDetail { get; set; }
        public List<RefundDetail> RefundDetails { get; set; }
        public AddLoanApplicationCommand AddLoanApplicationCommand { get; set; }
        public List<Refund> Refunds { get; set; }
        public Refund Refund { get; set; }
        public Loan Loan { get; set; }
        public UpdateLoanApplicationCommand UpdateLoanApplicationCommand { get; set; }
        public List<Loan> Loans { get; set; }
        public List<Loan> SelectLoans { get; set; }
        public string ServiceOption { get; set; }
        public string Path { get; set; }
        public MemberOperationPanel()
        {
            UpdateLoanApplicationCommand=new UpdateLoanApplicationCommand();
            LoanCommentry = new LoanCommentry();
            LoanCommentries = new List<LoanCommentry>();
            AddLoanApplicationCommand = new AddLoanApplicationCommand();
            LoanApplication = new LoanApplication();
            Customer = new IndividualProfile();
            LoanCollatera = new LoanApplicationCollateral();
            DocumentAttachedToLoan = new DocumentAttachedToLoan();
            LoanCollateras = new List<LoanApplicationCollateral>();
            LoanCommiteeValidationHistories = new List<LoanCommiteeValidationHistory>();
            Account = new Account();
            AddOTPNotificationCommand=new AddOTPNotificationCommand();
            LoanCommiteeValidationHistory = new LoanCommiteeValidationHistory();
            LoanApplications = new List<LoanApplication>();
            Customers = new List<IndividualProfile>();
            Loan = new Loan();
            AddLoanDisbumentCommand=new AddLoanDisbumentCommand();
            LoanGuarantor = new LoanGuarantor();
            LoanGuarantors = new List<LoanGuarantor>();
            Refund = new Refund();
            UpdateLoanApplicationStatus = new UpdateLoanApplicationStatusCommand();
            RefundDetails = new List<RefundDetail>();
            Refunds = new List<Refund>();
            LoanAmortizations = new List<LoanAmortization>();
            RefundDetail = new RefundDetail();
            Loans = new List<Loan>();
            LoanAmortization = new LoanAmortization();
            LoanParameter = new LoanParameters();
            SelectLoans = new List<Loan>();
        }
    }
    public class UpdateLoanApplicationCommand
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Customer ID is required.")]
        public string CustomerId { get; set; } // Member Reference ID

        [Required(ErrorMessage = "Loan amount is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Loan amount must be greater than zero.")]
        public decimal Amount { get; set; } // Loan amount applied

        [Required(ErrorMessage = "Loan duration is required.")]
        [Range(1, 360, ErrorMessage = "Loan duration must be between 1 and 360 months.")]
        public int LoanDuration { get; set; } // Loan term in months

        [Required(ErrorMessage = "Interest rate is required.")]
        [Range(0, 100, ErrorMessage = "Interest rate must be between 0% and 100%.")]
        public decimal InterestRate { get; set; } // Loan interest rate

        public bool RequiredDownPaymentCoverageRate { get; set; } // Determines if a down payment is required

        public bool IsThereGuarantor { get; set; } // Determines if a guarantor is provided

        public bool IsThereCollateral { get; set; } // Determines if a collateral is provided

        [Range(0, double.MaxValue, ErrorMessage = "Ordinary share account coverage amount must be a non-negative value.")]
        public decimal ShareAccountCoverageAmount { get; set; } // Ordinary shares used for coverage

        [Range(0, 100, ErrorMessage = "Saving account coverage rate must be between 0% and 100%.")]
        public decimal SavingAccountCoverageRate { get; set; } // Saving coverage percentage

        public IndividualProfile Customer { get; set; }

        [Required(ErrorMessage = "At least one fee must be selected.")]
        [MinLength(1, ErrorMessage = "Select at least one loan fee.")]
        public string[] FeeIds { get; set; }

        public UpdateLoanApplicationCommand()
        {
            // Initialize FeeIds to ensure it's not null
            FeeIds = new string[0];
        }

       
    }

    //public class MemberOperationPanel
    //{
    //    public AddLoanDisbumentCommand AddLoanDisbumentCommand { get; set; }
    //    public LoanApplication LoanApplication { get; set; }
    //    public List<LoanApplicationFee> LoanApplicationFees { get; set; }
    //    public LoanApplicationFee LoanApplicationFee { get; set; }
    //    public LoanGuarantor LoanGuarantor { get; set; }
    //    public List<LoanGuarantor> LoanGuarantors { get; set; }
    //    public LoanCommentry LoanCommentry { get; set; }
    //    public List<LoanCommentry> LoanCommentries { get; set; }
    //    public DocumentAttachedToLoan DocumentAttachedToLoan { get; set; }
    //    public List<DocumentAttachedToLoan> DocumentAttachedToLoans { get; set; }
    //    public LoanApplicationCollateral LoanCollatera { get; set; }
    //    public List<LoanApplicationCollateral> LoanCollateras { get; set; }
    //    public UpdateLoanApplicationStatusCommand UpdateLoanApplicationStatus  { get; set; }
    //    public List<LoanApplication> LoanApplications { get; set; }
    //    public List<IndividualProfile> Customers { get; set; }
    //    public AddOTPNotificationCommand AddOTPNotificationCommand { get; set; }
    //    public IndividualProfile Customer { get; set; }
    //    public Account Account { get; set; }
    //    public List<LoanCommiteeValidationHistory> LoanCommiteeValidationHistories { get; set; }
    //    public LoanCommiteeValidationHistory LoanCommiteeValidationHistory { get; set; }
    //    public LoanParameters LoanParameter { get; set; }
    //    public LoanAmortization LoanAmortization { get; set; }
    //    public List<LoanAmortization> LoanAmortizations { get; set; }
    //    public RefundDetail RefundDetail { get; set; }
    //    public List<RefundDetail> RefundDetails { get; set; }
    //    public AddLoanApplicationCommand AddLoanApplicationCommand { get; set; }
    //    public List<Refund> Refunds { get; set; }
    //    public Refund Refund { get; set; }
    //    public Loan Loan { get; set; }
    //    public List<Loan> Loans { get; set; }
    //    public List<Loan> SelectLoans { get; set; }
    //    public string ServiceOption { get; set; }
    //    public string Path { get; set; }
    //    public MemberOperationPanel()
    //    {
    //        LoanCommentry = new LoanCommentry();
    //        LoanCommentries = new List<LoanCommentry>();
    //        AddLoanApplicationCommand = new AddLoanApplicationCommand();
    //        LoanApplication = new LoanApplication();
    //        Customer = new IndividualProfile();
    //        LoanCollatera = new LoanApplicationCollateral();
    //        DocumentAttachedToLoan = new DocumentAttachedToLoan();
    //        LoanCollateras = new List<LoanApplicationCollateral>();
    //        LoanCommiteeValidationHistories = new List<LoanCommiteeValidationHistory>();
    //        Account = new Account();
    //        AddOTPNotificationCommand=new AddOTPNotificationCommand();
    //        LoanCommiteeValidationHistory = new LoanCommiteeValidationHistory();
    //        LoanApplications = new List<LoanApplication>();
    //        Customers = new List<IndividualProfile>();
    //        Loan = new Loan();
    //        AddLoanDisbumentCommand=new AddLoanDisbumentCommand();
    //        LoanGuarantor = new LoanGuarantor();
    //        LoanGuarantors = new List<LoanGuarantor>();
    //        Refund = new Refund();
    //        UpdateLoanApplicationStatus = new UpdateLoanApplicationStatusCommand();
    //        RefundDetails = new List<RefundDetail>();
    //        Refunds = new List<Refund>();
    //        LoanAmortizations = new List<LoanAmortization>();
    //        RefundDetail = new RefundDetail();
    //        Loans = new List<Loan>();
    //        LoanAmortization = new LoanAmortization();
    //        LoanParameter = new LoanParameters();
    //        SelectLoans = new List<Loan>();
    //    }
    //}
}
