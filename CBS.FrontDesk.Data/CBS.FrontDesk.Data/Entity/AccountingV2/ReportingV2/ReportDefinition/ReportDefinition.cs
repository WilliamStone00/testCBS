using System;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportDefinition
{
	public class ReportDefinition
	{
		public string Id { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
		public string ReportType { get; set; }
		public string Currency { get; set; }
		public bool IsActive { get; set; }
		public DateTime? CreatedDate { get; set; }
	}
}
