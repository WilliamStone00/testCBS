namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class BranchToBranchTransfer
    {
        public string AccountId { get; set; }
        public string Balance { get; set; }
        public string LiaiAccountId { get; set; }
        public string Amount { get; set; }
        public string Description { get; set; }
    }
}