using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.AffiliateAccount
{
    public class AffiliateAccountDto
    {
        public string Id { get; set; }
        public string AffiliateId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public string HoPcmfAccountId { get; set; }
        public string ParentId { get; set; }
        public string NameFr { get; set; }
        public string NameEn { get; set; }
        public string Path { get; set; }
        public bool IsActive { get; set; }
        public bool IsHeadOffice { get; set; }
        public int Depth { get; set; }
        public bool PostingAllowed { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }



    //public class UpdateAffiliateAccountCommand
    //{
    //    public string Id { get; set; }
    //    public string Code { get; set; }
    //    public string NameEn { get; set; }
    //    public string NameFr { get; set; }
    //    public string Class { get; set; }
    //    public string HoPcmfAccountId { get; set; }
    //    public string ParentId { get; set; }
    //    public bool PostingAllowed { get; set; }
    //}

    public sealed class AddAffiliateAccountCommand
    {
        public string Id { get; set; }
        public string AffiliateId { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameFr { get; set; }
        public bool IsActive { get; set; }
        public string Class { get; set; }
        public string HoPcmfAccountId { get; set; }
        public string ParentId { get; set; }
        public bool PostingAllowed { get; set; } = true;
    }

    public class AffiliateAccountQuery
    {
        public DataTableOptions Options { get; set; }
        public AffiliateAccountQuery() { Options = new DataTableOptions(); }

        public string AffiliateId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public string ParentId { get; set; }
        public bool? PostingAllowed { get; set; }
        public DateTime? CreatedFromUtc { get; set; }
        public DateTime? CreatedToUtc { get; set; }
        public bool IncludeDeleted { get; set; } = false;
        public string Language { get; set; }

    }

    public class AffiliateAccountTreeViewModel
    {
        public AffiliateAccountDto Node { get; set; }
        public List<AffiliateAccountDto> ParentChain { get; set; } = new List<AffiliateAccountDto>();
        public TreeNodeaccounting Children { get; set; }
    }

    public class TreeNodeaccounting
    {
        public AffiliateAccountDto Item { get; set; }
        public int Count { get; set; }
        public List<TreeNodeaccounting> Children { get; set; } = new List<TreeNodeaccounting>();
    }
}
