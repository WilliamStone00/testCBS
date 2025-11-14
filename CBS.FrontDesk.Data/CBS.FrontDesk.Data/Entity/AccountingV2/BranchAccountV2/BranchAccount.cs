using System;
using System.Collections.Generic;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.BranchAccountV2
{
	public class BranchAccount
	{
		public string Id { get; set; }
		public string Code { get; set; }
		public string AffiliateAccountId { get; set; }
		public string AffiliateAccountName { get; set; }
		public string Name { get; set; }
		public string Class { get; set; }
		public bool PostingAllowed { get; set; }
		public int Depth { get; set; }
		public string Path { get; set; }
		public DateTime CreatedDate { get; set; }
		public bool IsDeleted { get; set; }
		public List<BranchAccount> Children { get; set; } = new List<BranchAccount>();
	}
}
