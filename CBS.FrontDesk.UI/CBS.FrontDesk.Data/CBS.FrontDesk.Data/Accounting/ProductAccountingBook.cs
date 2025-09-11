using CBS.FrontDesk.Data.Entity.Accounting;

namespace CBS.FrontDesk.Data
{
    public class ProductAccountingBook
    {
        public string Id { get; set; }
        public string ProductAccountingBookId { get; set; }
        public string ChartOfAccountId { get; set; } //ChartOfAccountId 
        public string ChartOfAccountManagementPositionId { get; set; }
        public string Name { get; set; }
        public string ProductAccountingBookName { get; set; }
        public string AccountTypeId { get; set; }
        public string OperationEventAttributeId { get; set; }
        public string ProductType { get; set; }

    }
}