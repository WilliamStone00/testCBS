using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.LoanCommitee;
using CBS.FrontDesk.Data.Entity.LoanConf;
using System;
using System.Collections.Generic;
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
        public UpdateLoanApplicationStatusCommand UpdateLoanApplicationStatus  { get; set; }
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
        public List<Loan> Loans { get; set; }
        public List<Loan> SelectLoans { get; set; }
        public string ServiceOption { get; set; }
        public string Path { get; set; }
        public MemberOperationPanel()
        {
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
}
