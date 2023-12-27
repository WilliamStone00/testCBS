using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class LoanCommeteeMember
    {
        public string id { get; set; }
        public string commiteeName { get; set; }
        public List<string> userId { get; set; }
        public string suppervisor_userId { get; set; }
        public int loanLimit { get; set; }
    }

}
