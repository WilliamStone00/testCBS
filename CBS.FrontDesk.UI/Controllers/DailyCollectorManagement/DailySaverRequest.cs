namespace CBS.FrontDesk.UI.Controllers
{
    internal class DailySaverRequest
    {
        public object AccountNumber { get; set; }
        public object FirstName { get; set; }
        public object Username { get; set; }
        public object BranchCode { get; set; }
        public object DailySaverId { get; set; }
        public object BranchName { get; set; }
        public bool IsNewCustomer { get; set; }
        public string BankCode { get; set; }
        public object AccountId { get; set; }
        public decimal Amount { get; set; }
    }
}