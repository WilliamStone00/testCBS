using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.HoPcmfAccount
{
    public class BranchAccountTreeDto
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public bool PostingAllowed { get; set; }
        public int Depth { get; set; }
        public string Path { get; set; }
        public List<BranchAccountTreeDto> Children { get; set; } = new List<BranchAccountTreeDto>();
    }

    public class JsTreeNode
    {
        public string id { get; set; }         // required
        public string text { get; set; }       // label shown on node
        public bool children { get; set; }     // whether node has children (or List<JsTreeNode> children)
        public object state { get; set; }      // optional: opened/selected/disabled
        public Dictionary<string, object> li_attr { get; set; } // optional
        public Dictionary<string, object> a_attr { get; set; }  // optional
    }

}
