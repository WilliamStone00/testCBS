using System.Collections.Generic;

namespace CBS.FrontDesk.Data
{
    public class StatementModel
    {
        public string Id { get; set; }
        public string Reference { get; set; }
        public string Heading { get; set; }
        public string OperationSide { get; set; }
        public string OperationSideGross  { get; set; }
        public string OperationSideAmortization { get; set; }
        public string Document_type { get; set; }
        public List<string> AccountIds { get; set; }
        public List<string> AmortizationChartOfAccountId { get; set; }
        public List<string> GrossChartOfAccountId { get; set; }

        //public string? ChartOfAccountId { get; set; }

        //public string? OperationSide { get; set; }

        //public string? OperationSideAmortization { get; set; }
        //public string? OperationSideGross { get; set; }

        //public string? AmortizationChartOfAccountId { get; set; }
        //public string StatementModelId { get; set; }
        //public string? GrossChartOfAccountId { get; set; }
    }

    public class StatementModel00
    {
        public string Id { get; set; }
        public string Reference { get; set; }
        public string Heading { get; set; }
 
        public string OperationSideGross { get; set; }
        public string OperationSideAmortizations { get; set; }
        public string Document_type { get; set; }
 
        public List<string> AmortizationAccountIds { get; set; }
        public List<string> GrossAccountIds { get; set; }
    }
}