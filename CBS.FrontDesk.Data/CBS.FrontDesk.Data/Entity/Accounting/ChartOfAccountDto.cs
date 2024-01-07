using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class ChartOfAccountDto
    {
        public string RootParentId { get; set; }
        public string AccountNumber { get; set; }
        public string LabelEn { get; set; }
        public string LabelFr { get; set; }
        public bool IsBalanceAccount { get; set; }


    }

    public class JsData
    {
        public string id { get; set; }
        public string text { get; set; }
        public string icon { get; set; }
        public State state { get; set; }
        public string parentId { get; set; }
        public List<JsData> children { get; set; }
        public Dictionary<string, object> li_attr { get; set; }
        public Dictionary<string, object> a_attr { get; set; }
    }
}