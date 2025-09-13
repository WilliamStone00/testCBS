using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    

    public class AccountTreeNode
    {
        public string id { get; set; }
        public string text { get; set; }
        public string icon { get; set; }
        public State state { get; set; }
        public string parentId { get; set; }
        public List<AccountTreeNode> children { get; set; }
        public Dictionary<string, object> li_attr { get; set; }
        public Dictionary<string, object> a_attr { get; set; }

    }

    public class State
    {
        public bool opened { get; set; }
        public bool disabled { get; set; }
        public bool selected { get; set; }
    }
}
