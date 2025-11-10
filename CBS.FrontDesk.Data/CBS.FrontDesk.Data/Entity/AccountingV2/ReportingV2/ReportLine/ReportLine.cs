using System;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportLine
{
	public class ReportLine
	{
		public string Id { get; set; }
		public string ReportId { get; set; }
		public string SectionId { get; set; }
		public string RefCode { get; set; }
		public string Caption { get; set; }
		public int SortOrder { get; set; }
		public string CalcMode { get; set; }
		public bool ShowInOutput { get; set; }
		public DateTime? CreatedDate { get; set; }
	}
}