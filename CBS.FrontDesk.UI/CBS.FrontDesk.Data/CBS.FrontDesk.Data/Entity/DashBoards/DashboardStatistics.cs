using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.DashBoards
{
    public class MemberStatistics
    {
        public string Members { get; set; }
        public string ActiveMmembers { get; set; }
        public string InActiveMmembers { get; set; }
    }
    public class LoanStatistics
    {
        public string NumberOfLoans { get; set; }
        public string VolumeOfLoansCapital { get; set; }
        public string OutstandingBalance { get; set; }
        public string NumberOfLoansRefunded { get; set; }
        public string VolumeRefunded { get; set; }
        public string AccrualInterest { get; set; }
        public string AccrualInterestPaid { get; set; }
        public string AccrualInterestOutstanding { get; set; }
        public string Penalties { get; set; }
        public string PenaltiesPaid { get; set; }
        public string PenaltiesOutstanding { get; set; }
        public string Vat { get; set; }
        public string VatPaid { get; set; }
        public string VatOutstanding { get; set; }
    }
    public class OrdinaryAccountStatistics
    {
        public string SN { get; set; }
        public string OrdinaryAccountBalance { get; set; }
        public string AccountName { get; set; }
      
    }
    public class AccountingBookingStatistics
    {
        public string TotalLiquidity { get; set; }
        public string CashInBank { get; set; }
        public string CashInHand56 { get; set; }
        public string CashInHand57 { get; set; }
        public string TotalInCome { get; set; }
        public string TotalExpenses { get; set; }
    }

    public class DailyOperationStatistics
    {
        public string NumberOfCashIn { get; set; }
        public string VolumeOfCashIn { get; set; }
        public string NumberOfCashOut { get; set; }
        public string VolumeOfCashOut { get; set; }
        public string NumberOfLoans { get; set; }
        public string VolumeOfLoans { get; set; }
        public string NumberOfRepayments { get; set; }
        public string VolumeOfRepayments { get; set; }
    }

}
