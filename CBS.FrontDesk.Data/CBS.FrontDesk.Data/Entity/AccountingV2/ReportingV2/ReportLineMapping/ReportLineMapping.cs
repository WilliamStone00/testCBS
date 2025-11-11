using System;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportLineMapping
{
	public class ReportLineMapping
	{
		public string Id { get; set; }
		public string LineId { get; set; }
		public string MatchType { get; set; }
		public string FromValue { get; set; }
		public string ToValue { get; set; }
		public string Side { get; set; }
		public short Sign { get; set; }
		public bool IncludeLiaison { get; set; }
		public DateTime? CreatedDate { get; set; }
	}
}

