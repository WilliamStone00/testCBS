namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class TrialBalanceDto
    {
        public double beginningDebitBalance { get; internal set; }

        public double creditBalance { get; internal set; }
        public string accountNumber { get; internal set; }
        public string accountName { get; internal set; }
        public double beginningCreditBalance { get; internal set; }
        public double debitBalance { get; internal set; }
        public double endDebitBalance { get; internal set; }
        public double endCreditBalance { get; internal set; }
    }
}