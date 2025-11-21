namespace CBS.BusinessService.AccountingV2.GLSystemReconciliation
{
    public class CloseOfDayModel
    {

        public string BranchId { get; set; }
        public string Reference { get; set; }
        public bool AllTransactionsReconciled { get; set; }
        public bool CashVaultVerified { get; set; }
        public bool InterBranchConfirmed { get; set; }
        public bool AnyIssueToBeRaised { get; set; }
        public string Comment { get; set; }
        public string ClosedBy { get; set; }
    }
}