using System;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportSection
{
	public class ReportSection
	{
		public string Id { get; set; }
		public string ReportId { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
		public int SortOrder { get; set; }
		public DateTime? CreatedDate { get; set; }
	}
}