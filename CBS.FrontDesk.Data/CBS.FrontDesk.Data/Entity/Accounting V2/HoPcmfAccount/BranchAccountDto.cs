using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.HoPcmfAccount
{
    public class BranchAccountDto
    {
        public string Id { get; set; }
        public string BranchId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public string AffiliateAccountId { get; set; }
        public string ParentId { get; set; }
        public string Path { get; set; }
        public int Depth { get; set; }
        public bool PostingAllowed { get; set; }


    }

    public class CreateBranchAccountCommand
    {
        public string BranchId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public string AffiliateAccountId { get; set; }
        public string ParentId { get; set; }
        public bool PostingAllowed { get; set; } = true;
    }

    public class UpdateBranchAccountCommand
    {
        public string Id { get; set; } 
        public string Code { get; set; } 
        public string Name { get; set; } 
        public string Class { get; set; } 
        public string AffiliateAccountId { get; set; } 

    }

    public class BranchAccountsDataTableQuery
    {
        public DataTableOptions Options { get; set; }
        public BranchAccountsDataTableQuery() { Options = new DataTableOptions(); }

        public string BranchId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public string AffiliateAccountId { get; set; }
        public string ParentId { get; set; }
        public string PathContains { get; set; }
        public int? DepthFrom { get; set; }
        public int? DepthTo { get; set; }
        public bool? PostingAllowed { get; set; }
        public DateTime? CreatedFromUtc { get; set; }
        public DateTime? CreatedToUtc { get; set; }
        public bool IncludeDeleted { get; set; } = false;

    }
}
