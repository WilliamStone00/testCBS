using CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonMapping;
using System;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.MobileMoneyV2
{
	public class MobileMoney
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string BranchId { get; set; }

		public string TelNumber { get; set; }
		public string Address { get; set; }
		public string BranchName { get; set; }
		public string BranchCode { get; set; }
		public string OperatorType { get; set; }
		public bool ActiveStatus { get; set; }

		public string BranchAccountId { get; set; }
		public string COAName { get; set; }
		public string COANumber { get; set; }

		public DateTime? CreatedDate { get; set; }
	}
}
