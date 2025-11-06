using System;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonMappingV2
{
	public class LiaisonMapping
	{
		public string Id { get; set; }
		public string BranchId { get; set; }
		public string BranchName { get; set; }
		public string BranchCode { get; set; }
		public string AccountNumber { get; set; }

		public string CounterpartyBranchId { get; set; }
		public string CounterpartyBranchName { get; set; }

		public string DueFromAssetAccountId { get; set; }
		public string DueFromAssetAccountName { get; set; }
		public string DueFromAssetAccountNumber { get; set; }

		public string DueToLiabilityAccountId { get; set; }
		public string DueToLiabilityAccountNumber { get; set; }
		public string DueToLiabilityAccountName { get; set; }

		public DateTime? CreatedDate { get; set; }
	}
}
