using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.HoPcmfAccount
{
    public class HoPcmfAccount
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameFr { get; set; }
        public string Class { get; set; }
        public string ParentId { get; set; }
        public string Path { get; set; }
        public int Depth { get; set; }
        public bool PostingAllowed { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }

    // Entities/HoPcmfAccountTreeDto.cs
    public class HoPcmfAccountTreeDto
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int AccountNumber { get; set; }
        public string NameEn { get; set; }
        public string NameFr { get; set; }
        public string Class { get; set; }
        public string ParentId { get; set; }
        public bool PostingAllowed { get; set; }
        public int Depth { get; set; }
        public string Path { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public List<HoPcmfAccountTreeDto> Children { get; set; } = new List<HoPcmfAccountTreeDto>();

         }

    // Entities/AccountTreeNode.cs (for jsTree compatibility)
    public class AccountTreeNode
    {
        public string id { get; set; }
        public string text { get; set; }
        public string icon { get; set; } = "jstree-file";
        public State state { get; set; } = new State();
        public string parentId { get; set; } = "#";
        public List<AccountTreeNode> children { get; set; } = new List<AccountTreeNode>();
        public Dictionary<string, object> li_attr { get; set; }
        public Dictionary<string, object> a_attr { get; set; }
    }

    public class HoPcmfAccountTreeViewModel
    {
        public HoPcmfAccountTreeDto Node { get; set; }
        public IEnumerable<HoPcmfAccountTreeDto> ParentChain { get; set; }
        public HoPcmfAccountTreeDto Children { get; set; } // can be null or represent subtree
    }

    public class State
    {
        public bool opened { get; set; } = false;
        public bool disabled { get; set; } = false;
        public bool selected { get; set; } = false;
    }

    // Entities/UpdateAccountNameRequest.cs
    public class UpdateAccountNameRequest
    {
        public string Id { get; set; }
        public string NameEn { get; set; }
        public string NameFr { get; set; }
        public string Code { get; set; }
        public string Class { get; set; }
        public string Language { get; set; } = "en";
    }

    public class GetHoPcmfCoaQuery
    {
        public DataTableOptions Options { get; set; }
        public GetHoPcmfCoaQuery() { Options = new DataTableOptions(); }
       
            public string Code { get; set; }
            public string Name { get; set; }
            public string Class { get; set; }
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
