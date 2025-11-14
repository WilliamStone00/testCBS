using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.MockData.DTos
{
    public class GenericTransferDto
    {
        public string ReportKind { get; set; }
        public string ReportName { get; set; }
        public string RptPath { get; set; }
        public string RptTitle { get; set; }
    }

    public class SingleReportParams : GenericTransferDto
    {
        public const string BasePath = "~/AppFiles/Accountingv2Reporting/ReportRPT/";

        private string _reportName;

        // Use 'new' to hide base property
        public new string ReportName
        {
            get => _reportName;
            set
            {
                _reportName = value;
                // Automatically update RptPath whenever ReportName is set
                RptPath = $"{BasePath}{_reportName}";
            }
        }

        public SingleReportParams()
        {
            ReportKind = "ReportParameterLess"; // default
        }
    }


}
