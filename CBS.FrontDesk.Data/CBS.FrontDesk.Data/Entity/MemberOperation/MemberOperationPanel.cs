using CBS.FrontDesk.Data.Entity.CustomerManagement;
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
        public LoanApplication LoanApplication { get; set; }
        public List<LoanApplication> LoanApplications { get; set; }
        public List<IndividualProfile> Customers { get; set; }
        public IndividualProfile Customer { get; set; }
        public Account Account { get; set; }
        public LoanParameters LoanParameter { get; set; }
        public LoanAmortization LoanAmortization { get; set; }
        public List<LoanAmortization> LoanAmortizations { get; set; }
        public RefundDetail RefundDetail { get; set; }
        public List<RefundDetail> RefundDetails { get; set; }
        public List<Refund> Refunds { get; set; }
        public Refund Refund { get; set; }
        public Loan Loan { get; set; }
        public List<Loan> Loans { get; set; }
        public List<Loan> SelectLoans { get; set; }
        public MemberOperationPanel()
        {
            LoanApplication = new LoanApplication();
            Customer = new IndividualProfile();
            Account = new Account();
            LoanApplications = new List<LoanApplication>();
            Customers = new List<IndividualProfile>();
            Loan = new Loan();
            Refund = new Refund();
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
