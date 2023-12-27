using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class LoanCollatera
    {
        public string id { get; set; }
        public string collateraId { get; set; }
        public string loanApplicationId { get; set; }
        public string value { get; set; }
    }

}
