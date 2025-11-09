namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.TrialBalance
{
    internal class TrialBalanceReportItem
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal OpeningDebit { get; set; }
        public decimal OpeningCredit { get; set; }
        public decimal MovementDebit { get; set; }
        public decimal MovementCredit { get; set; }
        public decimal ClosingDebit { get; set; }
        public decimal ClosingCredit { get; set; }
    }
}