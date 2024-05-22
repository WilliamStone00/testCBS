using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class ChartofAccountManagementPosition
    {
        public string Id { get; set; }
        public string PositionNumber { get; set; }
        public string Description { get; set; }
        public string RootDescription { get; set; }
        public string ChartOfAccountId { get; set; }
    }
    public class ManagementSelectionOption
    {
        public string Id { get;   set; }
        public string AccountNumber { get;   set; }
        public string PositionNumber { get;   set; }
        public string Description { get;   set; }
        public object GeneralRepresentation { get;   set; }
    }
}
