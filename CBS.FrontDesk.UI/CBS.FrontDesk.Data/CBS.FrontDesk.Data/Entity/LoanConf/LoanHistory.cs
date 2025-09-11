using CBS.FrontDesk.Data.Entity.LoanConf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Config
{
    public class LoanHistory
    {

        public List<Loan> Loans { get; set; }
        public string ServiceOption { get; set; }
        public string Action { get; set; }
        public LoanHistory()
        {

            Loans = new List<Loan>();
        }

    }
}
