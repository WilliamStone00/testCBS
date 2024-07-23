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
        public string Description { get; set; }
        public string RootDescription { get; set; }
        public string ChartOfAccountId { get; set; }
        public string PositionNumber { get; set; }
        public string Level_Management { get; set; }
        public string AccountNumber { get; set; }
        public string Old_AccountNumber { get; set; }
        public string New_AccountNumber { get; set; }
        public bool IsHeadOfficeAccount { get; set; }
    }

    public class ChartofAccountMFI
    {

        public string Description { get; set; }

        public string Old_AccountNumber { get; set; }
        public string New_AccountNumber { get; set; }

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
