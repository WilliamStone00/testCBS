namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class IncomeStatement
    {
        public string Reference { get; set; }
        public string Document_type { get; set; }

        public string GrossChartOfAccountId { get; set; }

        public string OperationSideGross { get; set; }

        public string AmortizationChartOfAccountId { get; set; }

        public string OperationSideAmortization { get; set; }
        public string Heading { get; set; }
        public string Id { get; set; }
 
    }
}