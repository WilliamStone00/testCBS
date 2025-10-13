using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate
{
    public class Affiliateresponse
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsHeadOffice { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class AffiliateCommand
    {
        public string Id { get; set; }
        public string Code { get; set; } 
        public string Name { get; set; } 
        public bool IsActive { get; set; } = true;
        public bool IsHeadOffice { get; set; }
    }

    public class AffiliateQuery
    {
        public DataTableOptions Options { get; set; }
        public AffiliateQuery() { Options = new DataTableOptions(); }

        public string Code { get; set; } = null;
        public string Name { get; set; } = null;
        public bool? IsActive { get; set; }
        public bool? IncludeDeleted { get; set; }
        public DateTime? CreatedFromUtc { get; set; }
        public DateTime? CreatedToUtc { get; set; }

    }
}
