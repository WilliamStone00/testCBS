namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class TrialBalanceDt
    {
 
        public string accountName { get; set; }
        public string accountNumber { get; set; }
        public double debitBalance { get; set; }
        public double creditBalance { get; set; }
    }
}