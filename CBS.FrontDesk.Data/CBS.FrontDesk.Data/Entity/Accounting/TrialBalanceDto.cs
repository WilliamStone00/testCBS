namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class TrialBalanceDto
    {
        public string beginningDebitBalance { get; internal set; }

        public string creditBalance { get; internal set; }
        public string accountNumber { get; internal set; }
        public string accountName { get; internal set; }
        public string beginningCreditBalance { get; internal set; }
        public string debitBalance { get; internal set; }
        public string endDebitBalance { get; internal set; }
        public string endCreditBalance { get; internal set; }
    }
}