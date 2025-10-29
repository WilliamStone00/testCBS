using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.BranchAccount
{
    public class BranchAccountCommand
    {
        public string Id { get; set; }
        public string BranchId { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameFr { get; set; }
        public string Class { get; set; }
        public string AffiliateAccountId { get; set; }
        public string ParentId { get; set; }
        public bool PostingAllowed { get; set; } = false;
    }

    public class moveAccounttonewparent
    {
        public string Id { get; set; }
        public string NewParentId { get; set; }
    }

    public class postingAllowed
    {
        public string Id { get; set; }
        public string PostingAllowed { get; set; }
    }

    public class BRANCHTreeDto
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string NameEn { get; set; }
        public string BranchId { get; set; }
        public string AffiliateAccountId { get; set; }
        public string AffiliateAccountName { get; set; }
        public string NameFr { get; set; }
        public string Class { get; set; }
        public string ParentId { get; set; }
        public bool PostingAllowed { get; set; }
        public int Depth { get; set; }
        public string Path { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public List<BRANCHTreeDto> Children { get; set; } = new List<BRANCHTreeDto>();
    }

    public class BranchAccountResponse
    {
        public string Id { get; set; }
        public string BranchId { get; set; }
        public string NameEn { get; set; }
        public string Scope { get; set; }
        public string Code { get; set; }
        public string NameFr { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public string Class { get; set; }
        public string AffiliateAccountId { get; set; }
        public string ParentId { get; set; }
        public string Path { get; set; }
        public string  Language { get; set; }
        public int? Depth { get; set; }
        public bool PostingAllowed { get; set; }
        public bool RequiresMapping { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class BranchAccountQuery
    {
        public DataTableOptions Options { get; set; }
        public BranchAccountQuery() { Options = new DataTableOptions(); }

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
